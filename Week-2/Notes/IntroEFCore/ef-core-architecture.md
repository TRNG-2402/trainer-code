# EF Core Architecture

## Learning Objectives

- Describe the primary architectural components of EF Core: `DbContext`, `DbSet<T>`, the Change Tracker, and the provider model.
- Explain how those components interact during a typical query and save operation.
- Understand what the provider model abstracts and why it matters for portability.

---

## Why This Matters

Before writing production code with any library, understanding its architecture prevents you from treating it as a black box. When something unexpected happens — a query returns stale data, a `SaveChanges()` call throws a concurrency exception, or performance degrades — you need a mental model of what EF Core is doing internally. This module gives you that model.

---

## The Four Core Components

### 1. DbContext

`DbContext` is the central class in EF Core. It represents a **session with the database** and is the entry point for all EF Core operations: querying, inserting, updating, deleting, and running migrations.

Every EF Core application defines at least one class that inherits from `DbContext`:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=app.db");
}
```

**Responsibilities of DbContext:**

- Holds the database connection (via the provider).
- Exposes `DbSet<T>` properties for each entity type.
- Owns the Change Tracker.
- Provides `SaveChanges()` / `SaveChangesAsync()` to persist tracked changes.
- Houses `OnModelCreating()` — the hook for configuring entity mappings (covered in depth in the entity configuration and relationships modules).

**Lifetime:** `DbContext` is designed to be a **short-lived, per-request/per-operation** object. It is not thread-safe and should not be used as a singleton. In ASP.NET Core, it is registered with a **Scoped** lifetime so that one instance is created per HTTP request.

---

### 2. DbSet\<T\>

`DbSet<T>` represents a **collection of entities of type T** that maps to a specific database table. It is exposed as a property on your `DbContext`.

```csharp
// Accessing the Products table
context.Products  // DbSet<Product>
```

`DbSet<T>` implements `IQueryable<T>`, which means you can compose LINQ queries directly against it:

```csharp
var expensive = context.Products
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .ToList();
```

The query is not executed until you call a terminal operator (`ToList()`, `FirstOrDefault()`, `Count()`, etc.). Until then, EF Core is building an expression tree that it will translate into SQL.

**DbSet operations:**

| Method | Effect |
|---|---|
| `Add(entity)` | Marks the entity as `Added` — will be inserted on `SaveChanges()` |
| `Remove(entity)` | Marks the entity as `Deleted` — will be deleted on `SaveChanges()` |
| `Find(id)` | Looks up by primary key; checks the Change Tracker first before hitting the DB |
| `Where(...)` | Composes a LINQ filter (deferred) |
| `ToList()` | Executes the query and materializes results |

---

### 3. The Change Tracker

The **Change Tracker** is EF Core's internal state machine. When you load entities from the database, EF Core automatically starts tracking them. Every tracked entity has an **EntityState**:

| EntityState | Meaning |
|---|---|
| `Unchanged` | Loaded from the DB, no modifications detected |
| `Added` | New entity, not yet in the DB — will be `INSERT`ed |
| `Modified` | Loaded from the DB, one or more properties changed — will be `UPDATE`d |
| `Deleted` | Marked for removal — will be `DELETE`d |
| `Detached` | Not tracked by this context instance |

**How tracking works:**

When you execute a LINQ query, EF Core materializes the resulting rows into entity objects *and* creates a snapshot of their original property values. On `SaveChanges()`, EF Core compares current property values against the snapshot to detect modifications and generate `UPDATE` statements for only the changed columns.

```csharp
var product = context.Products.Find(1);   // State: Unchanged, snapshot stored
product.Price = 49.99m;                   // State: Modified (detected automatically)
context.SaveChanges();                    // Generates: UPDATE Products SET Price = 49.99 WHERE Id = 1
```

**Query without tracking** (read-only scenarios):

For queries where you only need to read data and do not intend to update it, disable tracking to improve performance:

```csharp
var products = context.Products
    .AsNoTracking()
    .ToList();
```

No snapshot is stored, reducing memory usage and overhead.

---

### 4. The Provider Model

EF Core is decoupled from any specific database engine through its **provider model**. The core library defines abstract interfaces. A provider package implements those interfaces for a specific database.

```
Your Application Code
        |
        v
Microsoft.EntityFrameworkCore   (core ORM logic, provider-agnostic)
        |
        v
Database Provider Package        (e.g., Microsoft.EntityFrameworkCore.Sqlite)
        |
        v
ADO.NET Driver / Native Client
        |
        v
Database Engine                  (SQLite, SQL Server, PostgreSQL, etc.)
```

When you call `options.UseSqlite(...)` or `options.UseSqlServer(...)`, you are registering the provider. From that point, every SQL string, parameter type mapping, and connection management detail is handled by the provider — the rest of your code stays the same regardless of which database you target.

---

## How the Components Interact: A Request Walkthrough

### Scenario: Load products, update one, save.

```csharp
// 1. Open a DbContext session
using var context = new AppDbContext();

// 2. Query via DbSet — EF Core translates LINQ to SQL, executes, materializes entities
//    Change Tracker begins tracking the returned objects (state: Unchanged)
var products = context.Products.Where(p => p.Price < 50).ToList();

// 3. Modify a tracked entity
products[0].Price = 45.00m;
// Change Tracker detects the change (state: Modified)

// 4. SaveChanges — Change Tracker emits an UPDATE for the modified entity only
context.SaveChanges();
```

At step 4, EF Core asks the Change Tracker which entities have changed, generates the appropriate SQL via the provider, and executes it within a transaction.

---

## Summary

- **DbContext** is the session object — it owns the connection, the DbSets, and the Change Tracker.
- **DbSet\<T\>** is a queryable, typed gateway to a specific table.
- The **Change Tracker** automatically detects modifications to tracked entities and emits the correct SQL on `SaveChanges()`.
- The **provider model** decouples EF Core from specific databases — swap the provider package to target a different engine with minimal code changes.
- Use **`AsNoTracking()`** for read-only queries to skip snapshot overhead.

---

## Additional Resources

- [Microsoft Docs — DbContext Lifetime, Configuration, and Initialization](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Microsoft Docs — Change Tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- [Microsoft Docs — EF Core Database Providers](https://learn.microsoft.com/en-us/ef/core/providers/)
