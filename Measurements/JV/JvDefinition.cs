using Cicci.SampleManager.Measurements.Common;
using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Measurements.JV;

public class JvDefinition : IMeasurementDefinition
{
    public MeasurementType Type =>
        MeasurementType.JV;

    public string DisplayName =>
        "JV";

    public MeasurementStatisticDefinition Statistic =>
        new()
        {
            MetricName = "Efficiency",
            Unit = "%"
        };

    // Adds the JV-specific database row when Measurement data is queried.
    public IQueryable<Measurement> IncludeData(
        IQueryable<Measurement> query)
    {
        return query
            .Include(measurement => measurement.Jv);
    }

    // Returns the short result shown in measurement lists and summaries.
    public string GetHighlight(
        Measurement measurement)
    {
        if (measurement.Jv?.EfficiencyPercent is double efficiency)
            return $"η {efficiency:0.##} %";

        if (measurement.Jv?.VocV is double voc)
            return $"Voc {voc:0.###} V";

        return "Measured";
    }

    // Converts the JV-specific EF model into generic presentation data.
    public MeasurementPresentation GetPresentation(
        Measurement measurement)
    {
        var presentation = new MeasurementPresentation
        {
            Title = "JV results"
        };

        var jv = measurement.Jv;

        if (jv == null)
            return presentation;

        presentation.PrimaryResults.Add(
            new MeasurementResult
            {
                Label = "Efficiency",
                Value = FormatValue(
                    jv.EfficiencyPercent,
                    "%")
            });

        presentation.PrimaryResults.Add(
            new MeasurementResult
            {
                Label = "Voc",
                Value = FormatValue(
                    jv.VocV,
                    "V",
                    "0.###")
            });

        presentation.PrimaryResults.Add(
            new MeasurementResult
            {
                Label = "Jsc",
                Value = FormatValue(
                    jv.JscMilliampPerCm2,
                    "mA/cm²")
            });

        presentation.PrimaryResults.Add(
            new MeasurementResult
            {
                Label = "Fill factor",
                Value = FormatValue(
                    jv.FillFactorPercent,
                    "%")
            });

        presentation.SecondaryResults.Add(
            new MeasurementResult
            {
                Label = "Vmpp",
                Value = FormatValue(
                    jv.VmppV,
                    "V",
                    "0.###")
            });

        presentation.SecondaryResults.Add(
            new MeasurementResult
            {
                Label = "Jmpp",
                Value = FormatValue(
                    jv.JmppMilliampPerCm2,
                    "mA/cm²")
            });

        presentation.SecondaryResults.Add(
            new MeasurementResult
            {
                Label = "Pmpp",
                Value = FormatValue(
                    jv.PmppMilliwattPerCm2,
                    "mW/cm²")
            });

        return presentation;
    }

    // Returns the numerical value used for batch statistics.
    public double? GetStatisticValue(
        Measurement measurement)
    {
        return measurement.Jv?.EfficiencyPercent;
    }

    // Reads the raw ARKEO data file and converts it into the generic plot model.
    public async Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(
        Measurement measurement)
    {
        if (string.IsNullOrWhiteSpace(measurement.DataPath))
            return [];

        var jvData =
            await JvDataFileReader.ReadAsync(
                measurement.DataPath);

        if (!jvData.HasData)
            return [];

        var plot = new MeasurementPlot
        {
            Title = "JV curve",
            XAxisLabel = "Voltage (V)",
            YAxisLabel = "Current density (mA/cm²)"
        };

        if (jvData.Forward.Count > 0)
        {
            plot.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "Forward",
                    Points = jvData.Forward
                        .Select(point =>
                            new MeasurementPlotPoint
                            {
                                X = point.X,
                                Y = point.Y
                            })
                        .ToList()
                });
        }

        if (jvData.Reverse.Count > 0)
        {
            plot.Series.Add(
                new MeasurementPlotSeries
                {
                    Label = "Reverse",
                    Points = jvData.Reverse
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

    // Formats nullable scientific results consistently for the UI.
    private static string FormatValue(
        double? value,
        string unit,
        string format = "0.##")
    {
        if (!value.HasValue)
            return "—";

        return string.IsNullOrWhiteSpace(unit)
            ? value.Value.ToString(format)
            : $"{value.Value.ToString(format)} {unit}";
    }
}