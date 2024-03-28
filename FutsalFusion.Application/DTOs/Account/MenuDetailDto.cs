namespace FutsalFusion.Application.DTOs.Account;

public class MenuDetailDto
{
    public Guid MenuId { get; set; }

    public string MenuName { get; set; }

    public int? ParentMenuId { get; set; }

    public int? SequenceNo { get; set; }

    public string? URL { get; set; }

    public string? IconClass { get; set; }

    public string? Area { get; set; }

    public string? Help { get; set; }
}