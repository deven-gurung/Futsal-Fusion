namespace FutsalFusion.Application.DTOs.Order;

public class OrderResponseDto
{
    public string Id { get; set; }

    public Guid OrderId { get; set; }
    
    public bool IsEditable { get; set; }
    
    public string Description { get; set; }
    
    public int OrderStatus { get; set; }
    
    public string OrderedDate { get; set; }
    
    public decimal TotalAmount { get; set; }

    public decimal GrandTotal { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal ShippingCharge { get; set; }
    
    public decimal EstimatedTax { get; set; }
    
    public List<Products> ProductsList { get; set; }
}

public class Products
{
    public Guid Id { get; set; }
    
    public string ImageUrl { get; set; }
    
    public string Title { get; set; }
    
    public string AddedDate { get; set; }
    
    public int Quantity { get; set; }
    
    public string UnitPrice { get; set; }
    
    public decimal TotalAmount { get; set; }
}