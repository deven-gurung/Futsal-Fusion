using FutsalFusion.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace FutsalFusion.Domain.Helper;

public class FutsalReservationContext 
{
    public DbSet<User> User { get; set; }
    
    public DbSet<Court> Court { get; set; }
    
    public DbSet<Reservation> Reservation { get; set; }
    
    public DbSet<Timing> Timing { get; set; }
}