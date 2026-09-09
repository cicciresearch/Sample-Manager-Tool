using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class DeviceDetailsModel : PageModel
{
    private readonly SampleDbContext _database;

    // Gives the page access to the Sample Manager database.
    public DeviceDetailsModel(SampleDbContext database)
    {
        _database = database;
    }

    public Device Device { get; set; } = null!;
    public List<Measurement> Measurements { get; set; } = [];
    public Measurement? SelectedMeasurement { get; set; }

    // Loads one pixel, its batch/sample context and its complete measurement history.
    public async Task<IActionResult> OnGetAsync(
        Guid id,
        Guid? measurementId)
    {
        var device = await _database.Devices
            .AsNoTracking()
            .Include(device => device.Sample)
                .ThenInclude(sample => sample.Batch)
            .Include(device => device.Measurements)
                .ThenInclude(measurement => measurement.Jv)
            .Include(device => device.Measurements)
                .ThenInclude(measurement => measurement.Eqe)
            .AsSplitQuery()
            .FirstOrDefaultAsync(device => device.Id == id);

        if (device == null)
            return NotFound();

        Device = device;

        Measurements = device.Measurements
            .OrderByDescending(measurement => measurement.MeasuredAt)
            .ThenByDescending(measurement => measurement.Id)
            .ToList();

        if (measurementId.HasValue)
        {
            SelectedMeasurement = Measurements
                .FirstOrDefault(measurement =>
                    measurement.Id == measurementId.Value);

            if (SelectedMeasurement == null)
                return NotFound();
        }
        else
        {
            // The newest measurement is selected when the page is first opened.
            SelectedMeasurement = Measurements.FirstOrDefault();
        }

        return Page();
    }

    // Returns the small result highlighted in the measurement history.
    public string GetMeasurementHighlight(Measurement measurement)
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

            _ => "No summary"
        };
    }

    // Formats nullable scientific results for display.
    public string FormatValue(
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