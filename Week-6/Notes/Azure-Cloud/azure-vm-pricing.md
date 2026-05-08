# Azure VMs and the Pricing Model

## Learning Objectives
- Create a basic Azure Virtual Machine from the Portal and from the CLI.
- Explain how Azure bills VMs (per-second compute, plus storage, plus outbound bandwidth, plus optional add-ons).
- Stop-deallocate a VM correctly so compute charges actually stop.
- Use the Azure Pricing Calculator to estimate the monthly cost of a workload before provisioning it.

## Why This Matters

VMs are the most expensive resource trainees accidentally leave running. Knowing how billing works — and the difference between "Stopped" and "Stopped (deallocated)" — is the difference between "$5 of practice this month" and "$300 of forgotten machines." This topic is also the canonical example of IaaS pricing on Azure: once you understand it, App Service, Functions, and SQL DB pricing make sense by analogy.

## The Concept

### What an Azure VM Is

A VM is an IaaS compute resource: a virtualized server with a chosen OS image, CPU + RAM size, attached disks, and a NIC connected to a VNet. You manage everything from the OS up — patching, runtime, app deployment, scaling. Azure manages the hypervisor, the host hardware, the datacenter.

### Creating a Basic VM (Portal)

1. **Portal -> Virtual machines -> Create -> Azure virtual machine**.
2. **Basics tab:**
   - **Subscription** + **Resource group** (existing or create new).
   - **Virtual machine name** (e.g., `vm-trainee-01`).
   - **Region** (e.g., `East US`).
   - **Availability options** — `No infrastructure redundancy required` (cheapest), or `Availability zone` for HA across DCs.
   - **Image** — Ubuntu, Windows Server, etc.
   - **Size** — pick a small size for learning (e.g., `B2s` — 2 vCPU, 4 GB RAM).
   - **Authentication** — SSH public key (Linux) or password (Windows).
   - **Inbound port rules** — open only what you need (SSH 22 for Linux dev; RDP 3389 for Windows; nothing for production VMs that should be reached via Bastion / private link).
3. **Disks tab:** OS disk type (Standard SSD is the cost-conscious default; Premium SSD for production).
4. **Networking tab:** VNet + subnet (creates a default if none exists), public IP (optional), NSG rules.
5. **Management / Monitoring / Advanced / Tags:** sensible defaults; add `Environment` and `Owner` tags.
6. **Review + create**. Azure validates and shows the **estimated monthly cost** before you click Create.

### The Pricing Model

Azure bills a VM as the **sum of multiple line items**. You are not paying one flat rate; you are paying for each resource the VM consumes.

| Component | Billing basis | Notes |
|---|---|---|
| **Compute (the VM size)** | Per second the VM is in **Running** state. Rounded to per-minute for display. | The big one. Stops when the VM is **deallocated** (see below). |
| **OS and data disks** | Per GB allocated, per hour. Charged whether the VM is on or off. | Premium SSD costs more than Standard SSD, both more than Standard HDD. |
| **Public IP** | Per hour for Standard SKU. Static is more than Dynamic. | Charged whether the VM is running or not, while reserved. |
| **Outbound bandwidth** | Per GB egressed to the internet. The first 100 GB/month per region is typically free; pricing drops after high tiers. | Inbound traffic is free. Traffic between Azure regions is billed. |
| **Software / OS license** | Some images include the license (Windows Server, SQL Server) and bake it into the per-second compute rate. Linux images are typically free aside from compute. | Use Azure Hybrid Benefit if you have on-prem Windows licenses. |
| **Backup / Site Recovery / Defender** | Per VM, per month, if enabled. | Optional add-ons. |

Two critical implications:

1. **Stopping is not deallocating.** "Stop" inside the OS (`shutdown /s` on Windows, `sudo shutdown -h now` on Linux) puts the VM in a **Stopped** state — but Azure still considers the compute reserved and **keeps charging compute**. To actually stop compute charges, **deallocate** from Azure: Portal -> VM -> Stop button (which deallocates), or `az vm deallocate`. The state changes to **Stopped (deallocated)**.
2. **Storage and reserved IPs keep charging even when the VM is deallocated.** A deallocated VM still has its OS disk, data disks, and any reserved Public IP. Those line items continue. The savings come from the much larger compute line going to zero.

### Reading the Status

| Status | Compute charged? | Storage charged? | What it means |
|---|---|---|---|
| **Running** | Yes | Yes | Normal operation. |
| **Stopped** | **Yes** | Yes | OS-level shutdown only; Azure still has compute reserved. |
| **Stopped (deallocated)** | No | Yes | Fully released to the host pool; compute charges stop. Disks and IPs still bill. |
| **Deleted** | No | No | Resource gone; no charges (verify the disks were deleted with it — they sometimes outlive the VM). |

