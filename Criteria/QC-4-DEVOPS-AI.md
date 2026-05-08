# QC 4 - DEVOPS - AI

## Docker

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Describe the purpose and advantages of Docker. | Packages an application and its dependencies into a portable container image that runs identically across dev, test, and prod — eliminates "works on my machine" drift. |
| Must Know | Describe containerization vs virtualization. | VMs virtualize hardware and ship a full guest OS (heavy, minutes to boot). Containers virtualize the OS and share the host kernel (lightweight, seconds to start). |
| Must Know | Pull/push image from/to DockerHub | `docker pull nginx:latest`<br>`docker tag myapp user/myapp:1.0 && docker push user/myapp:1.0` |
| Must Know | Utilize Docker CLI to manage locally running Docker containers. | `docker ps`, `docker run -d -p 8080:80 nginx`, `docker stop <id>`, `docker logs <id>`, `docker exec -it <id> bash`. |
| Must Know | Describe the purpose and usage of Dockerfile. | A text recipe of instructions (`FROM`, `COPY`, `RUN`, `CMD`) that `docker build` uses to produce a reproducible image. |
| Must Know | Describe the purpose and usage of .dockerignore. | Lists files/directories excluded from the build context (e.g., `node_modules`, `bin/`, `.git`) — shrinks image size and speeds up builds. |
| Should Know | Utilize a docker container to build and output an artifact to the local environment. | `docker run --rm -v ${PWD}:/src -w /src node:20 npm run build` — runs the build inside a container and writes `dist/` back to the host via a volume. |
| Should Know | Utilize a Dockerfile to create a docker image from scratch, and push it to DockerHub | `docker build -t user/myapp:1.0 .`<br>`docker login && docker push user/myapp:1.0` |
| Should Know | Create a Dockerfile for a multi-stage build. | `FROM node:20 AS build`<br>`...`<br>`FROM nginx:alpine`<br>`COPY --from=build /app/dist /usr/share/nginx/html` — keeps build tools out of the final image. |
| Should Know | Connect a volume to docker container to give it access to local files outside of the container. | `docker run -v C:/data:/app/data myimage` (bind mount) or `docker volume create mydata && docker run -v mydata:/app/data myimage` (named volume). |
| Nice to have | Utilize docker plugins for enhanced container functionality. | Volume / network / log driver plugins extend the engine — e.g., a logging plugin that ships container stdout to a remote aggregator. |
| Nice to have | Use a remote cloud resource to run a dockerized application hosted on the cloud. | Push image to a registry (Docker Hub / ACR), then run on Azure Container Instances, Azure App Service for Containers, or AKS. |

