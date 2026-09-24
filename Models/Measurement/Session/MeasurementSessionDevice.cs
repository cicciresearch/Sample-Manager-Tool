namespace Cicci.SampleManager.Models;

public class MeasurementSessionDevice
{
    public Guid MeasurementSessionId { get; set; }
    public MeasurementSession MeasurementSession { get; set; } = null!;

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}