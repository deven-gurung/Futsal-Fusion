namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalResponseDto
{
    public Guid FutsalId { get; set; }

    public string FutsalImage { get; set; }

    public string FutsalName { get; set; }
    
    public string FutsalPhrase { get; set; }

    public string FutsalLocationAddress { get; set; }

    public string OwnerName { get; set; }

    public string? OwnerEmail { get; set; }
}

public class FutsalDetailResponseDto
{
    public Guid FutsalId { get; set; }

    public string FutsalName { get; set; }
    
    public string FutsalPhrase { get; set; }

    public string FutsalLocationAddress { get; set; }

    public string OwnerName { get; set; }

    public string? OwnerEmail { get; set; }
    
    public List<string> Images { get; set; }
    
    public List<CourtResponseDto> Courts { get; set; }
}

public class CourtResponseDto
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public int Type { get; set; }
    
    public List<string> Images { get; set; }
}