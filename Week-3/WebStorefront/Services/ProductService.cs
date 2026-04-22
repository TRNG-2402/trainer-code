using ProductCatalog.Data;
using ProductCatalog.DTOs;
using ProductCatalog.Models;

namespace ProductCatalog.Services;
public class ProductService : IProductService // Remember to implement your interfaces
{
    // We will need a constructor eventually
    // And we will need to implement the IProductService interface
    private readonly IProductRepo _repo;

    public ProductService(IProductRepo repo)
    {
        _repo = repo;
    }

    //Get all Products. This method is called by the controller
    //And it itself calls upon the repo layer
    public async Task<List<Product>> GetAllProductsAsync()
    {
        // Creating a list called result to hold whatever we get back from the data layer
        List<Product> result = await _repo.GetAllProductsAsync();

        // Doing... whatever we need to do here. 
        // Potentially mapping our list of raw product objects to simplified DTO objects
        // and maybe some future business logic? 

        //Maybe a basic check, if somehow we didn't get back any Products 
        if(result is null)
            throw new NullReferenceException("Somehow... no Products?");

        return result;
    }

    public async Task AddTagToProductAsync(TagProductDTO updateInfo)
    {
        // We can imagine having some business logic here
        // Perhaps if our schema/domain was larger, and we had more business rules 
        // we could check for product discontinuations, edit locks, etc
        await _repo.UpdateProductTagAsync(updateInfo);


    }

}