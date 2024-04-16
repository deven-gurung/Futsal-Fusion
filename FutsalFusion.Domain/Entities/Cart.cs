using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Cart : BaseEntity<Guid>
{
    public Guid KitId { get; set; }
    
    public Guid UserId { get; set; }
    
    public int Count { get; set; }
    
    [ForeignKey("KitId")]
    public virtual Kit Kit { get; set; }
    
    [ForeignKey("UserId")]
    public virtual AppUser User { get; set; }
}