using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Controllers;

// www.someurl.com/api/Category/whatever/the/rest/of/the/route
[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    //Eventually here, I will create places to hold things like 
    // a CatergoryService object (injected at runtime) and maybe 
    // a logger

    public CategoryController()
    {
        //Eventually, we will take in a CategoryService object as a constructor argument
        //and set it here
    }

    //Below the constructor... I will begin adding my Controller methods

    //We'll start with Getting all Categories from the database
    //Our controller methods are NEVER void. They always return 
    //atleast a Task that is wrapping an ActionResult. 
    // public async Task<ActionResult<>>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        //I almost always (as in 100% of the time, cant think of a reason not to)
        //want to wrap the actual logic of my controller methods in a try-catch
        try
        {
            // In here, I will have a call to my service layer method. 
            // Even though this request is conceptually simple, I don't want 
            // to get in the habit of skipping layers
            // Controller <-> Service <-> Data <-> Database
        }
        catch (Exception e)
        {   
            //If we hit an exception (that can be thrown manually in some deeper layer
            // take that error message, and stick it in a 400 family HTTP response)
            return BadRequest(e.Message);
        }
    }



}
