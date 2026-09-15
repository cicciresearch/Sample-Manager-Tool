using Cicci.SampleManager.Models;

namespace Cicci.SampleManager.Measurements.Common;

public interface IMeasurementDefinition
{
    MeasurementType Type { get; }

    string DisplayName { get; }

    IQueryable<Measurement> IncludeData(
        IQueryable<Measurement> query);

    string GetHighlight(
        Measurement measurement);

    MeasurementPresentation GetPresentation(
        Measurement measurement);

    MeasurementStatisticDefinition? Statistic { get; }

    double? GetStatisticValue(
        Measurement measurement);

    Task<IReadOnlyList<MeasurementPlot>> LoadPlotsAsync(
        Measurement measurement);
}