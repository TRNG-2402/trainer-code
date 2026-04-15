# Fluent API

## Learning Objectives

- Understand the role of Fluent API in EF Core configuration and how it relates to conventions and Data Annotations.
- Use `ModelBuilder` in `OnModelCreating` to configure entities, properties, and relationships.
- Apply key `ModelBuilder` methods: `HasKey`, `HasMany`/`WithOne`, `HasOne`/`WithMany`, `HasMany`/`WithMany`, and `Property` configuration.
- Know when Fluent API overrides Data Annotations and which layer takes precedence.

---

## Why This Matters

Data Annotations are convenient for simple cases, but they have limits: they cannot configure composite primary keys, they mix persistence concerns into your domain model, and they cannot express all the relationship configurations EF Core supports. Fluent API is the configuration layer that has no such limits. Enterprise EF Core projects commonly use Fluent API exclusively and keep entity classes free of attributes — making the domain model clean and the persistence logic centralized in configuration classes.

---

## The Three Configuration Layers (Revisited)

EF Core applies configuration in this order, with later layers overriding earlier ones:

| Layer | Where | Limitations |
|---|---|---|
| Conventions | Implicit (EF Core defaults) | Cannot express all cases |
| Data Annotations | Attribute on entity class | No composite keys, mixes concerns |
| Fluent API | `OnModelCreating` on `DbContext` | No meaningful limitations |

Fluent API is the most powerful layer. Anything you can express with Data Annotations, you can also express with Fluent API — and many things you cannot.

---

## Where Fluent API Lives

All Fluent API configuration goes inside `OnModelCreating`, which you override on your `DbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // All configuration goes here
    modelBuilder.Entity<Product>(entity =>
    {
        entity.ToTable("products");
        entity.HasKey(p => p.Id);
        entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
        entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
    });
}
```

For larger projects, you split configuration into separate classes that implement `IEntityTypeConfiguration<T>` (shown below).

---

## Key Fluent API Methods

### HasKey — Primary Key Configuration

```csharp
// Simple primary key
modelBuilder.Entity<Product>()
    .HasKey(p => p.Id);

// Composite primary key (only possible via Fluent API — not Data Annotations)
modelBuilder.Entity<Enrollment>()
    .HasKey(e => new { e.StudentId, e.CourseId });
```

---

### Property Configuration

```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.Name)
    .IsRequired()
    .HasMaxLength(200)
    .HasColumnName("product_name");

modelBuilder.Entity<Product>()
    .Property(p => p.Price)
    .HasColumnType("decimal(18,2)")
    .HasDefaultValue(0.00m);

modelBuilder.Entity<Product>()
    .Property(p => p.CreatedAt)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");
```

`HasColumnType`, `HasDefaultValue`, and `HasDefaultValueSql` are not available via Data Annotations — Fluent API only.

---

### Table and Schema Configuration

```csharp
modelBuilder.Entity<Product>()
    .ToTable("products", schema: "catalog");
```

---

### Indexes

```csharp
modelBuilder.Entity<Product>()
    .HasIndex(p => p.Sku)
    .IsUnique();

// Composite index
modelBuilder.Entity<Order>()
    .HasIndex(o => new { o.CustomerId, o.OrderDate });
```

---

### One-to-Many Relationship

```csharp
modelBuilder.Entity<Product>()
    .HasOne(p => p.Category)            // Product has one Category
    .WithMany(c => c.Products)          // Category has many Products
    .HasForeignKey(p => p.CategoryId)   // Foreign key property
    .OnDelete(DeleteBehavior.Restrict); // Override cascade delete default
```

The delete behavior options:

| Option | Effect |
|---|---|
| `Cascade` | Delete dependents when principal is deleted |
| `Restrict` | Prevent deletion if dependents exist |
| `SetNull` | Set FK to NULL on principal delete (FK must be nullable) |
| `NoAction` | No action taken at DB level |

---

### One-to-One Relationship

```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.Profile)
    .WithOne(p => p.User)
    .HasForeignKey<UserProfile>(p => p.UserId);
```

Note: In a one-to-one, `HasForeignKey<T>` takes a type argument specifying which entity is the dependent (holds the foreign key).

---

### Many-to-Many with Explicit Join Entity

```csharp
modelBuilder.Entity<Enrollment>()
    .HasKey(e => new { e.StudentId, e.CourseId });

modelBuilder.Entity<Enrollment>()
    .HasOne(e => e.Student)
    .WithMany(s => s.Enrollments)
    .HasForeignKey(e => e.StudentId);

modelBuilder.Entity<Enrollment>()
    .HasOne(e => e.Course)
    .WithMany(c => c.Enrollments)
    .HasForeignKey(e => e.CourseId);
```

---

## IEntityTypeConfiguration\<T\> — Splitting Configuration Into Classes

When all entity configurations live in `OnModelCreating`, the method grows unwieldy. The recommended pattern is to split each entity's configuration into its own class:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
```

Then apply all configurations in `OnModelCreating` using `ApplyConfigurationsFromAssembly`, which scans the assembly for all `IEntityTypeConfiguration<T>` implementations automatically:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

This pattern scales to any size schema without touching `OnModelCreating` again.

---

## Fluent API vs. Data Annotations: When Each Wins

| Configuration Need | Data Annotations | Fluent API |
|---|---|---|
| Primary key (simple) | `[Key]` | `HasKey(...)` |
| Composite primary key | Not supported | `HasKey(e => new { ... })` |
| Required / NOT NULL | `[Required]` | `IsRequired()` |
| Max length | `[MaxLength(n)]` | `HasMaxLength(n)` |
| Column name | `[Column("name")]` | `HasColumnName("name")` |
| Column type | `[Column(TypeName = "...")]` | `HasColumnType("...")` |
| Table / schema | `[Table("name", Schema = "s")]` | `ToTable("name", "s")` |
| Default value | Not supported | `HasDefaultValue(...)` |
| Default SQL expression | Not supported | `HasDefaultValueSql("...")` |
| Relationship config | `[ForeignKey]` (basic) | `HasOne/WithMany/HasForeignKey` (full) |
| Delete behavior | Not supported | `OnDelete(DeleteBehavior.X)` |
| Index | `[Index]` (class attribute) | `HasIndex(...)` with full options |
| Clean domain model | Pollutes with attributes | No attributes on entity class |

**Rule of thumb used by many enterprise teams:** Use Fluent API for all relationship configuration and anything not expressible via annotations. Use Data Annotations only for simple property-level constraints (`[Required]`, `[MaxLength]`) when keeping configuration close to the property is a team preference.

---

## Summary

- **Fluent API** is the most powerful EF Core configuration layer, configured in `OnModelCreating` via `ModelBuilder`.
- It overrides conventions and Data Annotations — the last configuration wins.
- Key operations: `HasKey`, `Property` (with `IsRequired`, `HasMaxLength`, `HasColumnType`, `HasDefaultValue`), `ToTable`, `HasIndex`, and relationship methods (`HasOne`/`HasMany` with `WithOne`/`WithMany` and `HasForeignKey`).
- `OnDelete(DeleteBehavior.X)` is Fluent API only — critical for controlling cascade delete.
- Split configuration into `IEntityTypeConfiguration<T>` classes and load them with `ApplyConfigurationsFromAssembly` for maintainability at scale.

---

## Additional Resources

- [Microsoft Docs — Fluent API Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [Microsoft Docs — Relationships via Fluent API](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many)
- [Microsoft Docs — IEntityTypeConfiguration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.ientitytypeconfiguration-1)
