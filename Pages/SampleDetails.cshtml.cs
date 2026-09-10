using Cicci.SampleManager.Data;
using Cicci.SampleManager.Helpers;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class SampleDetailsModel : PageModel
{
    private readonly SampleDbContext _database;

    // Gives the page access to the Sample Manager database.
    public SampleDetailsModel(SampleDbContext database)
    {
        _database = database;
    }

    public Sample Sample { get; set; } = null!;

    public int TotalMeasurements =>
        Sample.Devices.Sum(device => device.Measurements.Count);

    public int MeasuredDevices =>
        Sample.Devices.Count(device => device.Measurements.Count > 0);

    // Loads one substrate, all its pixels and their measurement summaries.
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var sample = await _database.Samples
            .AsNoTracking()
            .Include(sample => sample.Batch)
            .Include(sample => sample.Devices)
                .ThenInclude(device => device.Measurements)
                    .ThenInclude(measurement => measurement.Jv)
            .Include(sample => sample.Devices)
                .ThenInclude(device => device.Measurements)
                    .ThenInclude(measurement => measurement.Eqe)
            .AsSplitQuery()
            .FirstOrDefaultAsync(sample => sample.Id == id);

        if (sample == null)
            return NotFound();

        Sample = sample;

        return Page();
    }

    // Returns only the newest measurement of each type for one pixel.
    public IEnumerable<Measurement> GetLatestMeasurements(Device device)
    {
        return device.Measurements
            .GroupBy(measurement => measurement.Type)
            .Select(group => group
                .OrderByDescending(measurement => measurement.MeasuredAt)
                .ThenByDescending(measurement => measurement.Id)
                .First())
            .OrderBy(measurement => measurement.Type);
    }

    // Returns the shared compact result used by measurement overview pages.
    public string GetMeasurementHighlight(Measurement measurement)
    {
        return MeasurementDisplay.GetHighlight(measurement);
    }
}