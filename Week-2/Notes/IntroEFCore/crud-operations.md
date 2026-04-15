# CRUD Operations

## Learning Objectives

- Use EF Core's `DbSet<T>` and `DbContext` methods to perform Create, Read, Update, and Delete operations.
- Understand the difference between tracked and untracked entities as it applies to updates.
- Know when `SaveChanges()` must be called and what it does.
- Apply `Find`, `FirstOrDefault`, and LINQ queries for reading data.

---

## Why This Matters

Every data-driven application is built on four fundamental operations: Create, Read, Update, Delete. EF Core provides a clean, object-oriented API for all four. Understanding the nuances — particularly around change tracking for updates and deletes — prevents subtle bugs like partial updates, phantom inserts, or unintentional deletes.

---

## The SaveChanges Contract

Before looking at individual operations, it is important to establish one rule: **EF Core does not hit the database until you call `SaveChanges()`** (or `SaveChangesAsync()`).

Operations like `Add()`, `Update()`, and `Remove()` only change the state of entities in the Change Tracker. `SaveChanges()` is the commit — it inspects the Change Tracker, generates the appropriate SQL, and executes it in a single database transaction.

```csharp
context.Add(entity);       // No DB call yet — entity state set to Added
context.SaveChanges();     // DB call happens here — INSERT executed
```

---

## Create

### Adding a Single Entity

```csharp
using var context = new AppDbContext();

var product = new Product
{
    Name       = "Mechanical Keyboard",
    Price      = 129.99m,
    CategoryId = 1
};

context.Products.Add(product);
context.SaveChanges();

// After SaveChanges(), product.Id is populated with the DB-generated primary key
Console.WriteLine(product.Id);
```

`Add()` marks the entity as `Added`. On `SaveChanges()`, EF Core generates an `INSERT` statement. For auto-increment primary keys, the generated key value is written back into the entity object.

### Adding Multiple Entities

```csharp
var products = new List<Product>
{
    new Product { Name = "Mouse", Price = 39.99m, CategoryId = 1 },
    new Product { Name = "Monitor", Price = 299.99m, CategoryId = 1 }
};

context.Products.AddRange(products);
context.SaveChanges();  // Single DB round-trip for both inserts
```

`AddRange()` is more efficient than calling `Add()` in a loop — EF Core can batch the inserts.

---

## Read

### Find by Primary Key

```csharp
var product = context.Products.Find(1);
// Returns Product with Id = 1, or null if not found
```

`Find()` checks the Change Tracker first before hitting the database. If the entity is already loaded in the current context session, it returns the in-memory instance. This makes `Find()` efficient for repeated lookups within the same session.

### FirstOrDefault with a Filter

```csharp
var product = context.Products
    .FirstOrDefault(p => p.Name == "Mouse");
// Returns first match, or null
```

Unlike `Find()`, `FirstOrDefault()` always queries the database (unless the full result set has already been loaded). It supports arbitrary LINQ predicates.

### Querying a List

```csharp
var affordableProducts = context.Products
    .Where(p => p.Price < 50)
    .OrderBy(p => p.Name)
    .ToList();
```

The query is built as an expression tree and translated to SQL. Only `ToList()` (or another terminal operator) triggers execution.

### Read-Only Query (No Tracking)

```csharp
var products = context.Products
    .AsNoTracking()
    .ToList();
```

For purely read-only operations, `AsNoTracking()` skips change tracking entirely — EF Core does not store snapshots for the returned entities. This is faster and uses less memory. Use it whenever you know you will not be updating the results.

---

## Update

The correct update approach depends on whether you have a **tracked** entity (loaded in the current context session) or an **untracked** entity (created outside the context, e.g., from a DTO mapping).

### Updating a Tracked Entity (Most Common)

```csharp
using var context = new AppDbContext();

var product = context.Products.Find(1);  // Entity is now tracked (Unchanged)
product.Price = 99.99m;                  // Change Tracker detects modification (Modified)
context.SaveChanges();                   // Generates: UPDATE Products SET Price = 99.99 WHERE Id = 1
```

