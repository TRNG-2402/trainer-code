using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.DTOs;

public class NewCategoryDTO
{
    // This is the bare minimum info that I need to have a user supply
    // to create a new Category row in my DB
    
    [Required]
    public string? Name { get; set; }
    public string? Description { get ; set; }

}