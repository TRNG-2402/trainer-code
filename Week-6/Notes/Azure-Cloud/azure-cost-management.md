# Cost Management: Estimate, Budget, Alert

## Learning Objectives
- Use the Pricing Calculator to forecast spend for a planned workload.
- Navigate Cost Management to see actual spend, forecasts, and breakdowns.
- Create a budget with threshold alerts so you find out about overspend before it hits the credit card.
- Apply a "requirement-analysis-then-tier" mindset when sizing Azure resources.

## Why This Matters

Cloud bills surprise people. Most surprises are not "service Y is expensive" — they are "I forgot service Y was running" or "I picked the production tier when basic would have done." A 30-minute habit (estimate -> tag -> budget -> review weekly) keeps spend predictable. The QC-4 objectives "set up cost-management budget alert" and "manage selection and cost through requirement analysis" both live here.

## The Concept

### The Two Tools

Azure ships two cost tools that complement each other:

| Tool | What it shows | When you use it |
|---|---|---|
| **Pricing Calculator** | *Forecasted* monthly cost based on planned configuration | Before provisioning. Architecture review. Tier selection. |
| **Cost Management + Billing** | *Actual* spend on resources you have | After provisioning. Daily/weekly checks. Tracking against budgets. |

Use them together: the Calculator predicts, Cost Management measures, the gap between them tells you whether your sizing assumptions held.

### Pricing Calculator

`https://azure.microsoft.com/pricing/calculator`. Add services, pick tiers/regions/usage, get a monthly estimate.

Workflow:

1. Add each planned service: Web App, SQL DB, Storage, Application Insights, etc.
2. For compute: pick the same region and tier you plan to provision; set hours per month (730 = always-on, 160 = 8 hr/day weekdays).
3. For storage: estimate GB stored, transactions, egress.
4. For databases: pick DTU/vCore tier and the data size.
5. Save and **share the URL**. Architecture decisions captured against a saved estimate are reviewable.

Two habits make estimates honest:

- **Pick the same region you will deploy to.** Same SKU in `eastus` vs `westeurope` can differ by 5-15%.
- **Estimate the long tail.** People overestimate compute cost and underestimate egress, log ingestion, and "tiny services" (Application Insights, Log Analytics, NAT Gateway egress). Add them.

### Cost Management Blade

Portal -> Cost Management + Billing. Key views:

- **Cost analysis** — pivot actual spend by Resource group, Service, Location, Tag, Subscription. The most-used view.
- **Cost alerts** — Anomaly detection alerts plus your budget alerts.
- **Budgets** — what we cover next.
- **Advisor recommendations** — cost rightsizing tips (idle VMs, oversized DBs).
- **Billing accounts / invoices** — formal invoices. (Skip unless you handle finance.)

In Cost analysis, **save views** filtered by tag (`Environment=Dev`) or RG so the same query is one click next time. **Group by Resource group** is the highest-signal default.

Cost data lags 8-24 hours, so today's spend usually shows up tomorrow. Forecasts reflect this delay.

### Budgets and Threshold Alerts

A **budget** is a monthly (or quarterly/annually) spend cap with **thresholds** that fire alerts. The cap itself does **not** stop spending — it triggers notifications. (You build automated stops with Logic Apps + the alert webhook if you really need them.)

Define:

- **Scope** — Subscription / Resource Group / Management Group.
- **Amount** — the cap.
- **Reset period** — Monthly, Quarterly, Annually, BillingMonth, BillingQuarter, BillingAnnual.
- **Filters** (optional) — tag, service, region.
- **Alert thresholds** — percentages (50%, 80%, 100%) of either actual spend or forecasted spend, each with email recipients and (optionally) action group / webhook.

Sensible defaults for a trainee or small team subscription:

- One budget per RG with thresholds at 50% (heads-up), 80% (caution), 100% (act).
- One subscription-wide budget as a backstop with thresholds at 80% and 100%.
- **Forecast** alerts in addition to actual — they fire when the projected month-end spend will exceed the threshold, giving you days of lead time instead of hours.

### Requirement Analysis Before Tier Selection

The cheapest way to control spend is to pick the right tier the first time. The QC-4 "manage selection and cost through requirement analysis" objective is asking for this discipline.

A short loop you can apply per resource:

1. **Estimate the workload.** RPS at peak, RPS at average, data size in GB, expected growth, peak hours / always-on, expected concurrent users.
2. **Pick the smallest tier that meets it.** App Service B1 vs P1v3 is a 5-10x cost difference; SQL DB Basic vs S0 is 2x; Premium Storage is 2-3x Standard.
3. **Validate against the Pricing Calculator.**
4. **Provision and observe.** Use Azure Monitor + Cost Management for two-three weeks.
5. **Right-size based on real metrics.** If CPU tops out at 30%, drop a tier. If responses queue up, climb one.

