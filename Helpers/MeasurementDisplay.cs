using Cicci.SampleManager.Measurements.Common;
using Cicci.SampleManager.Models;

namespace Cicci.SampleManager.Helpers;

// Temporary compatibility helper while measurement types are
// migrated to the new definition/registry architecture.
public static class MeasurementDisplay
{
    // Returns the compact result highlighted in measurement summaries.
    public static string GetHighlight(Measurement measurement)
    {
        var definition = MeasurementRegistry.Get(measurement.Type);

        // Registered measurement types use their own child-class behavior.
        if (definition != null)
        {
            return definition.GetHighlight(measurement);
        }
        else
        {
            return "Measured";
        }

    }
}