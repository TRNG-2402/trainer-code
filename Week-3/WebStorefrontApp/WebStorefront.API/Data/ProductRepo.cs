using Microsoft.EntityFrameworkCore;
using ProductCatalog.DTOs;
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

    //Method to add an existing tag to an existing product
    public async Task UpdateProductTagAsync(TagProductDTO updateInfo)
    {
        // First, we want to grab our product using it's primary key (coming from updateInfo)
        Product? product = await _context.Products
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.ProductId == updateInfo.ProductId);
        
        // Then, if asking the database for that product returns a null, throw an exception
        if (product is null)
            throw new KeyNotFoundException($"Product {updateInfo.ProductId} not found.");

        // Third, grab the tag with the PK from updateInfo
        Tag? tag = await _context.Tags.FindAsync(updateInfo.TagId);

        // If null, throw an exception
        if (tag is null)
            throw new KeyNotFoundException($"Tag {updateInfo.TagId} not found.");

        // We want to evaluate: does this relationship Tag <-> Product already exist?
        if (product.Tags.Any(t => t.TagId == updateInfo.TagId))
            throw new Exception("This product already contains this tag");
        
        // If this relationship doesn't already exist
        product.Tags.Add(tag);

        await _context.SaveChangesAsync();

    }

}