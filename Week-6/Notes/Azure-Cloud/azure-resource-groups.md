# Resource Groups: Create, Tag, Manage

## Learning Objectives
- Create and delete a resource group from the Portal and the Azure CLI.
- Apply tags to a resource group and to individual resources, and explain why tagging matters for cost and ownership tracking.
- Manage multiple related resources through a single resource group as a lifecycle unit.
- Recognize the safety implications of resource-group operations.

## Why This Matters

A resource group (RG) is the smallest unit at which Azure groups things for billing, RBAC, and lifecycle. Pick the wrong grouping and your cost reports become useless, your role assignments leak privilege, and a "clean up this project" command takes hours of clicking. Pick the right grouping and an entire app's resources can be deployed, billed, and torn down as one unit. This topic is short and high-leverage.

## The Concept

### What an RG Is and Is Not

An RG is a **logical container** for resources that share a lifecycle. Think of it as a labeled box. Resources in the same box are deployed together, monitored together, and torn down together.

What an RG **is**:
- A scope for RBAC role assignments (give the dev team `Contributor` on `rg-app-dev` and they can manage everything inside).
- A scope for ARM deployments (`az deployment group create -g rg-foo ...` deploys a template into one RG).
- A scope for cost reports and tagging propagation.
- A target for `az group delete` — wiping the RG wipes every resource inside.

What an RG **is not**:
- A network boundary (VNets do that).
- A geography (a region does that — though the RG has a default location for resources that need one).
- A subscription (an RG lives inside exactly one subscription).
- A security boundary in itself (NSGs, Private Link, RBAC do that).

### Sensible Grouping

A good rule: **one RG per app per environment**.

```
rg-productcatalog-dev      # dev: App Service + SQL DB + Storage for the dev environment
rg-productcatalog-staging  # staging: same shape, different scale
rg-productcatalog-prod     # prod: same shape, prod scale, prod data
rg-shared-networking       # shared: VNet, DNS zones used across apps (separate lifecycle)
```

That layout gives you:
- Easy cleanup: `az group delete -n rg-productcatalog-dev` removes the entire dev environment.
- Clear cost lines: cost-management view by RG shows spend per app per environment.
- Targeted RBAC: grant the dev team `Contributor` on dev only; promote to prod via pipelines, not human access.

Anti-patterns: one giant RG holding everything; one RG per resource; mixing environments in one RG.

### Tagging

Tags are flat `key=value` metadata. Apply them at the RG level (and at the resource level for finer-grained reporting). The conventions to set on day one:

- `Environment` — `Dev` / `Staging` / `Prod`.
- `Owner` — team or individual responsible.
- `CostCenter` — billing code your finance team uses.
- `Project` or `Application` — for cross-cutting reports.
- `ExpiresOn` — for short-lived experiments; some teams have automation that warns or deletes after this date.

Tags propagate into Cost Management. With them, "what is the React app costing us in production this month?" is a one-filter query. Without them, it is a manual reconciliation.

### Operations on an RG

The lifecycle commands you will use most:

- **Create** — pick a name and a default region.
- **List** — see all RGs in the subscription, optionally filter by tag.
- **Show** — read the RG's metadata (location, tags, provisioning state).
- **Update** — change tags or the default location (rarely).
- **Delete** — destroys every resource inside. Asks for confirmation in the Portal; supports `--yes --no-wait` from the CLI.
- **Move resources** — `az resource move` shifts resources between RGs (within the same subscription) when the resource type supports it.
- **Lock** — apply a `CanNotDelete` or `ReadOnly` lock so accidental teardown is blocked. Critical for prod RGs.

### Safety: Locks and Soft-Delete

The single highest-leverage safety control on a production RG is a **`CanNotDelete` lock**:

```bash
az lock create -g rg-productcatalog-prod -n NoDelete --lock-type CanNotDelete
```

With that lock applied, no one — not even an Owner — can delete the RG without explicitly removing the lock first. Two-step destruction is the goal.

For storage and Key Vault specifically, enable **soft delete** so accidentally deleted blobs/secrets can be recovered within a retention window. RG-level locks plus per-service soft delete is the practical "oops" net.

## Code Examples

### Portal

1. **Create**: Portal -> Resource groups -> Create -> pick a Subscription, type a name, choose a Region -> Tags tab -> add `Environment=Dev`, `Owner=<you>` -> Review + create.
2. **Manage**: open the RG -> the **Overview** blade lists every resource inside -> **Access control (IAM)** for RBAC, **Tags** for metadata, **Locks** for safety, **Cost analysis** for spend.
3. **Delete**: RG -> Delete resource group at the top -> the Portal asks you to type the RG name to confirm.

### Azure CLI

```bash
# Create
az group create --name rg-productcatalog-dev --location eastus \
  --tags Environment=Dev Owner=$USER Project=ProductCatalog

# List (filter by tag)
az group list --query "[?tags.Environment=='Dev'].{Name:name, Location:location}" -o table

# Show
az group show --name rg-productcatalog-dev

# Add or update a tag without erasing the others
az group update --name rg-productcatalog-dev --set tags.CostCenter=12345

# Lock against deletion
az lock create --resource-group rg-productcatalog-dev \
  --name NoDelete --lock-type CanNotDelete

# Move a resource to another RG
az resource move \
  --destination-group rg-productcatalog-staging \
  --ids $(az webapp show -g rg-productcatalog-dev -n my-api --query id -o tsv)

# Delete (irreversible; remove locks first)
az lock delete --resource-group rg-productcatalog-dev --name NoDelete
az group delete --name rg-productcatalog-dev --yes --no-wait
```

### Treating an RG as a Lifecycle Unit

A common pattern for trainees and small teams: spin up an entire experiment under one RG, work in it for a week, then `az group delete` to wipe everything in one command — no orphaned resources, no surprise charges.

```bash
# Day 1: spin up the experiment
az group create -n rg-trainee-experiment -l eastus --tags ExpiresOn=2026-05-15

# ... add VMs, databases, storage, networking inside ...

# Day 7: tear it all down
az group delete -n rg-trainee-experiment --yes
```

That is the value an RG delivers in one command.

## Summary

- An RG is a logical lifecycle / billing / RBAC container — not a network or security boundary.
- Default layout: one RG per app per environment; a separate RG for shared infrastructure that outlives any one app.
- Tag every RG and resource (`Environment`, `Owner`, `CostCenter`, `Project`) for cost reporting and ownership.
- Apply `CanNotDelete` locks to production RGs; pair with soft-delete on storage and Key Vault.
- Deleting an RG deletes everything inside — that is the feature, but be deliberate.

## Additional Resources

- [Manage Azure Resource Groups (Portal)](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal)
- [Manage Azure Resource Groups (Azure CLI)](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-cli)
- [Tag resources, resource groups, and subscriptions](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/tag-resources)
