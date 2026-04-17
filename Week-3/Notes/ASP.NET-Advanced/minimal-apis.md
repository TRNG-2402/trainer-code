# Minimal APIs

## Learning Objectives

- Describe what Minimal APIs are and how they differ from the controller-based pattern.
- Define routes using `app.MapGet`, `app.MapPost`, `app.MapPut`, and `app.MapDelete`.
- Bind parameters from the route, query string, and request body in Minimal API handlers.
- Return strongly-typed results using `Results` and `TypedResults`.
- Organize Minimal API endpoints using route groups and extension methods.
- Determine when to use Minimal APIs versus controller-based APIs.

---

## Why This Matters

ASP.NET Core 6 introduced Minimal APIs as a first-class alternative to the traditional `ControllerBase` pattern. Since then, they have become a prominent part of the ecosystem -- Microsoft's own templates and many open-source libraries now use them. Understanding Minimal APIs is no longer optional; you will encounter them in codebases and job interviews alike.

More importantly, Minimal APIs reflect a broader industry trend toward explicit, functional-style HTTP handler definitions (similar to Express.js, Fastify, and Go's `net/http`). For engineers with a JavaScript background, the pattern may feel immediately familiar.

---

## The Concept

### What Minimal APIs Are

Minimal APIs are a way to define HTTP endpoints directly in `Program.cs` (or in any file that has access to `WebApplication app`) without a controller class. Each endpoint is a call to a mapping method on the `WebApplication` instance, accepting a route pattern and a delegate (lambda or method group).

A complete Minimal API application can be as short as this:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello, World!");

app.Run();
```

Compare that with the equivalent controller overhead: a `ControllerBase` subclass, action method, `[HttpGet]`, `[Route]`, and `[ApiController]` attributes. For simple resources, the reduction in ceremony is significant.

### Mapping HTTP Methods

```csharp
app.MapGet("/api/products", () => Results.Ok(new[] { "Widget A", "Widget B" }));

app.MapGet("/api/products/{id:int}", (int id) =>
    id > 0
        ? Results.Ok(new { Id = id, Name = "Widget A" })
        : Results.NotFound());

app.MapPost("/api/products", (CreateProductDto dto) =>
{
    // Validation, persistence omitted for brevity.
    var created = new Product { Id = 42, Name = dto.Name };
    return Results.Created($"/api/products/{created.Id}", created);
});

app.MapPut("/api/products/{id:int}", (int id, UpdateProductDto dto) =>
    Results.NoContent());

app.MapDelete("/api/products/{id:int}", (int id) =>
    Results.NoContent());
```

### Parameter Binding

Minimal API handlers bind parameters by convention, the same sources the controller model binder uses:

| Source | Convention |
|---|---|
| Route segment | Parameter name matches the route template token |
| Query string | Parameter name matches the query key |
| Request body | Complex type (POCO) -- treated as `[FromBody]` by default |
| Services (DI) | Registered service type -- injected automatically |
| `HttpContext` | `HttpContext` parameter -- special type, injected directly |
| `CancellationToken` | `CancellationToken` parameter -- injected directly |

```csharp
// Route parameter: id comes from {id:int}
// Query string: search comes from ?search=widget
// Body: dto is deserialized from JSON body
app.MapGet("/api/products/{id:int}", (int id, string? search, IProductService svc) =>
{
    var product = svc.FindById(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});
```

To be explicit about the source, use `[FromRoute]`, `[FromQuery]`, `[FromBody]`, and `[FromHeader]` attributes in the method signature, exactly as in controllers.

### `Results` and `TypedResults`

Minimal API handlers return `IResult`. The static `Results` class provides factory methods for all standard HTTP responses:

```csharp
Results.Ok(value)              // 200 with body
Results.Created(uri, value)    // 201 with Location header and body
Results.NoContent()            // 204
Results.BadRequest(problem)    // 400 with ProblemDetails
Results.NotFound()             // 404
Results.Unauthorized()         // 401
Results.Forbid()               // 403
Results.Problem(...)           // RFC 7807 ProblemDetails
```

`TypedResults` is the strongly-typed equivalent, introduced in .NET 7. It enables OpenAPI (Swagger) to infer return types without manual `Produces<T>()` metadata calls:

```csharp
app.MapGet("/api/products/{id:int}", async (int id, IProductService svc) =>
{
    var product = await svc.FindByIdAsync(id);
    return product is not null
        ? TypedResults.Ok(product)
        : TypedResults.NotFound();
});
```

Prefer `TypedResults` when OpenAPI documentation accuracy matters.

### Route Groups

As endpoint counts grow, placing all mappings in `Program.cs` becomes unwieldy. Route groups let you apply a common prefix and shared middleware (filters, authorization) to a set of related endpoints.

```csharp
var products = app.MapGroup("/api/products");

products.MapGet("/", GetAllProducts);
products.MapGet("/{id:int}", GetProductById);
products.MapPost("/", CreateProduct);
products.MapPut("/{id:int}", UpdateProduct);
products.MapDelete("/{id:int}", DeleteProduct);

// Apply authorization to the entire group.
products.RequireAuthorization();
```

The prefix `/api/products` is declared once on the group; individual mappings use relative paths.

### Organizing Endpoints with Extension Methods

Moving endpoint definitions into static extension methods on `WebApplication` or `RouteGroupBuilder` keeps `Program.cs` clean and makes endpoint code testable in isolation.

```csharp
// CategoryEndpoints.cs
public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);

        return group;
    }

    private static async Task<IResult> GetAll(ICategoryService svc)
    {
        var categories = await svc.GetAllAsync();
        return TypedResults.Ok(categories);
    }

    private static async Task<IResult> GetById(int id, ICategoryService svc)
    {
        var category = await svc.FindByIdAsync(id);
        return category is not null ? TypedResults.Ok(category) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(
        CreateCategoryDto dto, ICategoryService svc)
    {
        var created = await svc.CreateAsync(dto);
        return TypedResults.Created($"/api/categories/{created.Id}", created);
    }
}
```

```csharp
// Program.cs
app.MapCategoryEndpoints();
```

This pattern mirrors the pattern many .NET teams use in production services.

### Filters in Minimal APIs

Minimal APIs support endpoint filters via `IEndpointFilter`. This is the Minimal API equivalent of controller action filters. Filters can perform cross-cutting concerns -- validation, logging, rate-limiting -- without duplicating code in every handler.

```csharp
// Validation filter that checks ModelState equivalent for Minimal APIs.
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
            return Results.BadRequest("Invalid request body.");

        var validationContext = new ValidationContext(argument);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(argument, validationContext, results, true))
            return Results.ValidationProblem(results.ToDictionary(
                r => r.MemberNames.FirstOrDefault() ?? "",
                r => new[] { r.ErrorMessage ?? string.Empty }));

        return await next(context);
    }
}

