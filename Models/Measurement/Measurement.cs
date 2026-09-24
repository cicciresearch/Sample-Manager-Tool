using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

public class Measurement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    // Optional session relationship. Standalone measurements leave this foreign key NULL.
    public Guid? MeasurementSessionId { get; set; }
    public MeasurementSession? MeasurementSession { get; set; }

    public MeasurementType Type { get; set; }

    // Measurement timestamps are stored in UTC so measurements remain
    // unambiguous if data is accessed from systems in different time zones.
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    [MaxLength(2000)]
    public string? DataPath { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public JvMeasurement? Jv { get; set; }
    public EqeMeasurement? Eqe { get; set; }
    public EisMeasurement? Eis { get; set; }
}
