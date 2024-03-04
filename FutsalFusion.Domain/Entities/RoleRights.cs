using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class RoleRights : BaseEntity<Guid>
{
    public Guid RoleId { get; set; }
    
    public Guid MenuId { get; set; }
    
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; }

    [ForeignKey("MenuId")]
    public virtual Menu Menu { get; set; }
}