// Apply to a single endpoint:
products.MapPost("/", CreateProduct)
        .AddEndpointFilter<ValidationFilter<CreateProductDto>>();
```

### Minimal APIs vs. Controller-Based APIs

| Factor | Minimal APIs | Controller-Based APIs |
|---|---|---|
| Boilerplate | Minimal | More |
| Organization | Requires explicit structure (groups, extension methods) | Built-in by class hierarchy |
| Filters | `IEndpointFilter` | Rich filter pipeline (Action, Exception, Resource...) |
| Model binding attributes | Supported | Fully supported, more sources |
| OpenAPI support | `TypedResults` + `.WithOpenApi()` | Automatic via Swashbuckle |
| Testing | Can test delegates directly | `WebApplicationFactory` covers controllers |
| When to use | Simple services, lightweight endpoints, microservices | Complex APIs, large teams, many cross-cutting concerns |

The two approaches coexist in the same application. A common pattern is to use controllers for the primary resource API surface and Minimal APIs for lightweight endpoints such as health checks, version lookups, or category/lookup tables.

---

## Summary

- Minimal APIs define HTTP endpoints by calling `app.MapGet`, `app.MapPost`, `app.MapPut`, and `app.MapDelete` with a route pattern and a delegate.
- Parameters are bound by convention from the route, query string, body, or DI container.
- `Results` provides factory methods for all standard HTTP response types; `TypedResults` adds OpenAPI type inference.
- Route groups (`app.MapGroup(prefix)`) apply shared prefixes and middleware to a set of related endpoints.
- Move endpoint definitions into static extension methods (e.g., `MapCategoryEndpoints`) to keep `Program.cs` maintainable.
- Endpoint filters (`IEndpointFilter`) provide cross-cutting logic equivalent to controller action filters.
- Minimal APIs and controller-based APIs are not mutually exclusive; use each where it fits best.

---

## Additional Resources

- [Minimal APIs overview - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview)
- [Route groups in Minimal APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-groups)
- [Endpoint filters in Minimal API apps - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters)
