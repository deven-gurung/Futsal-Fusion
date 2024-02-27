using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutsalFusion.Domain.Helper;

[Table("Timing")]
public class Timing
{
    public Timing(string startTime, string endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }
    
    [Key]
    public int TimingId { get; set; }
    
    public string StartTime { get; set; }
    
    public string EndTime { get; set; }
        
    [ForeignKey("CourtId")]
    public virtual Court? Court { get; set; }
}