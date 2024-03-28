using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class CourtPrice : BaseEntity<Guid>
{
    public Guid CourtId { get; set; }
    
    public decimal PricePerHour { get; set; }
    
    public TimeSpan TimeFrom { get; set; }

    public TimeSpan TimeTo { get; set; }
    
    [ForeignKey("CourtId")]
    public virtual Court Court { get; set; }
}