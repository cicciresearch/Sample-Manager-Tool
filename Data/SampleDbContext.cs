using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Data;

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceStack> DeviceStacks => Set<DeviceStack>();
    public DbSet<StackLayer> StackLayers => Set<StackLayer>();

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
    }
}