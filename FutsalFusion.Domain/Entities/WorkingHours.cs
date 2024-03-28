using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class WorkingHours : BaseEntity<Guid>
{
    public int Day { get; set; }
    
    public Guid FutsalId { get; set; }
    
    public TimeSpan OpenTime { get; set; }

    public TimeSpan CloseTime { get; set; }

    [ForeignKey("FutsalId")]
    public virtual Futsal Futsal { get; set; }
}