# Azure Portal & Resource Manager

## Learning Objectives
- Navigate the Azure Portal and identify the core UI components.
- Define Azure Resource Manager (ARM) and explain why it is the deployment/management plane.
- Place every Azure resource correctly in the ARM hierarchy: Management Group -> Subscription -> Resource Group -> Resource.
- Distinguish a region, an availability zone, and a resource group, and recognize what each implies for deployment.

## Why This Matters

Every Azure command, every deployment, every cost line, every RBAC assignment routes through ARM. The Portal is just a web client on top of ARM. If you understand the hierarchy and the Portal layout, almost every "where do I configure X?" question answers itself, because Azure's UI is consistent across services. This is the reading that turns "I clicked around until something worked" into "I know what I am looking for."

## The Concept

### The Azure Portal

The Portal is the web UI for everything in Azure: `https://portal.azure.com`. Sign in with a Microsoft Entra ID account that has access to one or more subscriptions.

Components you will use daily:

- **Left-hand navigation.** Home, Dashboard, All resources, Resource groups, Subscriptions, plus pinned services. Use the **All services** entry point or the top **search bar** to jump to anything.
- **Top search bar.** Searches across resources, services, marketplace, and documentation. Faster than navigating menus once you know service names.
- **Cloud Shell** (the `>_` icon top right). An in-browser Bash or PowerShell session pre-authenticated to your subscription with `az` and Azure PowerShell already installed. Use it when you need a CLI but cannot install one locally.
- **Notifications bell.** Status of recent deployments, role assignments, alerts.
- **Subscription / directory selector** (top right). When you have multiple tenants or subscriptions, this is where you switch context — many "I cannot see my resource" issues are wrong-subscription issues.
- **Cost Management blade.** Your spend, budgets, forecasts, and cost analysis broken down by resource group, tag, or service.

The Portal is the easiest entry point. For repeatable work, prefer the CLI (`az`) or infrastructure-as-code (Bicep/ARM) — covered in later topics.

### Azure Resource Manager (ARM)

**ARM is the deployment and management layer for Azure.** Everything you do — creating a VM, listing storage accounts, assigning a role, deleting a database — goes through one consistent REST API exposed by ARM. The Portal, the CLI, the SDKs, Bicep, and Terraform all call ARM under the hood.

Three properties of ARM that matter:

1. **One API surface.** The same authentication, RBAC, and tagging story applies to every resource type. Learn the pattern once; it works everywhere.
2. **Declarative deployments.** ARM accepts JSON (ARM templates) or Bicep describing the desired state of a group of resources. ARM figures out create/update/delete to match.
3. **Idempotent.** Submit the same template twice and you get the same end state, not duplicates. Submit a slightly changed template and ARM updates only what differs.

When the Portal shows you a "JSON view" link on a resource — that is the ARM representation. When the CLI returns a JSON object — same thing.

### The Hierarchy: MG -> Subscription -> RG -> Resource

Azure organizes everything into a four-level hierarchy. Knowing the level at which you operate is half the battle.

```
Management Group(s)
    |
    +-- Subscription
            |
            +-- Resource Group
                    |
                    +-- Resource (VM, Storage Account, App Service, ...)
```

- **Management Group** — top-level container for organizing many subscriptions. Used by enterprises to apply policies and RBAC at scale (e.g., "all subscriptions under the Production MG must use eastus2 only"). A small team or solo developer rarely creates these.

- **Subscription** — a billing and access boundary. Costs roll up here; quota limits apply here; you sign in to a "subscription context." Every resource lives in exactly one subscription. Most personal/training accounts have one or two subscriptions.

- **Resource Group (RG)** — a logical container for a set of related resources that share a lifecycle. Deploy together, monitor together, tear down together. An RG has a default location (used when a resource type does not specify its own region), but the resources inside it can live in different regions if you choose. **Deleting an RG deletes every resource inside it** — useful for clean teardown of a project, dangerous if you grab the wrong RG.

- **Resource** — an actual cloud entity: a VM, a Storage Account, a Web App, a SQL Database, a Virtual Network. Each has a globally unique `resourceId` of the form `/subscriptions/<sub-id>/resourceGroups/<rg-name>/providers/<provider>/<type>/<name>`. That string is what RBAC, ARM templates, and CLI commands use to address a resource precisely.

Tags are flat metadata you can apply at any level (`Environment=Prod`, `Owner=team-foo`, `CostCenter=12345`). Tags propagate to cost reports — tag aggressively from day one or you will regret it later.

### Region vs Availability Zone vs Resource Group

These three are routinely conflated. They are different things.

| Concept | What it is | Boundary |
|---|---|---|
| **Region** | A geographic area Azure operates in (e.g., `eastus`, `westeurope`, `southeastasia`). Each region is a cluster of one or more datacenters. | Geography. Affects latency, data residency, available services, pricing. |
| **Availability Zone (AZ)** | A physically separate datacenter inside a region with independent power, cooling, and networking. Most large regions have 3 AZs. | High-availability inside a region. Deploying across AZs survives a single-DC failure. |
| **Resource Group (RG)** | A logical management container. | Lifecycle / billing / RBAC. **Not** a geographic boundary — only a default region for resources placed in it. |

Practical implications:

- Deploy your prod VMs across **multiple AZs** in one region for HA. Deploy across **multiple regions** for disaster recovery / geo-redundancy.
- Group resources for one app (App Service + SQL DB + Storage Account) into one RG so they share a lifecycle. Don't put unrelated apps in the same RG just because "they are in the same region."
- An RG can contain resources in different regions; you are not forced to align RG and resource regions.

Pick a region close to your users for latency. Pick AZs for HA. Pick RGs for organization.

## Code Examples

Most of today's work is in the Portal, but here is the Azure CLI shape so you can connect Portal actions to ARM calls:

```bash
# Sign in. Opens a browser the first time.
az login

# List subscriptions; the * marks the active one.
az account list --output table

# Switch to a specific subscription.
az account set --subscription "<subscription-id-or-name>"

# Create a resource group in eastus.
az group create --name rg-trainee-demo --location eastus

# List everything in that RG.
az resource list --resource-group rg-trainee-demo --output table

# Tag the RG.
az group update --name rg-trainee-demo --set tags.Environment=Demo tags.Owner=$USER

# Get the JSON representation of an RG (this is what ARM stores).
az group show --name rg-trainee-demo

# Tear down the entire RG and everything in it (irreversible).
az group delete --name rg-trainee-demo --yes
```

Every one of these calls hits the same ARM REST API the Portal uses.

## Summary

- The Azure Portal is a web client on top of ARM; everything you click eventually calls the same REST API.
- ARM is the deployment + management plane: one API, one RBAC model, declarative + idempotent.
- Hierarchy is Management Group -> Subscription -> Resource Group -> Resource. Every resource lives in exactly one RG in exactly one subscription.
- Region = geography; Availability Zone = isolated DC inside a region (HA boundary); Resource Group = logical lifecycle/billing container, not a geography boundary.
- Tag resources from day one; deleting an RG deletes everything inside it — be deliberate.

## Additional Resources

- [Azure Resource Manager overview](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/overview)
- [Azure regions and availability zones](https://learn.microsoft.com/en-us/azure/reliability/availability-zones-overview)
- [Organize your Azure resources effectively](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-setup-guide/organize-resources)
