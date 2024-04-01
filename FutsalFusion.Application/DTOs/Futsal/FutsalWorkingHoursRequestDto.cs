namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalWorkingHoursRequestDto
{
    public int WorkingDay { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
}