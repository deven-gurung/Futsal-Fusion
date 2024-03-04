using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class Role : BaseEntity<Guid>
{
    public string Name { get; set; }
    
    public string Description { get; set; }
}