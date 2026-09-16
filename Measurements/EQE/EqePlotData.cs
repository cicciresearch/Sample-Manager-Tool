namespace Cicci.SampleManager.Measurements.EQE;

public class EqePlotData
{
    public List<EqePlotPoint> EQE { get; set; } = [];
    public List<EqePlotPoint> Jsc { get; set; } = [];


    public string? Error { get; set; }

    public bool HasData =>
        EQE.Count > 0 ||
        Jsc.Count > 0;
}

public class EqePlotPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}