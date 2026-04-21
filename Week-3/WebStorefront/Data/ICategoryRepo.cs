using ProductCatalog.Models;

namespace ProductCatalog.Data;

public interface ICategoryRepo
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(Category categoryToAdd);
    Task<Category?> GetCategoryByIdAsync(int id);
    Task DeleteCategoryAsync(Category categoryToDelete);
}