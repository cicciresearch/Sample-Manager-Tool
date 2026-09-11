namespace Cicci.SampleManager.Models.Plotting;

public class JvPlotData
{
    public List<JvPlotPoint> Forward { get; set; } = [];
    public List<JvPlotPoint> Reverse { get; set; } = [];

    public string? Error { get; set; }

    public bool HasData =>
        Forward.Count > 0 ||
        Reverse.Count > 0;
}

public class JvPlotPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}