This is the preferred pattern. EF Core only generates an `UPDATE` for the columns that actually changed.

### Updating an Untracked Entity

If you have an entity that was created or modified outside of a DbContext session (common in web APIs where entities are deserialized from a request body), you need to attach it and mark it as modified:

```csharp
using var context = new AppDbContext();

// Entity came from outside the context (e.g., API request body)
var product = new Product { Id = 1, Name = "Updated Mouse", Price = 44.99m, CategoryId = 1 };

context.Products.Update(product);  // Marks all properties as Modified
context.SaveChanges();             // Generates UPDATE for all columns — even ones you didn't change
```

**Caution:** `context.Update()` on an untracked entity marks *all* properties as modified, which generates an `UPDATE` that overwrites every column. This can cause data loss if the entity object is not fully populated. In production applications, prefer loading the entity first (tracked update) or using Fluent API partial updates.

### Checking Entity State (Diagnostic)

```csharp
var state = context.Entry(product).State;
Console.WriteLine(state);  // Unchanged, Modified, Added, Deleted, Detached
```

---

## Delete

### Deleting a Tracked Entity

```csharp
using var context = new AppDbContext();

var product = context.Products.Find(1);  // Load and track
context.Products.Remove(product);        // Mark as Deleted
context.SaveChanges();                   // Generates: DELETE FROM Products WHERE Id = 1
```

### Deleting Without Loading the Entity

If you know the primary key and want to avoid a round-trip query, you can create a stub entity and attach it:

```csharp
using var context = new AppDbContext();

var productStub = new Product { Id = 1 };
context.Products.Remove(productStub);   // Attach stub directly as Deleted
context.SaveChanges();
```

This works as long as the entity is not already tracked in the context. It avoids the initial SELECT query.

---

## Async Variants

All EF Core operations that hit the database have async equivalents. Prefer the async versions in ASP.NET Core applications to avoid blocking threads:

```csharp
// Async read
var products = await context.Products.ToListAsync();
var product  = await context.Products.FindAsync(id);
var first    = await context.Products.FirstOrDefaultAsync(p => p.Price < 50);

// Async save
await context.SaveChangesAsync();
```

Async variants require `using Microsoft.EntityFrameworkCore;` for extension methods like `ToListAsync()` and `FirstOrDefaultAsync()`.

---

## CRUD at a Glance

| Operation | Method | DB Triggered By |
|---|---|---|
| Create | `Add()` / `AddRange()` | `SaveChanges()` |
| Read (by key) | `Find(id)` | Query (or Change Tracker hit) |
| Read (filter) | `Where(...).ToList()` | `ToList()` / terminal operator |
| Update (tracked) | Modify property directly | `SaveChanges()` |
| Update (untracked) | `Update(entity)` | `SaveChanges()` |
| Delete | `Remove(entity)` | `SaveChanges()` |

---

## Summary

- **`SaveChanges()`** is the single commit point — all pending operations are sent to the database in one transaction.
- **`Add()` / `AddRange()`** mark entities as `Added`; the primary key is populated after save.
- **`Find()`** checks the Change Tracker before querying; use **`FirstOrDefault()`** or **`Where()`** for arbitrary filter queries.
- **Tracked updates** (load then modify) are precise — only changed columns are updated.
- **Untracked updates** (`context.Update()`) overwrite all columns — use with care.
- **`Remove()`** marks the entity as `Deleted`; a stub entity (PK only) can avoid a prior SELECT.
- Use **`AsNoTracking()`** for read-only queries.
- Use **async methods** (`ToListAsync`, `SaveChangesAsync`, etc.) in web application contexts.

---

## Additional Resources

- [Microsoft Docs — Saving Data in EF Core](https://learn.microsoft.com/en-us/ef/core/saving/)
- [Microsoft Docs — Querying Data in EF Core](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Microsoft Docs — Change Tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
