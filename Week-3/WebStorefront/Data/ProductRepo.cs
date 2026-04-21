using Microsoft.EntityFrameworkCore;
using ProductCatalog.Models;

namespace ProductCatalog.Data;

public class ProductRepo : IProductRepo
{
    // Take in our DbContext as a dependency
    // so we can use it to access our database
    private readonly AppDbContext _context;

    public ProductRepo(AppDbContext context)
    {
        _context = context;
    }

    // Method to get all Products from the database
    // Our repos should be Model/Entity specific. 
    public async Task<List<Product>> GetAllProductsAsync()
    {
        // All business logic is handled inside the service layer. 
        // All the repo/data layer cares about is grabbing objects from the database
        List<Product> result = await _context.Products.ToListAsync();

        return result;
    }

}