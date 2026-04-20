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

}