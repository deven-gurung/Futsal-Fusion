namespace FutsalFusion.Application.DTOs.Product;

public class ProductRequestDto
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public int Type { get; set; }
    
    public int Unit { get; set; }
    
    public decimal Price { get; set; }
}