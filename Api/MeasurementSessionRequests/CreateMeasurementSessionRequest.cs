using System.ComponentModel.DataAnnotations;
using Cicci.SampleManager.Models;

namespace Cicci.SampleManager.Api;

// Defines the information ARKEO sends when starting a new measurement session.
public class CreateMeasurementSessionRequest
{
    // Devices intended to participate in the session. Duplicate GUIDs are removed by the controller.
    public List<Guid> DeviceIds { get; set; } = [];

    // Optional user-facing name for identifying the experiment later.
    [MaxLength(200)]
    public string? Name { get; set; }

    // Session category is independent of the JV/EQE/EIS measurement types it may contain.
    public MeasurementSessionType Type { get; set; }
        = MeasurementSessionType.General;

    // Optional start time supplied by ARKEO; the server uses the current UTC time when omitted.
    public DateTimeOffset? StartedAt { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
