using ProductCatalog.Models;

namespace ProductCatalog.Data;

public interface ICategoryRepo
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(Category categoryToAdd);
}