# Code First vs. Database First

## Learning Objectives

- Describe the Code First workflow and its end-to-end process.
- Describe the Database First approach conceptually and identify when it is appropriate.
- Use `dotnet ef dbcontext scaffold` to understand how Database First scaffolding works.
- Identify the trade-offs of each approach and when to choose one over the other.

---

## Why This Matters

When you join a new project, you will encounter one of two situations: either the database does not yet exist and the team designs the schema via C# models (Code First), or a legacy database already exists and the team needs to work with it from .NET (Database First). Knowing both workflows prevents you from defaulting to the wrong tool or reimplementing what EF Core already provides.

---

## Code First

**Code First** is the approach you have been using since Wednesday: you write C# entity classes and configuration, then use `dotnet ef migrations` to generate and apply the schema to the database. The C# model is the single source of truth.

### End-to-End Code First Workflow

```
1. Define entity classes (Product, Category, etc.)
          |
2. Configure via Data Annotations or Fluent API
          |
3. Register entities in DbContext (DbSet<T> properties)
          |
4. dotnet ef migrations add <Name>
          |
   EF Core compares model to last migration snapshot
   Generates migration C# file (Up / Down methods)
          |
5. dotnet ef database update
          |
   EF Core runs pending migrations against the database
   Database schema is updated to match the model
          |
6. Application uses DbContext to query and save
```

### Code First Advantages

- The schema is derived from and stays in sync with the code.
- Migrations are version-controlled alongside the application.
- Schema changes follow a predictable, reviewable process (generate migration, review, apply).
- Works well for greenfield applications and teams comfortable with C#.

### Code First Limitations

- Generating complex schemas (custom indexes, stored procedures, views) sometimes requires manual migration edits.
- Team members must coordinate migration generation — two developers generating migrations simultaneously on different branches creates conflicts in the snapshot file.

---

## Database First

**Database First** is the reverse workflow: the database already exists, and you use EF Core tooling to generate entity classes and a `DbContext` from the existing schema. The database is the source of truth.

### When Database First Is Appropriate

- You are integrating with an existing legacy database that the DBA team owns.
- The database schema is maintained by a separate team or tooling (e.g., Liquibase, Flyway, manual SQL scripts).
- The database contains stored procedures, views, or complex constraints that are easier to define in SQL than in EF Core configuration.

### dotnet ef dbcontext scaffold

The `scaffold` command connects to an existing database, reads its schema, and generates entity classes and a `DbContext`:

```bash
dotnet ef dbcontext scaffold \
  "Data Source=existing.db" \
  Microsoft.EntityFrameworkCore.Sqlite \
  --output-dir Models \
  --context-dir Data \
  --context ExistingDbContext \
  --no-onconfiguring
```

**Key options:**

| Option | Purpose |
|---|---|
| First argument | Connection string |
| Second argument | Provider package name |
| `--output-dir` | Directory for generated entity classes |
| `--context-dir` | Directory for the generated DbContext |
| `--context` | Name for the generated DbContext class |
| `--no-onconfiguring` | Omit the `OnConfiguring` override (use DI instead) |
| `--tables` | Scaffold only specific tables |
| `--data-annotations` | Use Data Annotations where possible (default is Fluent API) |

### What Gets Generated

For each table, EF Core generates a C# class with properties matching the columns. For example, given:

```sql
CREATE TABLE Products (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Name       TEXT NOT NULL,
    Price      REAL NOT NULL,
    CategoryId INTEGER NOT NULL REFERENCES Categories(Id)
);
```

The scaffold command generates:

```csharp
public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;
}
```

And the DbContext will contain Fluent API configuration reflecting the schema constraints.

### Database First Limitations

- Re-running scaffold overwrites the generated files — any customizations you added are lost unless you use partial classes.
- You do not own the schema, so EF Core migrations are generally not used; the database team manages schema changes.
- Round-tripping (scaffold → modify entity → scaffold again) is fragile.

---

## Choosing Between the Two

| Factor | Prefer Code First | Prefer Database First |
|---|---|---|
| Project lifecycle | Greenfield / new project | Legacy system integration |
| Schema ownership | Application team | DBA or separate team |
| Schema evolution | Via EF Core migrations | Via external scripts or tools |
| Team familiarity | Comfortable with C# | Comfortable with SQL/DB tooling |
| Complexity | Most business applications | Highly customized schemas |

In practice, most modern .NET projects default to Code First. Database First is a pragmatic choice when inheriting an existing system.

---

## A Note on Model First

You may see references to a third approach called **Model First**, where you design a visual diagram in Visual Studio and generate both the entity classes and the database from the diagram. This was specific to EF6's EDMX tooling. EF Core does not support Model First — the choice is Code First or Database First.

---

## Summary

- **Code First**: Write entities in C#, generate the schema via migrations. The C# model is the source of truth. Best for greenfield projects.
- **Database First**: Use `dotnet ef dbcontext scaffold` to generate entities and `DbContext` from an existing database. The database is the source of truth. Best for legacy system integration.
- `dotnet ef dbcontext scaffold` takes a connection string and a provider name, and outputs entity classes and a configured `DbContext`.
- Code First is the standard for modern .NET teams; Database First is a pragmatic fallback for inherited systems.

---

## Additional Resources

- [Microsoft Docs — Reverse Engineering (Database First)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/)
- [Microsoft Docs — dotnet ef dbcontext scaffold](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-dbcontext-scaffold)
- [Microsoft Docs — Migrations Overview (Code First)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