## Github Actions CI/CD

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Use the "workflow_dispatch" keyword to manually trigger a workflow. | `on: workflow_dispatch:` — adds a "Run workflow" button in the Actions tab. Useful for manual deploys or ad-hoc jobs. |
| Must Know | Create a workflow by writing a YAML file in the .github/workflows directory on the repository that builds an application. | `.github/workflows/build.yml`:<br>`on: [push]`<br>`jobs:`<br>`  build:`<br>`    runs-on: ubuntu-latest`<br>`    steps:`<br>`      - uses: actions/checkout@v4`<br>`      - run: dotnet build` |
| Must Know | Describe the use of "secrets" to store credentials and other sensitive information | Repo → Settings → Secrets. Reference in YAML as `${{ secrets.AZURE_CREDENTIALS }}`. Values are masked in logs and never exposed to forked PRs by default. |
| Must Know | Explain the relationships between "triggers", "jobs" and "steps." | Trigger (event like `push`) starts a workflow → workflow runs one or more **jobs** (each on its own runner) → each job runs sequential **steps** (shell commands or actions). |
| Must Know | Read and understand workflow run logs to debug a failed workflow. | Open the failed run, expand the failing job, then the failing step. Logs show the exact command and stderr — same skill as reading a stack trace. |
| Must Know | Describe and discuss GitHub Actions and how it's used in development. | GitHub-native CI/CD platform. Automates build, test, lint, security scans, and deploy on repo events — free minutes for public repos and limited free tier for private. |
| Must Know | Explain the structure and syntax of a GitHub Actions workflow YAML file. | Top-level keys: `name`, `on` (triggers), `env`, `jobs`. Each job has `runs-on`, optional `needs`, `env`, and a list of `steps` (each `uses:` an action or `run:`s a command). |
| Must Know | Describe how GitHub Actions workflows are triggered using built-in events. | `push`, `pull_request`, `schedule` (cron), `workflow_dispatch`, `release`, `issues`, etc. Multiple triggers allowed: `on: [push, pull_request]`. |
| Must Know | Identify the components of a job and how steps execute in sequence. | A job runs on a fresh runner. Steps run sequentially in the same workspace; a failed step fails the job (unless `continue-on-error: true`). |
| Must Know | Can secure pipeline configuration using secrets and repository permissions. | Store credentials as encrypted secrets, scope `permissions:` block per workflow (least privilege), require approvals on protected environments. |
| Should Know | Explain built in triggers, and what repository events can trigger a workflow run | Code events (`push`, `pull_request`), issue events (`issues`, `issue_comment`), release events (`release`), schedule (`cron`), manual (`workflow_dispatch`), repository_dispatch (external webhook). |
| Should Know | Use a testing step to automatically run unit tests as part of a workflow. | `- name: Test`<br>`  run: dotnet test --no-build --verbosity normal` — fails the job if any test fails, blocking the merge. |
| Should Know | Create a workflow that uses a static analysis tool to scan code for defects. | `- uses: github/codeql-action/analyze@v3` or `- run: npm run lint` — surfaces issues before review. |
| Should Know | Describe the steps of a sequential multi-job workflow that passes artifacts from one job to another. | Job A: `actions/upload-artifact@v4` writes build output. Job B: `needs: A` then `actions/download-artifact@v4` retrieves it for deploy. |
| Should Know | Utilize conditions to control job execution within a workflow. | `if: github.ref == 'refs/heads/main'` on a deploy job ensures it runs only on `main`. Step-level `if` works too. |
| Should Know | Explain how to use GitHub-hosted runners to execute workflows in different runtime environments. | `runs-on: ubuntu-latest` / `windows-latest` / `macos-latest`. Use a `strategy.matrix` to run the same job across multiple OSes/versions in parallel. |
| Nice to have | Create workflow that calls other workflows. | `jobs.deploy.uses: ./.github/workflows/deploy.yml` — reusable workflow accepts `inputs` and `secrets`, called from many places. |
| Nice to have | Use triggers to run branch specific workflows that target different environments. | `on: push: branches: [main, develop]` plus `if: github.ref == 'refs/heads/develop'` to deploy to staging vs prod. |
| Nice to have | Create a workflow that commits output artifacts to the repository for future reuse/review. | `- run: git add dist/ && git commit -m "build" && git push` (with appropriate `permissions: contents: write`). |
| Nice to have | Use dependency caching to speed up future workflow runs. | `- uses: actions/cache@v4`<br>`  with: { path: ~/.nuget/packages, key: nuget-${{ hashFiles('**/*.csproj') }} }` |

