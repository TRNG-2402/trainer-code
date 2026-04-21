using Microsoft.EntityFrameworkCore;
using ProductCatalog.Models;

namespace ProductCatalog.Data;

public class CategoryRepo : ICategoryRepo
{
    // Take in our DbContext as a dependency
    // so we can use it to access our database
    private readonly AppDbContext _context;

    public CategoryRepo(AppDbContext context)
    {
        _context = context;
    }

    // Method to get all categories from the database
    // Our repos should be Model/Entity specific. 
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        // All business logic is handled inside the service layer. 
        // All the repo/data layer cares about is grabbing objects from the database
        List<Category> result = await _context.Categories.ToListAsync();

        return result;
    }

    public async Task<Category> CreateCategoryAsync(Category categoryToAdd)
    {
        _context.Categories.Add(categoryToAdd);
        await _context.SaveChangesAsync();
        
        // I want to have my repo method return the newly created record.
        // Including the generated PK, any timestamps that are created automatically,
        // etc. How can I have EF Core do this for me?

        // Trick question, when we call SaveChangesAsync() EF Core updates the local object
        // in our case categoryToAdd, to reflect what was created or updated in the DB
        return categoryToAdd;
    }

}