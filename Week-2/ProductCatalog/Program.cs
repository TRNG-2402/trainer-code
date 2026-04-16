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
Console.WriteLine(context.Entry(foundCategory).State); // Compared to db, no changes

Console.WriteLine(foundCategory.Description); 

foundCategory.Description = "Update: Only non-computer electronics.";

Console.WriteLine(foundCategory.Description);
Console.WriteLine(context.Entry(foundCategory).State); // Compared to db, modified

context.SaveChanges(); //writing changes to the database

Console.WriteLine(context.Entry(foundCategory).State); // compared to db, again no changes


Product charger = new Product
{
  Name = "usb-c charger",
  Price = 17.99M,
  Stock = 10,
  Category = foundCategory
};

Product heater = new Product
{
  Name = "space heater",
  Price = 43.99M,
  Stock = 10,
  Category = foundCategory
};

Product fan = new Product
{
  Name = "desk fan",
  Price = 13.99M,
  Stock = 10,
  Category = foundCategory
};

context.Products.Add(charger);
context.Products.Add(heater);
context.Products.Add(fan);

context.SaveChanges();

Console.WriteLine(foundCategory.Products.Count);

Product piranesi = new Product
{
  Name = "Piranesi",
  Price = 15.99M,
  Stock = 10,
  CategoryId = 1
};

context.Products.Add(piranesi);
context.SaveChanges();

var bookCategory = context.Categories.Find(1);

Console.WriteLine(bookCategory.Name);

Console.WriteLine(bookCategory.Products[0].Name);