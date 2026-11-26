using Microsoft.EntityFrameworkCore;

namespace SoftwareDeployment.Server.Data;

public sealed class DeploymentDbContext(DbContextOptions<DeploymentDbContext> options)
    : DbContext(options)
{
    public DbSet<ClientEntity> Clients => Set<ClientEntity>();
    public DbSet<PackageEntity> Packages => Set<PackageEntity>();
    public DbSet<PackageVersionEntity> PackageVersions => Set<PackageVersionEntity>();
    public DbSet<AssignmentEntity> Assignments => Set<AssignmentEntity>();
    public DbSet<ClientPackageStateEntity> ClientPackageStates => Set<ClientPackageStateEntity>();
    public DbSet<InstallLogEntity> InstallLogs => Set<InstallLogEntity>();
    public DbSet<SettingsEntity> Settings => Set<SettingsEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ClientEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.MachineName, x.Domain });
            e.Property(x => x.MachineName).HasMaxLength(256);
            e.Property(x => x.Domain).HasMaxLength(256);
        });

        b.Entity<PackageEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<PackageVersionEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Package)
                .WithMany(p => p.Versions)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.PackageId, x.Version }).IsUnique();
            e.Property(x => x.Version).HasMaxLength(128).IsRequired();
        });

        b.Entity<AssignmentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Package)
                .WithMany(p => p.Assignments)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade);
            // A principal cannot be assigned the same package twice.
            e.HasIndex(x => new { x.PackageId, x.TargetSid }).IsUnique();
            e.Property(x => x.TargetSid).HasMaxLength(256);
        });

        b.Entity<ClientPackageStateEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Client)
                .WithMany(c => c.PackageStates)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ClientId, x.PackageId }).IsUnique();
        });

        b.Entity<InstallLogEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Client)
                .WithMany(c => c.Logs)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.ReportedAtUtc);
        });

        b.Entity<SettingsEntity>(e =>
        {
            e.HasKey(x => x.Id);
            // Single fixed row (Id = 1); do not treat as auto-increment.
            e.Property(x => x.Id).ValueGeneratedNever();
        });
    }
}
