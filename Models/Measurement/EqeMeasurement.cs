namespace Cicci.SampleManager.Models;

public class EqeMeasurement
{
    public Guid MeasurementId { get; set; }
    public Measurement Measurement { get; set; } = null!;

    public double? IntegratedJscMilliampPerCm2 { get; set; }
    public double? PeakEqePercent { get; set; }
}