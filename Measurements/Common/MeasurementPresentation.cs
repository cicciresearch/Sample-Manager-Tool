namespace Cicci.SampleManager.Measurements.Common;

public class MeasurementPresentation
{
    public string Title { get; set; } = "";

    public List<MeasurementResult> PrimaryResults { get; set; } = [];

    public List<MeasurementResult> SecondaryResults { get; set; } = [];
}

public class MeasurementResult
{
    public string Label { get; set; } = "";

    public string Value { get; set; } = "";
}