# Data Annotations in ASP.NET Core

## Learning Objectives

- Apply the most common built-in validation attributes to DTO properties.
- Explain how `ModelState` accumulates validation errors and how `[ApiController]` automates validation responses.
- Check `ModelState.IsValid` manually in controllers that do not use `[ApiController]`.
- Create a custom validation attribute by inheriting from `ValidationAttribute`.
- Implement `IValidatableObject` for cross-property validation logic.

---

## Why This Matters

Request DTOs define the shape of what the client sends. Data annotations enforce the integrity of that data -- ensuring a `Name` field is not empty, a `Price` is within a reasonable range, and an email address at least resembles valid syntax. This enforcement happens before your action method executes, meaning no invalid data reaches your service or data layer.

Centralized, declarative validation keeps action methods clean. Instead of guarding every entry point with `if (string.IsNullOrEmpty(dto.Name))` blocks, you express constraints as attributes on the DTO itself. The framework reads those attributes and enforces them automatically.

---

## The Concept

### Built-in Validation Attributes

ASP.NET Core inherits the `System.ComponentModel.DataAnnotations` namespace from the broader .NET ecosystem. These attributes apply to model properties and are evaluated by the model binder during request processing.

#### `[Required]`

The annotated property must be present and non-null in the request body. For strings, also non-empty unless `AllowEmptyStrings = true` is set.

```csharp
[Required]
public string Name { get; set; } = string.Empty;
```

#### `[StringLength]`

Validates minimum and maximum string length.

```csharp
[Required]
[StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
public string Name { get; set; } = string.Empty;
```

#### `[Range]`

Validates that a numeric value falls within an inclusive range.

```csharp
[Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99.")]
public decimal Price { get; set; }
```

#### `[RegularExpression]`

Validates that the value matches a given regular expression pattern.

```csharp
[RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "PostalCode must be a valid US ZIP code.")]
public string PostalCode { get; set; } = string.Empty;
```

#### `[EmailAddress]`

A shorthand that validates the value is in a plausible email format.

```csharp
[EmailAddress]
public string ContactEmail { get; set; } = string.Empty;
```

#### `[Url]`

Validates that the value resembles a well-formed URL.

```csharp
[Url]
public string? ImageUrl { get; set; }
```

#### `[Compare]`

Validates that the annotated property's value matches another named property's value. Useful for password confirmation fields.

```csharp
public string Password { get; set; } = string.Empty;

[Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
public string ConfirmPassword { get; set; } = string.Empty;
```

#### `[MaxLength]` and `[MinLength]`

Similar to `[StringLength]` but also applicable to arrays and collections.

```csharp
[MaxLength(50)]
public string? Description { get; set; }
```

### How `ModelState` Works

When ASP.NET Core binds a request to a model, it evaluates all validation attributes and records any failures in `ModelState` -- an `ModelStateDictionary` attached to the current `ControllerContext`. Each entry identifies the property name and the validation error message.

#### Checking `ModelState.IsValid` manually

In controllers that do not have `[ApiController]`, you must check `ModelState.IsValid` explicitly:

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateProductDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState); // Returns 400 with validation errors
    }

    var result = _service.Create(dto);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

#### Automatic validation with `[ApiController]`

When `[ApiController]` is applied to a controller, this manual check is unnecessary. The framework intercepts model binding, evaluates `ModelState`, and automatically returns a `400 Bad Request` with a `ValidationProblemDetails` body if validation fails -- before your action method is called.

```json
// Automatic 400 response when Name is missing:
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

This is one of the most valuable behaviors `[ApiController]` enables. You saw this listed in Monday's `controllers.md`; now you understand the mechanism behind it.

### Custom Validation Attributes

When the built-in attributes do not cover a requirement (for example, "the SKU must start with a category prefix"), you can write a custom attribute by inheriting from `ValidationAttribute` and overriding `IsValid`.

```csharp
public class SkuFormatAttribute : ValidationAttribute
{
    private readonly string _prefix;

    public SkuFormatAttribute(string prefix)
    {
        _prefix = prefix;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string sku && sku.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            $"SKU must start with '{_prefix}'.",
            new[] { validationContext.MemberName! });
    }
}

// Usage on a DTO property:
[SkuFormat("PROD-")]
public string Sku { get; set; } = string.Empty;
```

The custom attribute integrates seamlessly with `ModelState` and `[ApiController]`'s automatic validation.

### `IValidatableObject` for Cross-Property Validation

Attribute-based validation works on single properties. When a validation rule involves multiple properties (for example, "DiscountPrice must be less than Price"), implement `IValidatableObject` on the DTO.

```csharp
public class CreateProductDto : IValidatableObject
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 99999.99)]
    public decimal Price { get; set; }

    [Range(0, 99999.99)]
    public decimal? DiscountPrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DiscountPrice.HasValue && DiscountPrice >= Price)
        {
            yield return new ValidationResult(
                "DiscountPrice must be less than Price.",
                new[] { nameof(DiscountPrice) });
        }
    }
}
```

`IValidatableObject.Validate` is called only after all attribute-based validations have passed. If any attribute-level validation fails, `Validate` is not invoked -- the framework fails fast with the simpler errors first.

---

## Code Example

A complete request DTO combining multiple validation attributes with a cross-property `IValidatableObject` implementation, plus the controller that relies on `[ApiController]` for automatic enforcement:

```csharp
// CreateProductDto.cs
public class CreateProductDto : IValidatableObject
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [SkuFormat("PROD-")]
    public string Sku { get; set; } = string.Empty;

    [Range(0.01, 99999.99)]
    public decimal Price { get; set; }

    [Range(0, 99999.99)]
    public decimal? DiscountPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "StockQuantity cannot be negative.")]
    public int StockQuantity { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DiscountPrice.HasValue && DiscountPrice >= Price)
        {
            yield return new ValidationResult(
                "DiscountPrice must be strictly less than Price.",
                new[] { nameof(DiscountPrice) });
        }
    }
}

// ProductsController.cs (with [ApiController] -- no manual ModelState check needed)
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpPost]
    public ActionResult<ProductResponseDto> Create([FromBody] CreateProductDto dto)
    {
        // If we reach this line, dto has already been validated.
        // [ApiController] returned 400 automatically if validation failed.
        var result = _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

To test validation, POST a body with `Price: 10` and `DiscountPrice: 15`. The `[ApiController]` mechanism will return a `400` with a `ValidationProblemDetails` body noting that `DiscountPrice` must be less than `Price`, without the action method ever executing.

---

## Summary

- Data annotations are attributes from `System.ComponentModel.DataAnnotations` applied to model properties to declare validation rules declaratively.
- Common built-in attributes: `[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`, `[EmailAddress]`, `[Compare]`.
- When `[ApiController]` is applied, invalid `ModelState` automatically returns a `400 Bad Request` with an RFC 7807 `ValidationProblemDetails` body before the action method executes.
- Without `[ApiController]`, check `ModelState.IsValid` manually at the start of action methods.
- Extend validation logic with custom `ValidationAttribute` subclasses for single-property rules and `IValidatableObject` for cross-property rules.

---

## Additional Resources

- [Microsoft Docs -- Model validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [Microsoft Docs -- System.ComponentModel.DataAnnotations namespace](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [Microsoft Docs -- Create custom validation attributes](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation#custom-attributes)