## Azure

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Explain what cloud computing is and the benefits of using Azure. | On-demand delivery of compute, storage, and services over the internet. Benefits: elasticity, pay-as-you-go, global reach, managed infrastructure, faster time-to-market. |
| Must Know | Navigate the Azure Portal and identify core UI components. | portal.azure.com — left-hand nav (Home, Resource Groups, All resources), top search, Cloud Shell, notifications bell, Cost Management blade. |
| Must Know | Identify Azure core services such as Virtual Machines, Storage, and Networking. | Compute: VMs, App Service, Functions. Storage: Blob, Table, Queue, Files. Networking: VNet, Load Balancer, Application Gateway, NSG. |
| Must Know | Explain Azure Resource Manager and how resources are organized. | ARM is the deployment/management layer. Hierarchy: Management Group → Subscription → Resource Group → Resource. ARM exposes one consistent API surface for all services. |
| Must Know | Create a basic Azure Virtual Machine and explain its pricing model. | Portal → Create → Virtual Machine → pick image, size, region. Billed per second of run time + storage + outbound bandwidth; stop-deallocate to halt compute charges. |
| Must Know | Demonstrate setting up and using Azure Blob Storage. | Create Storage Account → create Container → upload blobs (block, page, append). Access via portal, Storage Explorer, CLI (`az storage blob upload`), or SDK. |
| Must Know | Explain the different Azure service models: IaaS, PaaS, and SaaS. | IaaS: you manage OS+app (VMs). PaaS: cloud manages OS, you manage app (App Service, Azure SQL DB). SaaS: vendor manages everything (M365, Dynamics). |
| Must Know | Describe Azure regions and availability zones and their impact on deployment. | Region = a geographic area with one or more datacenters. Availability Zones = physically separate datacenters within a region. Deploying across zones survives a single-DC failure. |
| Must Know | Describe the difference between Azure regions, availability zones, and resource groups, and their impact on service deployment. | Region = where (geography). AZ = isolated DC inside a region (HA boundary). Resource Group = logical management/billing container (no geographic implication beyond default location for new resources). |
| Must Know | Demonstrate the use of the Azure Pricing Calculator to compare cost estimates for different service tiers or regions. | azure.microsoft.com/pricing/calculator — add services, choose tier/region/hours, export estimate. Useful for sizing before provisioning. |
| Should Know | Create and manage Resource Groups using the Azure Portal or CLI. | `az group create --name MyRG --location eastus`<br>`az group delete --name MyRG` — deleting a RG deletes all resources inside it. |
| Should Know | Describe Azure identity services including Azure Active Directory. | Microsoft Entra ID (formerly Azure AD) — cloud identity provider. Handles users, groups, app registrations, conditional access, MFA, and SSO. |
| Should Know | Configure Azure Monitor to track resource performance and health. | Enable diagnostic settings → send metrics/logs to a Log Analytics Workspace → query with KQL, build dashboards, set alerts. |
| Should Know | Deploy resources using ARM Templates or Bicep. | Bicep is the modern DSL that transpiles to ARM JSON: `bicep build main.bicep` then `az deployment group create -g MyRG -f main.json`. |
| Should Know | Use pricing calculators and cost management tools to estimate Azure cost. | Cost Management + Billing blade shows actual spend, forecasts, and lets you set budgets and group costs by tag/RG/subscription. |
| Should Know | Configure secure communication between services using service endpoints or private links. | Service endpoint: extends VNet identity to the PaaS service over the Azure backbone. Private Link: gives the PaaS service a private IP inside your VNet (stronger isolation). |
| Should Know | Apply Role-Based Access Control (RBAC) to manage permissions. | Assign built-in roles (Reader, Contributor, Owner) or custom roles to a user/group/service principal at MG / Sub / RG / Resource scope. |
| Should Know | Configure basic alert rules and diagnostic settings in Azure. | Monitor → Alerts → New alert rule: pick scope, condition (e.g., CPU > 80% for 5 min), action group (email/SMS/webhook). |
| Should Know | Deploy a static website using Azure Storage. | Storage Account → Static website blade → enable, set index.html. Upload to the `$web` container; serves over an auto-generated endpoint URL. |
| Nice to have | Execute basic Azure CLI commands to manage services. | `az login`, `az account set --subscription <id>`, `az resource list -g MyRG`, `az vm start --name MyVM -g MyRG`. |
| Nice to have | Build an Azure DevOps pipeline for app deployment to Azure. | YAML pipeline in Azure Repos: build stage (`dotnet build`/`publish`) → deploy stage uses `AzureWebApp@1` task with a service connection. |
| Nice to have | Use Azure Advisor to identify optimization opportunities. | Built-in recommendations engine across Cost, Security, Reliability, Performance, Operational excellence. Suggests rightsizing, idle resource cleanup, etc. |
| Nice to have | Connect to Azure SQL Database and run queries using query editor. | Portal → SQL DB → Query editor (preview) → sign in with SQL or Entra ID auth → run T-SQL directly in browser. |
| Nice to have | Deploy and test a web application using Azure App Services. | Create App Service → set runtime stack → deploy via GitHub Actions, ZIP deploy, or VS publish. Test at `https://<app>.azurewebsites.net`. |
| Nice to have | Explore Azure Marketplace and describe how to provision services from it. | Pre-built solution images (databases, security appliances, dev tools) from Microsoft and partners. Provision via portal "Create a resource" → Marketplace search. |

