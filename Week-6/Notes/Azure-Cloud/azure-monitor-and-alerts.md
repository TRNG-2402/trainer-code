# Azure Monitor: Diagnostics, Logs, Alerts

## Learning Objectives
- Configure diagnostic settings on a resource so its metrics and logs flow into Log Analytics.
- Run basic Kusto Query Language (KQL) queries against a Log Analytics Workspace to find problems.
- Create an alert rule that fires on a metric or a log condition and routes through an action group.
- Use the same toolkit to debug a misbehaving resource end-to-end.

## Why This Matters

Cloud apps fail in ways your IDE can't show you — a database firewall blocks a deploy, a 500 in production touches a dependency that's two hops away, a slow request lives between the load balancer and the App Service. Azure Monitor is the layer where you see and diagnose those failures. Same toolkit you would use on the QC-4 "debug a cloud resource through resource logs and analytics" objective.

## The Concept

### Azure Monitor at a Glance

Azure Monitor is the umbrella for Azure's observability stack. The pieces:

- **Metrics** — numeric time-series, every 1 minute by default (CPU %, request count, latency, available memory). Cheap to store and query, instantly graphable.
- **Logs** — structured events (HTTP requests, exceptions, audit entries, custom telemetry). Stored in a **Log Analytics Workspace**. Richer than metrics; queried with KQL.
- **Application Insights** — APM (request traces, dependency calls, exceptions, custom events) for app code. Implemented as a flavor of Log Analytics with an SDK that auto-instruments .NET / Node / Python apps.
- **Alerts** — fire when a metric or log condition is met; route to an action group (email, SMS, webhook, Logic App, ITSM ticket).
- **Dashboards / Workbooks** — visualizations built on metrics and KQL queries.

### Diagnostic Settings

Azure resources do **not** automatically send their logs anywhere. You opt in per-resource by configuring a **diagnostic setting** that names the destination(s):

1. **Log Analytics Workspace** — the right default; lets you query with KQL.
2. **Storage Account** — cheap long-term archival for compliance.
3. **Event Hub** — stream to a SIEM or third-party tool.

Every resource exposes its own log categories. App Service offers `AppServiceHTTPLogs`, `AppServiceConsoleLogs`, `AppServiceAppLogs`, etc. SQL DB offers `SQLSecurityAuditEvents`, `QueryStoreRuntimeStatistics`. You toggle the categories you want and Azure starts forwarding.

### Log Analytics Workspace

The destination resource for Azure-side logs. Pricing is **per GB ingested + retention** (default 30 days, extendable to 2 years). Group resources that you query together into the same workspace — querying across workspaces works but is slower.

### Kusto Query Language (KQL)

KQL is the query language used in Log Analytics, Application Insights, Microsoft Sentinel, and Azure Resource Graph. It reads top-down: each `|` is a step in a pipeline.

Skeleton:

```kusto
<Table>
| where <filter>
| project <fields to keep>
| summarize <aggregation> by <bucket>
| order by <field> desc
| take <N>
```

Most KQL queries you write are five operators or fewer.

Key operators to learn first:

| Operator | What it does |
|---|---|
| `where` | Filter rows. |
| `project` | Pick columns. |
| `extend` | Add a computed column. |
| `summarize` | Group + aggregate (count, avg, percentiles). |
| `order by` | Sort. |
| `take` | Limit rows. |
| `join` | Combine two tables on a key. |
| `bin()` | Bucket numeric or time values. |

### Alerts

An **alert rule** has three pieces:

1. **Scope** — which resource(s) to monitor.
2. **Condition** — the signal and threshold (e.g., "CPU > 80% averaged over 5 minutes" or "any log entry matches this KQL"). Two main flavors:
   - **Metric alerts** — fast, cheap, fire within a minute or two when a metric crosses a threshold.
   - **Log alerts (a.k.a. log search alerts)** — run a KQL query on a schedule and fire if results meet a condition. Slower but immensely flexible.
3. **Action group** — a reusable list of actions: email, SMS, push, voice, webhook, Azure Function, Logic App, ITSM connector, runbook. Define once, attach to many alerts.

Alert severity levels (0 = critical, 4 = informational) drive what action group routing and on-call escalation looks like.

### A Debugging Loop with This Toolkit

When a deployed Web App returns 500s:

