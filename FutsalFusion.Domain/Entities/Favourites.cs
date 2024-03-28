using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;
using FutsalFusion.Domain.Entities.Identity;

namespace FutsalFusion.Domain.Entities;

public class Favourites: BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    
    public Guid CourtId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual AppUser User { get; set; }
    
    [ForeignKey("CourtId")]
    public virtual Court Court { get; set; }
}