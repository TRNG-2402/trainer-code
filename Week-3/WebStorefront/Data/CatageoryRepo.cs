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

    // GetCategoryByIdAsync - finds a specific category record in our db
    // via its PK
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        // We can use FindAsync() to have EF do a SELECT by the PK
        // If it doesn't find anything, it returns null
        return await _context.Categories.FindAsync(id);
    }

    // DeleteCategoryAsync
    public async Task DeleteCategoryAsync(Category categoryToDelete)
    {
        // First, we mark the entity/row's state as Deleted in EF Core's change tracker
        _context.Categories.Remove(categoryToDelete);

        // Then we call SaveChangesAsync() to execute the DELETE SQL operation
        await _context.SaveChangesAsync();
    }
}