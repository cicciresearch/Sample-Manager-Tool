using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Data;

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<ResearchUser> ResearchUsers => Set<ResearchUser>();

    // Batches, Samples, Devices, and DeviceStacks are stored in separate tables.
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceStack> DeviceStacks => Set<DeviceStack>();
    public DbSet<StackLayer> StackLayers => Set<StackLayer>();

    // Measurements are stored in a single table, with a discriminator column to distinguish between different measurement types.
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<JvMeasurement> JvMeasurements => Set<JvMeasurement>();
    public DbSet<EqeMeasurement> EqeMeasurements => Set<EqeMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Batch code must be unique.
        modelBuilder.Entity<Batch>()
            .HasIndex(batch => batch.Code)
            .IsUnique();

        // A Batch contains many Samples.
        modelBuilder.Entity<Batch>()
            .HasMany(batch => batch.Samples)
            .WithOne(sample => sample.Batch)
            .HasForeignKey(sample => sample.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // A ResearchUser has many Batches.
        modelBuilder.Entity<ResearchUser>()
            .HasIndex(user => user.Name)
            .IsUnique();

        modelBuilder.Entity<ResearchUser>()
            .HasMany(user => user.Batches)
            .WithOne(batch => batch.User)
            .HasForeignKey(batch => batch.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sample codes must be unique inside each Batch.
        modelBuilder.Entity<Sample>()
            .HasIndex(sample => new
            {
                sample.BatchId,
                sample.Code
            })
            .IsUnique();

        // A Sample contains many Devices.
        modelBuilder.Entity<Sample>()
            .HasMany(sample => sample.Devices)
            .WithOne(device => device.Sample)
            .HasForeignKey(device => device.SampleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pixel IDs must be unique inside each Sample.
        modelBuilder.Entity<Device>()
            .HasIndex(device => new
            {
                device.SampleId,
                device.Pixel
            })
            .IsUnique();

        modelBuilder.Entity<Batch>()
            .HasOne(batch => batch.DeviceStack)
            .WithMany(stack => stack.Batches)
            .HasForeignKey(batch => batch.DeviceStackId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeviceStack>()
            .HasMany(stack => stack.Layers)
            .WithOne(layer => layer.DeviceStack)
            .HasForeignKey(layer => layer.DeviceStackId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StackLayer>()
            .HasIndex(layer => new
            {
                layer.DeviceStackId,
                layer.Position
            })
            .IsUnique();

        // A Device can have any number of measurements over its lifetime.
        modelBuilder.Entity<Device>()
            .HasMany(device => device.Measurements)
            .WithOne(measurement => measurement.Device)
            .HasForeignKey(measurement => measurement.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        // This index makes queries such as "all JV measurements for P3,
        // newest first" efficient as the measurement database grows.
        modelBuilder.Entity<Measurement>()
            .HasIndex(measurement => new
            {
                measurement.DeviceId,
                measurement.Type,
                measurement.MeasuredAt
            });

        // Store the enum as readable strings such as "JV" and "EQE"
        // instead of database integers such as 0 and 1.
        modelBuilder.Entity<Measurement>()
            .Property(measurement => measurement.Type)
            .HasConversion<string>();

        // The type-specific measurement tables use MeasurementId as their primary key.
        modelBuilder.Entity<JvMeasurement>()
            .HasKey(jv => jv.MeasurementId);

        modelBuilder.Entity<EqeMeasurement>()
            .HasKey(eqe => eqe.MeasurementId);

        // JvMeasurement uses MeasurementId as both its primary key and
        // foreign key. This creates a one-to-one relationship.
        modelBuilder.Entity<Measurement>()
            .HasOne(measurement => measurement.Jv)
            .WithOne(jv => jv.Measurement)
            .HasForeignKey<JvMeasurement>(jv => jv.MeasurementId)
            .OnDelete(DeleteBehavior.Cascade);

        // EQE uses the same one-to-one pattern.
        modelBuilder.Entity<Measurement>()
            .HasOne(measurement => measurement.Eqe)
            .WithOne(eqe => eqe.Measurement)
            .HasForeignKey<EqeMeasurement>(eqe => eqe.MeasurementId)
            .OnDelete(DeleteBehavior.Cascade);
            
    }
}