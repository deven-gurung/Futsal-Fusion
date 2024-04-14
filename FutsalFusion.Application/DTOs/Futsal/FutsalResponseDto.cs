namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalResponseDto
{
    public Guid FutsalId { get; set; }

    public bool IsExotic { get; set; }
    
    public bool IsNew { get; set; }
    
    public bool IsPopular { get; set; }

    public bool IsActive { get; set; }
    
    public string FutsalName { get; set; }
    
    public string FutsalPhrase { get; set; }
    
    public string FutsalLocationAddress { get; set; }

    public string FutsalOverview { get; set; }

    public List<string> FutsalImages { get; set; }
    
    public string InitiatedDate { get; set; }
    
    public decimal TotalEarnings { get; set; }

    public string OwnerName { get; set; }

    public string? OwnerEmail { get; set; }
    
    public string? OwnerImageUrl { get; set; }
    
    public List<CourtResponseDto> Courts { get; set; }
    
    public List<FutsalWorkingHours> WorkingHours { get; set; }

    public List<FutsalWorkingHoursRequestDto> FutsalWorkingHours { get; set; }
}

public class CourtResponseDto
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string Type { get; set; }
    
    public List<string> Images { get; set; }
}

public class FutsalWorkingHours
{
    public int WeekDay { get; set; }
    
    public string Day { get; set; }
    
    public string TimePeriod { get; set; }
}