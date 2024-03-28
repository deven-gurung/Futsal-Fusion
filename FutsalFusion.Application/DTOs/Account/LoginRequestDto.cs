namespace FutsalFusion.Application.DTOs.Account;

public class LoginRequestDto
{
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    public string HiddenUsername { get; set; }
    
    public string HiddenPassword { get; set; }
}