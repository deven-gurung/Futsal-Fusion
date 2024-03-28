using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;
using FutsalFusion.Domain.Entities.Identity;

namespace FutsalFusion.Domain.Entities;

public class Futsal : BaseEntity<Guid>
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string Phrase { get; set; }
    
    public string LocationAddress { get; set; }
    
    public string City { get; set; }
    
    public Guid FutsalOwnerId { get; set; }
    
    [ForeignKey("FutsalOwnerId")]
    public virtual AppUser User { get; set; }
}