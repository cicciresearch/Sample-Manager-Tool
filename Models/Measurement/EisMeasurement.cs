namespace Cicci.SampleManager.Models;

public class EisMeasurement
{
    public Guid MeasurementId { get; set; }
    public Measurement Measurement { get; set; } = null!;

    public double? PeakFreq { get; set; }
}