using Cicci.SampleManager.Measurements.Common;
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
    // Calculates one batch summary for each registered measurement type.
    public static List<BatchMeasurementStatistics> GetBatchSummaries(
        Batch batch,
        IReadOnlyCollection<Measurement> latestMeasurements)
    {
        var summaries =
            new List<BatchMeasurementStatistics>();

        foreach (var definition in MeasurementRegistry.All
            .OrderBy(definition =>
                definition.Type.ToString()))
        {
            var summary = CalculateBatchSummary(
                batch,
                latestMeasurements,
                definition);

            if (summary != null &&
                summary.MeasuredDevices > 0)
            {
                summaries.Add(summary);
            }
        }

        return summaries;
    }

    // Calculates statistics from the newest measurement of this type
    // for each pixel.
    private static BatchMeasurementStatistics? CalculateBatchSummary(
        Batch batch,
        IReadOnlyCollection<Measurement> latestMeasurements,
        IMeasurementDefinition definition)
    {
        var statistic = definition.Statistic;

        if (statistic == null)
            return null;

        var typeMeasurements = latestMeasurements
            .Where(measurement =>
                measurement.Type == definition.Type)
            .ToList();

        var values = typeMeasurements
            .Select(definition.GetStatisticValue)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var summary =
            new BatchMeasurementStatistics
            {
                Type = definition.Type,
                MetricName = statistic.MetricName,
                Unit = statistic.Unit,

                TotalDevices = batch.Samples
                    .Sum(sample =>
                        sample.Devices.Count),

                MeasuredDevices =
                    typeMeasurements.Count,

                ValidResults =
                    values.Count,

                Average =
                    GetAverage(values),

                StandardDeviation =
                    GetStandardDeviation(values),

                Minimum =
                    values.Count > 0
                        ? values.Min()
                        : null,

                Maximum =
                    values.Count > 0
                        ? values.Max()
                        : null
            };

        foreach (var sample in batch.Samples
            .OrderBy(sample =>
                sample.Code))
        {
            var deviceIds = sample.Devices
                .Select(device =>
                    device.Id)
                .ToHashSet();

            var sampleMeasurements =
                typeMeasurements
                    .Where(measurement =>
                        deviceIds.Contains(
                            measurement.DeviceId))
                    .ToList();

            var sampleValues =
                sampleMeasurements
                    .Select(
                        definition.GetStatisticValue)
                    .Where(value =>
                        value.HasValue)
                    .Select(value =>
                        value!.Value)
                    .ToList();

            summary.Samples.Add(
                new SampleMeasurementStatistics
                {
                    SampleId = sample.Id,
                    SampleCode = sample.Code,

                    TotalDevices =
                        sample.Devices.Count,

                    MeasuredDevices =
                        sampleMeasurements.Count,

                    Average =
                        GetAverage(sampleValues)
                });
        }

        return summary;
    }

    // Calculates the arithmetic mean, or NULL if there are no valid values.
    private static double? GetAverage(
        List<double> values)
    {
        return values.Count > 0
            ? values.Average()
            : null;
    }

    // Calculates sample standard deviation using n - 1.
    private static double? GetStandardDeviation(
        List<double> values)
    {
        if (values.Count < 2)
            return null;

        var average =
            values.Average();

        var sumSquaredDifferences =
            values.Sum(value =>
                Math.Pow(
                    value - average,
                    2));

        return Math.Sqrt(
            sumSquaredDifferences /
            (values.Count - 1));
    }
}