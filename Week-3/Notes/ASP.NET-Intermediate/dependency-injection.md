# Dependency Injection

## Learning Objectives

- Explain what Dependency Injection (DI) is and why ASP.NET Core treats it as a first-class concern.
- Differentiate between `Transient`, `Scoped`, and `Singleton` service lifetimes and select the correct one for a given scenario.
- Register services in `Program.cs` using `AddTransient`, `AddScoped`, and `AddSingleton`.
- Inject services into controllers and other services via constructor injection.
- Describe the captive dependency problem and how to avoid it.

---

## Why This Matters

In Monday's `controllers.md`, the `ProductsController` example accepted an `IProductService` via its constructor without any explanation of how that instance was created or supplied. That "how" is Dependency Injection.

DI is not a pattern ASP.NET Core bolts on as an afterthought. The entire framework is built on top of it: logging, configuration, authentication, EF Core `DbContext` -- all of it flows through the same container. Understanding DI is the prerequisite for understanding everything else in ASP.NET Core.

Beyond the framework, DI is what makes your application code loosely coupled and unit-testable. Code that constructs its own dependencies (`new SomeService()`) cannot be tested in isolation. Code that receives its dependencies through an interface can be tested with mock implementations.

---

## The Concept

### What Dependency Injection Is

DI is a technique in which an object's dependencies are supplied by an external mechanism (the container) rather than created by the object itself. The object declares what it needs in its constructor; the container figures out how to satisfy those requirements.

This inverts the direction of control: instead of a class controlling how it acquires dependencies, the container controls that process. This is why DI is an application of the Inversion of Control principle.

ASP.NET Core ships with a built-in DI container (`Microsoft.Extensions.DependencyInjection`). It is sufficient for the vast majority of applications. Third-party containers (Autofac, Lamar) can be substituted for advanced scenarios, but you will rarely need them.

### Registering Services

All service registration happens in `Program.cs` before the application is built. The three core methods are:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register by interface -> implementation
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IConfigProvider, AppConfigProvider>();
```

Once registered, the container knows: "when something asks for `IProductService`, create and return a `ProductService`."

You can also register a concrete type without an interface (not recommended for components you want to test, but useful for framework types):

```csharp
builder.Services.AddScoped<ProductService>();
```

### Service Lifetimes

The lifetime controls how often the container creates a new instance of a registered service.

#### Transient

A new instance is created **every time** the service is requested from the container.

- Use for lightweight, stateless services where instance creation is cheap.
- Safe to use in any scope because each caller gets its own instance.

```csharp
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
```

#### Scoped

A new instance is created **once per HTTP request** (once per scope). Every component that resolves the service within the same request receives the same instance.

- The correct lifetime for services that hold per-request state, such as EF Core `DbContext`.
- Do not use in middleware unless you resolve the service via `IServiceScopeFactory` (middleware is a singleton).

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

#### Singleton

A single instance is created **once for the lifetime of the application** and reused for every request.

- Use for services that are expensive to initialize, thread-safe, and hold no per-request state (e.g., configuration providers, in-memory caches, HTTP clients via `IHttpClientFactory`).
- Singletons that hold mutable state must be implemented with thread safety in mind.

```csharp
builder.Services.AddSingleton<IConfigProvider, AppConfigProvider>();
```

#### Lifetime comparison table

| Lifetime | Instance created | Instance shared |
|---|---|---|
| Transient | Every request to the container | Never (always unique) |
| Scoped | Once per HTTP request | Within the same HTTP request |
| Singleton | Once at startup | Across all requests for the app's lifetime |

### Constructor Injection

The standard pattern for receiving a dependency is constructor injection. Add the dependency as a constructor parameter; the container resolves and injects it at runtime.

```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    // The container supplies both dependencies
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll()
    {
        _logger.LogInformation("Fetching all products.");
        return Ok(_productService.GetAll());
    }
}
```

The same pattern applies to services injecting other services:

```csharp
public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

### `IServiceProvider` -- Manual Resolution

Occasionally you need to resolve a service at runtime rather than at construction time, typically in middleware or factory methods. `IServiceProvider` serves this purpose.

```csharp
app.Use(async (context, next) =>
{
    // Do NOT inject Scoped services into middleware constructors (middleware is a singleton).
    // Instead, resolve from the request's scope:
    var service = context.RequestServices.GetRequiredService<IProductService>();
    // Use service...
    await next(context);
});
```

`GetRequiredService<T>()` throws `InvalidOperationException` if the service is not registered (fail-fast). Prefer it over `GetService<T>()`, which returns `null` for unregistered services.

### The Captive Dependency Problem

A captive dependency occurs when a longer-lived service holds a reference to a shorter-lived service. The longer-lived service keeps the shorter-lived one alive past its intended lifetime, which can cause stale data, threading bugs, and incorrect behavior.

The classic example: a Singleton service injecting a Scoped service.

```csharp
// PROBLEM: CacheService is Singleton, but IProductService is Scoped.
// The Singleton captures the first Scoped instance ever created and holds it forever.
public class CacheService
{
    private readonly IProductService _productService; // Scoped -- WRONG

    public CacheService(IProductService productService)
    {
        _productService = productService;
    }
}
```

To avoid captive dependencies:

1. **Do not inject short-lived services into longer-lived ones.**
2. If absolutely necessary, inject `IServiceScopeFactory` and create a scope manually when you need the shorter-lived service.

The ASP.NET Core DI container validates lifetime compatibility in the development environment (`builder.Environment.IsDevelopment()` triggers scope validation by default). You will see an `InvalidOperationException` at startup if you introduce a captive dependency.

---

## Code Example

A complete service registration and injection example showing interface-to-implementation registration, all three lifetimes, and constructor injection at multiple layers:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProductDb")); // Scoped by default

builder.Services.AddScoped<IProductRepository, EfProductRepository>(); // Scoped
builder.Services.AddScoped<IProductService, ProductService>();          // Scoped
builder.Services.AddSingleton<ISystemClock, UtcSystemClock>();          // Singleton
builder.Services.AddTransient<IEmailNotifier, SmtpEmailNotifier>();     // Transient

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

```csharp
// ProductService.cs
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly ISystemClock _clock;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repo,
        ISystemClock clock,
        ILogger<ProductService> logger)
    {
        _repo = repo;
        _clock = clock;
        _logger = logger;
    }

    public IEnumerable<Product> GetAll()
    {
        _logger.LogInformation("Products fetched at {Time}", _clock.UtcNow);
        return _repo.GetAll();
    }
}
```

```csharp
// ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll()
    {
        return Ok(_service.GetAll());
    }
}
```

The controller knows nothing about `EfProductRepository`, `AppDbContext`, or `UtcSystemClock`. It depends only on the `IProductService` abstraction. This makes the controller independently testable with a mock `IProductService`.

---

## Summary

- ASP.NET Core's built-in DI container resolves services declared as constructor parameters at runtime.
- Register services in `Program.cs` using `AddTransient`, `AddScoped`, or `AddSingleton` depending on the desired lifetime.
- `Transient` creates a new instance per resolution; `Scoped` creates one per HTTP request; `Singleton` creates one for the application's lifetime.
- Constructor injection is the standard pattern -- declare dependencies as constructor parameters and store them in `readonly` fields.
- Avoid captive dependencies: never inject a shorter-lived service into a longer-lived one. The container will throw at startup in development if you do.

---

## Additional Resources

- [Microsoft Docs -- Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Microsoft Docs -- Service lifetimes](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#service-lifetimes)
- [Microsoft Docs -- Scope validation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#scope-validation)
