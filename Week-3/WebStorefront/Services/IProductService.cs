
using ProductCatalog.Models;

namespace ProductCatalog.Services;

public interface IProductService
{
    //Add my controller methods
    Task<List<Product>> GetAllProductsAsync();
}