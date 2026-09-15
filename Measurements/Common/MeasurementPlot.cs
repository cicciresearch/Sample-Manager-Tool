namespace Cicci.SampleManager.Measurements.Common;

public class MeasurementPlot
{
    public string Title { get; set; } = "";

    public string XAxisLabel { get; set; } = "";

    public string YAxisLabel { get; set; } = "";

    public List<MeasurementPlotSeries> Series { get; set; } = [];
}

public class MeasurementPlotSeries
{
    public string Label { get; set; } = "";

    public List<MeasurementPlotPoint> Points { get; set; } = [];
}

public class MeasurementPlotPoint
{
    public double X { get; set; }

    public double Y { get; set; }
}