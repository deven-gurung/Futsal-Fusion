using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;
using FutsalFusion.Domain.Entities.Identity;

namespace FutsalFusion.Domain.Entities;

public class Appointment : BaseEntity<Guid>
{
    public Guid BookedUserId { get; set; }
    
    public Guid BookedCourtId { get; set; }
    
    public DateTime BookedDate { get; set; }
    
    public DateTime AppointedDate { get; set; }
    
    public string TimeSlot { get; set; }

    public bool IsApproved { get; set; } = false;

    public bool IsActionComplete { get; set; } = false;

    public bool IsPaid { get; set; } = false;
    
    public decimal NumberOfHours { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public string? FinalScore { get; set; }
    
    [ForeignKey("BookedUserId")]
    public virtual User User { get; set; }

    [ForeignKey("BookedCourtId")]
    public virtual Court Court { get; set; }
}