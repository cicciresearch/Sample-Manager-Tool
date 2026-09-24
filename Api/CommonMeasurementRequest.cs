using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Api;

public class CommonMeasurementRequest
{
    // Optional session link shared by every measurement type.
    // NULL keeps standalone measurements independent of any session.
    public Guid? MeasurementSessionId { get; set; }

    public DateTimeOffset? MeasuredAt { get; set; }

    [MaxLength(2000)]
    public string? DataPath { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
