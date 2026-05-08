# Microsoft Entra ID and Azure RBAC

## Learning Objectives
- Identify Microsoft Entra ID (formerly Azure AD) as Azure's identity provider and what it manages.
- Explain Role-Based Access Control (RBAC) and the four levels at which it can be assigned.
- Pick the right built-in role for common scenarios.
- Use a managed identity to give an Azure resource access to other Azure resources without storing secrets.

## Why This Matters

Every API call into Azure has two questions: "who is calling?" (identity) and "are they allowed?" (authorization). Entra ID answers the first; RBAC answers the second. Together they replace credentials-in-config — the most common security mistake in cloud apps. Get this right and your App Service can read from a Storage Account or a Key Vault without ever holding a secret.

## The Concept

### Microsoft Entra ID (formerly Azure AD)

**Entra ID** is Microsoft's cloud identity provider. One Entra tenant holds your users, groups, and app registrations and issues tokens that other Azure (and Microsoft 365, GitHub, third-party SaaS) services trust.

What lives in Entra ID:

- **Users** — humans (developers, ops, end users) with sign-in credentials, MFA, and conditional access policies.
- **Groups** — collections of users; assign permissions to a group and add/remove members instead of managing individual permissions.
- **App Registrations** — represents an application. Has a client ID and (for confidential clients) a client secret or certificate. Apps authenticate to Entra to get tokens for downstream APIs.
- **Service Principals** — the runtime identity of an app registration in your tenant. RBAC is assigned to service principals.
- **Managed Identities** — service principals that Azure manages for you, attached to specific Azure resources (more below).
- **Conditional Access** policies — rules like "block sign-in from outside the corporate network unless MFA is satisfied."

For developers, the practical surface is: sign in via `az login` (acts as your user), or your app authenticates via a service principal / managed identity to call Azure APIs.

### What RBAC Is

**RBAC** = Role-Based Access Control. The model is `who + what role + at what scope`:

```
<security principal>  +  <role definition>  +  <scope>  =  role assignment
```

- **Security principal** — a user, group, service principal, or managed identity.
- **Role definition** — a named set of permissions (e.g., `Reader`, `Contributor`, `Owner`, `Storage Blob Data Reader`).
- **Scope** — where the assignment applies. Inherited downward.

Permissions union across all assignments — if you have `Reader` at the subscription and `Contributor` on one RG, you have Contributor on that RG and Reader everywhere else in the subscription.

### The Four Scopes

RBAC assignments can attach at any of these levels, and they cascade down:

```
Management Group   <-- broadest
    Subscription
        Resource Group
            Resource     <-- most precise
```

Pick the **narrowest scope** that does the job. Granting `Owner` at the subscription "just to make it work" is the most common security antipattern.

### Built-in Roles You Will Actually Use

Azure ships hundreds of built-in roles. You will use a small subset day to day.

**General-purpose (broad scopes):**

| Role | What it grants |
|---|---|
| **Reader** | List and view metadata of any resource. No reads of data. |
| **Contributor** | Manage all resources (create/update/delete). Cannot grant access to others. |
| **Owner** | Contributor + grant access (manage role assignments). |
| **User Access Administrator** | Manage role assignments only. |

**Data-plane roles (specific to a service):**

| Role | What it grants |
|---|---|
| **Storage Blob Data Reader** | Read blob data. Note: the general `Reader` role above does **not** grant blob data access. |
| **Storage Blob Data Contributor** | Read/write/delete blobs. |
| **Key Vault Secrets User** | Read secrets from Key Vault. |
| **SQL DB Contributor** | Manage SQL databases (control plane); does not grant query access. SQL data access is via SQL logins or Entra-integrated SQL auth. |
| **Azure Service Bus Data Sender / Receiver** | Send / receive messages. |

The pattern: **control-plane** roles (manage the resource) are separate from **data-plane** roles (read/write the data inside the resource). `Contributor` on a Storage Account lets you create containers and rotate keys but does **not** by itself let you read blobs over the data plane — for that you need the data-plane role (or the account key, which is the antipattern).

### Managed Identities

