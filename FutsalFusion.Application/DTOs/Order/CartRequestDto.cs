namespace FutsalFusion.Application.DTOs.Order;

public class CartRequestDto
{
    public decimal TotalAmount { get; set; }

    public decimal GrandTotal { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal ShippingCharge { get; set; }
    
    public decimal EstimatedTax { get; set; }
    
    public List<CartProducts> CartProductsList { get; set; }
}

public class CartProducts
{
    public Guid CartId { get; set; }
    
    public Guid Id { get; set; }
    
    public string ImageUrl { get; set; }
    
    public string Title { get; set; }
    
    public string AddedDate { get; set; }
    
    public string Price { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalAmount { get; set; }
}