When you finish a session, **deallocate** if you will return to the VM, **delete** if you will not.

### Sizing and Cost Tradeoffs

VM sizes are organized in families with different CPU/RAM ratios:

- **B-series** — burstable. Cheap base price, accumulate "credits" while idle, burn them when busy. Right for dev, low-traffic web servers.
- **D-series / Dsv5 / Dasv5** — general purpose. Balanced CPU and RAM. Most production workloads start here.
- **E-series** — memory-optimized. More RAM per vCPU. Right for in-memory databases, caches.
- **F-series** — compute-optimized. More CPU per RAM. Right for batch processing, CPU-bound APIs.
- **L-series** — storage-optimized. Local NVMe. Right for big data engines.
- **N-series** — GPU. ML / rendering / HPC.

Within a family, the number indicates size (`D2`, `D4`, `D8` — 2, 4, 8 vCPU). The trailing letters indicate features (`s` for premium storage, `v5` for the generation, `a` for AMD).

For learning: **B1s** or **B2s** is plenty. Don't pay for D-series unless you actually need the perf.

### The Pricing Calculator

`https://azure.microsoft.com/pricing/calculator` is a Microsoft-hosted estimator. Drop in services you plan to use, configure tier/region/hours, and it produces a monthly estimate.

How to use it well:

1. Add each service you plan to deploy (VM, Storage, App Service, SQL DB).
2. For VMs: choose the same region and size you will provision; choose the OS and disk type; enter expected hours per month (730 = always-on; 160 = 8 hours/day on weekdays).
3. For Storage: estimate GB stored, transactions, egress.
4. **Save the estimate as a URL** and email/link it before provisioning. Architecture decisions made against a saved estimate are reproducible.
5. **Compare regions.** Same VM size in `eastus` vs `westeurope` can differ by 5-15%.

Pair the calculator with **Azure Cost Management** (covered separately) — calculator estimates *future* spend, Cost Management shows *actual* spend so you can correct estimates.

## Code Examples

### Azure CLI: Create a small Linux VM

```bash
RG=rg-trainee-vm-demo
LOCATION=eastus
VM_NAME=vm-trainee-01

az group create -n $RG -l $LOCATION

az vm create \
  --resource-group $RG \
  --name $VM_NAME \
  --image Ubuntu2204 \
  --size Standard_B2s \
  --admin-username azureuser \
  --generate-ssh-keys \
  --public-ip-sku Standard \
  --tags Environment=Dev Owner=$USER

# Connect (SSH).
az vm show -d -g $RG -n $VM_NAME --query publicIps -o tsv
ssh azureuser@<that-public-ip>
```

### Stop, deallocate, start, delete

```bash
# Deallocate (compute charges stop)
az vm deallocate -g $RG -n $VM_NAME

# Confirm state
az vm get-instance-view -g $RG -n $VM_NAME \
  --query "instanceView.statuses[?starts_with(code, 'PowerState/')].displayStatus" -o tsv
# Expected: "VM deallocated"

# Restart
az vm start -g $RG -n $VM_NAME

# Delete the VM (does NOT automatically delete the OS disk, NIC, or Public IP)
az vm delete -g $RG -n $VM_NAME --yes

# Tear down the whole RG when done with the experiment
az group delete -n $RG --yes
```

### Quick "what is this costing?" check

```bash
# Cost of the RG month-to-date (requires the Cost Management extension).
az consumption usage list --start-date 2026-05-01 --end-date 2026-05-31 \
  --query "[?contains(instanceName, 'rg-trainee-vm-demo')].{Name:instanceName, Cost:pretaxCost}" \
  -o table
```

## Summary

- A VM is IaaS compute; you manage from the OS up.
- Bill is the sum of compute + disks + public IP + bandwidth + optional add-ons. Storage and reserved IPs keep billing while the VM is deallocated.
- **Stopped** is not the same as **Stopped (deallocated)** — only deallocation halts compute charges. Use `az vm deallocate` or the Portal's Stop button.
- For learning, B-series sizes (`B1s`, `B2s`) are cheap and adequate.
- Use the Pricing Calculator before provisioning; compare regions; save the estimate URL.

## Additional Resources

- [Azure Virtual Machines pricing](https://azure.microsoft.com/pricing/details/virtual-machines/)
- [States and billing of Azure VMs](https://learn.microsoft.com/en-us/azure/virtual-machines/states-billing)
- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)
