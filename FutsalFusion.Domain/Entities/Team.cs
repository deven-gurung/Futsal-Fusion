using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Team : BaseEntity<Guid>
{
    public Guid PlayerId { get; set; }
    
    public Guid AssigneeId { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual AppUser Player { get; set; }
    
    [ForeignKey("AssigneeId")]
    public virtual AppUser Assignee { get; set; }

    public virtual List<Team> Teams { get; set; }
}