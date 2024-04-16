namespace FutsalFusion.Application.DTOs.Reservation;

public class ReservedAppointmentsDto
{
    public Guid PlayerId { get; set; }

    public string PlayerName { get; set; }

    public Guid AppointmentId { get; set; }
    
    public Guid FutsalId { get; set; }
    
    public string FutsalName { get; set; }
    
    public Guid CourtId { get; set; }
    
    public string CourtName { get; set; }

    public string CourtType { get; set; }
    
    public string BookedDate { get; set; }
    
    public string AppointedDate { get; set; }
    
    public string TimeSlot { get; set; }
    
    public bool IsAssignedPlayersRequired { get; set; }
    
    public int? NumberOfPlayers { get; set; }
    
    public bool IsEditable { get; set; }
    
    public List<AssignedPlayers> AssignedPlayersList { get; set; }
    
    public List<AssignedPlayers> RequestedPlayers { get; set; }
}

public class AssignedPlayers
{
    public Guid PlayerId { get; set; }
    
    public bool IsSelected { get; set; }
    
    public string ImageUrl { get; set; }
    
    public string PlayerName { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string Status { get; set; }
    
    public bool IsActive { get; set; }
}