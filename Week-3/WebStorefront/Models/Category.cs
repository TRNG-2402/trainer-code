using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    //Our models contain our relationships 
    //One Category has many Products
    //In EF Core, representing this relationship is really simple
    //Our Category has a list of associated products within it
    public List<Product> Products { get; set; } = new List<Product>();
}