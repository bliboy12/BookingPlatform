using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Api.Data;

// Generic Identity signature: <TUser, TRole, TKey>
// TKey = Guid because we changed ApplicationUser to IdentityUser<Guid>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ConsultantProfile> ConsultantProfiles => Set<ConsultantProfile>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<ConsultantSector> ConsultantSectors => Set<ConsultantSector>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Required first — wires up Identity's own tables (Users, Roles, Claims, etc.)
        // with the Guid key configuration. Skipping this silently breaks Identity.
        base.OnModelCreating(builder);

        // --- ConsultantProfile ---
        builder.Entity<ConsultantProfile>(entity =>
        {
            // 1-to-1 with ApplicationUser
            entity.HasOne(cp => cp.User)
                  .WithOne()
                  .HasForeignKey<ConsultantProfile>(cp => cp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Address is a value object, not its own table row —
            // OwnsOne embeds its columns directly into ConsultantProfiles table
            // (e.g. Address_StreetName, Address_City, etc.)
            entity.OwnsOne(cp => cp.Address);
        });

        // --- ConsultantSector (many-to-many join table) ---
        builder.Entity<ConsultantSector>(entity =>
        {
            entity.HasKey(cs => new { cs.ConsultantProfileId, cs.SectorId });

            entity.HasOne(cs => cs.ConsultantProfile)
                  .WithMany(cp => cp.ConsultantSectors)
                  .HasForeignKey(cs => cs.ConsultantProfileId);

            entity.HasOne(cs => cs.Sector)
                  .WithMany()
                  .HasForeignKey(cs => cs.SectorId);
        });

        // --- Availability ---
        builder.Entity<Availability>(entity =>
        {
            entity.HasOne(a => a.ConsultantProfile)
                  .WithMany(cp => cp.Availabilities)
                  .HasForeignKey(a => a.ConsultantProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Booking ---
        builder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Client)
                  .WithMany()
                  .HasForeignKey(b => b.ClientId)
                  .OnDelete(DeleteBehavior.Restrict); // don't cascade-delete bookings if a user is deleted

            entity.HasOne(b => b.ConsultantProfile)
                  .WithMany()
                  .HasForeignKey(b => b.ConsultantProfileId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Non-unique index — speeds up the conflict-detection query
            // (WHERE ConsultantProfileId = X AND StartTime overlaps ...)
            // Doesn't enforce uniqueness itself; overlap logic still needs to
            // happen in your BookingService, this just makes that query fast.
            entity.HasIndex(b => new { b.ConsultantProfileId, b.StartTime });

            // Store enum as string in the DB instead of int (0,1,2,3) —
            // makes the raw table human-readable and safer if you reorder the enum later
            entity.Property(b => b.Status).HasConversion<string>();
        });
    }
}