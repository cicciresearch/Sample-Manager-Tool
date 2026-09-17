using Cicci.SampleManager.Data;
using Cicci.SampleManager.Helpers;
using Cicci.SampleManager.Models;
using Cicci.SampleManager.Measurements.Common;
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

    public Dictionary<Guid, int> MeasurementCounts { get; set; } = [];

    public Dictionary<Guid, List<Measurement>> LatestMeasurementsByDevice
    { get; set; } = [];

    public int TotalMeasurements =>
        MeasurementCounts.Values.Sum();

    public int MeasuredDevices =>
        MeasurementCounts.Count(item =>
            item.Value > 0);

    // Loads one substrate, all its pixels and their measurement summaries.
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var sample = await _database.Samples
            .AsNoTracking()
            .Include(sample => sample.Batch)
            .Include(sample => sample.Devices)
            .FirstOrDefaultAsync(sample =>
                sample.Id == id);

        if (sample == null)
            return NotFound();

        var deviceIds = sample.Devices
            .Select(device => device.Id)
            .ToList();

        MeasurementCounts =
            await MeasurementLoader.LoadCountsForDevicesAsync(
                _database,
                deviceIds);

        var latestMeasurements =
            await MeasurementLoader.LoadLatestForDevicesAsync(
                _database,
                deviceIds);

        LatestMeasurementsByDevice =
            latestMeasurements
                .GroupBy(measurement =>
                    measurement.DeviceId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(measurement =>
                            measurement.Type)
                        .ToList());

        Sample = sample;

        return Page();
    }

    // Returns the newest measurement of each type for one pixel.
    public IReadOnlyList<Measurement> GetLatestMeasurements(
        Device device)
    {
        return LatestMeasurementsByDevice.TryGetValue(
            device.Id,
            out var measurements)
            ? measurements
            : [];
    }

    // Returns the complete measurement count for one pixel.
    public int GetMeasurementCount(Device device)
    {
        return MeasurementCounts.TryGetValue(
            device.Id,
            out var count)
            ? count
            : 0;
    }
    
    // Returns the shared compact result used by measurement overview pages.
    public string GetMeasurementHighlight(Measurement measurement)
    {
        return MeasurementDisplay.GetHighlight(measurement);
    }
}