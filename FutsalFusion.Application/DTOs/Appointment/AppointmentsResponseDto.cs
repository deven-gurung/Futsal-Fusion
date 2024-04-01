namespace FutsalFusion.Application.DTOs.Appointment;

public class AppointmentsResponseDto
{
    public Guid AppointmentId { get; set; }
    
    public Guid CourtId { get; set; }
    
    public string CourtName { get; set; }

    public string BookedDate { get; set; }
    
    public string AppointedDate { get; set; }

    public string AppointedUser { get; set; }

    public string? ActionDoneUser { get; set; }

    public string? ActionDoneDate { get; set; }
    
    public string TimeSlot { get; set; }

    public string TotalPrice { get; set; }

    public string PaymentStatus { get; set; }
    
    public bool IsApproved { get; set; }
    
    public bool IsActionCompleted { get; set; }
}