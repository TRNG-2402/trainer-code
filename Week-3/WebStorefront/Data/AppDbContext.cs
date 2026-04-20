using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models;

namespace ProductCatalog.Data;

//Every EF Core app has it's own DbContext. You create a class that inherits
//the exisitng DbContext class - then set your models inside. You can also
//do deep configuration for your entities/models here, though you may not want to
public class AppDbContext : DbContext
{
    // We use our models to tell EF Core what tables to create inside of this dbcontext
    // class. If we set up our models (and their relationships) correctly, this is
    // all we'll need.
    public DbSet<Category> Categories { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Tag> Tags { get; set; }

    // We're going to override a method that comes in from DbContext
    // to tell it where to find (or create) our database

    // I pulled in my old DbContext, but we no longer need to use OnConfiguring
    // We will supply the connection string + database type inside of Program.cs
    // not in this AppDbContext class

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder
    //         .UseSqlite("Data Source = ProductCatalog.db")
    //         .LogTo(Console.WriteLine, LogLevel.Information);
    // }

    // Level 1 Config: Conventions
    // Level 2 Config: Data Annotations
    // Level 3 Config: Fluent API inside of your DbContext class

    // Category Entity
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Lets configure the One to Many
        // Between Categories and Products
        // In order to do this, we need to set things for both entities.
        modelBuilder.Entity<Category>(entity =>
        {
            //Manually configuring a primary key
            entity.HasKey(c => c.CategoryId);

            //We can set the name as required and give it a max length
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        // Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.ProductId);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);

            // One to many: Each product belongs to one category
            entity
                .HasOne(p => p.Category) // Each Product belongs to a Category
                .WithMany(c => c.Products) // Each Category has many products
                .HasForeignKey(p => p.CategoryId) // The FK for this relationship
                .OnDelete(DeleteBehavior.Restrict); // Delete behavior


            //Many to many: Products can have many tags and vice versa
            //Note: we seed this relationship where we define it
            // because we didn't model the ProductTags entity ourself
            // we just let EF Core create it. 
            entity
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .UsingEntity(j => j.HasData(
                    new { ProductsProductId = 1, TagsTagId = 1 }
                ));
        });

        // While we're here... lets seed some data.
        // Inside of OnModelCreating, we can seed data with a method called
        // HasData(). This method inserts records when the schema is created
        // (at migration time, NOT during program startup)
        // One BIG difference, is that seed data MUST be provided with a PK
        // EF uses that provided PK to make sure it doesn't apply the seed data twice
        // over different migrations
        modelBuilder
            .Entity<Category>()
            .HasData(
                new Category
                {
                    CategoryId = 1,
                    Name = "Electronics",
                    Description = "Chargers and stuff"
                },
                new Category
                {
                    CategoryId = 2,
                    Name = "Books",
                    Description = "Books and stuff"
                },
                new Category
                {
                    CategoryId = 3,
                    Name = "Kitchen",
                    Description = "Pans and stuff"
                }
            );

        modelBuilder
            .Entity<Product>()
            .HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Thinkpad Charger",
                    Price = 1299.99M,
                    Stock = 10,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CategoryId = 1
                }
            );

        modelBuilder.Entity<Tag>().HasData(
            new Tag { TagId = 1, Name = "New Arrival" }
        );


    }
}
