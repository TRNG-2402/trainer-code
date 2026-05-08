# Azure Core Services

## Learning Objectives
- Identify Azure's core compute services and when each fits.
- Identify Azure's core storage services and the differences among Blob, Table, Queue, and File.
- Identify Azure's core networking services and the role each plays.
- Place these services in the IaaS / PaaS / SaaS spectrum so the right tool gets picked for the right job.

## Why This Matters

Azure has hundreds of services. Most of them are variations on the same dozen ideas. Once you know the core services — VMs, App Service, Functions, Blob Storage, VNet, NSG — you can place almost anything else by analogy. This is the catalog you reach for when designing or deploying an app.

## The Concept

### The IaaS / PaaS / SaaS Spectrum

Quick recap, anchored to Azure:

- **IaaS** (you run the OS and the app) — Virtual Machines, Virtual Networks, Disks. Maximum control, maximum operational burden.
- **PaaS** (Azure runs the OS, you run the app) — App Service, Azure Functions, Azure SQL Database. Less control, far less operational burden.
- **SaaS** (Azure runs the whole product) — Microsoft 365, Dynamics 365. You configure; you do not deploy.

Defaulting to PaaS is the right starting point for a new app. Choose IaaS only when you actually need OS-level control.

### Compute

**Virtual Machines (VMs).** The IaaS workhorse. Pick a Windows or Linux image, a size (CPU + RAM), a region, attach a disk, get a publicly addressable Linux/Windows server. You handle everything from the OS up: patching, runtime, app code, scaling. Use when you need a specific OS, a specific kernel, custom drivers, or are lifting-and-shifting a legacy workload.

**App Service.** PaaS for web apps and APIs. Deploy a `.zip`, a container, or a Git push and Azure runs it on a managed Linux or Windows host. Built-in scaling, HTTPS, custom domains, deployment slots (warm staging slots that swap into production), auto-patching, integrated CI/CD with GitHub Actions. The right default for an ASP.NET Core API or a static React build behind a node host.

**Azure Functions.** Serverless compute. You write a function (HTTP trigger, queue trigger, timer trigger, blob trigger); Azure runs it on demand and scales to zero when idle. Pay per execution (Consumption plan) or use a Premium/App Service plan for warm instances. Right for event-driven workloads, lightweight APIs, scheduled jobs, glue between services.

**Azure Kubernetes Service (AKS).** Managed Kubernetes. Azure runs the control plane; you manage worker nodes and your container workloads. Use when you have many services, need fine-grained orchestration, or are standardizing on Kubernetes across clouds.

**Container Instances (ACI).** A single container as a service — no orchestration, no cluster. Quick way to run a containerized app for short-lived tasks or simple sidecars.

For this curriculum's project (React + ASP.NET Core), the natural pick is **App Service** for the API and **App Service** (or **Azure Storage static website**) for the React build.

### Storage

**Storage Account.** The top-level container for all Azure Storage offerings. Inside one Storage Account you can have any combination of Blob, Table, Queue, and File services. Names are globally unique; they form the URL host (`<name>.blob.core.windows.net`).

The four offerings:

| Service | What it stores | Typical use | Access |
|---|---|---|---|
| **Blob** | Unstructured object data — files, images, video, backups, build artifacts. Three tiers: Block (default), Page (VHDs), Append (logs). | Backups, static assets, file uploads, Data Lake (with hierarchical namespace). | HTTP REST, SDKs, AzCopy, `azure/cli`, Storage Explorer. |
| **Table** | Schemaless key/value rows (NoSQL). Cheap, scales to billions of rows. | Log/event archives, simple lookups. For richer queries / global distribution, use Cosmos DB instead. | Table API, SDKs. |
| **Queue** | Simple FIFO message queue. Messages up to 64 KB. | Decoupling work between services, batching jobs, basic event-driven triggers. For richer messaging, use Service Bus. | Queue API, SDKs. |
| **File** | SMB / NFS file share — looks like a network drive. | Lift-and-shift workloads expecting a file share, shared application config, dev workstations. | Mount as `\\<account>.file.core.windows.net\share` (SMB) or NFS, REST. |

You will use **Blob** the most. Static website hosting on Blob is how you serve a React `dist/` folder cheaply.

### Networking

**Virtual Network (VNet).** Your private network in Azure. You pick the address space (`10.0.0.0/16`) and carve it into subnets (`10.0.1.0/24`, `10.0.2.0/24`). Resources placed in a VNet can talk over private IPs without traversing the public internet. The networking foundation for anything that should not be public.

