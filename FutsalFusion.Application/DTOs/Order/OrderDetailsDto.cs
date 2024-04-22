namespace FutsalFusion.Application.DTOs.Order;

public class OrderDetailsDto
{
    public string Id { get; set; }
    
    public Guid OrderId { get; set; }
    
    public string CustomerName { get; set; }
    
    public string OrderedDate { get; set; }

    public string OrderedTime { get; set; }
    
    public int PaymentStatus { get; set; }
    
    public string OrderTotal { get; set; }
    
    public int OrderStatus { get; set; }
}