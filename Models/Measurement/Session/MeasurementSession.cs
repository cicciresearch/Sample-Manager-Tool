using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class MeasurementSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string? Name { get; set; }

    public MeasurementSessionType Type { get; set; }
        = MeasurementSessionType.General;

    public MeasurementSessionStatus Status { get; set; }
        = MeasurementSessionStatus.Running;

    public DateTime StartedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public ICollection<MeasurementSessionDevice> Participants { get; set; }
        = new List<MeasurementSessionDevice>();

    public ICollection<Measurement> Measurements { get; set; }
        = new List<Measurement>();
}