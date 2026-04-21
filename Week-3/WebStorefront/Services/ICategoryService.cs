
using ProductCatalog.DTOs;
using ProductCatalog.Models;

namespace ProductCatalog.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(NewCategoryDTO newCategory);
}