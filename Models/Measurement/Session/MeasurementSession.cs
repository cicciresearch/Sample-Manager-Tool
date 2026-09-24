using System.ComponentModel.DataAnnotations;

namespace Cicci.SampleManager.Models;

// Groups measurements that belong to the same experimental run or workflow.
// A session can involve one or more devices and can contain multiple measurement types.
public class MeasurementSession
{
    // Stable identifier returned to ARKEO and stored with each related measurement.
    public Guid Id { get; set; } = Guid.NewGuid();

    // Optional human-readable label for the session.
    [MaxLength(200)]
    public string? Name { get; set; }

    // Describes the broad purpose of the session without tying it to one measurement type.
    public MeasurementSessionType Type { get; set; }
        = MeasurementSessionType.General;

    // Tracks whether the experimental workflow is still active or has ended.
    public MeasurementSessionStatus Status { get; set; }
        = MeasurementSessionStatus.Running;

    // Session timestamps are stored in UTC, consistent with individual measurements.
    public DateTime StartedAt { get; set; }
        = DateTime.UtcNow;

    // Remains NULL while the session is still running.
    public DateTime? EndedAt { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Explicit participant list. This records intended devices even if one never produces data.
    public ICollection<MeasurementSessionDevice> Participants { get; set; }
        = new List<MeasurementSessionDevice>();

    // Measurements actually produced during this session.
    public ICollection<Measurement> Measurements { get; set; }
        = new List<Measurement>();
}
