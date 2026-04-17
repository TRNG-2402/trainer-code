# Data Transfer Objects

## Learning Objectives

- Define what a Data Transfer Object (DTO) is and explain why it exists as a pattern.
- Distinguish between domain models and API contracts and explain why they should not be the same type.
- Differentiate between request DTOs and response DTOs.
- Implement manual mapping between domain models and DTOs.
- Explain the over-posting vulnerability and how DTOs prevent it.

---

## Why This Matters

When Monday's `controllers.md` showed a controller returning a `Product` entity directly from an action method, that was a simplification made deliberately so routing and action methods could be the focus. In a real API, returning your domain model directly creates problems that surface quickly in production:

- A change to your data model (adding a column, renaming a field) breaks your API contract for every client consuming it.
- Navigation properties from EF Core entities can cause circular reference serialization errors.
- Sensitive fields (password hashes, internal flags, audit columns) are exposed directly to API consumers.
- Clients can post arbitrary fields that map to your entity, letting them overwrite columns they should not have access to (the over-posting vulnerability).

DTOs resolve all of these problems by creating a clear boundary between your internal data model and your external API contract.

---

## The Concept

### What a DTO Is

A Data Transfer Object is a plain class whose sole purpose is to carry data across a boundary -- in this case, between your application's internal logic and the HTTP API surface. DTOs have no behavior (no methods, no business logic), only properties.

```csharp
// Domain model -- lives in the data layer, EF Core maps this to a table
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }   // Soft-delete column -- internal concern
    public int SupplierId { get; set; }         // Foreign key -- internal concern
    public Supplier Supplier { get; set; } = null!; // Navigation property
}
```

If you return `Product` directly from a controller, every caller sees `DeletedAt`, `SupplierId`, and the `Supplier` navigation property. The serializer may also throw when it attempts to traverse circular EF Core navigation chains.

### Request DTOs vs. Response DTOs

There are two directions data travels in an API:

**Request DTOs** model what the client sends to the API. They define only the fields the API accepts for a given operation.

**Response DTOs** model what the API returns to the client. They define only the fields the client needs to see.

A single domain model typically has multiple DTOs -- one per operation:

| DTO | Direction | Contains |
|---|---|---|
| `CreateProductDto` | Client -> API | Fields required to create a product |
| `UpdateProductDto` | Client -> API | Fields the client can modify |
| `ProductResponseDto` | API -> Client | Fields returned to the caller |
| `ProductSummaryDto` | API -> Client | Condensed view for list endpoints |

Separating these shapes gives each operation an independent contract. Adding an internal field to `Product` does not change any DTO. Changing a DTO does not ripple into the database schema.

```csharp
// Request DTO: what the client provides on POST
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InitialStock { get; set; }
}

// Request DTO: what the client provides on PUT
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Response DTO: what the API returns (no internal columns)
public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

### The Over-Posting Vulnerability

Over-posting (also called mass assignment) is a class of vulnerability where a client sends extra fields in a request body and those fields are bound directly to a domain model that is then persisted.

Consider what happens if a `Product` has an `IsAdmin` flag and you bind the request body directly to `Product`:

```csharp
// VULNERABLE: client can POST { "name": "Widget", "isAdmin": true }
[HttpPost]
public IActionResult Create([FromBody] Product product) // Direct entity
{
    _db.Products.Add(product);
    _db.SaveChanges();
    return Ok(product);
}
```

A `CreateProductDto` that does not include `IsAdmin` closes this attack surface completely. The client can post the field, but the DTO does not have a property to receive it, so the framework discards it silently.

```csharp
// SAFE: only Name, Price, and InitialStock can be set by the client
[HttpPost]
public IActionResult Create([FromBody] CreateProductDto dto)
{
    var product = MapToProduct(dto);
    _db.Products.Add(product);
    _db.SaveChanges();
    return Ok(MapToResponse(product));
}
```

### Manual Mapping

The most transparent mapping strategy is to write the mapping logic explicitly. While it is more verbose than using a library, it has zero hidden complexity and is immediately readable.

```csharp
// Mapping from request DTO to domain model (on create)
private static Product MapToProduct(CreateProductDto dto)
{
    return new Product
    {
        Name = dto.Name,
        Price = dto.Price,
        StockQuantity = dto.InitialStock,
        CreatedAt = DateTime.UtcNow
    };
}

// Mapping from domain model to response DTO
private static ProductResponseDto MapToResponse(Product product)
{
    return new ProductResponseDto
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        StockQuantity = product.StockQuantity
    };
}
```

In practice, mapping methods grow repetitive across a large service. Libraries such as **AutoMapper** automate this work through convention-based property name matching. We will reference AutoMapper briefly, but the mapping logic you implement in this week's exercises will remain explicit so the mechanics are clear.

### AutoMapper (Brief Mention)

AutoMapper is the most widely used .NET mapping library. It uses reflection to match property names between source and destination types, drastically reducing boilerplate.

```csharp
// AutoMapper configuration (Program.cs)
builder.Services.AddAutoMapper(typeof(Program));

// Mapping profile
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<CreateProductDto, Product>();
        CreateMap<Product, ProductResponseDto>();
    }
}

// Controller usage
public class ProductsController : ControllerBase
{
    private readonly IMapper _mapper;

    public ProductsController(IMapper mapper) { _mapper = mapper; }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        // ...
        return Ok(_mapper.Map<ProductResponseDto>(product));
    }
}
```

AutoMapper is powerful and appropriate for large services, but its implicit nature means mapping bugs (mismatched property names, missing maps) can surface at runtime rather than compile time. Explicit mapping gives you compile-time guarantees at the cost of verbosity.

---

## Code Example

A controller that correctly uses request and response DTOs with explicit mapping, demonstrating all patterns covered above:

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

    // GET api/products
    [HttpGet]
    public ActionResult<IEnumerable<ProductResponseDto>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    // GET api/products/5
    [HttpGet("{id:int}")]
    public ActionResult<ProductResponseDto> GetById(int id)
    {
        var product = _service.GetById(id);
        if (product is null) return NotFound();
        return product;
    }

    // POST api/products
    [HttpPost]
    public ActionResult<ProductResponseDto> Create([FromBody] CreateProductDto dto)
    {
        var result = _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT api/products/5
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto)
    {
        var success = _service.Update(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    // DELETE api/products/5
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var success = _service.Delete(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
```

Note: the controller returns `ProductResponseDto` and `IEnumerable<ProductResponseDto>` -- never the raw `Product` entity. The service layer handles mapping. The controller is unaware of the domain model's internal structure.

---

## Summary

- A DTO is a plain class whose sole purpose is carrying data across the API boundary. It has no business logic.
- Separate request DTOs from response DTOs. Each operation gets its own shape that matches only the data relevant to that operation.
- Returning domain models directly exposes internal structure to callers and risks over-posting vulnerabilities.
- Manual mapping is explicit and compile-time safe; AutoMapper reduces boilerplate at the cost of runtime discoverability.
- The controller layer should work exclusively with DTOs; domain model awareness belongs in the service and repository layers.

---

## Additional Resources

- [Microsoft Docs -- Create Data Transfer Objects (DTOs) in ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api#prevent-over-posting)
- [AutoMapper Documentation](https://docs.automapper.org/en/stable/)
- [Microsoft Docs -- Model validation in ASP.NET Core MVC and Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
