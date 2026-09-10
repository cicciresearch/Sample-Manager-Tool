using Cicci.SampleManager.Models;

namespace Cicci.SampleManager.Helpers;

// Static helper centralizes measurement display logic shared by multiple Razor Pages.
public static class MeasurementDisplay
{
    // Returns the compact result highlighted when a measurement is shown in a summary.
    public static string GetHighlight(Measurement measurement)
    {
        return measurement.Type switch
        {
            MeasurementType.JV
                when measurement.Jv?.EfficiencyPercent is double efficiency
                => $"η {efficiency:0.##} %",

            MeasurementType.JV
                when measurement.Jv?.VocV is double voc
                => $"Voc {voc:0.###} V",

            MeasurementType.EQE
                when measurement.Eqe?.Jsc is double jsc
                => $"Jsc {jsc:0.##} mA/cm²",

            MeasurementType.EQE
                when measurement.Eqe?.PeakEQE is double peakEqe
                => $"Peak EQE {peakEqe:0.##} %",

            _ => "Measured"
        };
    }
}