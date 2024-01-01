using System.ComponentModel.DataAnnotations;

namespace FutsalFusion.Application.DTOs.Identity;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}