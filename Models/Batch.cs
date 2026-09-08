using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class Batch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string User { get; set; } = "";
    
    [Range(0, 1000)]
    public float? Area { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = "";

    public DateOnly ProductionDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? DeviceStackId { get; set; }
    public DeviceStack? DeviceStack { get; set; }

    public ICollection<Sample> Samples { get; set; } = new List<Sample>();
}