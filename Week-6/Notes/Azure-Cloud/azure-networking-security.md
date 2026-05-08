# Networking Security: NSGs, Service Endpoints, Private Link

## Learning Objectives
- Apply Network Security Groups (NSGs) to control traffic at the subnet or NIC level.
- Distinguish service endpoints from Private Link and pick the right one.
- Configure resource-level firewalls (Azure SQL, Storage) to restrict who can reach the data plane.
- Enable secure inter-resource communication (e.g., App Service -> SQL DB) without exposing endpoints to the public internet.

## Why This Matters

A defaulted-public PaaS resource is the most common Azure security gap in trainee projects. Anyone with the connection string can hit it — and connection strings leak. The pattern that fixes this is the same every time: lock the data plane to your VNet, give your app a managed identity, and authenticate over the Azure backbone. The QC-4 "security features and how to enable communication between resources and IPs" objective lives entirely in this topic.

## The Concept

### Network Security Groups (NSGs)

An NSG is a stateful, distributed firewall attached to a **subnet** or a **NIC** (or both — rules at both layers compose).

- **Stateful** — return traffic for an allowed flow is automatically permitted.
- **Rules** are ordered by priority (100-4096, lower = evaluated first). First match wins.
- Rules have: priority, direction (Inbound/Outbound), source (CIDR or service tag), source ports, destination, destination ports, protocol, action (Allow/Deny).
- **Service tags** (`Internet`, `VirtualNetwork`, `AzureLoadBalancer`, `Storage`, `Sql`, `AzureCloud.eastus`) — symbolic names for groups of Azure IPs. Use these instead of hard-coding IPs.

Azure ships **default rules** that allow VNet-internal traffic and Load Balancer health probes, then deny inbound from the internet by default. You add rules to permit what you actually want.

A typical web-tier inbound NSG:

| Priority | Name | Direction | Source | Dest port | Action |
|---|---|---|---|---|---|
| 100 | AllowHttps | Inbound | Internet | 443 | Allow |
| 200 | AllowSshFromBastion | Inbound | AzureBastionSubnet | 22 | Allow |
| 4096 | DenyAll | Inbound | * | * | Deny (default) |

**Apply NSGs to subnets, not NICs**, when possible — fewer places to track, less drift.

### Service Endpoints

A **service endpoint** extends your VNet identity to a PaaS service over the Azure backbone. The PaaS resource still has a public DNS name, but its firewall can now allow traffic *from your VNet* without you adding the public IP of every VM that might call it.

Properties:

- The service still has a public endpoint; service endpoints affect only the firewall logic on the service side.
- Traffic stays on the Azure backbone (does not traverse the public internet) but the service is still reachable from outside your VNet if its firewall allows it.
- Cheap and simple — toggle on the subnet, allow the VNet on the service's firewall.

Configure on a subnet for `Microsoft.Storage`, `Microsoft.Sql`, `Microsoft.KeyVault`, etc.

### Private Link / Private Endpoints

A **Private Endpoint** is the strongest isolation: Azure injects a NIC into your VNet that holds a **private IP** for the PaaS resource. The PaaS resource is reachable only via that private IP. The public endpoint is typically disabled.

Properties:

- The PaaS resource has a private IP inside your VNet — like it has joined your network.
- DNS for the resource resolves to that private IP via a Private DNS Zone (Azure can auto-link this).
- You can disable the public endpoint entirely. Outside your VNet, the resource simply does not exist on the network.
- Costs more than a service endpoint (per-endpoint hourly charge + bandwidth) but provides true isolation.

Use Private Link for **production** workloads where the service should never accept public traffic. Service endpoints are fine for dev / less sensitive scenarios.

### Comparing the Options

| | Service Endpoint | Private Endpoint |
|---|---|---|
| PaaS still has public IP? | Yes | Optional (best practice: no) |
| Identity used in service firewall | VNet/subnet | Private endpoint resource |
| DNS resolves to | Public name -> public IP | Public name -> private IP (via Private DNS Zone) |
| Cost | Free | Per endpoint + bandwidth |
| Isolation | Backbone-only routing; service still public | Service is private to your VNet |
| Right when | Internal apps in dev/non-sensitive | Production / regulated / strong isolation |

### Resource-Level Firewalls

Many PaaS services have a **firewall built into the resource itself** that runs *before* RBAC. These take effect even before authentication.

- **Azure SQL Server firewall** — rules under the SQL Server resource. Server-level rules apply to all databases on the server; database-level rules apply to one DB. Allow specific IPs, IP ranges, "Azure services and resources to access this server," or VNet rules.
- **Storage Account firewall** — Networking blade -> "Selected networks." Allow specific VNets/subnets (service endpoints) or specific public IP ranges; pair with Private Endpoints for full lockdown.
- **App Service Access Restrictions** — IP allow/deny rules on the inbound HTTP traffic to the Web App. Useful when the app should only accept traffic from a specific gateway or office.
- **Key Vault firewall** — same model: allow VNets, allow specific public IPs, integrate with managed identities for the data plane.

