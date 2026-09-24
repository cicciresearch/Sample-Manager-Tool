namespace Cicci.SampleManager.Models;

// Broad session categories. These describe the experiment, not the individual measurements inside it.
public enum MeasurementSessionType
{
    // General-purpose grouping for a sequence of related measurements.
    General,

    // Long-running stability workflow that may contain repeated measurements and tracking data.
    Stability
}
