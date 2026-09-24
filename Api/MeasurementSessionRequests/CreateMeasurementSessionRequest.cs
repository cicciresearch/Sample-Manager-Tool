using System.ComponentModel.DataAnnotations;
using Cicci.SampleManager.Models;

namespace Cicci.SampleManager.Api;

public class CreateMeasurementSessionRequest
{
    public List<Guid> DeviceIds { get; set; } = [];

    [MaxLength(200)]
    public string? Name { get; set; }

    public MeasurementSessionType Type { get; set; }
        = MeasurementSessionType.General;

    public DateTimeOffset? StartedAt { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}