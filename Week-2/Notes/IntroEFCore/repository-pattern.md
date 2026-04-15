# The Repository Pattern

## Learning Objectives

- Articulate the purpose of the Repository pattern and the problem it solves.
- Define a generic repository interface and understand what it abstracts.
- Implement a concrete generic repository backed by EF Core.
- Evaluate the pros and cons of the Repository pattern in a modern EF Core context.

---

## Why This Matters

As a data layer grows, scattering direct `DbContext` calls across service classes creates tightly coupled code that is difficult to test and maintain. The Repository pattern centralizes data access behind a consistent interface, giving you a clean separation between business logic and persistence logic. It is a pattern you will encounter in many enterprise .NET codebases — knowing it well means you can read and contribute to those systems immediately.

---

## The Problem the Repository Pattern Solves

Without a repository, service classes call EF Core directly:

```csharp
public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public List<Order> GetOrdersForCustomer(int customerId)
    {
        return _context.Orders
            .Where(o => o.CustomerId == customerId)
            .ToList();
    }

    public void CreateOrder(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }
}
```

This works, but it has problems:

- **Testability:** Unit testing `OrderService` requires a real or in-memory database. You cannot simply swap it for a fake.
- **Coupling:** Business logic is aware of EF Core specifics (`DbContext`, `DbSet`, `SaveChanges`).
- **Duplication:** Common query patterns (GetById, GetAll, Add, Remove) are repeated across every service class.

The Repository pattern encapsulates data access behind an interface that business logic depends on — not on the concrete EF Core implementation.

---

## The Generic Repository Interface

A **generic repository interface** defines the standard CRUD operations for any entity type:

```csharp
public interface IRepository<T> where T : class
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
```

`Expression<Func<T, bool>>` is a LINQ expression predicate — it allows callers to pass filter criteria without the repository knowing what those criteria are. EF Core can translate these expressions to SQL.

---

## The Concrete Generic Repository

The `Repository<T>` class implements `IRepository<T>` using EF Core:

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public T GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }

    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate).ToList();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _dbSet.AddRange(entities);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
```

Note that `Add` and `Remove` do not call `SaveChanges()` — that responsibility belongs to the **Unit of Work** or the caller. The repository manages access; the save is a separate concern.

---

## Entity-Specific Repositories

For queries that go beyond what the generic interface provides, you define a **specific repository interface** that extends the generic one:

```csharp
public interface IProductRepository : IRepository<Product>
{
    IEnumerable<Product> GetByCategory(int categoryId);
    IEnumerable<Product> GetAffordable(decimal maxPrice);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public IEnumerable<Product> GetByCategory(int categoryId)
    {
        return _dbSet.Where(p => p.CategoryId == categoryId).ToList();
    }

    public IEnumerable<Product> GetAffordable(decimal maxPrice)
    {
        return _dbSet.Where(p => p.Price <= maxPrice).OrderBy(p => p.Price).ToList();
    }
}
```

---

## The Unit of Work

The **Unit of Work** pattern is commonly paired with repositories. It wraps multiple repository operations in a single transaction and holds the single `SaveChanges()` call:

```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IRepository<Category> Categories { get; }
    int Complete();  // == SaveChanges()
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IProductRepository Products { get; }
    public IRepository<Category> Categories { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context   = context;
        Products   = new ProductRepository(context);
        Categories = new Repository<Category>(context);
    }

    public int Complete() => _context.SaveChanges();

    public void Dispose() => _context.Dispose();
}
```

Usage:

```csharp
using var uow = new UnitOfWork(new AppDbContext());

var category = new Category { Name = "Electronics" };
uow.Categories.Add(category);

var product = new Product { Name = "Laptop", Price = 999m, CategoryId = category.Id };
uow.Products.Add(product);

uow.Complete();  // Both inserts committed in one transaction
```

---

## Direct DbContext vs. Repository: A Comparison

| Concern | Direct DbContext | Repository Pattern |
|---|---|---|
| Testability | Requires EF Core In-Memory or Moq setup for DbContext | Swap repository interface with a fake |
| Code volume | Less — direct and concise | More — interfaces, concrete classes, UoW |
| Flexibility | Full EF Core surface available | Limited to what the interface exposes |
| Separation of concerns | Business logic aware of EF Core | Business logic unaware of EF Core |
| LINQ composition | Full, deferred composition possible | Can leak materialization if not careful |
| Appropriate for | Small projects, scripts, demos | Enterprise apps, teams with distinct layers |

---

## When to Use the Repository Pattern

Use it when:
- You have a multi-developer codebase where the data access layer needs clear boundaries.
- You write unit tests for service classes and need to mock the data layer.
- The project will live long enough that discoverability of data access code matters.

Consider skipping it when:
- You are building a small application or a prototype.
- The team already uses EF Core's `IQueryable` composition as the abstraction boundary.
- Your integration tests use EF Core In-Memory and you test the full stack.

EF Core's `DbContext` and `DbSet<T>` already implement `IQueryable` — some engineers argue this is a sufficient abstraction layer on its own. Both viewpoints are defensible; the right answer depends on team size and project lifespan.

---

## Summary

- The **Repository pattern** abstracts data access behind an interface, decoupling business logic from EF Core.
- A **generic repository** (`IRepository<T>`) provides standard CRUD for any entity type.
- **Entity-specific repositories** extend the generic interface with domain-specific queries.
- The **Unit of Work** pattern pairs with repositories to provide a single `SaveChanges()` commit point across multiple repository operations.
- The pattern adds code volume; its value is most pronounced in large, team-developed, long-lived applications.

---

## Additional Resources

- [Microsoft Docs — Repository Pattern with EF Core](https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)
- [Martin Fowler — Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Microsoft Architecture Docs — Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
