using Microsoft.AspNetCore.Mvc;
using ProductCatalog.DTOs;
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
    // PATCH is for updating specific information on a resource/row
    // PUT is for replacing a resource/row
    [HttpPatch]
    public async Task<ActionResult> AddTagToProduct(TagProductDTO updateInfo)
    {
        try
        {
            // We call the service layer (which then calls the repo layer)
            await _productService.AddTagToProductAsync(updateInfo);
        } catch (Exception e)
        {
            return BadRequest(e.Message); 
        }

        return NoContent(); // Since we aren't echoing back to updated object, we can return
        // a 204 No Content success response
    }




}
