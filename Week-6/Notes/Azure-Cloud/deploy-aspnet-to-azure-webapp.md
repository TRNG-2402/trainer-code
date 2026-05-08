# Deploying ASP.NET Core to Azure App Service via GitHub Actions

## Learning Objectives
- Provision a Linux Azure Web App that runs an ASP.NET Core 8 API.
- Author a GitHub Actions workflow that builds, publishes, and deploys the API on every push to main.
- Use a publish profile stored as a GitHub Actions secret to authenticate the deploy.
- Verify the deployment and read the workflow logs to debug failures.

## Why This Matters

This is the integration the Week 4 epic builds toward: the React + ASP.NET Core app you have been writing all term, deployed to a managed Azure host through an automated pipeline. The QC-4 Must-Know "deploy an ASP.NET API to an Azure Web App via GitHub Actions deployment" objective is the workflow on this page.

## The Concept

### App Service for ASP.NET Core

Azure App Service is the PaaS host for web apps and APIs. For ASP.NET Core 8 you provision:

- **App Service Plan** — the underlying compute (Linux or Windows; SKU determines CPU/RAM and features). For the API on Linux, `B1` (Basic) is the cheapest tier that supports custom domains and HTTPS.
- **Web App** — the application running on that plan. Choose the runtime stack (e.g., `DOTNETCORE:8.0`).

You ship a published `.zip` (or container image, or Git push) to the Web App, and App Service runs it. App Service handles HTTPS, custom domains, scaling, deployment slots, and patching.

### Authentication for the Deploy

A GitHub Actions workflow needs credentials to push code to App Service. Two options:

| Option | Security | Setup | Best when |
|---|---|---|---|
| **Publish profile** | App-scoped secret. Single-app credential. Easy to rotate. | Download from Web App Overview -> Get publish profile. Store as a GitHub secret. | Single Web App, fast setup, training scenarios. |
| **Service principal / OIDC** | Federated identity, no long-lived secret. Stronger. | Create an Entra app registration; configure federated credential bound to the GitHub repo. | Production, multi-app pipelines, security-conscious orgs. |

For the QC-4 Should-Know objective and this curriculum, use the **publish profile** approach. It is the most common path on Microsoft Learn and the simplest to wire up.

### The Workflow Shape

A complete deploy workflow does five things:

1. **Checkout** the repo at the triggering commit.
2. **Set up the .NET SDK** to build with.
3. **Restore + build + test + publish** the project, producing a self-contained folder.
4. **Upload as an artifact** so deploy is a separate, traceable step.
5. **Deploy** the artifact to the Web App using `azure/webapps-deploy@v3` and the publish profile.

Splitting build and deploy into two jobs (with `needs:`) gives you traceability — if the deploy fails, the build artifact is still there and you can re-deploy without rebuilding.

## Code Examples

### 1. Provision the Web App (one-time, Azure CLI)

```bash
RG=rg-productcatalog-prod
LOC=eastus
PLAN=plan-productcatalog
APP=productcatalog-api-eastus

az group create -n $RG -l $LOC

# Linux Basic plan.
az appservice plan create -g $RG -n $PLAN -l $LOC \
  --is-linux --sku B1

# ASP.NET Core 8 Web App.
az webapp create -g $RG -p $PLAN -n $APP \
  --runtime "DOTNETCORE:8.0"

# Confirm reachable.
az webapp show -g $RG -n $APP --query defaultHostName -o tsv
# Expected: productcatalog-api-eastus.azurewebsites.net
```

### 2. Get the publish profile and store it as a secret

```bash
# Print the publish profile XML to stdout.
az webapp deployment list-publishing-profiles \
  -g $RG -n $APP --xml
```

Copy the entire XML. In the GitHub repo: **Settings -> Secrets and variables -> Actions -> New repository secret**. Name: `AZUREAPPSERVICE_PUBLISHPROFILE`. Value: paste.

(In the Portal: Web App -> Overview -> "Get publish profile" downloads the same XML as a `.PublishSettings` file.)

### 3. The Workflow

`.github/workflows/deploy-api.yml`:

