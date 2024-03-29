using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FutsalFusion.Application.DTOs.Futsal;

public class FutsalRequestDto
{
    public Guid Id { get; set; } = Guid.Empty;
    
    public string FutsalName { get; set; }
    
    public string FutsalDescription { get; set; }
    
    public string FutsalPhrase { get; set; }
    
    public string FutsalLocationAddress { get; set; }
    
    public string FutsalCity { get; set; }
    
    [Display(Name = "Futsal Owner Name")]
    public string OwnerName { get; set; }

    [Display(Name = "Futsal Owner Phone Number")]
    public string OwnerPhoneNumber { get; set; }

    public IFormFile? OwnerProfileImage { get; set; }

    [Display(Name = "Futsal Owner Username")]
    public string OwnerUsername { get; set; }

    [Display(Name = "Futsal Owner Email Address")]
    public string OwnerEmail { get; set; }

    public string? OwnerAddress { get; set; }

    public string? OwnerState { get; set; }
    
    public List<CourtRequestDto>?Courts { get; set; }
}

public class CourtRequestDto
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public int Type { get; set; }
    
    public List<IFormFile> Images { get; set; }
}