using Cicci.SampleManager.Measurements.Common;
using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Measurements.DarkJV;

public class DarkJvDefinition : IMeasurementDefinition
{
    // Variables
    public MeasurementType Type => MeasurementType.DarkJV;

    public string DisplayName => "DarkJV";

    public MeasurementStatisticDefinition Statistic =>
        new()
        {
            MetricName = "Peak EQE",
            Unit = "%"
        };

    // Functions
    public IQueryable<Measurement> IncludeData(IQueryable<Measurement> query)
    {
        return query
            .Include(measurement => measurement.DarkJV);
    }

    public string GetHighlight(Measurement measurement)
    {
        return "Measured";
    }

    public MeasurementPresentation GetPresentation(Measurement measurement)
    {
        var presentation = new MeasurementPresentation
        {
            Title = "DarkJV results"
        };

        var darkjv = measurement.DarkJV;

        if (darkjv == null)
            return presentation;

        presentation.PrimaryResults.Add(new MeasurementResult
        {
            Label = "Peak EQE",
            Value = $"{darkjv.PeakElEqePercent} %"
        });

        return presentation;
    }

    public double? GetStatisticValue(Measurement measurement)
    {
        return measurement.DarkJV?.PeakElEqePercent;
    }

    public async Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(Measurement measurement)
    {
        if (string.IsNullOrWhiteSpace(measurement.DataPath))
            return [];

        var DarkJvData = await DarkJvDataFileReader.ReadAsync(measurement.DataPath);
        if (!DarkJvData.HasData)
            return [];

        var plotDarkJV = new MeasurementPlot
        {
            Title = "DarkJV curve",
            XAxisLabel = "Voltage (V)",
            YAxisLabel = "Current (A)"
        };

        if (DarkJvData.DarkJV.Count > 0)
        {
            plotDarkJV.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "DarkJV",
                    Points = DarkJvData.DarkJV
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        // var plotJsc = new MeasurementPlot
        // {
        //     Title = "EQE curve",
        //     XAxisLabel = "Current (A)",
        //     YAxisLabel = "EQE (%)"
        // };

        // if (DarkJvData.Jsc.Count > 0)
        // {
        //     plotJsc.Series.Add(
        //         new MeasurementPlotSeries
        //         {
        //             Label = "Jsc",
        //             Points = DarkJvData.Jsc
        //                 .Select(point =>
        //                     new MeasurementPlotPoint
        //                     {
        //                         X = point.X,
        //                         Y = point.Y
        //                     })
        //                 .ToList()
        //         });
        // }

        return [plotDarkJV];
    }
}

