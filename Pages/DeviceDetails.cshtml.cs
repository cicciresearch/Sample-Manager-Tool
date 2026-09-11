using Cicci.SampleManager.Data;
using Cicci.SampleManager.Helpers;
using Cicci.SampleManager.Models;
using Cicci.SampleManager.Models.Plotting;
using Cicci.SampleManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class DeviceDetailsModel : PageModel
{
    private readonly SampleDbContext _database;

    private const int HistoryPageSize = 25;

    // Gives the page access to the Sample Manager database.
    public DeviceDetailsModel(SampleDbContext database)
    {
        _database = database;
    }

    public Device Device { get; set; } = null!;

    // Only the measurements belonging to the current history page.
    public List<Measurement> Measurements { get; set; } = [];

    // Latest measurement of each available measurement type.
    public List<Measurement> LatestMeasurements { get; set; } = [];

    public List<MeasurementTypeCount> MeasurementTypeCounts { get; set; } = [];

    public Measurement? SelectedMeasurement { get; set; }

    public MeasurementType? SelectedType { get; set; }

    public int TotalMeasurementCount { get; set; }
    public int FilteredMeasurementCount { get; set; }

    public int CurrentPage { get; set; } = 1;
    public int PageCount { get; set; } = 1;
    public int PageSize => HistoryPageSize;

    public JvPlotData? JvPlot { get; set; }

    

    // Loads device metadata, measurement summaries and one page of history.
    public async Task<IActionResult> OnGetAsync(
        Guid id,
        Guid? measurementId,
        MeasurementType? type,
        int historyPage = 1)
    {
        // Device metadata is loaded separately from measurements so opening
        // this page never loads the complete measurement history.
        var device = await _database.Devices
            .AsNoTracking()
            .Include(device => device.Sample)
                .ThenInclude(sample => sample.Batch)
            .FirstOrDefaultAsync(device => device.Id == id);

        if (device == null)
            return NotFound();

        Device = device;
        SelectedType = type;

        TotalMeasurementCount = await _database.Measurements
            .AsNoTracking()
            .CountAsync(measurement =>
                measurement.DeviceId == id);

        // Counts are used to build the All / JV / EQE / ... filter buttons.
        MeasurementTypeCounts = await _database.Measurements
            .AsNoTracking()
            .Where(measurement =>
                measurement.DeviceId == id)
            .GroupBy(measurement =>
                measurement.Type)
            .Select(group => new MeasurementTypeCount
            {
                Type = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        MeasurementTypeCounts = MeasurementTypeCounts
            .OrderBy(item => item.Type.ToString())
            .ToList();

        await LoadLatestMeasurementsAsync(id);

        var historyQuery = _database.Measurements
            .AsNoTracking()
            .Where(measurement =>
                measurement.DeviceId == id);

        if (SelectedType.HasValue)
        {
            historyQuery = historyQuery.Where(measurement =>
                measurement.Type == SelectedType.Value);
        }

        FilteredMeasurementCount =
            await historyQuery.CountAsync();

        PageCount = Math.Max(
            1,
            (int)Math.Ceiling(
                FilteredMeasurementCount /
                (double)HistoryPageSize)
        );

        CurrentPage = Math.Clamp(
            historyPage,
            1,
            PageCount
        );

        // Only one page of measurement history is loaded from SQLite.
        Measurements = await historyQuery
            .Include(measurement => measurement.Jv)
            .Include(measurement => measurement.Eqe)
            .OrderByDescending(measurement =>
                measurement.MeasuredAt)
            .ThenByDescending(measurement =>
                measurement.Id)
            .Skip((CurrentPage - 1) * HistoryPageSize)
            .Take(HistoryPageSize)
            .ToListAsync();

        if (measurementId.HasValue)
        {
            // The selected measurement is loaded independently of the current
            // history page. This also allows direct links to old measurements.
            SelectedMeasurement = await _database.Measurements
                .AsNoTracking()
                .Include(measurement => measurement.Jv)
                .Include(measurement => measurement.Eqe)
                .FirstOrDefaultAsync(measurement =>
                    measurement.Id == measurementId.Value &&
                    measurement.DeviceId == id);

            if (SelectedMeasurement == null)
                return NotFound();
        }
        else
        {
            // By default, display the newest measurement visible on this page.
            SelectedMeasurement = Measurements.FirstOrDefault();
        }

        if (SelectedMeasurement?.Type == MeasurementType.JV &&
            !string.IsNullOrWhiteSpace(SelectedMeasurement.DataPath))
        {
            JvPlot = await JvDataFileReader.ReadAsync(SelectedMeasurement.DataPath);
        }

        return Page();
    }

    // Loads only the newest measurement of every measurement type.
    private async Task LoadLatestMeasurementsAsync(Guid deviceId)
    {
        foreach (var typeCount in MeasurementTypeCounts)
        {
            // The number of measurement types is small, so one lightweight
            // query per type avoids loading hundreds of historical records.
            var latest = await _database.Measurements
                .AsNoTracking()
                .Include(measurement => measurement.Jv)
                .Include(measurement => measurement.Eqe)
                .Where(measurement =>
                    measurement.DeviceId == deviceId &&
                    measurement.Type == typeCount.Type)
                .OrderByDescending(measurement =>
                    measurement.MeasuredAt)
                .ThenByDescending(measurement =>
                    measurement.Id)
                .FirstOrDefaultAsync();

            if (latest != null)
                LatestMeasurements.Add(latest);
        }
    }

    // Permanently deletes one measurement belonging to this pixel.
    public async Task<IActionResult> OnPostDeleteMeasurementAsync(
        Guid id,
        Guid measurementId,
        MeasurementType? type,
        int historyPage = 1)
    {
        var measurement = await _database.Measurements
            .FirstOrDefaultAsync(measurement =>
                measurement.Id == measurementId &&
                measurement.DeviceId == id);

        if (measurement == null)
            return NotFound();

        _database.Measurements.Remove(measurement);

        await _database.SaveChangesAsync();

        // Redirect explicitly back to DeviceDetails.
        return RedirectToPage(
            "/DeviceDetails",
            new
            {
                id,
                type,
                historyPage
            }
        );
    }

    // Opens the data file belonging to one measurement.
    public async Task<IActionResult> OnGetDataFileAsync(
        Guid id,
        Guid measurementId)
    {
        var measurement = await _database.Measurements
            .AsNoTracking()
            .FirstOrDefaultAsync(measurement =>
                measurement.Id == measurementId &&
                measurement.DeviceId == id);

        if (measurement == null ||
            string.IsNullOrWhiteSpace(measurement.DataPath))
            return NotFound();

        var dataPath = measurement.DataPath.Trim();

        if (!System.IO.File.Exists(dataPath))
            return NotFound(
                $"Data file not found: {dataPath}"
            );

        var contentTypeProvider =
            new FileExtensionContentTypeProvider();

        if (!contentTypeProvider.TryGetContentType(
            dataPath,
            out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(
            dataPath,
            contentType);
    }

    // Returns the compact result used in measurement summaries.
    public string GetMeasurementHighlight(
        Measurement measurement)
    {
        return MeasurementDisplay.GetHighlight(
            measurement);
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

// Small read-only model used by the measurement type filter.
public class MeasurementTypeCount
{
    public MeasurementType Type { get; set; }
    public int Count { get; set; }
}