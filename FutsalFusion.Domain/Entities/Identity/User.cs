using Microsoft.AspNetCore.Identity;

namespace FutsalFusion.Domain.Entities.Identity;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; }

    public string? Address { get; set; }

    public string? State { get; set; }

    public string? ImageURL { get; set; }

    public List<Appointment> Appointments { get; set; }

    public List<AppointmentDetail> AppointmentDetails { get; set; }
}