# Infrastructure as Code: ARM Templates and Bicep

## Learning Objectives
- Define Infrastructure as Code (IaC) and the properties that make it valuable.
- Identify ARM templates as the underlying JSON deployment format on Azure.
- Author a Bicep file and explain how it transpiles to ARM.
- Deploy a Bicep file to a resource group via the Azure CLI.

## Why This Matters

Clicking through the Portal works once. Doing it twice — for staging, prod, the trainee next month — is a recipe for drift. IaC turns "I built it" into "here is the file that builds it." Bicep is the modern Azure-native way to write IaC: shorter than ARM JSON, idempotent, and reviewable as code in a PR.

## The Concept

### What IaC Is

Infrastructure as Code: describe your cloud resources in version-controlled text files, deploy by running a tool that reconciles the file with the live state. Three properties make it valuable:

1. **Declarative** — you describe the *desired* state, not the steps to reach it. The tool figures out create/update/delete.
2. **Idempotent** — re-running the deployment produces the same result. No "did I run this twice?" anxiety.
3. **Reviewable** — diffs land in PRs; reviewers see "this PR adds a Storage Account with no firewall" before it ships.

The same files run in CI/CD, so dev/staging/prod use the same template with different parameters — no environment drift.

### ARM Templates

**ARM templates** are the underlying JSON format that Azure Resource Manager accepts directly. You write a JSON document describing the resources you want and submit it to ARM, which deploys it.

The JSON is verbose and finicky. For example, a Storage Account in ARM JSON:

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "storageAccountName": { "type": "string" },
    "location": { "type": "string", "defaultValue": "[resourceGroup().location]" }
  },
  "resources": [
    {
      "type": "Microsoft.Storage/storageAccounts",
      "apiVersion": "2023-01-01",
      "name": "[parameters('storageAccountName')]",
      "location": "[parameters('location')]",
      "sku": { "name": "Standard_LRS" },
      "kind": "StorageV2"
    }
  ]
}
```

ARM templates work everywhere — every Azure tool ultimately calls ARM with one of these. But authoring them by hand is painful, which is why Bicep exists.

### Bicep

**Bicep** is a domain-specific language that compiles 1:1 to ARM JSON. Same engine, same semantics, far less syntax. Microsoft maintains it and the Bicep team treats ARM as the compilation target — anything you can do in ARM you can do in Bicep, usually in a third the lines.

The same Storage Account in Bicep:

```bicep
param storageAccountName string
param location string = resourceGroup().location

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}
```

Compare line counts. Bicep is the right authoring layer; ARM JSON is the right understanding (because that is what ARM actually deploys).

### How Bicep Files Are Structured

Five sections you will use:

| Section | What it does |
|---|---|
| `targetScope` | Scope of the deployment: `resourceGroup` (default), `subscription`, `managementGroup`, `tenant`. |
| `param` | Inputs (`string`, `int`, `bool`, `object`, `array`). Optional defaults. |
| `var` | Computed values reused inside the file. |
| `resource` | Azure resources to deploy. |
| `output` | Values to return to the caller (e.g., the deployed resource's ID, an endpoint URL). |

Modules let you compose: a Bicep file can `module net './modules/network.bicep'` to call another Bicep file with parameters, just like a function.

### Deploying Bicep

Two flavors of deployment:

- **Resource group deployment** — most common. Creates resources inside an RG.
- **Subscription / management group / tenant deployment** — for resources that live above the RG layer (RGs themselves, policies, role assignments at sub scope).

The CLI commands:

```bash
# Validate before deploying.
az deployment group validate \
  --resource-group rg-productcatalog-dev \
  --template-file main.bicep \
  --parameters storageAccountName=storaineelab1234

# Preview what will change (the "what-if" engine).
az deployment group what-if \
  --resource-group rg-productcatalog-dev \
  --template-file main.bicep \
  --parameters storageAccountName=storaineelab1234

# Deploy.
az deployment group create \
  --resource-group rg-productcatalog-dev \
  --template-file main.bicep \
  --parameters storageAccountName=storaineelab1234
```

The CLI auto-transpiles Bicep to ARM JSON before submitting; you can also `bicep build main.bicep` to produce `main.json` explicitly.

`what-if` is the killer feature — a dry run that prints the planned changes (create / modify / delete / no change) before you commit. Always run `what-if` against production.

### Idempotency in Practice

Because ARM is declarative, running the same Bicep twice does not double-create resources. Adjust a parameter and re-deploy: ARM updates only the fields that changed; resources not in the template are left alone (unless you use **complete mode**, which deletes anything in the RG that the template does not include — handle with care).

This means your IaC file is the source of truth. To remove a resource, remove it from the file and redeploy in complete mode, or delete it explicitly.

### IaC in CI/CD

Wire deployment into a pipeline:

- Pull request opens -> `az deployment group what-if` runs and posts the diff as a PR comment.
- PR merges -> `az deployment group create` runs against staging.
- Manual approval -> the same template runs against prod with prod parameters.

Same template, environment-specific parameter files, code-reviewed changes.

## Code Examples

### A complete small Bicep deployment

`main.bicep` — a Storage Account + Log Analytics Workspace in one RG:

```bicep
@description('Globally unique name for the Storage Account.')
@minLength(3)
@maxLength(24)
param storageAccountName string

@description('Region for resources.')
param location string = resourceGroup().location

@description('Tags applied to every resource.')
param tags object = {
  Environment: 'Dev'
  Project: 'ProductCatalog'
}

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'law-${storageAccountName}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

output storageBlobEndpoint string = storage.properties.primaryEndpoints.blob
output workspaceId string = workspace.id
```

### Deploy via CLI

```bash
RG=rg-productcatalog-dev
ACCOUNT=storaineelab$(($RANDOM))

az group create -n $RG -l eastus

# Dry run.
az deployment group what-if \
  -g $RG -f main.bicep \
  --parameters storageAccountName=$ACCOUNT

# Real deploy.
az deployment group create \
  -g $RG -f main.bicep \
  --parameters storageAccountName=$ACCOUNT \
  --query "properties.outputs"
```

### Parameter file for environment separation

`main.dev.bicepparam`:

```bicep
using 'main.bicep'

param storageAccountName = 'storaineedev1234'
param tags = {
  Environment: 'Dev'
  Project: 'ProductCatalog'
}
```

Then:

```bash
az deployment group create -g $RG -f main.bicep -p main.dev.bicepparam
```

Same template, different env: just change the parameter file.

## Summary

- IaC = describe desired state in code; tool reconciles with live state. Declarative + idempotent + reviewable.
- ARM JSON is the underlying format ARM consumes; Bicep is the modern authoring DSL that compiles 1:1 to ARM.
- Bicep file shape: `targetScope`, `param`, `var`, `resource`, `output`. Modules compose multiple files.
- Deploy with `az deployment group create`; preview with `az deployment group what-if` (always do this against production).
- Pair templates with parameter files for environment separation; wire deployment into CI/CD for automated rollout.

## Additional Resources

- [What is Bicep?](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview)
- [Bicep file syntax](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/file)
- [Deploy with Azure CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/deploy-cli)
