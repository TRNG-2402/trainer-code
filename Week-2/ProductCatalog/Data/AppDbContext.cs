using Microsoft.EntityFrameworkCore;
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
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source = ProductCatalog.db");
    }
}
