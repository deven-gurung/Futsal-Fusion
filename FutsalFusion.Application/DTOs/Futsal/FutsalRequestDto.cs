using Microsoft.AspNetCore.Http;

namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalRequestDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    public string OwnerName { get; set; }

    public string OwnerEmail { get; set; }

    public string? OwnerAddress { get; set; }

    public string? OwnerState { get; set; }
    
    public string FutsalName { get; set; }
    
    public string FutsalDescription { get; set; }
    
    public string FutsalPhrase { get; set; }
    
    public string FutsalLocationAddress { get; set; }
    
    public string FutsalCity { get; set; }
    
    public List<IFormFile> Images { get; set; }
    
    public List<CourtRequestDto> Courts { get; set; }
    
    public Guid CreatedBy { get; set; }
}

public class CourtRequestDto
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public int Type { get; set; }
    
    public List<IFormFile> Images { get; set; }
}