using Cicci.SampleManager.Models;
using Cicci.SampleManager.Measurements.JV;
using Cicci.SampleManager.Measurements.EQE;


namespace Cicci.SampleManager.Measurements.Common;

public static class MeasurementRegistry
{
    private static readonly Dictionary<
        MeasurementType,
        IMeasurementDefinition> Definitions =
        new()
        {
            [MeasurementType.JV] =
                new JvDefinition(),
            [MeasurementType.EQE] =
                new EqeDefinition(),
            [MeasurementType.EIS] =
                new EqeDefinition()
        };

    // Returns the definition registered for one measurement type.
    public static IMeasurementDefinition? Get(
        MeasurementType type)
    {
        Definitions.TryGetValue(
            type,
            out var definition);

        return definition;
    }

    // Returns all measurement definitions currently registered.
    public static IReadOnlyCollection<IMeasurementDefinition> All =>
        Definitions.Values;
}