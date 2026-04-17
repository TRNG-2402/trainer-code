# Routing

## Learning Objectives

- Contrast conventional routing with attribute routing and identify when each is appropriate.
- Read and write route templates, including parameters, optional parameters, and constraints.
- Apply `[Route]`, `[HttpGet]`, `[HttpPost]`, and sibling attributes to controllers and action methods.
- Understand how route precedence is determined when multiple templates could match a request.

---

## Why This Matters

Routing is the mechanism that maps an incoming HTTP request to a specific action method. Every URL your API exposes is defined by routing rules. A wrong or ambiguous route template produces a 404 (no match), a 500 (ambiguous match), or — worse — silently matches the wrong action.

Understanding routing also matters for the Thursday pair programming day, where you will build Minimal API endpoints (`app.MapGet(...)`) alongside traditional controllers. Both systems use the same underlying endpoint routing infrastructure, so the concepts transfer directly.

---

## The Concept

### Two Routing Systems

ASP.NET Core supports two routing styles for controllers:

| Style | How it works | Recommended for |
|---|---|---|
| **Conventional routing** | Routes are defined once in `Program.cs` and matched by convention (controller name + action name). | MVC apps with Razor views. |
| **Attribute routing** | Routes are defined directly on controller classes and action methods using attributes. | Web APIs — the only option when `[ApiController]` is applied. |

For a Web API, use attribute routing exclusively.

### Route Templates

A route template is a pattern string that the routing engine matches against the URL path. It can contain:

- **Literal segments** — matched exactly: `/api/products`
- **Route parameters** — named placeholders in curly braces: `{id}`
- **Optional parameters** — curly braces with a trailing `?`: `{name?}`
- **Default values** — `{page=1}`
- **Catch-all parameters** — `{**slug}` matches the rest of the path

```csharp
[Route("api/[controller]")]         // Literal "api/" + controller name token
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpGet]                               // matches: GET api/products
    [HttpGet("{id:int}")]                   // matches: GET api/products/42
    [HttpGet("search")]                     // matches: GET api/products/search
    [HttpGet("category/{name}")]            // matches: GET api/products/category/electronics
    [HttpGet("{id:int}/variants/{variantId:guid}")]  // matches: GET api/products/5/variants/abc-...
    ...
}
```

The `[controller]` token in `[Route("api/[controller]")]` is a shorthand that is replaced with the controller class name minus the "Controller" suffix at startup. `ProductsController` becomes `products` (lowercase by default).

### Route Constraints

Route constraints restrict which URL values a parameter will match. They are appended to the parameter name after a colon.

| Constraint | Example | Matches |
|---|---|---|
| `int` | `{id:int}` | Integer values only |
| `guid` | `{id:guid}` | GUID-format strings |
| `bool` | `{flag:bool}` | `true` or `false` |
| `minlength(n)` | `{name:minlength(3)}` | Strings of at least n chars |
| `min(n)` | `{id:min(1)}` | Integers >= n |
| `max(n)` | `{id:max(100)}` | Integers <= n |
| `range(n,m)` | `{id:range(1,999)}` | Integers in range |
| `regex(pattern)` | `{code:regex(^[A-Z]{3}$)}` | Strings matching the regex |
| `alpha` | `{name:alpha}` | Alphabetic characters only |

Constraints prevent spurious 404 errors — a request to `GET /api/products/widget` will not match `[HttpGet("{id:int}")]` and will correctly fall through to a `404 Not Found` instead of attempting to parse `"widget"` as an integer.

### Route Precedence

When multiple route templates could potentially match a request, ASP.NET Core uses a deterministic precedence algorithm:

1. Routes with more literal segments rank higher (more specific wins).
2. Routes with constraints rank higher than unconstrained routes.
3. Catch-all parameters rank lowest.

```csharp
[HttpGet("top-rated")]          // 1 — literal only, ranks highest among these
[HttpGet("{id:int}")]           // 2 — constrained parameter
[HttpGet("{slug}")]             // 3 — unconstrained parameter
[HttpGet("{**catchAll}")]       // 4 — catch-all, ranks lowest
```

A request to `GET /api/products/top-rated` matches the first template, not the second, because the literal match is more specific.

### Nested and Prefixed Routes

You can define additional route prefixes on individual actions that are appended to the controller-level route:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // GET api/orders
    [HttpGet]
    public ActionResult<IEnumerable<OrderDto>> GetAll() { ... }

    // GET api/orders/42/items
    [HttpGet("{orderId:int}/items")]
    public ActionResult<IEnumerable<OrderItemDto>> GetItems(int orderId) { ... }

    // POST api/orders/42/items
    [HttpPost("{orderId:int}/items")]
    public ActionResult<OrderItemDto> AddItem(
        int orderId,
        [FromBody] AddOrderItemDto dto) { ... }
}
```

This pattern models sub-resources and maps directly to REST URI construction conventions from `rest-resources-uri-construction.md` (covered in Week 1).

### Route Names and `CreatedAtAction`

When a POST action creates a resource, it should return `201 Created` with a `Location` header pointing to the new resource. `CreatedAtAction` builds that URL using the route name of an existing action:

```csharp
[HttpPost]
public ActionResult<ProductDto> Create([FromBody] CreateProductDto dto)
{
    var created = _service.Create(dto);

    // Generates: Location: /api/products/99
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

---

## Summary

- Web API projects use **attribute routing** exclusively. Conventional routing is for MVC Razor applications.
- Route templates combine literal segments, parameters (`{id}`), constraints (`{id:int}`), and catch-all segments to form URL patterns.
- The `[controller]` token in route attributes is replaced by the controller class name (minus "Controller") at startup.
- Route constraints narrow which URL values match a parameter, preventing type conversion errors and improving specificity.
- When multiple templates could match, the more-specific template wins: literals > constrained parameters > unconstrained parameters > catch-alls.
- You will encounter these same route template concepts in Minimal APIs (Thursday) and in the advanced routing topics on Wednesday.

---

## Additional Resources

- [Microsoft Docs — Routing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [Microsoft Docs — Route constraints reference](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)
- [Microsoft Docs — Attribute routing for REST APIs](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing#attribute-routing-for-rest-apis)
