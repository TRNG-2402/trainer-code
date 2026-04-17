# Model Binding

## Learning Objectives

- Explain how ASP.NET Core maps incoming HTTP request data to action method parameters.
- Identify the five binding sources and the attribute that explicitly selects each one.
- Understand how complex types are bound from the request body.
- Use `ModelState` to detect and respond to validation failures.

---

## Why This Matters

Model binding is the mechanism that turns raw HTTP request data — URL segments, query strings, headers, and JSON bodies — into the strongly-typed C# parameters your action methods receive. Without understanding binding sources, you will write action methods that silently receive `null` values, fail to read posted JSON, or expose security vulnerabilities by accepting data from unexpected locations.

Every controller action you write relies on model binding. Getting it right is foundational.

---

## The Concept

### What Model Binding Does

When a request arrives, ASP.NET Core inspects the action method's parameter list and attempts to populate each parameter from the request. This process is called **model binding**. It handles:

- Extracting values from multiple sources (route, query string, body, headers, form).
- Converting raw strings to .NET types (e.g., `"42"` to `int`).
- Constructing complex objects from JSON bodies.
- Populating `ModelState` with any conversion or validation errors.

### Binding Sources

There are five binding source attributes that control where a parameter's value is read from:

| Attribute | Source | Typical Use |
|---|---|---|
| `[FromRoute]` | URL route template segments | Resource identifiers (`{id}`) |
| `[FromQuery]` | URL query string | Filters, pagination, sort options |
| `[FromBody]` | Request body (JSON, XML) | Create/update payloads |
| `[FromHeader]` | HTTP request headers | API keys, tracing identifiers |
| `[FromForm]` | Form data (`multipart/form-data`) | File uploads, HTML form posts |

### Binding Source Inference (with `[ApiController]`)

When `[ApiController]` is present, ASP.NET Core applies binding source inference automatically:

- **Complex types** (classes, records) → `[FromBody]`
- **Simple types** (int, string, Guid) from the route template → `[FromRoute]`
- **Simple types** not in the route template → `[FromQuery]`

This means you often do not need to write explicit binding attributes for straightforward parameters. However, explicit attributes are recommended in team settings for clarity.

### Binding Simple Types

```csharp
// GET /api/products/42?includeArchived=true
[HttpGet("{id:int}")]
public ActionResult<ProductDto> GetById(
    [FromRoute] int id,
    [FromQuery] bool includeArchived = false)
{
    // id = 42 (from route)
    // includeArchived = true (from query string)
    ...
}
```

### Binding Complex Types from the Body

When a client sends a POST or PUT with a JSON body, you bind the payload to a DTO using `[FromBody]`.

```csharp
// POST /api/products
// Body: { "name": "Widget", "price": 9.99 }
[HttpPost]
public ActionResult<ProductDto> Create([FromBody] CreateProductDto dto)
{
    // dto.Name = "Widget"
    // dto.Price = 9.99
    ...
}
```

ASP.NET Core uses `System.Text.Json` (or Newtonsoft.Json if configured) to deserialize the body into the target type. Only one `[FromBody]` parameter is allowed per action method, because the request body is a stream that can only be read once.

### Binding from Headers

```csharp
[HttpGet]
public IActionResult GetWithTracking(
    [FromHeader(Name = "X-Correlation-Id")] string? correlationId)
{
    // correlationId is null if the header is absent
    ...
}
```

### ModelState and Validation

After binding, ASP.NET Core runs model validation. Each bound parameter is checked against Data Annotation attributes on the DTO class. Results are recorded in `ModelState`.

```csharp
public class CreateProductDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10_000)]
    public decimal Price { get; set; }
}
```

Without `[ApiController]`, you must check `ModelState` manually:

```csharp
[HttpPost]
public ActionResult<ProductDto> Create([FromBody] CreateProductDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    ...
}
```

With `[ApiController]`, this check is performed automatically. If `ModelState` is invalid, the framework intercepts the request before your action method body even executes and returns a `400 Bad Request` with a structured `ValidationProblemDetails` body:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Price": ["The field Price must be between 0.01 and 10000."]
  }
}
```

### Binding Multiple Sources in One Action

Action methods can combine binding sources freely:

```csharp
// PUT /api/orders/7/items/3
// Header: X-Audit-User: jsmith
// Body: { "quantity": 5 }
[HttpPut("{orderId:int}/items/{itemId:int}")]
public IActionResult UpdateItem(
    [FromRoute] int orderId,
    [FromRoute] int itemId,
    [FromBody] UpdateItemDto dto,
    [FromHeader(Name = "X-Audit-User")] string? auditUser)
{
    ...
}
```

---

## Summary

- Model binding automatically maps request data to action parameters from route, query string, body, headers, or form data.
- Use the explicit attribute (`[FromRoute]`, `[FromQuery]`, `[FromBody]`, etc.) to remove ambiguity and improve readability.
- `[ApiController]` infers binding sources: complex types come from the body; route-segment names bind to route parameters; everything else is query string.
- Only one `[FromBody]` parameter is allowed per action because the request body stream can only be read once.
- `ModelState` accumulates validation errors from Data Annotations. With `[ApiController]`, an invalid `ModelState` automatically produces a structured `400 Bad Request` before your code runs.

---

## Additional Resources

- [Microsoft Docs — Model Binding in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
- [Microsoft Docs — Model validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [Microsoft Docs — Custom model binders](https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding)
