using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class FutsalImage : BaseEntity<Guid>
{
    public string ImageURL { get; set; }

    public int ImageType { get; set; } = 1;
    
    public Guid FutsalId { get; set; }
    
    [ForeignKey("FutsalId")]
    public virtual Futsal Futsal { get; set; }
}