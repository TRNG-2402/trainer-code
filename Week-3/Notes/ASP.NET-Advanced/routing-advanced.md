# Advanced Routing

## Learning Objectives

- Apply route constraints to restrict which values a route parameter accepts.
- Use `LinkGenerator` to programmatically build URLs without string literals.
- Understand route transformers and endpoint metadata.
- Describe area routing and how it organizes large controller hierarchies.
- Explain how the routing middleware integrates with the authorization middleware pipeline.

---

## Why This Matters

Monday's `routing.md` introduced the foundations: attribute routing, route templates, `[HttpGet]`, `[HttpPost]`, and route parameters. That knowledge gets you to a working API quickly. But as a codebase grows -- more controllers, more endpoints, more security rules, more URL patterns -- naive routing configurations become a source of bugs. A mistyped string route, the wrong middleware ordering, or a missing constraint that allows `"abc"` into an integer parameter can all cause runtime failures that are hard to trace.

Advanced routing features address these problems systematically: constraints enforce input validity at the routing layer before the action method runs, `LinkGenerator` eliminates hard-coded URL strings throughout the codebase, and metadata allows cross-cutting concerns like authorization to be declared alongside routes.

---

## Route Constraints

A route constraint restricts the values a route parameter will match. If an incoming URL does not satisfy the constraint, the route does not match and routing continues to the next candidate.

### Inline constraint syntax

```csharp
// Only matches if {id} is a valid integer
[HttpGet("{id:int}")]
public IActionResult GetById(int id) { /* ... */ }

// Only matches GUIDs
[HttpGet("{guid:guid}")]
public IActionResult GetByGuid(Guid guid) { /* ... */ }

// Only matches if the length is between 3 and 10 characters
[HttpGet("{slug:minlength(3):maxlength(10)}")]
public IActionResult GetBySlug(string slug) { /* ... */ }

// Regular expression constraint
[HttpGet(@"{sku:regex(^[A-Z]{{3}}-\d{{4}}$)}")]
public IActionResult GetBySku(string sku) { /* ... */ }
```

> Note the double braces `{{` and `}}` inside `regex(...)`. Inside a C# verbatim or regular string, you need `{{` and `}}` to produce a literal `{` and `}` in the route template.

### Common built-in constraints

| Constraint | Accepted values |
|---|---|
| `int` | Any 32-bit integer |
| `long` | Any 64-bit integer |
| `double` | Any double-precision float |
| `bool` | `true` or `false` |
| `guid` | Any GUID in standard formats |
| `datetime` | Any parseable date/time value |
| `alpha` | One or more ASCII letters only |
| `minlength(n)` | String with at least n characters |
| `maxlength(n)` | String with no more than n characters |
| `length(n)` / `length(min,max)` | Exact length or length range |
| `min(n)` / `max(n)` | Numeric minimum/maximum |
| `range(min,max)` | Numeric value within the range |
| `regex(expression)` | Matches the regular expression |

### Chaining constraints

Multiple constraints are chained with colons and all must pass:

```csharp
// Must be an integer, and at minimum 1
[HttpGet("{id:int:min(1)}")]
public IActionResult GetById(int id) { /* ... */ }
```

### Custom route constraints

For logic beyond what built-in constraints offer, implement `IRouteConstraint`:

```csharp
public class EvenNumberConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route,
                      string routeKey, RouteValueDictionary values,
                      RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) &&
            int.TryParse(value?.ToString(), out int intValue))
        {
            return intValue % 2 == 0;
        }
        return false;
    }
}
```

Register it in `Program.cs` and reference it by its alias:

```csharp
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("even", typeof(EvenNumberConstraint));
});

// Usage
[HttpGet("{batchId:even}")]
public IActionResult GetEvenBatch(int batchId) { /* ... */ }
```

---

## Route Transformers

A route transformer converts a route value before it appears in the URL or is matched from an incoming URL. The most common use case is transforming PascalCase controller and action names into kebab-case URL segments.

```csharp
// Without a transformer:  /ProductCategories/GetAll
// With SlugifyTransformer: /product-categories/get-all
```

### Implementing a `SlugifyParameterTransformer`

```csharp
using System.Text.RegularExpressions;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null) return null;
        return Regex.Replace(
            value.ToString()!,
            "([a-z])([A-Z])",
            "$1-$2",
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100)
        ).ToLowerInvariant();
    }
}
```

Register it as a convention:

```csharp
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});
```

By convention, `ProductsController` will be available at `/products` (the "Controller" suffix is already stripped by the framework) and action names are transformed at URL generation time.

---

## Endpoint Metadata

Every route in ASP.NET Core's endpoint routing system is an `Endpoint` object that carries a metadata collection. Filters, attributes, and middleware all read from this metadata to make decisions.

Authorization, for example, works entirely through metadata: `[Authorize]` attaches `IAuthorizeData` metadata to the endpoint, and the authorization middleware reads it at request time.

