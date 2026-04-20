using ProductCatalog.Data;
using ProductCatalog.Models;

namespace ProductCatalog.Services;
public class CategoryService : ICategoryService // Remember to implement your interfaces
{
    // We will need a constructor eventually
    // And we will need to implement the ICategoryService interface
    private readonly ICategoryRepo _repo;

    public CategoryService(ICategoryRepo repo)
    {
        _repo = repo;
    }

    //Get all categories. This method is called by the controller
    //And it itself calls upon the repo layer
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        // Creating a list called result to hold whatever we get back from the data layer
        List<Category> result = await _repo.GetAllCategoriesAsync();

        // Doing... whatever we need to do here. 
        // Potentially mapping our list of raw category objects to simplified DTO objects
        // and maybe some future business logic? 

        //Maybe a basic check, if somehow we didn't get back any categories 
        if(result is null)
            throw new NullReferenceException("Somehow... no categories?");

        return result;
    }


}