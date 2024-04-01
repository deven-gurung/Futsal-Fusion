namespace FutsalFusion.Application.DTOs.User;

public class UserResponseDto
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; }

    public string Username { get; set; }
    
    public string EmailAddress { get; set; }
    
    public string ImageURL { get; set; }
    
    public string RegisteredDate { get; set; }
    
    public string RoleName { get; set; }
}