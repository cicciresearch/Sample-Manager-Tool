using Cicci.SampleManager.Measurements.Common;
using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Measurements.EQE;

// Functions to implement:
// MeasurementPresentation GetPresentation(Measurement measurement);
// double? GetStatisticValue(Measurement measurement);
// Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(Measurement measurement);

public class EqeDefinition : IMeasurementDefinition
{
    // Variables
    public MeasurementType Type => MeasurementType.EQE;

    public string DisplayName => "EQE";

    public MeasurementStatisticDefinition Statistic =>
        new()
        {
            MetricName = "Integrated Jsc",
            Unit = "mA/cm²"
        };

    // Functions
    public IQueryable<Measurement> IncludeData(IQueryable<Measurement> query)
    {
        return query
            .Include(measurement => measurement.Eqe);
    }

    public string GetHighlight(Measurement measurement)
    {
        if (measurement.Eqe?.Jsc is double jsc)
            return $"Jsc {jsc:0.###} mA/cm²";
        if (measurement.Eqe?.PeakEQE is double eqe)
            return $"PeakEQE {eqe:0.###} %";
        return "Measured";
    }

    public MeasurementPresentation GetPresentation(Measurement measurement)
    {
        var presentation = new MeasurementPresentation
        {
            Title = "EQE results"
        };

        var eqe = measurement.Eqe;

        if (eqe == null)
            return presentation;

        presentation.PrimaryResults.Add(new MeasurementResult
        {
            Label = "Jsc",
            Value = $"{eqe.Jsc} mA/cm²"
        });

        return presentation;
    }

    public double? GetStatisticValue(Measurement measurement)
    {
        return measurement.Eqe?.Jsc;
    }

    public async Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(Measurement measurement)
    {
        if (string.IsNullOrWhiteSpace(measurement.DataPath))
            return [];

        var EqeData = await EqeDataFileReader.ReadEQEAsync(measurement.DataPath);
        if (!EqeData.HasData)
            return [];

        var plot = new MeasurementPlot
        {
            Title = "EQE curve",
            XAxisLabel = "Wavelength (nm)",
            YAxisLabel = "EQE (%)"
        };

        if (EqeData.EQE.Count > 0)
        {
            plot.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "EQE",
                    Points = EqeData.EQE
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        if (EqeData.Jsc.Count > 0)
        {
            plot.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "Jsc",
                    Points = EqeData.Jsc
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        return [plot];
    }
}

