namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalOwnerResponseDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public string ImageURL { get; set; }
    
    public string Email { get; set; }
    
    public string FutsalName { get; set; }
    
    public string FutsalLocation { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public int TotalAppointments { get; set; }
}