using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutsalFusion.Domain.Helper;

[Table("Court")]
public class Court
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public virtual ICollection<Reservation>? Reservations { get; set; }
    
    public virtual ICollection<Timing>? Timings { get; set; }
}