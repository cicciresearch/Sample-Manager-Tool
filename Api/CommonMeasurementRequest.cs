using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Api;

public class CommonMeasurementRequest
{
    public Guid? MeasurementSessionId { get; set; }

    public DateTimeOffset? MeasuredAt { get; set; }

    [MaxLength(2000)]
    public string? DataPath { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}