using ProductCatalog.Data;
using ProductCatalog.Models;

Console.WriteLine("Hello, World!");

Category books = new Category
{
  Name = "Books",
  Description = "Books and book accesories."  
};

Category electronics = new Category
{
  Name = "Electronics",
  Description = "Electronics and computer stuff."  
};

// For now, we will manually create our AppDbContext object
// Once we get to ASP, we'll let the dependency manager handle all that
var context = new AppDbContext(); //don't misuse var 

// Telling EF Core we want to add these objects as rows in our Categories table
context.Categories.Add(books);
context.Categories.Add(electronics);

Console.WriteLine("Created my objects... didn't save yet");

// Nothing get's written until...
context.SaveChanges();

Console.WriteLine("Saved changes, check db!");

context.Categories.Remove(books);
context.Categories.Remove(electronics);

Console.WriteLine("removed books category");

context.SaveChanges();

// Reading from my db
Category foundCategory = context.Categories.Find(2);

Console.WriteLine(foundCategory.Description);
