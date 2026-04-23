using Microsoft.AspNetCore.Mvc;
using ProductCatalog.DTOs;
using ProductCatalog.Models;
using ProductCatalog.Services;

namespace ProductCatalog.Controllers;

// www.someurl.com/api/Category/whatever/the/rest/of/the/route
[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    //Eventually here, I will create places to hold things like 
    // a CatergoryService object (injected at runtime) and maybe 
    // a logger
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        //Eventually, we will take in a CategoryService object as a constructor argument
        //and set it here
        _categoryService = categoryService;
    }

    //Below the constructor... I will begin adding my Controller methods

    //We'll start with Getting all Categories from the database
    //Our controller methods are NEVER void. They always return 
    //atleast a Task that is wrapping an ActionResult. 
    // public async Task<ActionResult<>>

    // Notes on the ResponseCache Attribute:
    // Duration, delineated in seconds. How long should the client hold this info
    // Location Any: Allow client or any proxies to cache this
    // NoStore = false: we DO want to store it.

    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        //I almost always (as in 100% of the time, cant think of a reason not to)
        //want to wrap the actual logic of my controller methods in a try-catch
        // try
        // {
            // In here, I will have a call to my service layer method. 
            // Even though this request is conceptually simple, I don't want 
            // to get in the habit of skipping layers
            // Controller <-> Service <-> Data <-> Database
            return await _categoryService.GetAllCategoriesAsync(); // placeholder 200. Eventually, this 200 will also contain a list 
            // with every category in my db
        // }
        // catch (Exception e)
        // {   
            //If we hit an exception (that can be thrown manually in some deeper layer
            // take that error message, and stick it in a 400 family HTTP response)
            //return BadRequest(e.Message);
        //}
    }

    // POST to add a new Category, using our NewCategoryDTO
    // This method returns a full Category... the actual new row 
    // created in the DB, as our C# object. This isn't stricly required...
    // but it is the expected behavior for a RESTful API
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(NewCategoryDTO newCategory)
    {
        // try
        // {
            return await _categoryService.CreateCategoryAsync(newCategory);
        // } catch (Exception e)
        // {
        //     return BadRequest(e.Message);
        // }
    }

    // Deleting a Category via it's ID
    // We need so little info (just an int) 
    // that we don't need a DTO
    // www.mysite.com/api/Category/{id} - This is the route, ASP pulls the int we need from the route
    [HttpDelete("{categoryId}")]
    public async Task<ActionResult> DeleteCategory(int categoryId)
    {
        // Just to show off Global Exception Handling in ASP.NET - COMING SOON!
        // We won't even use a try-catch. 
        // try{
            await _categoryService.DeleteCategoryAsync(categoryId);
        // } 
        // catch (Exception e)
        // {
        //     return BadRequest(e.Message);
        // }
        
        return NoContent(); // Returns a 204 No Content - things went smooth, but no data return

    }


}