## Azure with GHA

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must Know | Be able to setup cost management budget alert. | Cost Management + Billing → Budgets → New budget. Set monthly amount, scope (sub/RG), and threshold alerts (e.g., 80%, 100%) emailed to recipients. |
| Must Know | Describe how to deploy and interact with an Azure T-SQL database from a .NET application. | Provision Azure SQL DB → put connection string in `appsettings.json` (or Key Vault) → use EF Core or `SqlClient` from the .NET app. Open server firewall to the app's outbound IP / VNet. |
| Must Know | Be able to describe the different service types and models of cloud infrastructure, and identify service types for common resources. | IaaS (VM, VNet) — you own OS and up. PaaS (App Service, Azure SQL DB) — managed runtime. SaaS (M365) — fully delivered software. |
| Must Know | Understand the security features included with an Azure resource, and how to enable communication between resources and IPs. | Per-resource firewalls (e.g., SQL server firewall rules), NSGs on subnets/NICs, service endpoints, private endpoints, managed identities for service-to-service auth. |
| Must Know | Deploy an ASP.NET API to an Azure Web App via Github Actions deployment. | Workflow: `actions/checkout` → `setup-dotnet` → `dotnet publish` → `azure/webapps-deploy@v3` with `app-name`, `publish-profile`, and the publish output folder. |
| Must Know | Explain what a Service Level Agreement is, and be able to describe the billing, uptime, and other vital stipulations in the agreement. | Contract from cloud provider stating uptime target (e.g., 99.95%) and credits owed if missed. Tiers, multi-zone deployments, and paired services often raise the SLA. |
| Should Know | Be able to debug a cloud resource through the resource logs and analytics. | Enable diagnostic settings → query Log Analytics with KQL: `AppServiceHTTPLogs \| where ScStatus == 500 \| take 50` — find failing requests, latency spikes, exceptions. |
| Should Know | Deploy an ASP.NET API to an Azure Web App using a publish profile in a workflow file. | Download publish profile from the Web App → store as `AZUREAPPSERVICE_PUBLISHPROFILE` secret → reference in `azure/webapps-deploy@v3` `publish-profile:` input. |
| Should Know | Understand and describe availability zones and geo-redundancy. | AZs protect against single-DC failure within a region. Geo-redundancy (e.g., RA-GRS storage, geo-replicated SQL) protects against full-region failure by replicating to a paired region. |
| Should Know | Be able to create and manage multiple resources though a resource group. | Group related resources (App Service + SQL DB + Storage for one app) in one RG → tag, deploy, monitor, and tear down as a unit. |
| Should Know | Be able to manage the selection and cost of resources through requirement analysis before deployment. | Estimate workload (RPS, data size, peak hours) → pick tier (e.g., App Service B1 vs P1v3) → validate with Pricing Calculator → adjust after observing real metrics. |
| Nice to have | Be able to use the Azure CLI to create and manage cloud resources. | `az webapp create -g MyRG -p MyPlan -n MyApp --runtime "DOTNET:8.0"`<br>`az sql db create -g MyRG -s MyServer -n MyDb --service-objective Basic` |
| Nice to have | Describe the use of ARM templates as IAC. | Declarative JSON files describing desired Azure state. Idempotent deployments — same template + parameters yields same resource layout. Bicep is the friendlier authoring layer. |
