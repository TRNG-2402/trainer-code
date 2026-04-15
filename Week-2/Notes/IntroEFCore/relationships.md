# Relationships

## Learning Objectives

- Explain the three relationship cardinalities EF Core supports: one-to-one, one-to-many, and many-to-many.
- Define navigation properties and foreign key properties, and understand how they work together.
- Configure each relationship type using Data Annotations and EF Core conventions.
- Understand cascade delete behavior and when to change the default.

---

## Why This Matters

Real-world data models are never a single flat table. Products belong to categories, orders have lines, users have roles, and students enroll in many courses. Getting relationships right in EF Core means your schema accurately reflects your domain, your queries stay clean, and your data integrity is enforced at the database level. Misconfigured relationships are one of the most common sources of EF Core bugs.

---

## Core Vocabulary

Before looking at relationship types, three terms need to be clear:

**Navigation Property** — A property on an entity that refers to a related entity (or collection of related entities). EF Core uses navigation properties to understand how entities relate and to enable `.Include()` when querying (covered on Thursday).

```csharp
public class Order
{
    public Customer Customer { get; set; }      // Reference navigation (to one)
    public List<OrderLine> Lines { get; set; }  // Collection navigation (to many)
}
```

**Foreign Key Property** — A scalar property (usually `int` or `Guid`) that holds the primary key value of the related entity. EF Core uses this to generate the foreign key column in the database.

```csharp
public class Order
{
    public int CustomerId { get; set; }  // Foreign key property
    public Customer Customer { get; set; }
}
```

**Principal / Dependent** — In any relationship, the **principal** entity is the one that owns the primary key. The **dependent** entity holds the foreign key. In a Customer/Order relationship, Customer is the principal and Order is the dependent.

---

## One-to-Many

This is the most common relationship. One principal entity relates to zero, one, or many dependent entities.

**Example:** One `Category` has many `Products`.

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Collection navigation — one category to many products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Foreign key property
    public int CategoryId { get; set; }

    // Reference navigation — many products to one category
    public Category Category { get; set; }
}
```

EF Core detects this relationship by convention: `Product` has a property named `CategoryId` (matching `Category` + `Id`), and `Category` has a collection navigation `Products`. No annotations required.

**The generated schema:**

```sql
CREATE TABLE Categories (
    Id    INTEGER PRIMARY KEY AUTOINCREMENT,
    Name  TEXT NOT NULL
);

CREATE TABLE Products (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Name       TEXT NOT NULL,
    Price      REAL NOT NULL,
    CategoryId INTEGER NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

**When EF Core cannot infer the foreign key by convention**, use `[ForeignKey]`:

```csharp
public class Product
{
    public int Id { get; set; }
    public int CatId { get; set; }               // Non-conventional name

    [ForeignKey("CatId")]
    public Category Category { get; set; }
}
```

---

## One-to-One

One principal entity relates to exactly one dependent entity.

**Example:** One `User` has one `UserProfile`.

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }

    public UserProfile Profile { get; set; }     // Reference navigation
}

public class UserProfile
{
    public int Id { get; set; }

    [Required]
    public string DisplayName { get; set; }

    // Foreign key — also the primary key of this dependent entity
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }               // Reference navigation back
}
```

In a one-to-one, EF Core needs to determine which entity is the dependent (the one that holds the foreign key). Convention: if `UserProfile.UserId` matches the principal's primary key name pattern, EF Core infers the relationship correctly. `[ForeignKey]` makes it unambiguous.

**Important:** Both entities have reference navigations (not collections) in a one-to-one.

---

## Many-to-Many

Many principal entities relate to many other entities.

**Example:** Many `Students` can enroll in many `Courses`.

### Implicit Join (EF Core 5+)

In EF Core 5 and later, you can define a many-to-many without an explicit join entity. EF Core creates the join table automatically.

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
```

EF Core creates a `StudentCourse` join table with `StudentsId` and `CoursesId` columns. No additional configuration is required.

### Explicit Join Entity (Recommended for Production)

If the relationship has additional data (e.g., enrollment date, grade), you must define the join entity explicitly:

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; }
}

public class Enrollment
{
    public int StudentId { get; set; }
    public Student Student { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public DateTime EnrolledAt { get; set; }
    public string Grade { get; set; }
}
```

The composite primary key for `Enrollment` must be configured via **Fluent API** (covered on Thursday), since Data Annotations do not support composite keys.

---

## Cascade Delete

**Cascade delete** means: when a principal entity is deleted, its dependent entities are automatically deleted as well.

EF Core defaults:

| Relationship | Default Cascade |
|---|---|
| Required (foreign key NOT NULL) | `Cascade` — delete dependents when principal is deleted |
| Optional (foreign key nullable) | `ClientSetNull` — set FK to null in tracked entities; DB may error if not configured |

To change the default, use Fluent API (Thursday) or the `[DeleteBehavior]` configuration. For today, be aware of the default: deleting a `Category` will delete all its `Products` if the relationship is required (`CategoryId` is not nullable).

**Practical rule:** If your domain requires that dependent records survive the deletion of a principal (e.g., orders should not be deleted when a customer is soft-deleted), you need to explicitly configure the delete behavior — either via Fluent API or by making the foreign key nullable and managing the logic in your application.

---

## Relationship Configuration Checklist

When adding a relationship:

1. Add the **foreign key property** to the dependent entity (e.g., `int CategoryId`).
2. Add **navigation properties** to both entities as appropriate (reference on the dependent, collection on the principal, or both).
3. Use `[ForeignKey]` if the naming does not follow convention.
4. Consider whether to use `[Required]` on the foreign key to enforce NOT NULL.
5. Add the relevant `DbSet<T>` to your `DbContext` if not already present.

---

## Summary

- **Navigation properties** allow you to traverse relationships in code. EF Core uses them to generate JOINs in SQL.
- **Foreign key properties** are the scalar counterparts that EF Core maps to FK columns in the database.
- **One-to-many** is the most common relationship — one principal, many dependents, foreign key on the dependent.
- **One-to-one** — both sides have reference navigations; EF Core must determine which holds the foreign key.
- **Many-to-many** — use an explicit join entity when the relationship carries data; use implicit join for pure associations.
- **Cascade delete** defaults to `Cascade` for required relationships — be deliberate about this in production schemas.

---

## Additional Resources

- [Microsoft Docs — Relationships in EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)
- [Microsoft Docs — Cascade Delete](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete)
- [Microsoft Docs — Many-to-Many](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many)
