# DbContext and Migrations

## Learning Objectives

- Create a `DbContext` class and configure it to connect to a database.
- Understand the role of `OnModelCreating` and when to use it.
- Use the `dotnet ef` CLI to create and apply migrations.
- Read and interpret the files that EF Core generates for a migration.

---

## Why This Matters

Migrations are EF Core's answer to the question: "How does the database schema stay in sync with my C# model as the model evolves?" Without a migration system, every schema change requires manual SQL scripts, manual coordination across environments, and no single source of truth. EF Core migrations make the C# model the source of truth and generate the SQL for you.

---

## Creating a DbContext

Your `DbContext` subclass is the integration point between your entity model and the database. At a minimum it needs two things: `DbSet<T>` properties for each entity type, and a connection string.

### Option 1: Override OnConfiguring (Simple / Console Apps)

```csharp
public class LibraryContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=library.db");
    }
}
```

`OnConfiguring` is called once when the context is first created. It is the simplest approach for console applications and quick prototypes.

### Option 2: Constructor Injection (ASP.NET Core / DI)

In ASP.NET Core (covered in Week 2), the `DbContext` is registered with the DI container and receives its options via constructor injection:

```csharp
public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
}
```

The connection string is provided in `Program.cs`:

```csharp
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

You will use the constructor injection pattern starting next week. For today's exercises, `OnConfiguring` is sufficient.

---

## OnModelCreating

`OnModelCreating` is a virtual method on `DbContext` that EF Core calls during model initialization — after it has applied conventions, but before it finalizes the model. You override it to provide configuration that cannot be expressed through Data Annotations.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Example: configure a composite primary key (requires Fluent API)
    modelBuilder.Entity<Enrollment>()
        .HasKey(e => new { e.StudentId, e.CourseId });

    // Example: configure table and column names
    modelBuilder.Entity<Book>()
        .ToTable("tbl_books")
        .Property(b => b.Title)
        .HasMaxLength(300)
        .IsRequired();
}
```

For today, you do not need to write complex `OnModelCreating` code — Data Annotations cover the exercise requirements. Thursday's module covers Fluent API configuration in depth.

---

## The EF Core CLI: dotnet ef

The `dotnet ef` command-line tool drives migration management. It must be installed globally (or as a local tool):

```bash
dotnet tool install --global dotnet-ef
```

All migration commands are run from the **project directory** containing your `DbContext`.

---

## Creating a Migration

A **migration** is a snapshot of the difference between your current EF Core model and the last known database schema. It is represented as two generated C# files.

```bash
dotnet ef migrations add InitialCreate
```

Replace `InitialCreate` with a descriptive name that reflects what changed (e.g., `AddProductPriceColumn`, `CreateOrdersTable`).

**What this command does:**
1. Inspects your `DbContext` and all entity types.
2. Compares the current model to the last applied migration's model snapshot.
3. Generates C# code representing the difference as a set of schema operations.

---

## Applying a Migration

```bash
dotnet ef database update
```

This command:
1. Opens a connection to the database using your configured connection string.
2. Checks the `__EFMigrationsHistory` table to see which migrations have already been applied.
3. Runs any pending migrations in order.
4. Updates `__EFMigrationsHistory`.

On first run, the database file is created (for SQLite) if it does not exist.

**Apply up to a specific migration:**

```bash
dotnet ef database update AddProductPriceColumn
```

**Roll back to a previous migration** (reverts the schema):

```bash
dotnet ef database update PreviousMigrationName
```

---

## Understanding the Generated Migration Files

When you run `dotnet ef migrations add`, EF Core creates a `Migrations/` folder with three files.

### The Migration File

`Migrations/20260415120000_InitialCreate.cs`

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Authors",
            columns: table => new
            {
                Id   = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Authors", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Books",
            columns: table => new
            {
                Id       = table.Column<int>(type: "INTEGER", nullable: false)
                                .Annotation("Sqlite:Autoincrement", true),
                Title    = table.Column<string>(type: "TEXT", nullable: false),
                AuthorId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Books", x => x.Id);
                table.ForeignKey(
                    name: "FK_Books_Authors_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Authors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Books");
        migrationBuilder.DropTable(name: "Authors");
    }
}
```

- **`Up()`** contains the operations to apply the migration (schema forward).
- **`Down()`** contains the operations to revert it (schema rollback).

These are C# files — you can edit them if the generated SQL needs adjustment, though this is rarely necessary.

### The Model Snapshot

`Migrations/AppDbContextModelSnapshot.cs`

EF Core maintains a **model snapshot** — a C# representation of the entire schema at the time of the last migration. This is how EF Core knows what changed when you add the next migration. Do not edit this file manually.

### The Migration History Table

When migrations are applied, EF Core records them in `__EFMigrationsHistory`:

```sql
SELECT * FROM __EFMigrationsHistory;
-- MigrationId                                     | ProductVersion
-- 20260415120000_InitialCreate                    | 9.0.0
```

---

## Common Migration Workflow

```bash
# 1. Make changes to your entity classes
# 2. Generate a migration
dotnet ef migrations add DescriptiveName

# 3. Review the generated migration file in Migrations/
# 4. Apply it
dotnet ef database update

# 5. If you need to undo the last unapplied migration before applying:
dotnet ef migrations remove
```

---

## Summary

- `DbContext` is configured either via `OnConfiguring` (console apps) or constructor injection (ASP.NET Core).
- `OnModelCreating` is the hook for configuration that cannot be expressed through Data Annotations — you will use it heavily on Thursday.
- `dotnet ef migrations add <Name>` generates C# migration files representing schema changes.
- `dotnet ef database update` applies pending migrations to the database.
- Each migration consists of an `Up()` method (apply) and a `Down()` method (revert).
- EF Core tracks applied migrations in the `__EFMigrationsHistory` table.

---

## Additional Resources

- [Microsoft Docs — Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Microsoft Docs — dotnet ef CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [Microsoft Docs — DbContext Configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