**Subnet.** A range inside the VNet. Different subnets can have different rules (one for app servers, one for databases, one for management).

**Network Security Group (NSG).** A stateful firewall attached to a subnet or NIC. You write inbound and outbound rules — allow port 443 from anywhere, allow port 1433 from the app subnet only, deny everything else. Default rules deny inbound from the internet and allow outbound to it.

**Load Balancer.** Layer-4 (TCP/UDP) traffic distribution across a backend pool of VMs. Public Load Balancer fronts a VMSS or set of VMs from the internet; Internal Load Balancer balances traffic inside the VNet.

**Application Gateway.** Layer-7 (HTTP/HTTPS) load balancer with built-in TLS termination, WAF (Web Application Firewall), URL-based routing, session affinity. Use in front of web apps when you need WAF or path-based routing.

**Azure Front Door.** Global edge layer-7 load balancer with caching, WAF, and TLS at the edge. Sits *in front of* App Gateway / App Service / origin. Use for global, multi-region apps.

**Public IP.** An internet-routable IPv4/IPv6 address you can attach to a VM NIC, Load Balancer, App Gateway, or Bastion. Static or dynamic; SKU Basic or Standard (Standard for production / zone-redundant).

**Private DNS Zone.** Azure-managed DNS for names that should resolve only inside your VNet. Pairs with Private Link to give PaaS resources private IPs that resolve via your private DNS.

### Where Things Fit Together

A typical small production stack on Azure:

```
Internet
  |
  v
Azure Front Door  (global L7 + WAF + cache)
  |
  v
App Gateway       (regional L7 + WAF)
  |
  v
App Service       (your ASP.NET Core API, Linux)
  |    \
  |     \-- Storage Account (Blob: uploaded files, static React assets)
  |
  v
Azure SQL DB      (PaaS database, Private Link into the VNet)
```

The compute is PaaS, the storage is PaaS, the database is PaaS. The VNet, NSGs, and Private Link keep traffic off the public internet between tiers. This is the shape we will wire up in the Azure-with-GHA topics later in this folder.

## Code Example

Provisioning the skeleton of the stack via Azure CLI, just to show the surface area. Don't run all of these yet — they cost money. Read for shape.

```bash
# Resource group
az group create -n rg-trainee-demo -l eastus

# App Service plan + Web App (PaaS compute)
az appservice plan create -g rg-trainee-demo -n plan-trainee --sku B1 --is-linux
az webapp create -g rg-trainee-demo -p plan-trainee -n trainee-api-eastus \
  --runtime "DOTNETCORE:8.0"

# Storage Account (Blob + static website)
az storage account create -g rg-trainee-demo -n storaineedemo \
  --location eastus --sku Standard_LRS --kind StorageV2

# Virtual Network + subnet
az network vnet create -g rg-trainee-demo -n vnet-trainee \
  --address-prefix 10.0.0.0/16 --subnet-name app --subnet-prefix 10.0.1.0/24

# NSG with one allow-https rule
az network nsg create -g rg-trainee-demo -n nsg-app
az network nsg rule create -g rg-trainee-demo --nsg-name nsg-app -n allow-https \
  --priority 100 --direction Inbound --access Allow --protocol Tcp \
  --destination-port-ranges 443
```

In one resource group: PaaS compute, PaaS storage, plus VNet/NSG networking primitives. The same pattern scales up to many resources without changing how you think about the layout.

## Summary

- Compute: VMs (IaaS), App Service (PaaS web/API default), Functions (serverless), AKS (managed Kubernetes), ACI (single containers).
- Storage: a Storage Account holds Blob (objects/files), Table (NoSQL key-value), Queue (small messages), File (SMB/NFS shares). Blob is the workhorse.
- Networking: VNet + Subnets define your private IP space; NSGs are stateful firewalls; Load Balancer (L4) and Application Gateway (L7) distribute traffic; Front Door is the global edge.
- Default to PaaS (App Service, Azure SQL, Storage) unless you have a specific reason to manage a VM.

## Additional Resources

- [Azure compute services overview](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/compute-decision-tree)
- [Azure storage services overview](https://learn.microsoft.com/en-us/azure/storage/common/storage-introduction)
- [Azure networking services overview](https://learn.microsoft.com/en-us/azure/networking/fundamentals/networking-overview)
