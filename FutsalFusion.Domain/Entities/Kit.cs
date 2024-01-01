using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Kit : BaseEntity<Guid>
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public int Type { get; set; }
    
    public int Unit { get; set; }
    
    public decimal Price { get; set; }
    
    public string ImageURL { get; set; }
    
    public Guid FutsalId { get; set; }
    
    [ForeignKey("FutsalId")]
    public virtual Futsal Futsal { get; set; }
}