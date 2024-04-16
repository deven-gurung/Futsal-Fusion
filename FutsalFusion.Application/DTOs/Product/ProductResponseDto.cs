namespace FutsalFusion.Application.DTOs.Product;

public class ProductResponseDto
{
    public string Role { get; set; }
    
    public bool IsEditable { get; set; }
    
    public List<ProductDetails> ProductDetailsList { get; set; }
}

public class ProductDetails
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string ImageURL { get; set; }
    
    public string Category { get; set; }
    
    public string RegisteredDate { get; set; }
    
    public string Price { get; set; }
    
    public int Quantity { get; set; }
    
    public bool IsActive { get; set; }
}