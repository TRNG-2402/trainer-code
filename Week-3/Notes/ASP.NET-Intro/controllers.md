# Controllers

## Learning Objectives

- Distinguish `ControllerBase` from `Controller` and know when to use each.
- Explain the role of the `[ApiController]` attribute and the behaviors it enables.
- Define action methods and understand the return types available (`IActionResult`, `ActionResult<T>`).
- Apply attribute routing to a controller and its action methods.

---

## Why This Matters

If the middleware pipeline is the hallway of an ASP.NET Core application, controllers are the rooms at the end of it. They are where HTTP requests are matched to your application's logic. Every route you expose, every status code you return, and every model validation response the framework produces originates in a controller or in the behaviors the `[ApiController]` attribute enables on top of it.

Getting the anatomy of a controller right from day one prevents a category of bugs that are peculiar to controller design: incorrect status codes, mis-routed requests, bypassed model validation, and serialization mismatches.

---

## The Concept

### `ControllerBase` vs. `Controller`

ASP.NET Core provides two base classes for controllers:

| Base Class | Usage | Includes View Support |
|---|---|---|
| `ControllerBase` | Web APIs (JSON responses only) | No |
| `Controller` | MVC applications (Razor views, HTML responses) | Yes |

For a Web API project, always inherit from `ControllerBase`. Inheriting from `Controller` adds Razor view helper methods (`View()`, `PartialView()`, etc.) that have no use in an API context.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Action methods go here
}
```

### The `[ApiController]` Attribute

When you apply `[ApiController]` to a controller class, ASP.NET Core enables a set of opinionated behaviors that are off by default:

| Behavior | What it does |
|---|---|
| **Automatic model validation** | If `ModelState` is invalid, the framework automatically returns a `400 Bad Request` with a `ValidationProblemDetails` body — without any explicit check in your action method. |
| **Binding source inference** | Complex types in action parameters are automatically inferred as `[FromBody]`. Simple types from the route are inferred as `[FromRoute]`. Query string parameters are inferred as `[FromQuery]`. |
| **Problem details for errors** | Error responses use the RFC 7807 `ProblemDetails` format by default. |
| **Attribute routing required** | Controllers decorated with `[ApiController]` must use attribute routing. Conventional routing is not applied. |

These behaviors follow the principle of convention over configuration — they encode best practices so you do not have to write the same boilerplate in every action method.

### Action Methods

An action method is a public method on a controller that gets invoked when a matching route is hit. Action methods can return several types:

#### `IActionResult`

The most flexible return type. Allows returning different result types from the same method.

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var product = _repo.FindById(id);

    if (product == null)
        return NotFound();          // 404

    return Ok(product);             // 200 with JSON body
}
```

#### `ActionResult<T>`

The preferred return type when the success case has a known type. It provides the flexibility of `IActionResult` while also enabling Swagger/OpenAPI to infer the response schema.

```csharp
[HttpGet("{id}")]
public ActionResult<ProductDto> GetById(int id)
{
    var product = _repo.FindById(id);

    if (product == null)
        return NotFound();          // IActionResult path

    return product;                 // Implicit 200 OK — T path
}
```

Notice that returning `product` directly (without wrapping it in `Ok()`) is valid because `ActionResult<T>` has an implicit conversion from `T`.

#### Common Result Helpers

`ControllerBase` provides factory methods for every common HTTP response:

| Method | Status Code | Common Use |
|---|---|---|
| `Ok(value)` | 200 | Successful GET or PUT |
| `Created(uri, value)` | 201 | Successful POST that creates a resource |
| `CreatedAtAction(...)` | 201 | 201 with a `Location` header pointing to the new resource |
| `NoContent()` | 204 | Successful DELETE or PUT with no response body |
| `BadRequest(...)` | 400 | Invalid input |
| `Unauthorized()` | 401 | Missing or invalid credentials |
| `Forbid()` | 403 | Authenticated but not authorized |
| `NotFound()` | 404 | Resource does not exist |
| `Conflict(...)` | 409 | State conflict (e.g., duplicate key) |
| `Problem(...)` | 500 | Unexpected server error |

### Attribute Routing

Controllers use attribute routing to bind HTTP verbs and URL templates to action methods.

```csharp
[ApiController]
[Route("api/[controller]")]         // expands to "api/products"
public class ProductsController : ControllerBase
{
    // GET api/products
    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll() { ... }

    // GET api/products/42
    [HttpGet("{id:int}")]
    public ActionResult<ProductDto> GetById(int id) { ... }

    // POST api/products
    [HttpPost]
    public ActionResult<ProductDto> Create([FromBody] CreateProductDto dto) { ... }

    // PUT api/products/42
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto) { ... }

    // DELETE api/products/42
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) { ... }
}
```

The `[controller]` token in `[Route("api/[controller]")]` is replaced at runtime with the controller's class name minus the "Controller" suffix (`ProductsController` becomes `products`).

Route constraints like `{id:int}` tell the routing engine to only match this route when the `id` segment can be parsed as an integer, preventing unexpected matches. You will explore routing constraints in depth in `routing.md`.

---

## Code Example

A complete, minimal controller demonstrating common patterns:

```csharp
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
        var products = _service.GetAll();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public ActionResult<ProductDto> GetById(int id)
    {
        var product = _service.GetById(id);
        if (product is null)
            return NotFound();

        return product;
    }

    [HttpPost]
    public ActionResult<ProductDto> Create([FromBody] CreateProductDto dto)
    {
        // ModelState validation is automatic via [ApiController]
        var created = _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto)
    {
        var success = _service.Update(id, dto);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var success = _service.Delete(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
```

Notice:
- The constructor receives `IProductService` via dependency injection — you will cover DI registration on Tuesday.
- `CreatedAtAction` generates a `Location` response header pointing to `GetById` for the newly created resource.
- `NoContent()` signals a successful operation with no body, which is the correct response for PUT and DELETE.

---

## Summary

- Inherit from `ControllerBase` for Web API controllers; `Controller` is only needed for MVC/Razor applications.
- `[ApiController]` enables automatic model validation, binding source inference, and RFC 7807 error responses.
- Use `ActionResult<T>` as the return type when the success response has a known type — it supports both typed and `IActionResult`-based return paths.
- Attribute routing (`[HttpGet]`, `[HttpPost]`, etc.) maps HTTP verbs and URL templates to specific action methods.
- Use the correct status code helpers: `Ok`, `Created`/`CreatedAtAction`, `NoContent`, `NotFound`, `BadRequest`, `Conflict`.

---

## Additional Resources

- [Microsoft Docs — Controllers in ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Microsoft Docs — Action return types in ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types)
- [Microsoft Docs — Handle errors in ASP.NET Core Web APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors)