You can read endpoint metadata yourself inside middleware or filter code:

```csharp
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    if (endpoint is not null)
    {
        // Check whether the endpoint has [AllowAnonymous]
        var allowAnonymous = endpoint.Metadata
            .GetMetadata<IAllowAnonymous>() is not null;

        var controllerName = endpoint.Metadata
            .GetMetadata<ControllerActionDescriptor>()?.ControllerName;
    }

    await next(context);
});
```

You can attach custom metadata by creating a custom attribute that implements `IEndpointMetadataProvider` or by using `MapGet(...).WithMetadata(new MyMetadata())` on Minimal API endpoints (which we will cover on Thursday).

---

## `LinkGenerator` for URL Generation

Hard-coded URL strings in code are fragile -- they break silently when routes change. `LinkGenerator` is the service-oriented, refactoring-safe alternative to string concatenation.

### Injecting `LinkGenerator`

```csharp
[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly LinkGenerator _links;

    public ProductsController(IProductService service, LinkGenerator links)
    {
        _service = service;
        _links   = links;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        var product = _service.Create(dto);

        // Generate the URL for the GetById action, including the new id
        var locationUrl = _links.GetPathByAction(
            httpContext: HttpContext,
            action:      nameof(GetById),
            controller:  "Products",
            values:      new { id = product.Id });

        return Created(locationUrl!, product);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) { /* ... */ }
}
```

`LinkGenerator` can also generate absolute URIs with `GetUriByAction`, which is useful for producing links in emails, webhooks, or any response body where the caller is not the direct originator of the HTTP request.

### Controller helpers

Inside a controller, the convenient shorthand methods `CreatedAtAction` and `CreatedAtRoute` achieve the same result without injecting `LinkGenerator` directly:

```csharp
return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
```

These methods rely on `LinkGenerator` internally.

---

## Area Routing (Brief Overview)

Areas partition a large application into distinct functional groups, each with its own controllers, views, and routes. They are particularly common in MVC applications with separate admin and public-facing sections, but they also apply to large Web API projects.

```csharp
// Mark a controller as belonging to an area
[Area("Admin")]
[Route("admin/[controller]/[action]")]
[ApiController]
public class UsersController : ControllerBase { /* ... */ }

// Map area routes in Program.cs
app.MapControllerRoute(
    name:    "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

The `:exists` constraint on the area token ensures the segment only matches if a controller with the corresponding `[Area("...")]` attribute exists. Deep coverage of areas is beyond today's scope, but you now know where to look when organizing very large API surfaces.

---

## How Routing Integrates with Authorization Middleware

A common source of bugs is placing middleware in the wrong order relative to `UseRouting` and `UseAuthorization`. The correct ordering for a production API is:

```csharp
var app = builder.Build();

// 1. Exception handling / HSTS
app.UseExceptionHandler("/error");
app.UseHsts();

// 2. HTTPS redirection
app.UseHttpsRedirection();

// 3. Static files (served before routing, no auth needed)
app.UseStaticFiles();

// 4. Routing -- matches the endpoint but does NOT execute it yet
app.UseRouting();

// 5. CORS -- must be between UseRouting and UseAuthorization
app.UseCors();

// 6. Authentication -- reads the credential and populates HttpContext.User
app.UseAuthentication();

// 7. Authorization -- reads endpoint metadata (e.g., [Authorize]) and User
app.UseAuthorization();

// 8. Controller execution
app.MapControllers();

app.Run();
```

Why does order matter here?

- `UseRouting` selects the endpoint and loads its metadata into `HttpContext`.
- `UseAuthorization` reads that metadata to know which policies apply.
- If `UseAuthorization` runs before `UseRouting`, it has no endpoint metadata to evaluate and cannot enforce any policy.
- If `UseAuthentication` runs after `UseAuthorization`, `HttpContext.User` is not populated when authorization checks run -- all authenticated requirements fail.

---

## Summary

- **Route constraints** restrict parameter values at the routing layer, eliminating a class of invalid-input bugs before action methods execute.
- **Custom constraints** implement `IRouteConstraint` for business-specific matching logic.
- **Route transformers** normalize URL segment casing (e.g., PascalCase to kebab-case) consistently across the entire application.
- **Endpoint metadata** is the mechanism by which attributes like `[Authorize]` communicate with middleware -- understanding it demystifies how cross-cutting concerns work.
- **`LinkGenerator`** and `CreatedAtAction` replace hard-coded URL strings, making routes refactoring-safe.
- **Areas** are the organizational unit for large controller hierarchies with distinct URL prefixes.
- **Middleware ordering** is critical: `UseRouting` -> `UseCors` -> `UseAuthentication` -> `UseAuthorization` -> `MapControllers` is the required sequence for authorization to function correctly.

---

## Additional Resources

- [Routing in ASP.NET Core (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [Route constraints reference (Microsoft Docs)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)
- [LinkGenerator class (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.linkgenerator)
