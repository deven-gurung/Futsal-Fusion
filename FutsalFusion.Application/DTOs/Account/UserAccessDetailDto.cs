namespace FutsalFusion.Application.DTOs.Account;

public class UserAccessDetailDto
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }
    
    public Guid MenuId { get; set; }
    
    public bool ShowMenu { get; set; }

    public MenuDetailDto MenuDetail { set; get; }
}