```yaml
name: Build and deploy API

on:
  push:
    branches: [main]
    paths:
      - 'src/Api/**'
      - '.github/workflows/deploy-api.yml'
  workflow_dispatch:

permissions:
  contents: read

env:
  DOTNET_VERSION: '8.0.x'
  PROJECT_PATH: 'src/Api/Api.csproj'
  PUBLISH_DIR: 'publish'
  WEBAPP_NAME: 'productcatalog-api-eastus'

jobs:
  build:
    name: Build and publish
    runs-on: ubuntu-latest

    steps:
      - name: Check out source
        uses: actions/checkout@v4

      - name: Set up .NET ${{ env.DOTNET_VERSION }}
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore ${{ env.PROJECT_PATH }}

      - name: Build
        run: dotnet build ${{ env.PROJECT_PATH }} --configuration Release --no-restore

      - name: Test
        run: dotnet test ${{ env.PROJECT_PATH }} --configuration Release --no-build --verbosity normal

      - name: Publish
        run: >
          dotnet publish ${{ env.PROJECT_PATH }}
          --configuration Release
          --no-build
          --output ${{ env.PUBLISH_DIR }}

      - name: Upload publish artifact
        uses: actions/upload-artifact@v4
        with:
          name: api-publish
          path: ${{ env.PUBLISH_DIR }}
          retention-days: 7

  deploy:
    name: Deploy to Azure Web App
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: production
      url: https://${{ env.WEBAPP_NAME }}.azurewebsites.net

    steps:
      - name: Download publish artifact
        uses: actions/download-artifact@v4
        with:
          name: api-publish
          path: ${{ env.PUBLISH_DIR }}

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ env.WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZUREAPPSERVICE_PUBLISHPROFILE }}
          package: ${{ env.PUBLISH_DIR }}
```

What this does:

- Triggers on push to `main` when the API code or this workflow file changes (also runnable on demand via `workflow_dispatch`).
- Locks `GITHUB_TOKEN` permissions to read-only.
- One job restores/builds/tests/publishes and uploads the result as an artifact.
- A second job downloads the artifact and deploys it. The `environment:` block lets you require manual approval before the deploy job runs (configure in Settings -> Environments).

### 4. Verify

After the workflow runs green:

```bash
curl -i https://productcatalog-api-eastus.azurewebsites.net/health
# Expected: HTTP/1.1 200 OK
```

Or open `https://<your-app>.azurewebsites.net/swagger` if your API has Swagger enabled.

### 5. Debug a Failure

Standard loop:

1. **GitHub Actions tab** -> open the failed run -> click the failed job -> expand the failed step -> scan the log bottom-up for the first `error`.
2. **`webapps-deploy` step fails with 401/403** — bad publish profile. Re-download from Azure (publish profiles rotate) and replace the secret value.
3. **App returns 500 after deploy** — App Service logs are the next stop. Either:
   - Web App -> Log stream blade in the Portal (live tail), or
   - `az webapp log tail -g $RG -n $APP` from the CLI.
   The startup exception is almost always at the top of the live stream.
4. **Diagnostic settings already routing to Log Analytics?** Run a KQL query against `AppServiceConsoleLogs` for the deployment time window — see the Azure Monitor topic.

## Summary

- App Service hosts ASP.NET Core 8 on a Linux plan with one CLI command to provision.
- Workflow shape: `checkout` -> `setup-dotnet` -> `restore` / `build` / `test` / `publish` -> `upload-artifact`. Separate `deploy` job uses `download-artifact` + `azure/webapps-deploy@v3`.
- Authenticate the deploy with a publish profile downloaded from the Web App and stored as the `AZUREAPPSERVICE_PUBLISHPROFILE` secret.
- Lock `permissions:` to least-privilege; gate prod deploy with a GitHub Environment that requires approval.
- Debug via the workflow logs first, then App Service log stream / Log Analytics.

## Additional Resources

- [Quickstart: Deploy an ASP.NET Core app to App Service from GitHub Actions](https://learn.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore?pivots=development-environment-vscode#deploy-the-sample-application)
- [`azure/webapps-deploy` action on GitHub Marketplace](https://github.com/marketplace/actions/azure-webapp)
- [Configure GitHub Actions deployment to App Service](https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions)
