using System.Reflection;
using System.Reflection.Emit;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Role = FutsalFusion.Domain.Entities.Identity.Role;

namespace FutsalFusion.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    // #region Identity Tables
    // public DbSet<User> Users { get; set; }
    //
    // public DbSet<UserRoles> UserRoles { get; set; }
    //
    // public DbSet<UserToken> UserToken { get; set; }
    //
    // public DbSet<UserLogin> UserLogin { get; set; }
    //
    // public DbSet<UserClaims> UserClaims { get; set; }
    //
    // public DbSet<RoleClaims> RoleClaims { get; set; }
    // #endregion

    #region Other Entities
    public DbSet<Appointment> Appointments { get; set; }
    
    public DbSet<AppointmentDetail> AppointmentDetails { get; set; }

    public DbSet<AppointmentRequest> AppointmentRequests { get; set; }
    
    public DbSet<AppRole> AppRoles { get; set; }

    public DbSet<AppUser> AppUsers { get; set; }

    public DbSet<Court> Courts { get; set; }

    public DbSet<CourtImage> CourtImages { get; set; }

    public DbSet<CourtPrice> CourtPrices { get; set; }
    
    public DbSet<Futsal> Futsals { get; set; }

    public DbSet<FutsalImage> FutsalImages { get; set; }
    
    public DbSet<Kit> Kits { get; set; }

    public DbSet<Menu> Menus { get; set; }
    
    public DbSet<Notification> Notifications { get; set; }
    
    public DbSet<Order> Orders { get; set; }
    
    public DbSet<OrderDetail> OrderDetails { get; set; }

    public DbSet<RoleRights> RoleRights { get; set; }

    public DbSet<Team> Teams { get; set; }
    
    public DbSet<WorkingHours> WorkingHours { get; set; }
    #endregion
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(builder);

        // #region Identity Entities Configuration
        // builder.Entity<User>().ToTable("Users");
        // builder.Entity<UserToken>().ToTable("Tokens");
        // builder.Entity<UserRoles>().ToTable("UserRoles");
        // builder.Entity<RoleClaims>().ToTable("RoleClaims");
        // builder.Entity<UserClaims>().ToTable("UserClaims");
        // builder.Entity<UserLogin>().ToTable("LoginAttempts");
        // #endregion

        builder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.BookedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<AppointmentDetail>()
            .HasOne(ad => ad.User)
            .WithMany(u => u.AppointmentDetails)
            .HasForeignKey(ad => ad.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Notification>(entity =>
		{
			entity.HasOne(n => n.Sender)
				.WithMany(u => u.SentNotifications)
				.HasForeignKey(n => n.SenderId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(n => n.Receiver)
				.WithMany(u => u.ReceivedNotifications)
				.HasForeignKey(n => n.ReceiverId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		builder.Entity<AppUser>(entity =>
		{
			entity.HasMany(u => u.SentNotifications)
				.WithOne(n => n.Sender)
				.HasForeignKey(n => n.SenderId);

			entity.HasMany(u => u.ReceivedNotifications)
				.WithOne(n => n.Receiver)
				.HasForeignKey(n => n.ReceiverId);
		});

		builder.Entity<Team>()
	        .HasOne<AppUser>(s => s.Player)
	        .WithMany(g => g.Teams)
	        .HasForeignKey(s => s.PlayerId)
	        .OnDelete(DeleteBehavior.Restrict);
	}
}