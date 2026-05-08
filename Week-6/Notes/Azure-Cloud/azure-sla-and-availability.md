# SLAs, Availability Zones, and Geo-Redundancy

## Learning Objectives
- Define a Service Level Agreement (SLA) and identify the key terms (uptime target, exclusions, service credits).
- Distinguish Availability Zones (AZ) and geo-redundancy and what each survives.
- Pick a redundancy strategy that matches the failure mode you need to mitigate.
- Use paired regions correctly when designing for disaster recovery.

## Why This Matters

Cloud uptime is a contract, not a wish. The vendor states a target; the customer designs to meet a target equal or better. Knowing the wording lets you (a) read the SLA and not be surprised, (b) raise your effective SLA by combining services, and (c) make defensible choices when leadership asks "is this design highly available?"

## The Concept

### What an SLA Is

A **Service Level Agreement** is a contract from a cloud provider (Microsoft) that states a measurable target for a service — almost always availability — and the credits owed back to the customer if the target is missed in a given billing month. It includes:

- **Uptime target** — a percentage like 99.9%, 99.95%, 99.99%. Different services have different targets.
- **Calculation method** — how Azure measures availability (typically the percentage of time the service is reachable / functioning per region per month).
- **Exclusions** — outages that don't count against the SLA (planned maintenance with notice, customer error, force majeure, beta features).
- **Service credits** — the discount applied to the next month's bill if the SLA is missed. Credits scale with how badly the SLA was missed (e.g., 10% credit if uptime fell below 99.9% but stayed above 99%; 25% if it fell below 99%; 100% in extreme cases).
- **Process** — typically the customer must file a claim within 30-60 days; Azure does not auto-credit.

A few uptime targets to anchor on:

| "Nines" | Target | Annual downtime budget | Monthly downtime budget |
|---|---|---|---|
| Two nines | 99% | ~3.65 days | ~7.2 hours |
| Three nines | 99.9% | ~8.76 hours | ~43.8 minutes |
| Three-and-a-half | 99.95% | ~4.38 hours | ~21.9 minutes |
| Four nines | 99.99% | ~52.6 minutes | ~4.4 minutes |
| Five nines | 99.999% | ~5.26 minutes | ~26 seconds |

Two practical points:

1. **Read the actual SLA per service.** Storage redundant tiers, App Service tiers, SQL DB tiers, AKS — each has its own number. Different tiers of the same service often have different SLAs.
2. **The SLA is a refund mechanism, not a fix.** Your users do not get their afternoon back because you got 10% off. Design to *exceed* the SLA, not just meet it.

### Multiplying SLAs (the hidden trap)

When your app depends on multiple services, the **effective SLA is the product** of each service's SLA, not the minimum.

Example: Web App at 99.95% + SQL DB at 99.99% + Storage at 99.9% (single-service path).
- Effective: 0.9995 x 0.9999 x 0.999 = **~99.84%** (~14 hours/year).

Each dependency drags the overall number down. Two ways to push it back up:

- **Redundancy across zones / regions** of the same service so the per-service availability is itself the product of redundant copies. Two AZ-spread copies at 99.95% each = ~99.9999975% combined (theoretically; practical limits apply).
- **Reduce dependencies in the hot path.** Cache, queue, and degrade gracefully so that one service being down does not take the whole flow with it.

### Availability Zones

**Availability Zones** are physically separate datacenters within a region — independent power, cooling, networking. Most "zonal" Azure regions have **3 AZs**.

Key properties:

- **Spread your replicas across AZs** for in-region HA. A single-DC fire / power loss / network blip in one AZ does not affect the others.
- Many Azure services support **zone-redundant** SKUs that spread replicas automatically (Standard Load Balancer, Standard Public IP, Application Gateway v2, Storage ZRS, Azure SQL DB Premium/Business Critical, App Service Premium v3 with Zone Redundancy enabled).
- **Latency between AZs** is typically <2 ms — synchronous replication is feasible.
- AZs raise SLA: a single-AZ App Service might offer 99.95%, the same App Service deployed zone-redundant offers 99.99%.

When you see "highly available" in an architecture doc, the bar is at minimum: **multi-AZ within one region.**

### Geo-Redundancy: Multi-Region

Single-region multi-AZ does not survive a **regional** outage (rare, but they happen — power, fiber, weather, software). For that, you replicate **across regions**.

Two flavors:

- **Active-passive (failover)** — primary region serves traffic; secondary region runs idle (or warm). On failure, you fail traffic over (manual or automated). Simpler, cheaper, longer Recovery Time Objective (RTO).
- **Active-active** — traffic is served from both regions simultaneously, fronted by **Azure Front Door** or **Traffic Manager** for routing. Much harder (data consistency, conflict resolution), shorter RTO, much higher cost.

