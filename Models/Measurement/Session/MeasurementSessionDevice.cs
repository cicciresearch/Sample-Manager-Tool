namespace Cicci.SampleManager.Models;

// Join entity between sessions and devices.
// It records session participation independently of whether measurements were produced.
public class MeasurementSessionDevice
{
    // Session side of the composite key.
    public Guid MeasurementSessionId { get; set; }
    public MeasurementSession MeasurementSession { get; set; } = null!;

    // Device side of the composite key.
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}
