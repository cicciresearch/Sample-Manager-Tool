using Cicci.SampleManager.Models;

namespace Cicci.SampleManager.Helpers;

public class BatchMeasurementStatistics
{
    public MeasurementType Type { get; set; }

    public string MetricName { get; set; } = "";
    public string Unit { get; set; } = "";

    public int TotalDevices { get; set; }
    public int MeasuredDevices { get; set; }
    public int ValidResults { get; set; }

    public double? Average { get; set; }
    public double? StandardDeviation { get; set; }
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }

    public List<SampleMeasurementStatistics> Samples { get; set; } = [];
}

public class SampleMeasurementStatistics
{
    public Guid SampleId { get; set; }
    public string SampleCode { get; set; } = "";

    public int TotalDevices { get; set; }
    public int MeasuredDevices { get; set; }

    public double? Average { get; set; }
}

public static class MeasurementStatistics
{
    // Calculates one batch summary for each supported measurement type.
    public static List<BatchMeasurementStatistics> GetBatchSummaries(Batch batch)
    {
        var summaries = new List<BatchMeasurementStatistics>();

        foreach (var type in Enum.GetValues<MeasurementType>())
        {
            var summary = CalculateBatchSummary(batch, type);

            if (summary != null && summary.MeasuredDevices > 0)
                summaries.Add(summary);
        }

        return summaries;
    }

    // Calculates statistics using only the latest measurement of this type
    // for each pixel, so repeatedly measuring one pixel does not give it
    // more statistical weight than the other pixels.
    private static BatchMeasurementStatistics? CalculateBatchSummary(
        Batch batch,
        MeasurementType type)
    {
        var definition = GetMetricDefinition(type);

        if (definition == null)
            return null;

        var latestMeasurements = batch.Samples
            .SelectMany(sample => sample.Devices)
            .Select(device => GetLatestMeasurement(device, type))
            .Where(measurement => measurement != null)
            .Cast<Measurement>()
            .ToList();

        var values = latestMeasurements
            .Select(GetMetricValue)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var summary = new BatchMeasurementStatistics
        {
            Type = type,
            MetricName = definition.Value.MetricName,
            Unit = definition.Value.Unit,

            TotalDevices = batch.Samples
                .Sum(sample => sample.Devices.Count),

            MeasuredDevices = latestMeasurements.Count,
            ValidResults = values.Count,

            Average = GetAverage(values),
            StandardDeviation = GetStandardDeviation(values),
            Minimum = values.Count > 0 ? values.Min() : null,
            Maximum = values.Count > 0 ? values.Max() : null
        };

        foreach (var sample in batch.Samples.OrderBy(sample => sample.Code))
        {
            var sampleMeasurements = sample.Devices
                .Select(device => GetLatestMeasurement(device, type))
                .Where(measurement => measurement != null)
                .Cast<Measurement>()
                .ToList();

            var sampleValues = sampleMeasurements
                .Select(GetMetricValue)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            summary.Samples.Add(new SampleMeasurementStatistics
            {
                SampleId = sample.Id,
                SampleCode = sample.Code,
                TotalDevices = sample.Devices.Count,
                MeasuredDevices = sampleMeasurements.Count,
                Average = GetAverage(sampleValues)
            });
        }

        return summary;
    }

    // Finds the newest measurement of one type performed on one pixel.
    private static Measurement? GetLatestMeasurement(
        Device device,
        MeasurementType type)
    {
        return device.Measurements
            .Where(measurement => measurement.Type == type)
            .OrderByDescending(measurement => measurement.MeasuredAt)
            .ThenByDescending(measurement => measurement.Id)
            .FirstOrDefault();
    }

    // Defines which result is used as the primary statistic for each test.
    private static (string MetricName, string Unit)? GetMetricDefinition(
        MeasurementType type)
    {
        return type switch
        {
            MeasurementType.JV =>
                ("Efficiency", "%"),

            MeasurementType.EQE =>
                ("Integrated Jsc", "mA/cm²"),

            _ => null
        };
    }

    // Extracts the primary numerical result from one measurement.
    private static double? GetMetricValue(Measurement measurement)
    {
        return measurement.Type switch
        {
            MeasurementType.JV =>
                measurement.Jv?.EfficiencyPercent,

            MeasurementType.EQE =>
                measurement.Eqe?.Jsc,

            _ => null
        };
    }

    // Calculates the arithmetic mean, or NULL if there are no valid values.
    private static double? GetAverage(List<double> values)
    {
        return values.Count > 0
            ? values.Average()
            : null;
    }

    // Calculates sample standard deviation using n - 1.
    // At least two valid measurements are required.
    private static double? GetStandardDeviation(List<double> values)
    {
        if (values.Count < 2)
            return null;

        var average = values.Average();

        var sumSquaredDifferences = values
            .Sum(value => Math.Pow(value - average, 2));

        return Math.Sqrt(
            sumSquaredDifferences / (values.Count - 1)
        );
    }
}