# Azure Blob Storage

## Learning Objectives
- Provision a Storage Account and a Blob container.
- Distinguish the three blob types (block, append, page) and pick the right one.
- Upload and access blobs from the Portal, the CLI, the SDK, and Storage Explorer.
- Host a static website on Blob Storage.

## Why This Matters

Blob Storage is Azure's most general-purpose storage and the cheapest place to put data. Backups, build artifacts, user-uploaded files, the static React build the rest of this curriculum produces — all of it lands in Blob. Static-website hosting on Blob is also the lowest-cost way to serve a single-page app with a custom domain over HTTPS.

## The Concept

### The Storage Account

Everything in Azure Storage lives inside a **Storage Account**. The account is the top-level resource; inside it you can use any combination of the four offerings (Blob, File, Queue, Table). The Storage Account name is **globally unique** (3-24 lowercase letters/digits) because it forms the URL host:

```
https://<account-name>.blob.core.windows.net/<container>/<blob>
```

Pick a name early; Azure will tell you immediately if it is taken.

When you create one, you choose:

- **Subscription / RG / Region**.
- **Performance** — `Standard` (cheap, HDD-backed) or `Premium` (expensive, SSD, only for specific scenarios like Premium block blobs or Premium files).
- **Redundancy** — how many copies and where:

| SKU | Copies | Survives |
|---|---|---|
| **LRS** (Locally Redundant) | 3, in one DC | Disk / rack failure |
| **ZRS** (Zone Redundant) | 3, across 3 AZs in one region | Single-DC failure |
| **GRS** (Geo Redundant) | 6 — 3 in primary region, 3 in paired region | Regional disaster (manual failover) |
| **GZRS** (Geo-Zone Redundant) | 3 ZRS in primary + 3 LRS in paired | Combined zone + region resilience |
| **RA-GRS / RA-GZRS** | GRS/GZRS with **read access** to the secondary | Lets you read from the secondary while primary is healthy |

For experiments use LRS. For production, ZRS or GRS depending on whether the worry is "single-DC fire" or "whole-region outage."

- **Access tier (default)** — `Hot` (frequent reads), `Cool` (rare reads, lower storage cost, higher transaction cost), `Cold`/`Archive` (deep storage, hours to retrieve).
- **Hierarchical namespace** — turn this on if you want **Azure Data Lake Storage Gen2** semantics (real folders, ACLs). Don't enable for general-purpose blob unless you want ADLS.

### Containers and Blobs

Inside the Storage Account, the Blob service is structured as:

- **Containers** — flat namespaces, like S3 buckets. A blob is addressed by `container/blob-path`. Container names are lowercase letters/digits/dashes.
- **Blobs** — actual objects.

Three blob types — pick by **how the object is written**, not what it contains:

| Type | What it is | Use for |
|---|---|---|
| **Block blob** | The default. Up to ~190 TB, written as blocks that get committed. | Files, images, documents, backups, build artifacts, the React `dist/` folder. |
| **Append blob** | Optimized for sequential append-only writes. | Log files. Cheap to add to, expensive to modify. |
| **Page blob** | 512-byte page-aligned random access; up to 8 TB. | Backing storage for VM disks (VHDs). You will rarely create one yourself. |

In practice, 95% of what you write is a block blob.

### Access Control

Three layers, applied together:

1. **Networking** — public endpoint by default. Lock down to a VNet (service endpoints / Private Link) or to specific IP ranges in the Storage Account's **Networking** blade.
2. **Authentication** — choose one:
   - **Shared key** (account key) — full control. Treat like a root password.
   - **Shared Access Signatures (SAS)** — time-limited, scoped tokens you generate and hand out.
   - **Microsoft Entra ID** — RBAC roles like `Storage Blob Data Reader`, `Storage Blob Data Contributor`. Strongly preferred.
3. **Authorization** — RBAC at Subscription/RG/Account/Container level controls who can read/write/delete.

For app code, the modern pattern is: enable a **managed identity** on your App Service, grant it `Storage Blob Data Contributor` on the container, and let the SDK acquire tokens automatically. No keys in `appsettings.json`.

