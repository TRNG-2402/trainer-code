# What Are ORMs?

## Learning Objectives

- Define an Object-Relational Mapper (ORM) and explain its core purpose.
- Articulate the object/relational impedance mismatch and why it creates friction.
- Compare raw ADO.NET data access against an ORM-based approach.
- Understand the trade-offs of each approach so you can make an informed choice.

---

## Why This Matters

Every non-trivial .NET application that stores data eventually needs to bridge two fundamentally different worlds: the object-oriented world of C# classes and the relational world of tables, rows, and foreign keys. How you manage that bridge determines the maintainability of your data layer for the lifetime of the application.

This week's epic is "From Zero to Data Layer." Before writing a single line of EF Core code, you need a clear mental model of *why* the tool exists. Engineers who skip this step often misuse ORMs or reach for raw SQL in situations where the ORM would have been the better choice — and vice versa.

---

## The Object/Relational Impedance Mismatch

The term **impedance mismatch** comes from electrical engineering: two components connected in a circuit can lose energy when their impedances do not match. In software, the same idea applies when you try to map between two systems that model data differently.

### How Objects Model Data

In C#, data and behavior live together in classes. Objects have identity (references), support inheritance, and compose via properties and collections.

```csharp
public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; }   // reference to another object
    public List<OrderLine> Lines { get; set; } // collection of child objects
    public decimal Total => Lines.Sum(l => l.LineTotal);
}
```

### How Relational Databases Model Data

A relational database models data as tables of rows and columns. Relationships are expressed through foreign keys — integers that point to rows in other tables. There are no object references, no inheritance hierarchies in the base model, and certainly no computed properties.

```sql
SELECT o.Id, c.Name, ol.ProductId, ol.Quantity, ol.UnitPrice
FROM Orders o
INNER JOIN Customers c ON c.Id = o.CustomerId
INNER JOIN OrderLines ol ON ol.OrderId = o.Id;
```

### Where the Mismatch Occurs

| Concept | Object World | Relational World |
|---|---|---|
| Identity | Object reference in memory | Primary key value |
| Relationships | Navigation properties / collections | Foreign key columns + JOIN |
| Inheritance | `class Dog : Animal` | No native equivalent (requires patterns like TPH, TPT) |
| Granularity | Fine-grained nested objects | Flat rows |
| Type system | Rich .NET types | Limited SQL types |

Writing code that manually translates between these two worlds is possible, but it is repetitive, error-prone, and grows in complexity as your schema grows. This translation problem is what ORMs are designed to solve.

---

## What Is an ORM?

An **Object-Relational Mapper** is a library that automates the translation between your object model and your relational schema. You define your data as C# classes. The ORM generates the SQL, executes it against the database, and materializes the results back into objects.

The ORM acts as a translation layer, so your application code works almost entirely with objects and LINQ rather than raw SQL strings.

### What an ORM Handles For You

- Generating `SELECT`, `INSERT`, `UPDATE`, and `DELETE` SQL from method calls.
- Tracking which objects have changed since they were loaded.
- Managing database connections and command objects.
- Mapping database column types to .NET types.
- Translating C# LINQ expressions into optimized SQL.
- Generating and applying schema changes (migrations).

---

## Raw ADO.NET vs. ORM: A Direct Comparison

ADO.NET is the low-level .NET data access API. It is not an ORM — it gives you direct control over connections, commands, and data readers. An ORM like EF Core sits on top of ADO.NET (or another provider) and adds the mapping layer.

### Inserting a Record

**Raw ADO.NET:**

```csharp
using var connection = new SqlConnection(connectionString);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)";
command.Parameters.AddWithValue("@Name", product.Name);
command.Parameters.AddWithValue("@Price", product.Price);
command.ExecuteNonQuery();
```

**EF Core (ORM):**

```csharp
context.Products.Add(product);
context.SaveChanges();
```

### Querying a List with a Filter

**Raw ADO.NET:**

```csharp
var results = new List<Product>();

using var connection = new SqlConnection(connectionString);
connection.Open();

var command = connection.CreateCommand();
command.CommandText = "SELECT Id, Name, Price FROM Products WHERE Price < @MaxPrice";
command.Parameters.AddWithValue("@MaxPrice", maxPrice);

using var reader = command.ExecuteReader();
while (reader.Read())
{
    results.Add(new Product
    {
        Id    = reader.GetInt32(0),
        Name  = reader.GetString(1),
        Price = reader.GetDecimal(2)
    });
}
```

**EF Core (ORM):**

```csharp
var results = context.Products
    .Where(p => p.Price < maxPrice)
    .ToList();
```

### Key Differences at a Glance

| Concern | Raw ADO.NET | ORM (EF Core) |
|---|---|---|
| SQL authorship | You write every query | ORM generates SQL |
| Object mapping | You map each column manually | Automatic via conventions/config |
| Change tracking | You manage state manually | Built-in change tracker |
| Schema changes | You write and run migration scripts manually | `dotnet ef migrations` tooling |
| SQL control | Total control | High control via raw SQL escape hatches |
| Performance ceiling | Highest (zero ORM overhead) | Very high with proper tuning |
| Boilerplate volume | High | Low |

---

## When to Use Each

ORMs do not eliminate the need to understand SQL — they raise the abstraction level. There are scenarios where raw ADO.NET (or Dapper, a lightweight micro-ORM) is the better tool:

- **High-performance bulk operations** — inserting or updating millions of rows at once.
- **Complex reporting queries** — analytical queries with heavy aggregation or window functions that the ORM cannot translate efficiently.
- **Legacy stored procedure-heavy systems** — where all data access already lives in the database.

EF Core is the right choice for the majority of CRUD-oriented business applications, where developer productivity, maintainability, and correctness matter more than raw query throughput.

---

## Summary

- The **impedance mismatch** is the fundamental tension between how objects and relational databases model data.
- An **ORM** automates the translation between those two models, reducing boilerplate and centralizing your data access logic.
- **Raw ADO.NET** gives you full control at the cost of verbose, manually maintained data access code.
- **EF Core** is Microsoft's modern ORM for .NET. You will build fluency with it across the rest of this week.

---

## Additional Resources

- [Microsoft Docs — EF Core Overview](https://learn.microsoft.com/en-us/ef/core/)
- [Martin Fowler — ORM Hate](https://martinfowler.com/bliki/OrmHate.html) — a balanced take on ORM trade-offs from a respected voice in software design.
- [Microsoft Docs — Dapper](https://github.com/DapperLib/Dapper) — a micro-ORM for situations where you want SQL control with minimal mapping boilerplate.
