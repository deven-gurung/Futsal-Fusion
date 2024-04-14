using System.ComponentModel.DataAnnotations.Schema;
using FutsalFusion.Domain.Base;

namespace FutsalFusion.Domain.Entities;

public class AppointmentRequest : BaseEntity<Guid>
{
    public int RequestedPlayers { get; set; }
    
    public Guid AppointmentId { get; set; }

    [ForeignKey("AppointmentId")]
    public virtual Appointment Appointment { get; set; }
}