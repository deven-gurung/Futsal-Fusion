using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;
using FutsalFusion.Domain.Entities.Identity;

namespace FutsalFusion.Domain.Entities;

public class AppointmentDetail : BaseEntity<Guid>
{
    public Guid AppointmentId { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public string PlayerStatus { get; set; }
    
    public string PlayerTeam { get; set; }
    
    [ForeignKey("AppointmentId")]
    public virtual Appointment Appointment { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual User User { get; set; }
}