using Cicci.SampleManager.Measurements.Common;
using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Measurements.EIS;

// Functions to implement:
// MeasurementPresentation GetPresentation(Measurement measurement);
// double? GetStatisticValue(Measurement measurement);
// Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(Measurement measurement);

public class EisDefinition : IMeasurementDefinition
{
    // Variables
    public MeasurementType Type => MeasurementType.EIS;

    public string DisplayName => "EIS";

    public MeasurementStatisticDefinition Statistic =>
        new()
        {
            MetricName = "Peak Frequency",
            Unit = "Hz"
        };

    // Functions
    public IQueryable<Measurement> IncludeData(IQueryable<Measurement> query)
    {
        return query
            .Include(measurement => measurement.Eis);
    }

    public string GetHighlight(Measurement measurement)
    {
        if (measurement.Eis?.PeakFreq is double PeakFreq)
            return $"Frequency {PeakFreq:0.###} Hz";
        return "Measured";
    }

    public MeasurementPresentation GetPresentation(Measurement measurement)
    {
        var presentation = new MeasurementPresentation
        {
            Title = "EIS results"
        };

        var eis = measurement.Eis;

        if (eis == null)
            return presentation;

        presentation.PrimaryResults.Add(new MeasurementResult
        {
            Label = "Peak Frequency",
            Value = $"{eis.PeakFreq} Hz"
        });

        return presentation;
    }

    public double? GetStatisticValue(Measurement measurement)
    {
        return measurement.Eis?.PeakFreq;
    }

    public async Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(Measurement measurement)
    {
        if (string.IsNullOrWhiteSpace(measurement.DataPath))
            return [];

        var EisData = await EisDataFileReader.ReadAsync(measurement.DataPath);
        if (!EisData.HasData)
            return [];

        var plotNyquist = new MeasurementPlot
        {
            Title = "Nyquist curve",
            XAxisLabel = "Z' (Ohm)",
            YAxisLabel = "Z'' (Ohm)"
        };

        if (EisData.Nyquist.Count > 0)
        {
            plotNyquist.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "Nyquist",
                    Points = EisData.Nyquist
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        var plotBodeMag = new MeasurementPlot
        {
            Title = "Bode curve (magnitude)",
            XAxisLabel = "Frequency (Hz)",
            YAxisLabel = "Magnitude (Ohm)"
        };

        if (EisData.Nyquist.Count > 0)
        {
            plotNyquist.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "Bode",
                    Points = EisData.BodeMagnitude
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        var plotBodePhase = new MeasurementPlot
        {
            Title = "Bode curve (phase)",
            XAxisLabel = "Frequency (Hz)",
            YAxisLabel = "Phase (°)"
        };

        if (EisData.Nyquist.Count > 0)
        {
            plotNyquist.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "Bode",
                    Points = EisData.BodePhase
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        return [plotNyquist,plotBodeMag,plotBodePhase];
    }
}