Azure-side primitives:

- **GRS / GZRS Storage** — automatic asynchronous replication of blobs to a paired region. Failover is manual (`az storage account failover`).
- **Azure SQL geo-replication / failover groups** — async secondary in a paired region, listener endpoint that switches on failover.
- **Front Door** — global anycast L7 load balancer with health probes; can route traffic between regional origins.
- **Traffic Manager** — DNS-level traffic routing across regions (priority, weighted, performance, geographic).

### Paired Regions

Most Azure regions have a **pair** — `eastus` <-> `westus`, `northeurope` <-> `westeurope`, `southeastasia` <-> `eastasia`. Pairs sit far enough apart that a single disaster won't take both, and Azure uses pairs for several behaviors:

- GRS / RA-GRS Storage replicates between the pair.
- Azure system updates are rolled out to one of the pair at a time, never both simultaneously, so a botched update cannot take down both.
- Some Azure services allow region recovery only across the documented pair.

You can build cross-region replication outside the pair, but pairs are the **default** because Azure's own engineering is pair-aware.

### How to Choose

A simple decision tree:

- **Dev / sandbox**: single AZ, LRS storage, no geo. Cheapest. Acceptable risk.
- **Production, region-local**: multi-AZ for compute, ZRS storage. Survives a single-DC failure. ~99.99% per service.
- **Production, regulated / mission-critical**: multi-AZ + geo-redundant storage (GZRS) + cross-region warm secondary, with documented failover runbook. Survives a regional disaster.
- **Globally distributed**: active-active across paired regions, fronted by Front Door, with conflict-tolerant data design.

Most apps land in the "multi-AZ in one region" tier and never need more.

## Code Examples

### Read the SLA target via Azure docs (no CLI for this)

SLA pages live at `https://www.microsoft.com/en-us/licensing/docs/view/Service-Level-Agreements` and per-service in the docs. Always pull the latest — Microsoft updates SLA documents periodically.

### Provision zone-redundant resources via CLI

```bash
RG=rg-productcatalog-prod
LOC=eastus

# Storage Account: ZRS = zone-redundant inside the region.
az storage account create -g $RG -n storaineeprod1234 -l $LOC \
  --sku Standard_ZRS --kind StorageV2

# App Service plan: Premium v3 with zone redundancy.
az appservice plan create -g $RG -n plan-prod-p1v3 \
  --is-linux --sku P1v3 --zone-redundant true

# SQL DB: Business Critical with zone redundancy.
az sql db create -g $RG -s sql-trainee-prod -n appdb \
  --service-objective BC_Gen5_2 --zone-redundant true
```

### Geo-redundant Storage and a manual failover

```bash
# Provision GZRS (geo-zone-redundant) at create time.
az storage account create -g $RG -n storaineeprod1234 -l $LOC \
  --sku Standard_GZRS --kind StorageV2

# Inspect last-sync time (RPO indicator).
az storage account show -g $RG -n storaineeprod1234 \
  --query "geoReplicationStats" -o json

# Trigger a customer-initiated failover (irreversible: pair becomes new primary).
az storage account failover -g $RG -n storaineeprod1234
```

### SQL DB failover group across paired regions

```bash
PRIMARY=sql-trainee-eastus
SECONDARY=sql-trainee-westus

az sql server create -g $RG -n $SECONDARY -l westus \
  --admin-user sqladmin --admin-password "<strong>"

az sql failover-group create -g $RG \
  --name appdb-fog \
  --partner-server $SECONDARY \
  --server $PRIMARY \
  --add-db appdb \
  --failover-policy Automatic --grace-period 1
```

After this, your app uses `appdb-fog.database.windows.net` — a single endpoint that always points at the current primary, with automatic failover on regional failure.

## Summary

- An SLA is a contract: uptime target, exclusions, service credits. Read each service's SLA; tiers within a service often differ.
- Effective SLA across dependent services is the **product** of each — chain of three services at three nines is below three nines overall.
- Availability Zones survive single-DC failure inside a region; zone-redundant SKUs raise per-service SLA.
- Geo-redundancy survives regional disasters; use paired regions for replication; pick active-passive vs active-active based on RTO and cost tolerance.
- Default ladder: single-AZ for dev, multi-AZ for prod, multi-AZ + multi-region for mission-critical.

## Additional Resources

- [Azure Service Level Agreements (SLA)](https://www.microsoft.com/en-us/licensing/docs/view/Service-Level-Agreements)
- [Azure regions and availability zones](https://learn.microsoft.com/en-us/azure/reliability/availability-zones-overview)
- [Cross-region replication and paired regions](https://learn.microsoft.com/en-us/azure/reliability/cross-region-replication-azure)
