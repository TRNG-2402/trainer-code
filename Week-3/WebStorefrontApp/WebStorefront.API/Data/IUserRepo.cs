using ProductCatalog.Models;

namespace ProductCatalog.Data;

// We follow the same pattern as IProductRepo / ICategoryRepo:
// the repo talks to EF Core, and nobody above the repo layer
// sees a DbContext. This keeps the service layer free of query code.
public interface IUserRepo
{
    
}