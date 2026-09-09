namespace Cicci.SampleManager.Api;

public class CreateJvMeasurementRequest : CreateMeasurementRequest
{
    public double? VocV { get; set; }
    public double? JscMilliampPerCm2 { get; set; }
    public double? FillFactorPercent { get; set; }
    public double? EfficiencyPercent { get; set; }

    public double? VmppV { get; set; }
    public double? JmppMilliampPerCm2 { get; set; }
    public double? PmppMilliwattPerCm2 { get; set; }
}