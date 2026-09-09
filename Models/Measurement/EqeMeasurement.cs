namespace Cicci.SampleManager.Models;

public class EqeMeasurement
{
    public Guid MeasurementId { get; set; }
    public Measurement Measurement { get; set; } = null!;

    public double? Jsc { get; set; }
    public double? PeakEQE { get; set; }

}