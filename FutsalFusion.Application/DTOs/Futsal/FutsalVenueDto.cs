namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalVenueDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public bool IsActive { get; set; }
    
    public string Description { get; set; }
    
    public string Phrase { get; set; }
    
    public string LocationAddress { get; set; }
    
    public string City { get; set; }
    
    public string OwnerName { get; set; }
    
    public string OwnerEmail { get; set; }
    
    public string RegisteredDate { get; set; }
    
    public int NumberOfCourts { get; set; }
    
    public int NumberOfAppointments { get; set; }
}