namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalGridResponseDto
{
    public Guid FutsalId { get; set; }
    
    public bool IsExotic { get; set; }
    
    public bool IsNew { get; set; }
    
    public bool IsPopular { get; set; }
    
    public string FutsalName { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string Slogan { get; set; }
    
    public int CurrentAppointments { get; set; }
    
    public int TotalBookings { get; set; }
    
    public string OwnerName { get; set; }
    
    public string OwnerImageUrl { get; set; }
}

