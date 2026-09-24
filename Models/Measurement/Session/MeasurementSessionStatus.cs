namespace Cicci.SampleManager.Models;

// Represents the lifecycle state of a measurement session.
public enum MeasurementSessionStatus
{
    // New measurements can still be associated with the session.
    Running,

    // The experiment finished normally.
    Completed,

    // The experiment ended before normal completion.
    Aborted
}
