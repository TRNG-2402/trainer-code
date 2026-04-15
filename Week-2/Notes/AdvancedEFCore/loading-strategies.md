# Loading Strategies

## Learning Objectives

- Differentiate between Eager Loading, Lazy Loading, and Explicit Loading in EF Core.
- Use `Include()` and `ThenInclude()` to perform eager loading.
- Configure and use Lazy Loading proxies, and understand their pitfalls.
- Use `Load()` for explicit loading.
- Explain the N+1 query problem and how each loading strategy handles it.

---

## Why This Matters

Related data does not load automatically in EF Core. By default, a navigation property is `null` after a query unless you explicitly request its data. Choosing the wrong loading strategy is one of the most common causes of performance problems in EF Core applications — particularly the N+1 query problem, which can silently turn a single request into hundreds of database round-trips.

---

## The Default: No Automatic Loading

By default, EF Core does not load navigation properties when you query an entity. This is intentional — loading all related data automatically would destroy performance on any schema with relationships.

```csharp
var book = context.Books.Find(1);
Console.WriteLine(book.Author.Name);  // NullReferenceException — Author is null
```

To load related data, you must choose a loading strategy explicitly.

---

## Eager Loading

**Eager loading** loads related data in the same database query as the primary entity, using a SQL `JOIN`. It is the most explicit and predictable strategy.

### Include()

Use `Include()` to load a direct navigation property:

```csharp
var books = context.Books
    .Include(b => b.Author)
    .ToList();

// Author is now populated on each Book — loaded in the same query
foreach (var book in books)
{
    Console.WriteLine($"{book.Title} by {book.Author.Name}");
}
```

Generated SQL (approximate):

```sql
SELECT b.Id, b.Title, b.AuthorId, a.Id, a.Name
FROM Books b
INNER JOIN Authors a ON a.Id = b.AuthorId;
```

### ThenInclude()

Use `ThenInclude()` to load navigation properties of navigation properties (multi-level eager loading):

```csharp
var orders = context.Orders
    .Include(o => o.Customer)
    .Include(o => o.Lines)
        .ThenInclude(l => l.Product)
    .ToList();
```

This loads `Order`, its `Customer`, its `Lines` collection, and each `OrderLine`'s `Product` — all in a small number of queries (EF Core splits multi-level includes across a few queries to avoid cartesian explosion).

### When to Use Eager Loading

- You know at query time which related data you will need.
- You are rendering a view or building a response DTO that requires the related data.
- You want SQL to handle the join, not your application code.

---

## Lazy Loading

**Lazy loading** defers loading of related data until the navigation property is first accessed in code. When you access `book.Author` for the first time, EF Core automatically issues a `SELECT` query to load the `Author`.

### Setup: Proxy-Based Lazy Loading

Lazy loading requires:

1. Install the proxy package:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.Proxies
   ```

2. Enable proxies in `DbContextOptionsBuilder`:
   ```csharp
   options.UseSqlite("Data Source=app.db")
          .UseLazyLoadingProxies();
   ```

3. Mark all navigation properties as `virtual`:
   ```csharp
   public class Book
   {
       public int Id { get; set; }
       public string Title { get; set; }
       public int AuthorId { get; set; }

       public virtual Author Author { get; set; }      // virtual required for lazy loading
       public virtual ICollection<Review> Reviews { get; set; }
   }
   ```

EF Core generates a proxy subclass at runtime that intercepts access to `virtual` navigation properties and triggers the database query.

### The N+1 Problem

Lazy loading's convenience conceals a severe performance trap. Consider:

```csharp
var books = context.Books.ToList();  // Query 1: SELECT * FROM Books (returns 100 books)

foreach (var book in books)
{
    // Each access issues a separate query — 100 more queries!
    Console.WriteLine(book.Author.Name);  // Query 2, 3, 4, ... 101
}
```

This is the **N+1 problem**: 1 query to get the list + N queries (one per row) to get related data. For 100 books, that is 101 database round-trips instead of 1.

**Lazy loading does not prevent the N+1 problem — it causes it silently.** The code looks correct but generates catastrophic query behavior under load.

### When Lazy Loading Has Value

Lazy loading is defensible in desktop or REPL contexts where query volume is predictable and performance is not a primary concern. In web applications, avoid it by default. If you enable it, use EF Core logging to monitor the number of queries generated per request.

---

## Explicit Loading

**Explicit loading** gives you manual control over when related data is loaded. You load navigation properties on demand after the primary entity has been retrieved.

```csharp
var book = context.Books.Find(1);  // Loaded, Author is null

// Explicitly load a reference navigation
context.Entry(book).Reference(b => b.Author).Load();

// Explicitly load a collection navigation
context.Entry(book).Collection(b => b.Reviews).Load();

Console.WriteLine(book.Author.Name);   // Now populated
Console.WriteLine(book.Reviews.Count); // Now populated
```

You can also apply a filter when explicitly loading a collection:

```csharp
context.Entry(book)
    .Collection(b => b.Reviews)
    .Query()
    .Where(r => r.Rating >= 4)
    .Load();
```

### When to Use Explicit Loading

- You conditionally need related data (sometimes you need it, sometimes you don't).
- You want to load related data based on user input or branching logic.
- You want the precision of explicit control without the infrastructure overhead of proxies.

---

## Strategy Comparison

| Strategy | When Data Is Loaded | SQL Behavior | N+1 Risk |
|---|---|---|---|
| Eager | At query time (with `Include`) | JOIN in the same query | None — all data in one or few queries |
| Lazy | On first navigation property access | Separate SELECT per access | High — silent N+1 in loops |
| Explicit | On demand via `Entry().Load()` | Separate SELECT, controlled | Low — you control when each load happens |

---

## Monitoring Generated SQL

Enable EF Core SQL logging to see every query generated during a session. In a console app:

```csharp
options.UseSqlite("Data Source=app.db")
       .LogTo(Console.WriteLine, LogLevel.Information)
       .EnableSensitiveDataLogging();
```

Review the logged SQL whenever you are unsure how many queries your code is generating. This is particularly important when lazy loading is enabled.

---

## Summary

- EF Core does not load related data automatically — `null` navigation properties are the default.
- **Eager loading** (`Include` / `ThenInclude`) loads related data in the same query. Preferred in most web application scenarios.
- **Lazy loading** loads related data on first access using proxies. Requires `virtual` navigation properties and the proxies package. Causes the N+1 problem in loops — use with caution.
- **Explicit loading** (`Entry().Load()`) gives precise, on-demand control over when each related entity is fetched.
- The **N+1 problem** is the single biggest performance gotcha in EF Core. Eager loading is its primary solution.
- Use **`LogTo(Console.WriteLine)`** or equivalent logging to see generated SQL and detect N+1 patterns.

---

## Additional Resources

- [Microsoft Docs — Loading Related Data](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)
- [Microsoft Docs — Eager Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)
- [Microsoft Docs — Lazy Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy)
- [Microsoft Docs — Explicit Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/explicit)
