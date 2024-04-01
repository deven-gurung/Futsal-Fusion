namespace FutsalFusion.Application.DTOs.User;

public class RoleRightsRequestDto
{
    public Guid MenuId { get; set; }

    public Guid? ParentMenuId { get; set; }
    
    public string MenuName { get; set; }
    
    public Guid RoleId { get; set; }
    
    public string URL { get; set; }
}