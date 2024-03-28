using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class OrderDetail : BaseEntity<Guid>
{
    public Guid KitId { get; set; }
    
    public Guid OrderId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal KitTotalAmount { get; set; }
    
    [ForeignKey("Kit")]
    public virtual Kit Kit { get; set; }
    
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
}