namespace FutsalFusion.Application.DTOs.Product;

public class ProductDetailsDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Date { get; set; }
    
    public string RetailPrice { get; set; }
    
    public int Quantity { get; set; }
    
    public string Description { get; set; }
    
    public int AvailableStock { get; set; }
    
    public int NumberOfOrders { get; set; }
    
    public string Revenue { get; set; }
    
    public bool IsDiscountAvailable { get; set; }
}