### Static Website Hosting

The Storage Account has a built-in **static website** feature that turns a special container into a public web host:

1. Create a Storage Account (Standard, GPv2 or BlockBlobStorage).
2. Open the **Static website** blade. Toggle Enabled.
3. Set **Index document name** to `index.html` and (for SPAs) **Error document path** to `index.html` — that bounces 404s back to the SPA so React Router handles routing client-side.
4. Azure creates a hidden container called `$web` and gives you a URL: `https://<account>.z<n>.web.core.windows.net/`.
5. Upload the React build's `dist/` (or `build/`) folder into `$web`.

You now have a public static site. Pair with **Azure Front Door** (or **Azure CDN**) for caching, custom domains, and HTTPS on a custom domain.

For the React app in this curriculum, this is the cheapest hosting option that still feels production-grade.

## Code Examples

### Provision and use Blob from the CLI

```bash
RG=rg-trainee-storage
LOC=eastus
ACCOUNT=storaineelab$(($RANDOM))   # globally unique
CONTAINER=uploads

az group create -n $RG -l $LOC

# Create a standard, locally-redundant, GPv2 account.
az storage account create \
  -g $RG -n $ACCOUNT -l $LOC \
  --sku Standard_LRS --kind StorageV2 --access-tier Hot

# Create a container (auth via Entra ID by default; falls back to key).
az storage container create -n $CONTAINER --account-name $ACCOUNT \
  --auth-mode login

# Upload a file.
az storage blob upload \
  --account-name $ACCOUNT -c $CONTAINER \
  -n images/logo.png \
  -f ./logo.png \
  --auth-mode login

# List blobs.
az storage blob list --account-name $ACCOUNT -c $CONTAINER \
  --auth-mode login -o table

# Generate a 1-hour read-only SAS URL for sharing.
EXPIRY=$(date -u -d '1 hour' '+%Y-%m-%dT%H:%MZ')
az storage blob generate-sas \
  --account-name $ACCOUNT -c $CONTAINER -n images/logo.png \
  --permissions r --expiry $EXPIRY --auth-mode login --as-user --full-uri
```

### Static website upload

```bash
# Enable static website on the account.
az storage blob service-properties update \
  --account-name $ACCOUNT \
  --static-website --index-document index.html --404-document index.html

# Build the React app first (npm run build), then upload to $web.
az storage blob upload-batch \
  --account-name $ACCOUNT \
  -d '$web' -s ./dist \
  --auth-mode login

# Get the static-website URL.
az storage account show -g $RG -n $ACCOUNT \
  --query "primaryEndpoints.web" -o tsv
```

### .NET SDK (App Service with managed identity)

```csharp
using Azure.Identity;
using Azure.Storage.Blobs;

var credential = new DefaultAzureCredential();
var serviceClient = new BlobServiceClient(
    new Uri("https://storaineelab1234.blob.core.windows.net"),
    credential);

var container = serviceClient.GetBlobContainerClient("uploads");
await container.CreateIfNotExistsAsync();

var blob = container.GetBlobClient("hello.txt");
await blob.UploadAsync(BinaryData.FromString("hello, blob"), overwrite: true);
```

`DefaultAzureCredential` picks up the App Service's managed identity in production and the developer's `az login` token locally. No connection string in config.

## Summary

- A Storage Account is the top-level container; Blob is one of its four services (Blob / File / Queue / Table).
- Pick redundancy (LRS / ZRS / GRS / GZRS) by what failure mode you need to survive.
- Block blobs are the default and what you almost always want.
- Prefer Entra ID + managed identity over shared keys; use SAS for time-limited share links.
- Static website hosting on Blob is the cheapest sane way to serve a React SPA — with `index.html` as both the default and 404 doc.

## Additional Resources

- [Introduction to Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction)
- [Storage redundancy options](https://learn.microsoft.com/en-us/azure/storage/common/storage-redundancy)
- [Host a static website in Azure Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website-how-to)
