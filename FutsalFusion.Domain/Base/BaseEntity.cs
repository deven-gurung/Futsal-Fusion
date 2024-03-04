using System.ComponentModel.DataAnnotations;

namespace FutsalFusion.Domain.Base;

public class BaseEntity<TPrimaryKey>
{
    [Key]
    public TPrimaryKey Id { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public Guid CreatedBy { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Guid? LastModifiedBy { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public Guid? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}