The lockdown order is consistent: **disable public access by default, allow your VNet via service endpoint or Private Endpoint, allow specific IPs only when needed (e.g., your CI runner or admin laptop).**

### App-to-DB: The Canonical Pattern

The QC-4 question "enable communication between an Azure App Service and an Azure SQL DB securely" has one clean answer:

1. Put the App Service in a VNet (Regional VNet integration on the App Service plan).
2. Place the SQL DB behind a Private Endpoint in the same VNet (or a peered one).
3. Disable the SQL Server's public network access entirely.
4. The App Service uses **managed identity** to authenticate to SQL (no password in `appsettings.json`).
5. SQL DB grants the app's managed identity an Entra-integrated SQL user with the necessary permissions.

Result: the connection string contains `Authentication=Active Directory Default` and a server name that resolves to a private IP. There is no password, no public endpoint, no internet-exposed surface. Same pattern for Storage, Key Vault, Cosmos DB.

## Code Examples

### NSG: allow HTTPS, deny everything else inbound

```bash
RG=rg-productcatalog-dev
LOC=eastus

az network nsg create -g $RG -n nsg-web -l $LOC

# Allow HTTPS from anywhere.
az network nsg rule create -g $RG --nsg-name nsg-web -n AllowHttps \
  --priority 100 --direction Inbound --access Allow \
  --protocol Tcp --destination-port-ranges 443

# Allow SSH only from the corporate range.
az network nsg rule create -g $RG --nsg-name nsg-web -n AllowSshCorp \
  --priority 200 --direction Inbound --access Allow \
  --protocol Tcp --destination-port-ranges 22 \
  --source-address-prefixes 203.0.113.0/24

# Attach NSG to the web subnet.
az network vnet subnet update -g $RG \
  --vnet-name vnet-app --name web --network-security-group nsg-web
```

### Service endpoint: lock Storage to a VNet

```bash
RG=rg-productcatalog-dev
ACCOUNT=storaineelab1234

# 1. Add Microsoft.Storage to the subnet's service endpoints.
az network vnet subnet update -g $RG \
  --vnet-name vnet-app --name app \
  --service-endpoints Microsoft.Storage

# 2. Default-deny on the Storage Account, then allow that subnet.
az storage account update -g $RG -n $ACCOUNT --default-action Deny

VNET_ID=$(az network vnet show -g $RG -n vnet-app --query id -o tsv)
az storage account network-rule add \
  -g $RG --account-name $ACCOUNT \
  --vnet-name vnet-app --subnet app
```

### Private Endpoint: SQL DB private to a VNet

```bash
SQL_SERVER=sql-trainee-prod
DB=appdb

# 1. Create the SQL Server (public endpoint allowed for now).
az sql server create -g $RG -n $SQL_SERVER -l eastus \
  --admin-user sqladmin --admin-password "<strong-password-or-Entra>"

# 2. Create the database.
az sql db create -g $RG -s $SQL_SERVER -n $DB --service-objective S0

# 3. Provision a private endpoint for the SQL Server in our app subnet.
SQL_ID=$(az sql server show -g $RG -n $SQL_SERVER --query id -o tsv)
az network private-endpoint create -g $RG \
  --name pe-sql-trainee \
  --vnet-name vnet-app --subnet app \
  --private-connection-resource-id $SQL_ID \
  --group-id sqlServer \
  --connection-name sql-trainee-conn

# 4. Hook a Private DNS Zone so the public name resolves to the private IP.
az network private-dns zone create -g $RG -n privatelink.database.windows.net
az network private-dns link vnet create -g $RG \
  --zone-name privatelink.database.windows.net \
  --name vnet-app-link --virtual-network vnet-app --registration-enabled false

# 5. Disable public network access on the SQL Server.
az sql server update -g $RG -n $SQL_SERVER --enable-public-network false
```

After step 5, the SQL Server is reachable **only** from inside the VNet — no public endpoint, no firewall holes.

## Summary

- NSGs are stateful firewalls on subnets/NICs; rules ordered by priority; default deny inbound from the internet.
- Service endpoints extend VNet identity to a PaaS service over the Azure backbone, but the service stays public.
- Private Endpoints inject a NIC with a private IP into your VNet — the service becomes reachable only via that IP. Use for production / strong isolation.
- Resource-level firewalls (SQL, Storage, Key Vault) live on the resource itself; default to deny and allow your VNet plus specific IPs explicitly.
- Canonical secure pattern: App Service with VNet integration -> Private Endpoint to the data resource -> managed identity for auth, no passwords in config.

## Additional Resources

- [Network Security Groups overview](https://learn.microsoft.com/en-us/azure/virtual-network/network-security-groups-overview)
- [Service endpoints for Azure VNets](https://learn.microsoft.com/en-us/azure/virtual-network/virtual-network-service-endpoints-overview)
- [Azure Private Link / Private Endpoints](https://learn.microsoft.com/en-us/azure/private-link/private-endpoint-overview)
