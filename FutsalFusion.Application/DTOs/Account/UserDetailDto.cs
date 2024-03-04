namespace FutsalFusion.Application.DTOs.Account;

public class UserDetailDto
{
    public Guid UserId { get; set; }
    
    public Guid RoleId { get; set; }

    public string UserName { get; set; }

    public string FullName { get; set; }
    
    public string? ImageName { get; set; }

    public string RoleName { get; set; }
}