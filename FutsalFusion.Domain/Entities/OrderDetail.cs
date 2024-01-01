using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class OrderDetail : BaseEntity<Guid>
{
    public Guid KitId { get; set; }
    
    public Guid OrderId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal KitTotalAmount { get; set; }
}