namespace Cicci.SampleManager.Measurements.DarkJV;

public class DarkJvPlotData
{
    public List<DarkJvPlotPoint> DarkJV { get; set; } = [];
    public List<DarkJvPlotPoint> EQE { get; set; } = [];


    public string? Error { get; set; }

    public bool HasData =>
        DarkJV.Count > 0;
}

public class DarkJvPlotPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}