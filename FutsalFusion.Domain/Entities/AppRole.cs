using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class AppRole : BaseEntity<Guid>
{
    public string Name { get; set; }
}