A **managed identity** is a service principal Azure provisions, rotates, and attaches to a specific Azure resource (App Service, Function, VM, AKS pod, etc.). The resource can request a token from the Azure Instance Metadata endpoint at runtime; Azure issues the token without you ever handling a credential.

Two flavors:

- **System-assigned** — created and tied to one resource's lifecycle. Deleted with the resource.
- **User-assigned** — a standalone Entra resource you create once and attach to many resources. Right when several resources need the same identity.

The goal: **no secrets in app config**. Connection strings to Storage / SQL / Key Vault use the managed identity instead of an account key or password.

Workflow:

1. Enable a managed identity on the resource (one toggle in Portal: App Service -> Identity -> System-assigned -> On).
2. Grant the identity an RBAC role at the right scope (e.g., `Storage Blob Data Contributor` on the Storage Account).
3. In code, use the `DefaultAzureCredential` from the Azure Identity SDK. It tries managed identity in cloud and `az login` locally.

### Custom Roles

If no built-in role matches, create a **custom role** with exactly the permissions you need. Define `Actions` (control plane), `NotActions`, `DataActions`, `NotDataActions`, and `AssignableScopes`. Reach for custom roles only when built-ins genuinely do not fit — they add maintenance.

## Code Examples

### Inspecting roles and assignments

```bash
# List built-in role names (truncated).
az role definition list --query "[?roleType=='BuiltInRole'].roleName" -o tsv | sort

# Show what a role can actually do.
az role definition list --name "Storage Blob Data Reader" --query "[0]"

# See current role assignments at a scope.
az role assignment list --resource-group rg-productcatalog-dev -o table

# See assignments for a specific user.
az role assignment list --assignee user@example.com --all -o table
```

### Assigning a role at the right scope

```bash
RG=rg-productcatalog-dev
ACCOUNT=storaineelab1234

# Grant a developer Reader on the whole RG.
az role assignment create \
  --assignee dev@example.com \
  --role "Reader" \
  --resource-group $RG

# Grant a service principal Storage Blob Data Contributor on a single Storage Account.
SP_ID=$(az ad sp show --id <app-client-id> --query id -o tsv)
SCOPE=$(az storage account show -g $RG -n $ACCOUNT --query id -o tsv)
az role assignment create \
  --assignee-object-id $SP_ID \
  --assignee-principal-type ServicePrincipal \
  --role "Storage Blob Data Contributor" \
  --scope $SCOPE
```

### App Service with system-assigned managed identity

```bash
RG=rg-productcatalog-dev
APP=trainee-api-eastus
ACCOUNT=storaineelab1234

# 1. Enable system-assigned identity on the Web App.
az webapp identity assign -g $RG -n $APP

# 2. Get the principal ID.
PRINCIPAL_ID=$(az webapp identity show -g $RG -n $APP --query principalId -o tsv)

# 3. Grant it data access on the Storage Account.
SCOPE=$(az storage account show -g $RG -n $ACCOUNT --query id -o tsv)
az role assignment create \
  --assignee-object-id $PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --role "Storage Blob Data Contributor" \
  --scope $SCOPE
```

In .NET, the app code stays simple:

```csharp
using Azure.Identity;
using Azure.Storage.Blobs;

var client = new BlobServiceClient(
    new Uri($"https://{accountName}.blob.core.windows.net"),
    new DefaultAzureCredential());
```

No keys, no secrets, no rotation.

## Summary

- Entra ID is Azure's identity provider — users, groups, app registrations, service principals, managed identities.
- RBAC = `principal + role + scope`. Permissions union across assignments and inherit downward.
- Four scopes (broadest to narrowest): Management Group, Subscription, Resource Group, Resource. Pick the narrowest that works.
- Control-plane roles (`Contributor`) and data-plane roles (`Storage Blob Data Reader`) are separate — do not assume one implies the other.
- Use managed identities + RBAC instead of connection strings or account keys; pair with `DefaultAzureCredential` in code.

## Additional Resources

- [What is Microsoft Entra ID?](https://learn.microsoft.com/en-us/entra/fundamentals/whatis)
- [Azure RBAC overview](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- [Managed identities for Azure resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
