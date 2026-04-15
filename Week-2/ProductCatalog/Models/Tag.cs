using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Models;

public class Tag
{
    [Key]
    public int TagId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    //Navigation property for many-to-many
    public List<Product> Products { get; set; } = new List<Product>();
}
