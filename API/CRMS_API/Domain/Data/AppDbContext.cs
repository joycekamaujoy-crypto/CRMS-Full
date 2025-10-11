using Microsoft.EntityFrameworkCore;
using CRMS_API.Domain.Entities;

namespace CRMS_API.Domain.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TelemetryPoint> TelemetryPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.OwnedVehicles)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Renter)
                .WithMany(u => u.RentedBookings)
                .HasForeignKey(b => b.RenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Plate)
                .IsUnique();

            modelBuilder.Entity<TelemetryPoint>()
                .HasOne(tp => tp.Vehicle)
                .WithMany(v => v.TelemetryPoints)
                .HasForeignKey(tp => tp.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TelemetryPoint>()
                .HasIndex(tp => new
                {
                    tp.VehicleId,
                    tp.TimeStamp
                });

        }
     }
}
