using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Order : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    
    public string Description { get; set; }
    
    public decimal OrderTotal { get; set; }
    
    public int OrderStatus { get; set; }
    
    public int PaymentStatus { get; set; }

    public DateTime OrderedDate { get; set; }

    public string? TrackingNumber { get; set; }
    
    public string? Carrier { get; set; }
    
    public DateTime? PaymentDate { get; set; }
    
    public string? SessionId { get; set; }
    
    public string? PaymentIntendId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual AppUser User { get; set; }
}