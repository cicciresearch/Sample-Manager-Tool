namespace Cicci.SampleManager.Models;

public class DarkJvMeasurement
{
    public Guid MeasurementId { get; set; }
    public Measurement Measurement { get; set; } = null!;
    public double? PeakElEqePercent { get; set; }
}