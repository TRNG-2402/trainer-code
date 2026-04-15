# Introduction to EF Core

## Learning Objectives

- Describe what Entity Framework Core is and where it fits in the .NET ecosystem.
- Distinguish EF Core from its predecessor, EF6.
- Identify the NuGet packages required to start an EF Core project.
- Understand the supported database providers and what "provider model" means at a high level.

---

## Why This Matters

EF Core is the standard data access layer in modern .NET applications. Whether you are building an ASP.NET Core Web API, a background worker, or a console tool, EF Core is the library the .NET ecosystem reaches for when object-to-database mapping is needed. Understanding what it is, what it is not, and how its package ecosystem is structured will save you hours of confusion when setting up new projects.

This topic continues the weekly epic of building your data layer from the ground up. You now know *why* ORMs exist — this module explains which ORM you will be using and how to wire it into a .NET project.

---

## What Is EF Core?

**Entity Framework Core (EF Core)** is an open-source, cross-platform Object-Relational Mapper (ORM) for .NET, maintained by Microsoft. It is the successor to Entity Framework 6 (EF6) and was designed from the ground up to run on .NET Core and beyond.

At its core, EF Core does three things:

1. **Maps** your C# classes (called *entities*) to relational database tables.
2. **Translates** LINQ queries written against those classes into SQL queries.
3. **Tracks** changes to entity instances and generates the appropriate `INSERT`, `UPDATE`, and `DELETE` statements when you call `SaveChanges()`.

EF Core does not replace SQL — it generates it. You can always inspect or override the generated SQL when needed.

---

## EF Core's Role in the .NET Ecosystem

EF Core sits in the data access layer of the application stack. In a typical ASP.NET Core Web API, the layering looks like this:

```
HTTP Request
    |
    v
ASP.NET Core Controller
    |
    v
Service / Business Logic Layer
    |
    v
Repository / Data Access Layer  <-- EF Core lives here
    |
    v
Database (SQL Server, SQLite, PostgreSQL, etc.)
```

EF Core is not a framework for the entire application — it is a focused library for the data access concern. You integrate it into whatever application architecture you are using.

---

## EF Core vs. EF6

You may encounter references to **Entity Framework 6 (EF6)** in older codebases or documentation. The two are distinct products.

| Feature | EF6 | EF Core |
|---|---|---|
| Platform | .NET Framework only | .NET Core / .NET 5+ (cross-platform) |
| Open Source | Limited | Fully open source |
| Performance | Baseline | Significantly faster |
| LINQ translation | Limited | Richer, more complete |
| Raw SQL | Basic | First-class via `FromSqlRaw` / `ExecuteSqlRaw` |
| Database providers | SQL Server, Oracle, etc. | Pluggable provider model (SQL Server, SQLite, PostgreSQL, MySQL, In-Memory, Cosmos, and more) |
| Migrations | Yes | Yes (improved tooling) |
| Active development | Maintenance mode only | Actively developed |

**Rule of thumb:** If you are starting a new project, use EF Core. EF6 is relevant only when maintaining legacy .NET Framework applications.

---

## The Provider Model

EF Core is database-agnostic by design. The core library (`Microsoft.EntityFrameworkCore`) does not know how to talk to any specific database engine. Instead, you install a **database provider** — a separate NuGet package that handles the connection, SQL dialect, and type mapping for a particular database.

Common providers:

| Database | NuGet Package |
|---|---|
| SQL Server / Azure SQL | `Microsoft.EntityFrameworkCore.SqlServer` |
| SQLite | `Microsoft.EntityFrameworkCore.Sqlite` |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| MySQL / MariaDB | `Pomelo.EntityFrameworkCore.MySql` |
| In-Memory (testing) | `Microsoft.EntityFrameworkCore.InMemory` |
| Azure Cosmos DB | `Microsoft.EntityFrameworkCore.Cosmos` |

This week's exercises use **SQLite** because it requires zero server setup — the database is a single file on disk, which makes it ideal for learning.

---

## Required NuGet Packages

Every EF Core project requires at minimum two packages:

### 1. The Core Package

```
Microsoft.EntityFrameworkCore
```

This is the ORM itself — DbContext, DbSet, LINQ translation, change tracking, and migrations infrastructure.

### 2. A Database Provider

For this week (SQLite):

```
Microsoft.EntityFrameworkCore.Sqlite
```

### 3. The Design-Time Tools (for migrations)

```
Microsoft.EntityFrameworkCore.Design
```

This package is required when you run `dotnet ef` CLI commands such as `migrations add` and `database update`. It is typically installed in the startup project.

### Installing via CLI

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Verifying the EF Core CLI Tool

The `dotnet ef` command-line tool must be installed globally (or per project as a local tool):

```bash
dotnet tool install --global dotnet-ef
```

Verify installation:

```bash
dotnet ef --version
```

---

## A Minimal EF Core Project Structure

Once packages are installed, a minimal EF Core setup requires two things: one or more **entity classes** and a **DbContext** class. You will explore both in depth in the next two modules. For now, here is what they look like at the highest level:

```csharp
// Entity class — maps to a database table
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// DbContext — represents the database session
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=app.db");
}
```

You will build out this pattern in full during today's instructor demo and exercise.

---

## Summary

- **EF Core** is Microsoft's modern, cross-platform ORM for .NET — actively developed and the standard choice for new projects.
- It differs from **EF6** in platform support, performance, and feature depth. EF6 is maintenance-only.
- EF Core uses a **provider model** — the core library is database-agnostic; a separate NuGet package handles each target database.
- Every EF Core project requires: the core package, a provider package, and the design-time tools package for migrations.
- The two fundamental building blocks are **entities** (C# classes) and **DbContext** (the database session) — covered in the next two modules.

---

## Additional Resources

- [Microsoft Docs — EF Core Getting Started](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app)
- [Microsoft Docs — EF Core vs EF6](https://learn.microsoft.com/en-us/ef/efcore-and-ef6/)
- [EF Core GitHub Repository](https://github.com/dotnet/efcore)
