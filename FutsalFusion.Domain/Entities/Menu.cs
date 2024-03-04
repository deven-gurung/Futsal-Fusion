using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Menu : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;

    public Guid? ParentMenuId { get; set; }

    public int? SequenceNo { get; set; }

    public string? URL { get; set; }

    public string? IconClass { get; set; }
}