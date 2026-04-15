# Entity Configuration

## Learning Objectives

- Define an EF Core entity and understand how conventions determine its default mapping.
- Apply Data Annotations to override default mapping behavior.
- Understand the key annotations used in everyday EF Core work: `[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`, and others.
- Distinguish between convention-based, annotation-based, and Fluent API configuration (with Fluent API reserved for Thursday).

---

## Why This Matters

EF Core needs to know how your C# classes map to database tables and columns. It uses a layered configuration system to determine that mapping. Getting entity configuration right is the foundation of a correct, maintainable schema — misconfigurations here cascade into data integrity issues, incorrect migrations, and unexpected query behavior.

---

## What Is an Entity?

An **entity** is any C# class that EF Core maps to a database table. By convention, EF Core discovers entity types by looking at `DbSet<T>` properties on your `DbContext`.

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }    // Product is an entity
    public DbSet<Category> Categories { get; set; } // Category is an entity
}
```

EF Core creates one table per entity type (by default named after the `DbSet` property).

---

## Convention-Based Configuration

EF Core applies a set of **conventions** — default rules that infer mapping without any explicit configuration. Knowing the conventions lets you write minimal configuration code.

### Key Conventions

| Convention | Rule |
|---|---|
| Primary Key | A property named `Id` or `{ClassName}Id` becomes the primary key |
| Column Name | Property name maps directly to column name |
| Table Name | `DbSet` property name becomes the table name |
| String columns | `nvarchar(max)` on SQL Server by default (nullable) |
| int primary key | Auto-generated (identity column) by default |
| Nullable reference types | If `string?`, the column is nullable; `string` (non-nullable) requires a value |

```csharp
public class Product
{
    public int Id { get; set; }          // Primary key by convention (int, named "Id")
    public string Name { get; set; }     // nvarchar(max), NOT NULL (non-nullable ref type)
    public decimal Price { get; set; }   // decimal column
}
```

This class requires zero annotations — EF Core maps it correctly by convention alone.

---

## Data Annotations

When conventions are insufficient or when you need to be explicit about constraints, **Data Annotations** let you decorate entity properties with attributes from `System.ComponentModel.DataAnnotations` and `System.ComponentModel.DataAnnotations.Schema`.

### [Key]

Marks a property as the primary key. Use this when your key property does not follow the `Id` / `{ClassName}Id` naming convention.

```csharp
public class Product
{
    [Key]
    public int ProductCode { get; set; }
    public string Name { get; set; }
}
```

For composite keys (multiple columns forming the primary key together), use Fluent API — Data Annotations do not support composite keys directly.

---

### [Required]

Makes a column `NOT NULL` in the database. Also participates in .NET model validation if you use ASP.NET Core validation.

```csharp
public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
}
```

Note: With C# nullable reference types enabled (the default in modern projects), a non-nullable `string` property is already treated as required by EF Core. `[Required]` is still useful for value types being used as optional fields or for explicit documentation intent.

---

### [MaxLength] and [StringLength]

Constrains the maximum length of a string column. Both work similarly in EF Core.

```csharp
public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }
}
```

`[MaxLength(200)]` generates `nvarchar(200)` on SQL Server instead of `nvarchar(max)`. This is important for columns that will be indexed — SQL Server limits index keys to 900 bytes.

---

### [Column]

Maps a property to a specific column name or data type, overriding the convention.

```csharp
public class Product
{
    public int Id { get; set; }

    [Column("product_name")]
    public string Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}
```

`decimal` columns require explicit precision and scale — EF Core warns if you leave them unspecified, as the default may differ per provider.

---

### [Table]

Maps an entity to a specific table name, overriding the `DbSet` property name convention.

```csharp
[Table("tbl_products", Schema = "catalog")]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

---

### [ForeignKey]

Explicitly specifies which property is the foreign key for a navigation property. EF Core usually infers this by convention, but `[ForeignKey]` removes ambiguity when you have multiple relationships or unconventional naming.

```csharp
public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }          // Foreign key property

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }        // Navigation property
}
```

Alternatively, you can place `[ForeignKey]` on the navigation property and pass the name of the key property, or place it on the key property and pass the name of the navigation property.

---

### [NotMapped]

Excludes a property from the database schema entirely. Useful for computed properties or transient values.

```csharp
public class Order
{
    public int Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }

    [NotMapped]
    public decimal Total => Subtotal * (1 + TaxRate);  // Not stored in DB
}
```

---

### [Index]

Adds a database index on one or more columns. Placed at the class level (not property level) in EF Core 5+.

```csharp
[Index(nameof(Email), IsUnique = true)]
public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
```

---

## Conventions vs. Annotations vs. Fluent API

EF Core supports three layered ways to configure entities. They apply in this order — later layers override earlier ones:

| Layer | Where Defined | Best For |
|---|---|---|
| Conventions | Implicit (EF Core defaults) | Standard naming, simple schemas |
| Data Annotations | On the entity class | Property-level constraints, readable inline config |
| Fluent API | In `OnModelCreating` | Complex relationships, composite keys, table splitting, full control |

You will work with Fluent API in depth on Thursday. For today, Data Annotations give you enough control for the exercise.

---

## A Fully Annotated Entity Example

```csharp
[Table("products", Schema = "catalog")]
[Index(nameof(Sku), IsUnique = true)]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(50)]
    [Column("sku")]
    public string Sku { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    // Foreign key
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }

    [NotMapped]
    public bool IsOnSale => Price < 20.00m;
}
```

---

## Summary

- EF Core discovers entities via `DbSet<T>` properties and maps them using **conventions** by default.
- **Data Annotations** override conventions at the property or class level using attributes.
- Key annotations: `[Key]`, `[Required]`, `[MaxLength]`, `[Column]`, `[Table]`, `[ForeignKey]`, `[NotMapped]`, `[Index]`.
- Always specify `decimal` precision with `[Column(TypeName = "decimal(18,2)")]` to avoid warnings.
- **Fluent API** is the most powerful configuration layer and will be covered on Thursday.

---

## Additional Resources

- [Microsoft Docs — Entity Properties](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties)
- [Microsoft Docs — Data Annotations](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types#using-data-annotations)
- [Microsoft Docs — Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
