namespace FutsalFusion.Application.DTOs.Notification;

public class AppointmentNotificationDto
{
    public Guid AppointmentId { get; set; }
    
    public string OrganizedBy { get; set; }
    
    public string FutsalName { get; set; }
    
    public string CourtName { get; set; }
    
    public int ConfirmedPlayers { get; set; }
    
    public string CourtType { get; set; }
    
    public string AppointedDate { get; set; }
    
    public string TimeSlot { get; set; }
    
    public string BookedDate { get; set; }
    
    public int NumberOfPlayers { get; set; }

    public List<AssignedPlayers> AssignedPlayersList { get; set; } = new();
}

public class AssignedPlayers
{
    public Guid PlayerId { get; set; }
    
    public bool IsSelected { get; set; }
    
    public string PlayerName { get; set; }
    
    public string PhoneNumber { get; set; }
}