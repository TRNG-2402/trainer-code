using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;

namespace ProductCatalog.Models;

public class Product
{
    [Key] //denotes this field as my primary key
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty; //give it a default empty string

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    //Even my relationships will eventually be set in my models
    // Whether O -> M
    // Or O -> O
    // Or M -> M

    //Though I don't need to do this, I can also use annotations to set 
    //foreign key attiributes manually 

    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    [Required]
    public Category Category { get; set; } = null!;

    //Navigation property for the tag-product many-to-many
    public List<Tag> Tags { get; set; } = new List<Tag>();

}