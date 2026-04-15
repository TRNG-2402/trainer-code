# LINQ: Query Syntax vs. Method Syntax

## Learning Objectives

- Read and write LINQ expressions in both query syntax and method syntax.
- Identify equivalent constructs in each syntax for filtering, projection, sorting, grouping, and joining.
- Know when each syntax is more readable or more practical.
- Understand how EF Core translates LINQ to SQL in either syntax.

---

## Why This Matters

LINQ (Language-Integrated Query) is the primary mechanism for reading data from EF Core. It is also used throughout the .NET ecosystem — in collections, streams, and data pipelines. Being fluent in both syntaxes means you can read any codebase (each team tends to prefer one), write queries in whichever form is clearer for the context, and debug EF Core-generated SQL more confidently.

---

## Two Syntaxes, One Feature

LINQ was introduced in C# 3.0 with two interchangeable syntaxes. The compiler translates query syntax into method syntax under the hood — they produce identical IL and identical SQL when used with EF Core.

```csharp
// Query syntax — resembles SQL
var result = from p in context.Products
             where p.Price < 50
             select p;

// Method syntax — fluent chaining
var result = context.Products
    .Where(p => p.Price < 50);
```

Both produce the same SQL:
```sql
SELECT * FROM Products WHERE Price < 50;
```

---

## Side-by-Side Comparison

### Filter (WHERE)

**Query syntax:**
```csharp
var affordable = from p in context.Products
                 where p.Price < 50
                 select p;
```

**Method syntax:**
```csharp
var affordable = context.Products
    .Where(p => p.Price < 50);
```

---

### Projection (SELECT with transformation)

**Query syntax:**
```csharp
var names = from p in context.Products
            select new { p.Name, p.Price };
```

**Method syntax:**
```csharp
var names = context.Products
    .Select(p => new { p.Name, p.Price });
```

Both project into an anonymous type — only the `Name` and `Price` columns are in the generated `SELECT`.

---

### Sort (ORDER BY)

**Query syntax:**
```csharp
var sorted = from p in context.Products
             orderby p.Price ascending, p.Name descending
             select p;
```

**Method syntax:**
```csharp
var sorted = context.Products
    .OrderBy(p => p.Price)
    .ThenByDescending(p => p.Name);
```

Note: Query syntax uses `orderby` in a single clause. Method syntax chains `OrderBy` and `ThenByDescending`.

---

### Filter + Sort + Project (Combined)

**Query syntax:**
```csharp
var result = from p in context.Products
             where p.Price < 100
             orderby p.Name
             select new { p.Id, p.Name, p.Price };
```

**Method syntax:**
```csharp
var result = context.Products
    .Where(p => p.Price < 100)
    .OrderBy(p => p.Name)
    .Select(p => new { p.Id, p.Name, p.Price });
```

---

### Group (GROUP BY)

**Query syntax:**
```csharp
var groups = from p in context.Products
             group p by p.CategoryId into g
             select new
             {
                 CategoryId = g.Key,
                 Count      = g.Count(),
                 AvgPrice   = g.Average(p => p.Price)
             };
```

**Method syntax:**
```csharp
var groups = context.Products
    .GroupBy(p => p.CategoryId)
    .Select(g => new
    {
        CategoryId = g.Key,
        Count      = g.Count(),
        AvgPrice   = g.Average(p => p.Price)
    });
```

`GroupBy` in method syntax returns `IGrouping<TKey, TElement>` — the key is accessible via `.Key`, the elements by iterating the group. You typically follow it with a `Select` to project the aggregation.

---

### Join (across navigation property — preferred in EF Core)

In EF Core, you rarely need an explicit `join` clause because navigation properties encode the relationship. `Include()` or simply accessing the navigation property handles most join scenarios.

However, when you need a manual join (e.g., projecting from two entities without a navigation link):

**Query syntax:**
```csharp
var result = from b in context.Books
             join a in context.Authors on b.AuthorId equals a.Id
             select new { b.Title, AuthorName = a.Name };
```

**Method syntax:**
```csharp
var result = context.Books
    .Join(
        context.Authors,
        b => b.AuthorId,
        a => a.Id,
        (b, a) => new { b.Title, AuthorName = a.Name }
    );
```

**Preferred EF Core approach** (when navigation properties are configured):
```csharp
var result = context.Books
    .Include(b => b.Author)
    .Select(b => new { b.Title, AuthorName = b.Author.Name });
```

EF Core translates the `Include` + `Select` pattern into a JOIN in SQL and does not over-fetch data when you project with `Select`.

---

### Multiple Filters and Complex Predicates

Method syntax composes cleanly for dynamic queries:

```csharp
IQueryable<Product> query = context.Products;

if (maxPrice.HasValue)
    query = query.Where(p => p.Price <= maxPrice.Value);

if (!string.IsNullOrEmpty(categoryName))
    query = query.Where(p => p.Category.Name == categoryName);

var results = query.OrderBy(p => p.Name).ToList();
```

This pattern — building a query dynamically by chaining `Where` calls — is not possible with query syntax, which requires the full expression to be written statically.

---

## When to Prefer Each Syntax

| Scenario | Prefer |
|---|---|
| Simple filters and projections | Either (team preference) |
| Complex multi-clause query (group by, nested `let`, multiple froms) | Query syntax — more readable |
| Dynamic query construction (conditional filters) | Method syntax — composable |
| Chaining operators (`.Where().OrderBy().Select()`) | Method syntax — natural chaining |
| Code matching SQL mental model for SQL-oriented teams | Query syntax |
| EF Core-heavy codebase (most .NET teams) | Method syntax — dominant convention |

In the .NET ecosystem, **method syntax is the convention used by the vast majority of EF Core codebases**. Query syntax sees more use in teams with strong SQL backgrounds or in data-processing pipelines. Either is valid — pick one style per query and be consistent.

---

## EF Core LINQ Translation Notes

- LINQ queries against `IQueryable<T>` (returned by `DbSet<T>`) are **not executed until a terminal operator** is called (`ToList()`, `FirstOrDefault()`, `Count()`, `Any()`, etc.).
- EF Core translates the expression tree to SQL. If you call a method EF Core cannot translate (e.g., a custom C# method), it throws a `InvalidOperationException` at runtime.
- To force client-side evaluation (evaluate in memory after fetching from DB), call `AsEnumerable()` first — but be aware that this fetches all matching rows before filtering.

---

## Summary

- LINQ provides two interchangeable syntaxes: **query syntax** (SQL-like, keyword-based) and **method syntax** (fluent, lambda-based).
- The compiler converts query syntax to method syntax — the output SQL is identical.
- **Method syntax** dominates production EF Core codebases and composes better for dynamic queries.
- **Query syntax** can be more readable for complex multi-clause operations (group by, multi-from).
- LINQ queries against `IQueryable<T>` are deferred — SQL is only executed at the terminal operator.
- EF Core must be able to translate your LINQ to SQL; arbitrary C# methods inside a query will throw at runtime.

---

## Additional Resources

- [Microsoft Docs — LINQ in C#](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)
- [Microsoft Docs — Querying in EF Core](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Microsoft Docs — Complex Query Operators](https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators)
