namespace Cicci.SampleManager.Measurements.EIS;

public class EisPlotData
{
    public List<EisPlotPoint> Nyquist { get; set; } = [];
    public List<EisPlotPoint> BodeMagnitude { get; set; } = [];
    public List<EisPlotPoint> BodePhase { get; set; } = [];


    public string? Error { get; set; }

    public bool HasData =>
        Nyquist.Count > 0 ||
        BodeMagnitude.Count > 0 ||
        BodePhase.Count > 0;
}

public class EisPlotPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}