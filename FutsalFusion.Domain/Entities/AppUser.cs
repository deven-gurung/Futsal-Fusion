using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class AppUser : BaseEntity<Guid>
{
    public Guid RoleId { get; set; }
    
    public string UserName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? MobileNo { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string? ImageURL { get; set; }
    
    [ForeignKey("RoleId")]
    public virtual AppRole Role { get; set; }
    
    public List<Appointment> Appointments { get; set; }

    public List<AppointmentDetail> AppointmentDetails { get; set; }

    public List<Notification> SentNotifications { get; set; }

    public List<Notification> ReceivedNotifications { get; set; }

	public virtual List<Team> Teams { get; set; }
}