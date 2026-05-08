# Azure SQL Database from a .NET App

## Learning Objectives
- Provision an Azure SQL Database and configure access for a deployed app.
- Compose a connection string the .NET app can consume from `appsettings.json`, environment variables, or Key Vault.
- Connect from EF Core and from `Microsoft.Data.SqlClient` directly.
- Authenticate using a managed identity (preferred) or SQL auth (fallback), and pick the right firewall path.

## Why This Matters

The QC-4 Must-Know "deploy and interact with an Azure T-SQL database from a .NET application" objective comes down to: provision, secure the network, hand the app a credential or identity, and call it. Get this right and the app you deployed to App Service yesterday now has a real database; get the auth wrong and either nothing connects or you ship secrets.

## The Concept

### Azure SQL DB at a Glance

**Azure SQL Database** is a managed T-SQL database (the same engine as SQL Server, run for you). Two billing models:

- **DTU-based** (Basic / Standard / Premium) — bundled CPU + memory + IO. Simpler to reason about.
- **vCore-based** (General Purpose / Business Critical / Hyperscale) — explicit CPU + RAM + storage; supports zone redundancy, read replicas, larger sizes.

A **logical SQL Server** holds one or more databases and owns server-level settings (firewall, Entra admin, audit). The database itself sits inside the server.

For training: Basic or S0 (Standard, 10 DTU) is plenty.

### Authentication Options

Three ways to authenticate from .NET to Azure SQL DB:

| Method | Credential in code | Where the secret lives | Use when |
|---|---|---|---|
| **SQL auth** | username + password | `appsettings.json` or env var or Key Vault | Quickest. Acceptable for training; risky for production. |
| **Entra ID (interactive / device code)** | none in code; user signs in | n/a | Local dev, ad-hoc tooling. |
| **Managed identity** | none in code; App Service or VM identity is granted SQL access | n/a | Production. Strongly preferred. |

Managed identity is the goal. The app gets a token from Azure at runtime and SQL DB checks the principal against an Entra-mapped database user. No password ever touches code or config.

### Network Path

A SQL Server has a **firewall** — by default it denies all network access. You poke holes for:

- **Specific public IPs / ranges** — your laptop, your CI runner, a partner's egress IP. Use sparingly.
- **"Allow Azure services and resources to access this server"** — a checkbox that allows traffic from any Azure resource in any subscription. Convenient, but it's also a wide door. Use only in dev.
- **VNet rule** (service endpoint) — allow a specific subnet. The PaaS firewall on the SQL Server side now trusts that subnet.
- **Private Endpoint** — give the SQL Server a private IP inside your VNet and disable the public endpoint entirely. Production default.

For a Web App in App Service talking to SQL DB, the production-grade setup is: VNet integration on the App Service, Private Endpoint on the SQL Server, public access disabled. (See `azure-networking-security.md`.)

### Connection String Shapes

For .NET, the connection string is passed via `appsettings.json`, an environment variable, or `IConfiguration` from Key Vault.

**SQL auth (simplest, least secure):**

