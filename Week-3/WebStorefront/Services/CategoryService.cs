using ProductCatalog.Data;
using ProductCatalog.DTOs;
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


    public async Task<Category> CreateCategoryAsync(NewCategoryDTO newCategory)
    {
        // We will do the mapping from the NewCategoryDTO -> The actual Category object here
        // traditionally I've seen it done in the Service layer

        // Lets create a new Category object from the info in our DTO
        // We will pass this to the repo layer for it to be inserted into the database
        Category newCat = new Category();

        // Mapping values from our DTO object to our new model
        newCat.Name = newCategory.Name;
        newCat.Description = newCategory.Description;

        // Returning whatever comes back from my repo/data layer method 
        return await _repo.CreateCategoryAsync(newCat);

    }

    public async Task DeleteCategoryAsync(int id)
    {
        // We probably want to check this atleast a little bit
        // PK's will (atleast since we offloaded their creation to EF Core)
        // never be 0 or negative
        if(id <= 0)
            throw new ArgumentOutOfRangeException("ID must be greater than 0!");

        // If the user gives us an int greater than 0 that doesn't correspond to any 
        // existing row in our DB, we don't want to silently fail.
        Category category = await _repo.GetCategoryByIdAsync(id);

        // If my GetCategoryById method finds nothing, category will be null
        // If so, alert the user
        if(category is null)
            throw new KeyNotFoundException("This category doesn't exist.");

        // If we made it this far, take that found Category object
        // pass it to the repo so it doesn't have to search again 
        // and then just wait for that to resolve. No return needed here. 
        await _repo.DeleteCategoryAsync(category);

    }

}