1. **Metrics first** — open the Web App -> Metrics, plot HTTP 5xx and Average Response Time over the last hour. Confirms there is a problem and when it started.
2. **Logs next** — Diagnostic Settings should already forward `AppServiceHTTPLogs` and `AppServiceAppLogs`. Open Logs (Log Analytics) and run KQL to find the failing requests, their timestamps, status codes, and the URL paths.
3. **Application Insights** for stack traces — find the exception type and the dependency it touched (SQL? a downstream API?).
4. **Cross-correlate** — if exceptions point to SQL, query `SQLSecurityAuditEvents` or query store data; if they point to Storage, check Storage metrics for throttling.
5. **Set an alert** so the next regression pages on-call before users tweet.

That is the pattern for "debug a cloud resource through logs and analytics."

## Code Examples

### Configure a diagnostic setting (CLI)

```bash
RG=rg-productcatalog-dev
APP=trainee-api-eastus
WORKSPACE=law-trainee
LOC=eastus

# 1. Create a Log Analytics Workspace.
az monitor log-analytics workspace create \
  -g $RG -n $WORKSPACE -l $LOC

WS_ID=$(az monitor log-analytics workspace show \
  -g $RG -n $WORKSPACE --query id -o tsv)

# 2. Send App Service logs + metrics to the workspace.
APP_ID=$(az webapp show -g $RG -n $APP --query id -o tsv)

az monitor diagnostic-settings create \
  --name "to-law-trainee" \
  --resource $APP_ID \
  --workspace $WS_ID \
  --logs '[{"category":"AppServiceHTTPLogs","enabled":true},
           {"category":"AppServiceConsoleLogs","enabled":true},
           {"category":"AppServiceAppLogs","enabled":true}]' \
  --metrics '[{"category":"AllMetrics","enabled":true}]'
```

### KQL queries you will actually run

```kusto
// 1. Recent 5xx responses on the Web App, last hour.
AppServiceHTTPLogs
| where TimeGenerated > ago(1h)
| where ScStatus >= 500
| project TimeGenerated, CsHost, CsUriStem, ScStatus, TimeTaken, UserAgent
| order by TimeGenerated desc
| take 50

// 2. p95 latency by URL, last 24 hours.
AppServiceHTTPLogs
| where TimeGenerated > ago(24h)
| summarize p50=percentile(TimeTaken, 50),
            p95=percentile(TimeTaken, 95),
            count()
            by CsUriStem
| order by p95 desc
| take 20

// 3. Top exceptions from Application Insights.
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| order by count_ desc

// 4. Errors per minute trend, plotted as a chart.
AppServiceHTTPLogs
| where TimeGenerated > ago(6h)
| where ScStatus >= 500
| summarize errors = count() by bin(TimeGenerated, 1m)
| render timechart
```

### Create a metric alert and an action group

```bash
# 1. Action group: send to a single email.
az monitor action-group create \
  -g $RG -n ag-trainee-email \
  --short-name TrainEmail \
  --action email triage triage@example.com

AG_ID=$(az monitor action-group show -g $RG -n ag-trainee-email --query id -o tsv)

# 2. Metric alert: HTTP 5xx > 5 in 5 minutes.
az monitor metrics alert create \
  -g $RG -n alert-app-5xx \
  --scopes $APP_ID \
  --condition "total Http5xx > 5" \
  --window-size 5m --evaluation-frequency 1m \
  --severity 2 \
  --action $AG_ID \
  --description "Web App returning more than 5 5xx in 5 minutes"
```

## Summary

- Resources don't ship logs anywhere by default; turn on a **diagnostic setting** pointing at a Log Analytics Workspace.
- Metrics for fast, numeric signals; Log Analytics + KQL for rich, structured events; Application Insights for in-app traces and exceptions.
- KQL is a small pipeline language: `where`, `project`, `summarize`, `order by`, `take` cover the majority of queries.
- Alerts attach metric or log conditions to action groups; severity levels drive escalation.
- Standard debug loop: metrics chart -> KQL on the workspace -> exception trace in App Insights -> set an alert so it pages next time.

## Additional Resources

- [Azure Monitor overview](https://learn.microsoft.com/en-us/azure/azure-monitor/overview)
- [Diagnostic settings in Azure Monitor](https://learn.microsoft.com/en-us/azure/azure-monitor/essentials/diagnostic-settings)
- [Get started with KQL](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/get-started-queries)
