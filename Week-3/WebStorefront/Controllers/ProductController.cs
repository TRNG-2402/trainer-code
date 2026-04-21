using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Models;
using ProductCatalog.Services;

namespace ProductCatalog.Controllers;

// www.someurl.com/api/Product/whatever/the/rest/of/the/route
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
   
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        //I almost always (as in 100% of the time, cant think of a reason not to)
        //want to wrap the actual logic of my controller methods in a try-catch
        try
        {
            // In here, I will have a call to my service layer method. 
            // Even though this request is conceptually simple, I don't want 
            // to get in the habit of skipping layers
            // Controller <-> Service <-> Data <-> Database
            return await _productService.GetAllProductsAsync(); // placeholder 200. Eventually, this 200 will also contain a list 
            // with every product in my db
        }
        catch (Exception e)
        {   
            //If we hit an exception (that can be thrown manually in some deeper layer
            // take that error message, and stick it in a 400 family HTTP response)
            return BadRequest(e.Message);
        }
    }


    //Add a PATCH to update a Product - changing it's category



}