```
Server=tcp:sql-trainee-prod.database.windows.net,1433;
Initial Catalog=appdb;
User ID=sqladmin;Password=<strong>;
Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Managed identity (preferred):**

```
Server=tcp:sql-trainee-prod.database.windows.net,1433;
Initial Catalog=appdb;
Authentication=Active Directory Default;
Encrypt=True;TrustServerCertificate=False;
```

`Authentication=Active Directory Default` tells `Microsoft.Data.SqlClient` to use `DefaultAzureCredential` semantics — managed identity in Azure, `az login` token locally. No password.

`Encrypt=True` + `TrustServerCertificate=False` enforces TLS with cert validation. Required by Azure SQL.

### Where the Connection String Lives in App Service

On Azure Web App:

1. **Configuration -> Connection strings** blade. Add a new entry, type `SQLAzure`, name `DefaultConnection`. Saved as an environment variable visible to the app.
2. The framework reads it via `builder.Configuration.GetConnectionString("DefaultConnection")` — same code path as `appsettings.json`.
3. For maximum safety, store the connection string in **Key Vault** as a secret and reference it from App Service Configuration with a Key Vault reference syntax: `@Microsoft.KeyVault(SecretUri=https://<vault>.vault.azure.net/secrets/DefaultConnection/)`. App Service uses its managed identity to fetch the secret at startup.

Never commit a real connection string. Local-dev `appsettings.Development.json` should be gitignored if it has one.

## Code Examples

### 1. Provision the database

```bash
RG=rg-productcatalog-prod
LOC=eastus
SQL_SERVER=sql-trainee-prod
DB=appdb
ADMIN=sqladmin

az group create -n $RG -l $LOC

# Logical SQL Server. Use a strong password or skip and add Entra admin instead.
az sql server create -g $RG -n $SQL_SERVER -l $LOC \
  --admin-user $ADMIN \
  --admin-password "$(openssl rand -base64 24)Aa1!"

# Database (Basic tier).
az sql db create -g $RG -s $SQL_SERVER -n $DB \
  --service-objective Basic

# Allow Azure services (quickest in training; tighten later).
az sql server firewall-rule create -g $RG -s $SQL_SERVER \
  -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

### 2. Set up Entra-integrated authentication (managed identity)

```bash
# Make a specific Entra group the SQL Server admin.
ADMIN_GROUP_ID=$(az ad group show --group "sql-admins" --query id -o tsv)
az sql server ad-admin create -g $RG -s $SQL_SERVER \
  --display-name "sql-admins" --object-id $ADMIN_GROUP_ID

# Assume the Web App's system-assigned identity is on.
APP=productcatalog-api-eastus
PRINCIPAL_ID=$(az webapp identity show -g $RG -n $APP --query principalId -o tsv)
APP_NAME=$(az webapp show -g $RG -n $APP --query name -o tsv)

# Connect to the database (as a sql-admins member) and create a contained user
# mapped to the Web App's identity, with the permissions it needs.
# Run this T-SQL via sqlcmd, Azure Data Studio, or the Portal Query editor:
```

```sql
-- Connect to the appdb database as an sql-admins member.
CREATE USER [productcatalog-api-eastus] FROM EXTERNAL PROVIDER;

ALTER ROLE db_datareader ADD MEMBER [productcatalog-api-eastus];
ALTER ROLE db_datawriter ADD MEMBER [productcatalog-api-eastus];
-- Grant EXECUTE on stored procs only if your code calls procs.
```

The Web App's managed identity is now a database user with read/write — no password.

### 3. Set the connection string in App Service (managed identity flavor)

```bash
CONN="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${DB};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;"

az webapp config connection-string set -g $RG -n $APP \
  --settings DefaultConnection="$CONN" \
  --connection-string-type SQLAzure
```

### 4. EF Core in the .NET app

`Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.AsNoTracking().ToListAsync());

app.Run();
```

`AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

`Microsoft.Data.SqlClient` (which EF Core uses underneath) honors `Authentication=Active Directory Default` with no extra setup beyond having the managed identity enabled.

### 5. Plain ADO.NET (when EF Core is overkill)

```csharp
using Microsoft.Data.SqlClient;

var conn = new SqlConnection(
    builder.Configuration.GetConnectionString("DefaultConnection"));

await conn.OpenAsync();

using var cmd = new SqlCommand(
    "SELECT TOP (10) Id, Name, Price FROM Products ORDER BY Id DESC", conn);
using var rdr = await cmd.ExecuteReaderAsync();
while (await rdr.ReadAsync())
{
    Console.WriteLine($"{rdr.GetInt32(0)}: {rdr.GetString(1)} ${rdr.GetDecimal(2)}");
}
```

### 6. Local development

For local dev, the same connection string works as long as `az login` has signed you in to an account that is a member of the SQL admin group. `DefaultAzureCredential` falls back to your CLI token. No code change between local and cloud.

## Summary

- Provision: `az sql server create` + `az sql db create`. Use Basic / S0 for training.
- Lock the network down: prefer Private Endpoint for production; for training, "Allow Azure services" is acceptable but not safe.
- Auth: managed identity + Entra-mapped contained user is the production path; SQL auth is fine for early-stage training. `Authentication=Active Directory Default` in the connection string activates the managed-identity flow.
- Connection string lives in App Service Configuration -> Connection strings (or Key Vault); read via `IConfiguration.GetConnectionString` in code.
- EF Core via `UseSqlServer(conn)` and `Microsoft.Data.SqlClient` directly both honor managed-identity auth without extra code.

## Additional Resources

- [Quickstart: Create an Azure SQL Database](https://learn.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart)
- [Microsoft Entra authentication for Azure SQL DB](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-overview)
- [Use managed identities to connect to Azure SQL from .NET](https://learn.microsoft.com/en-us/azure/azure-sql/database/azure-sql-dotnet-entity-framework-core-quickstart)
