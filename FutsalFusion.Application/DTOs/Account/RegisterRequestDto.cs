using Microsoft.AspNetCore.Http;

namespace FutsalFusion.Application.DTOs.Account;

public class RegisterRequestDto
{
    public string Username { get; set; }
    
    public string FullName { get; set; }
    
    public string EmailAddress { get; set; }
    
    public string Password { get; set; }
    
    public IFormFile? Image { get; set; }
    
    public string HiddenUsername { get; set; }
    
    public string HiddenPassword { get; set; }

    public string HiddenChangePassword { get; set; }
}