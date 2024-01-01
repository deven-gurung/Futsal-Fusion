using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class CourtImage : BaseEntity<Guid>
{
    public string ImageURL { get; set; }
    
    public int ImageType { get; set; }
    
    public Guid CourtId { get; set; }
    
    [ForeignKey("CourtId")]
    public virtual Court Court { get; set; }
}