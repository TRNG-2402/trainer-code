# Environment and Configuration

## Learning Objectives

- Explain how ASP.NET Core loads configuration from multiple layered sources.
- Use `IConfiguration` to read values directly and the Options pattern to bind configuration sections to typed objects.
- Distinguish between `appsettings.json`, environment-specific overrides, environment variables, and user secrets.
- Apply the `ASPNETCORE_ENVIRONMENT` variable to control which environment is active.

---

## Why This Matters

Configuration is one of the most operationally critical aspects of a web application. The same binary must behave differently depending on where it is deployed — different database connection strings, different log levels, different feature flags, different third-party API keys. A configuration system that handles this cleanly, without requiring code changes between environments, is not a convenience — it is a production requirement.

Beyond environment differences, secure configuration management prevents one of the most common security failures in deployed applications: accidentally committing secrets to source control. Understanding how ASP.NET Core's configuration system works is therefore both an architectural concern and a security concern.

---

## The Concept

### The Configuration Sources Stack

ASP.NET Core loads configuration from multiple sources in a **priority order**. Later sources override earlier ones for the same key.

```
Priority (lowest to highest):
1. appsettings.json
2. appsettings.{Environment}.json
3. User Secrets (Development environment only)
4. Environment variables
5. Command-line arguments
```

This means an environment variable always wins over an `appsettings.json` value for the same key, and a command-line argument wins over everything. This layered override behavior is what makes it possible to ship the same application binary to Development, Staging, and Production while injecting environment-specific values at the infrastructure level.

### The `ASPNETCORE_ENVIRONMENT` Variable

The active environment is controlled by the `ASPNETCORE_ENVIRONMENT` environment variable. ASP.NET Core uses this value to:

- Determine which `appsettings.{Environment}.json` overlay to load.
- Expose `app.Environment.IsDevelopment()`, `app.Environment.IsProduction()`, etc. in `Program.cs`.
- Enable developer-only tooling like the Swagger UI and the developer exception page.

Three built-in environment names are recognized: `Development`, `Staging`, and `Production`. You may define custom names.

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
```

### Reading Configuration with `IConfiguration`

`IConfiguration` is the lowest-level interface for accessing configuration. It is available via dependency injection.

```csharp
// appsettings.json
{
  "AppSettings": {
    "MaxPageSize": 50,
    "FeatureFlags": {
      "EnableExport": true
    }
  }
}
```

```csharp
public class ProductsController : ControllerBase
{
    private readonly IConfiguration _config;

    public ProductsController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var maxPage = _config.GetValue<int>("AppSettings:MaxPageSize");
        var exportEnabled = _config.GetValue<bool>("AppSettings:FeatureFlags:EnableExport");
        ...
    }
}
```

The colon (`:`) is the hierarchical separator for nested keys.

Direct use of `IConfiguration` is acceptable for one-off reads. For structured configuration sections used across multiple classes, the **Options pattern** is the correct approach.

### The Options Pattern (`IOptions<T>`)

The Options pattern binds a configuration section to a POCO (Plain Old C# Object) class. This provides:

- Strong typing — no magic strings.
- Validation — you can validate the configuration object on startup.
- A clean injection target that is scoped to a specific concern.

**Step 1: Define the options class.**

```csharp
public class AppSettings
{
    public int MaxPageSize { get; set; } = 25;
    public FeatureFlags FeatureFlags { get; set; } = new();
}

public class FeatureFlags
{
    public bool EnableExport { get; set; }
}
```

**Step 2: Register the binding in `Program.cs`.**

```csharp
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
```

**Step 3: Inject and consume.**

```csharp
public class ProductsController : ControllerBase
{
    private readonly AppSettings _settings;

    public ProductsController(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll()
    {
        var pageSize = _settings.MaxPageSize;
        ...
    }
}
```

#### `IOptions<T>` vs `IOptionsSnapshot<T>` vs `IOptionsMonitor<T>`

| Interface | Lifetime | Reloads at runtime |
|---|---|---|
| `IOptions<T>` | Singleton | No |
| `IOptionsSnapshot<T>` | Scoped (per request) | Yes, on next request |
| `IOptionsMonitor<T>` | Singleton with change notifications | Yes, immediately |

Use `IOptions<T>` for static configuration. Use `IOptionsSnapshot<T>` when you need the latest values per request (e.g., feature flags that change without restart).

### User Secrets

User secrets store sensitive values during local development without placing them in `appsettings.json` or any file that would be committed to source control. They are stored in a JSON file in the OS user profile directory, outside the project folder.

Initialize user secrets for a project:

```
dotnet user-secrets init
```

Set a secret:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=DevDb;..."
```

User secrets are automatically loaded when `ASPNETCORE_ENVIRONMENT` is `Development`. They override `appsettings.json` values for the same key.

**In production, use environment variables or a secrets manager (Azure Key Vault, AWS Secrets Manager) — never user secrets.**

### Environment Variables as Configuration Keys

ASP.NET Core maps environment variable names to configuration keys by replacing double underscores (`__`) with the colon separator. This allows nested keys to be set via environment variables.

```bash
# In the OS or container environment:
AppSettings__MaxPageSize=100
AppSettings__FeatureFlags__EnableExport=true
```

These values override the equivalent `appsettings.json` values at runtime.

---

## Summary

- ASP.NET Core loads configuration from a priority-ordered stack: `appsettings.json` first, then environment-specific overrides, user secrets, environment variables, and command-line arguments. Later sources win.
- `ASPNETCORE_ENVIRONMENT` controls which environment overlay is loaded and which environment-conditional code runs.
- Use `IConfiguration` for simple, ad-hoc key reads. Use the Options pattern (`IOptions<T>`) for structured, typed configuration sections used across multiple services.
- User secrets are for local development only. Production secrets belong in environment variables or a secrets management service.
- Double underscores (`__`) in environment variable names map to the colon (`:`) hierarchy separator in configuration keys.

---

## Additional Resources

- [Microsoft Docs — Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Microsoft Docs — Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Microsoft Docs — Safe storage of app secrets (User Secrets)](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
