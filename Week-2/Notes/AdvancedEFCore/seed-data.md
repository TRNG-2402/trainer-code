# Seed Data

## Learning Objectives

- Explain what seed data is and why it is useful in development and testing.
- Use `HasData()` in `OnModelCreating` to declare seed data in EF Core.
- Understand the requirements for seeding entities that have relationships.
- Recognize the migration implications of adding, changing, or removing seed data.

---

## Why This Matters

A database with no data is difficult to develop against. Every team member who clones the repository and runs migrations should get a consistent, predictable starting state — sample categories, reference data, test users, or whatever baseline the application requires. EF Core's seeding mechanism makes that baseline part of the migration history, so it is version-controlled, repeatable, and applied automatically.

---

## What Is Seed Data?

**Seed data** is a set of initial records inserted into the database as part of the migration process. Unlike application data (which users create at runtime), seed data exists to establish a known baseline — typically reference data, lookup tables, or development fixtures.

Examples of good seed data candidates:
- Status lookup values (`Active`, `Inactive`, `Pending`).
- Default admin user records.
- Product categories for a demo environment.
- Countries/regions for an address form.

---

## HasData() in OnModelCreating

EF Core supports seed data through the `HasData()` method called on an entity configuration inside `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Category>().HasData(
        new Category { Id = 1, Name = "Fiction" },
        new Category { Id = 2, Name = "Non-Fiction" },
        new Category { Id = 3, Name = "Science & Technology" }
    );
}
```

**Critical requirement:** All seeded entities **must have explicit primary key values**. EF Core uses these values to determine whether a seed record already exists (to avoid re-inserting it on subsequent migrations). You cannot rely on auto-generated keys for seed data.

---

## Seeding Dependent Entities

When seeding entities that have foreign key relationships, the foreign key values must match the primary key values of the seeded principal entities:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Seed the principal (Author) first
    modelBuilder.Entity<Author>().HasData(
        new Author { Id = 1, Name = "George Orwell" },
        new Author { Id = 2, Name = "Frank Herbert" }
    );

    // Seed the dependent (Book) — AuthorId must match seeded Author.Id values
    modelBuilder.Entity<Book>().HasData(
        new Book { Id = 1, Title = "1984",       AuthorId = 1, PublishedYear = 1949 },
        new Book { Id = 2, Title = "Animal Farm", AuthorId = 1, PublishedYear = 1945 },
        new Book { Id = 3, Title = "Dune",        AuthorId = 2, PublishedYear = 1965 }
    );
}
```

The order does not strictly matter within `HasData()` calls, but EF Core will generate the SQL `INSERT` statements in dependency order (principals before dependents) to satisfy foreign key constraints.

---

## Generating a Migration for Seed Data

After adding `HasData()` calls, generate a new migration:

```bash
dotnet ef migrations add SeedInitialData
```

EF Core will generate `InsertData` operations in the migration file:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "Authors",
        columns: new[] { "Id", "Name" },
        values: new object[,]
        {
            { 1, "George Orwell" },
            { 2, "Frank Herbert" }
        });

    migrationBuilder.InsertData(
        table: "Books",
        columns: new[] { "Id", "Title", "AuthorId", "PublishedYear" },
        values: new object[,]
        {
            { 1, "1984", 1, 1949 },
            { 2, "Animal Farm", 1, 1945 },
            { 3, "Dune", 2, 1965 }
        });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(table: "Books",   keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
    migrationBuilder.DeleteData(table: "Authors", keyColumn: "Id", keyValues: new object[] { 1, 2 });
}
```

Apply the migration as usual:

```bash
dotnet ef database update
```

---

## Migration Implications of Changing Seed Data

EF Core tracks seed data in the model snapshot. Any change to seed data — adding records, modifying existing values, or removing records — will generate a new migration containing the diff:

| Change | Generated Migration Operation |
|---|---|
| Add a new seeded record | `InsertData` |
| Modify a value in a seeded record | `UpdateData` |
| Remove a seeded record | `DeleteData` |

This means **seed data changes are versioned** alongside schema changes. Each team member who runs `dotnet ef database update` will get the same data state.

**Implication:** If you change a seed record's primary key, EF Core treats it as a delete of the old record and an insert of the new one. Avoid changing primary key values in seed data after the migration has been applied in production.

---

## Seed Data vs. Application Data

| | Seed Data (HasData) | Application Data |
|---|---|---|
| When created | During `database update` | At runtime by users or services |
| Source of truth | C# code / migrations | Database |
| Versioned | Yes | No |
| Best for | Reference/lookup data, dev fixtures | User-generated content |
| Primary key | Must be explicit | Can be auto-generated |

For large volumes of data or data that changes frequently, consider a dedicated seeding class that runs at application startup (checking if data already exists before inserting), rather than `HasData()`.

---

## Summary

- `HasData()` in `OnModelCreating` declares seed data that EF Core inserts via migrations.
- All seeded entities must have **explicit primary key values** — auto-generated keys are not supported for seed data.
- When seeding related entities, foreign key values must reference the explicit IDs of the seeded principals.
- Adding, modifying, or removing seed data generates a new migration — seed data changes are fully versioned.
- Use `HasData()` for stable reference/lookup data; consider a startup seeding approach for volatile or large datasets.

---

## Additional Resources

- [Microsoft Docs — Data Seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [Microsoft Docs — HasData Method](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1.hasdata)
- [Microsoft Docs — Migrations with Seed Data](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/seeding)
