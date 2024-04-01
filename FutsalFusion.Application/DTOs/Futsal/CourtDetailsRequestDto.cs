namespace FutsalFusion.Application.DTOs.Futsal;

public class CourtDetailsRequestDto
{
    public Guid FutsalId { get; set; }
    
    public string FutsalName { get; set; }
    
    public Guid CourtId { get; set; }
    
    public string CourtName { get; set; }

    public string CourtDescription { get; set; }
    
    public string Address { get; set; }

    public string OwnerName { get; set; }

    public string OwnerImage { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string EmailAddress { get; set; }
    
    public List<FutsalWorkingHours> WorkingHoursList { get; set; }
    
    public List<CourtPriceRange> CourtPrices { get; set; }
}

public class CourtPriceRange
{
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public decimal PricePerHour { get; set; }
}