using ProductCatalog.DTOs;
using ProductCatalog.Models;

namespace ProductCatalog.Data;

public interface IProductRepo
{
    Task<List<Product>> GetAllProductsAsync();

    Task UpdateProductTagAsync(TagProductDTO updateInfo);
}