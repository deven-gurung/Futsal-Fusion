namespace FutsalFusion.Application.DTOs.Menu;

public class MenuDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public Guid? ParentMenuId { get; set; }
    
    public int? SequenceNumber { get; set; }
    
    public string? URL { get; set; }
    
    public string? IconClass { get; set; }
}