Anti-patterns: picking Premium "to be safe" without measuring; picking Basic "to be cheap" then watching latency spike; never re-evaluating after launch.

### Other Cost Levers

- **Stop/deallocate dev resources off-hours.** A B2s VM at 730 hr/month vs 160 hr/month is ~4.5x cheaper. Same for App Service Dev/Test plans.
- **Use the right access tier for Blob Storage.** Cool tier costs ~half of Hot per GB stored, with higher per-transaction cost — perfect for backups and old logs.
- **Reserved Instances / Savings Plans.** 1- or 3-year commitments cut compute prices 30-65%. Good for predictable production workloads, not dev experiments.
- **Spot VMs.** Up to 90% discount in exchange for Azure being able to evict you with 30 seconds' notice. Right for batch / fault-tolerant workloads.
- **Azure Hybrid Benefit.** If your org has on-prem Windows or SQL licenses, you can apply them to Azure VMs and save on the OS license portion.
- **Auto-shutdown** on dev VMs. The DevTest Labs feature, or a tag-based runbook, halts forgetful trainees from leaving VMs running over the weekend.

## Code Examples

### Pricing Calculator workflow (manual)

The Calculator is a web UI; there is no CLI for it. The QC-4-relevant workflow:

1. Open `https://azure.microsoft.com/pricing/calculator`.
2. Add services (Virtual Machines, App Service, Azure SQL DB, Storage, Application Insights).
3. For each: pick region, tier, hours, transactions, GB.
4. Click **Save** (top right) -> sign in -> name the estimate.
5. **Share** -> copy the URL into your design doc, README, or architecture review.

### Create a budget via CLI

```bash
SUB_ID=$(az account show --query id -o tsv)
RG=rg-productcatalog-dev

# JSON payload for the budget. Notification list set to email triage@example.com.
cat > budget.json <<'EOF'
{
  "properties": {
    "category": "Cost",
    "amount": 100,
    "timeGrain": "Monthly",
    "timePeriod": {
      "startDate": "2026-05-01T00:00:00Z",
      "endDate": "2027-05-01T00:00:00Z"
    },
    "notifications": {
      "Heads-up": {
        "enabled": true, "operator": "GreaterThan", "threshold": 50,
        "thresholdType": "Actual",
        "contactEmails": ["triage@example.com"]
      },
      "Caution": {
        "enabled": true, "operator": "GreaterThan", "threshold": 80,
        "thresholdType": "Actual",
        "contactEmails": ["triage@example.com"]
      },
      "Forecast-100pct": {
        "enabled": true, "operator": "GreaterThan", "threshold": 100,
        "thresholdType": "Forecasted",
        "contactEmails": ["triage@example.com"]
      }
    }
  }
}
EOF

az rest --method put \
  --uri "https://management.azure.com/subscriptions/$SUB_ID/resourceGroups/$RG/providers/Microsoft.Consumption/budgets/budget-rg-dev?api-version=2023-05-01" \
  --body @budget.json
```

(There is also `az consumption budget create` — the syntax varies between CLI versions; the `az rest` form above is portable.)

### Cost analysis via CLI (month-to-date)

```bash
RG=rg-productcatalog-dev
START=2026-05-01
END=2026-05-31

az consumption usage list \
  --start-date $START --end-date $END \
  --query "[?contains(instanceName, '$RG')].{Name:instanceName, Cost:pretaxCost, Date:usageStart}" \
  -o table | head -50
```

For richer queries, prefer the Cost Analysis blade in the Portal — it has saved views and charts.

## Summary

- Pricing Calculator forecasts; Cost Management measures actuals. Use both.
- Right-size against requirement analysis (RPS, data size, peak hours), validate via Calculator, provision the smallest tier that meets the workload, and re-size after observing real metrics.
- Budgets do not enforce a cap — they alert. Set thresholds at 50% / 80% / 100%, include forecast-based alerts for lead time.
- Tag every resource (`Environment`, `Owner`, `CostCenter`, `Project`); the cost reports become useful only with tags.
- The cheapest knobs that work: deallocate dev resources off-hours, use the right Storage access tier, consider Reserved Instances / Spot for steady or fault-tolerant workloads.

## Additional Resources

- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)
- [Cost Management + Billing overview](https://learn.microsoft.com/en-us/azure/cost-management-billing/)
- [Create and manage budgets](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets)
