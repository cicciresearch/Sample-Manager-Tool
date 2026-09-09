using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class BatchDetailsModel : PageModel
{
    private readonly SampleDbContext _database;

    // Gives this page access to the Sample Manager database.
    public BatchDetailsModel(SampleDbContext database)
    {
        _database = database;
    }

    public Batch Batch { get; set; } = null!;

    // Loads the complete batch hierarchy used by the page.
    private async Task<bool> LoadBatchAsync(Guid id)
    {
        var batch = await _database.Batches
            .AsNoTracking()
            .Include(batch => batch.User)
            .Include(batch => batch.DeviceStack)
                .ThenInclude(stack => stack!.Layers)
            .Include(batch => batch.Samples)
                .ThenInclude(sample => sample.Devices)
            .AsSplitQuery()
            .FirstOrDefaultAsync(batch => batch.Id == id);

        if (batch == null)
            return false;

        Batch = batch;
        return true;
    }

    // Finds the first unused sequential code such as S01, S02 or P1, P2.
    private static string GetNextCode(
        IEnumerable<string> existingCodes,
        string prefix,
        int minimumDigits)
    {
        // HashSet provides a fast way to check whether a generated code
        // already exists, ignoring upper/lower case differences.
        var usedCodes = new HashSet<string>(
            existingCodes,
            StringComparer.OrdinalIgnoreCase
        );

        var number = 1;

        while (true)
        {
            var numberText = minimumDigits > 0
                ? number.ToString($"D{minimumDigits}")
                : number.ToString();

            var candidate = $"{prefix}{numberText}";

            if (!usedCodes.Contains(candidate))
                return candidate;

            number++;
        }
    }

    // Displays one batch and its substrates/pixels.
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!await LoadBatchAsync(id))
            return NotFound();

        return Page();
    }

    // Adds a new empty substrate to this batch using the next Sxx code.
    public async Task<IActionResult> OnPostAddSampleAsync(Guid id)
    {
        var batch = await _database.Batches
            .Include(batch => batch.Samples)
            .FirstOrDefaultAsync(batch => batch.Id == id);

        if (batch == null)
            return NotFound();

        var code = GetNextCode(
            batch.Samples.Select(sample => sample.Code),
            "S",
            2
        );

        var sample = new Sample
        {
            BatchId = batch.Id,
            Code = code
        };

        _database.Samples.Add(sample);
        await _database.SaveChangesAsync();

        // Redirect-after-POST prevents browser refresh from submitting
        // the Add Substrate operation a second time.
        return RedirectToPage(new { id });
    }

    // Renames one substrate while preserving its stable GUID and devices.
    public async Task<IActionResult> OnPostRenameSampleAsync(
        Guid id,
        Guid sampleId,
        string? code)
    {
        var sample = await _database.Samples
            .FirstOrDefaultAsync(sample =>
                sample.Id == sampleId &&
                sample.BatchId == id);

        if (sample == null)
            return NotFound();

        code = code?.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(
                string.Empty,
                "Substrate code cannot be empty."
            );

            return await ReloadPageAsync(id);
        }

        if (code.Length > 100)
        {
            ModelState.AddModelError(
                string.Empty,
                "Substrate code cannot exceed 100 characters."
            );

            return await ReloadPageAsync(id);
        }

        var duplicate = await _database.Samples
            .AnyAsync(other =>
                other.BatchId == id &&
                other.Id != sampleId &&
                other.Code.ToLower() == code.ToLower());

        if (duplicate)
        {
            ModelState.AddModelError(
                string.Empty,
                $"A substrate named '{code}' already exists in this batch."
            );

            return await ReloadPageAsync(id);
        }

        sample.Code = code;

        await _database.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    // Adds one pixel/device to the selected substrate using the next P# code.
    public async Task<IActionResult> OnPostAddDeviceAsync(
        Guid id,
        Guid sampleId)
    {
        var sample = await _database.Samples
            .Include(sample => sample.Devices)
            .FirstOrDefaultAsync(sample =>
                sample.Id == sampleId &&
                sample.BatchId == id);

        if (sample == null)
            return NotFound();

        var pixel = GetNextCode(
            sample.Devices.Select(device => device.Pixel),
            "P",
            0
        );

        var device = new Device
        {
            SampleId = sample.Id,
            Pixel = pixel
        };

        _database.Devices.Add(device);
        await _database.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    // Renames one pixel while preserving its stable Device GUID.
    public async Task<IActionResult> OnPostRenameDeviceAsync(
        Guid id,
        Guid deviceId,
        string? pixel)
    {
        var device = await _database.Devices
            .Include(device => device.Sample)
            .FirstOrDefaultAsync(device =>
                device.Id == deviceId &&
                device.Sample.BatchId == id);

        if (device == null)
            return NotFound();

        pixel = pixel?.Trim();

        if (string.IsNullOrWhiteSpace(pixel))
        {
            ModelState.AddModelError(
                string.Empty,
                "Pixel name cannot be empty."
            );

            return await ReloadPageAsync(id);
        }

        if (pixel.Length > 100)
        {
            ModelState.AddModelError(
                string.Empty,
                "Pixel name cannot exceed 100 characters."
            );

            return await ReloadPageAsync(id);
        }

        var duplicate = await _database.Devices
            .AnyAsync(other =>
                other.SampleId == device.SampleId &&
                other.Id != deviceId &&
                other.Pixel.ToLower() == pixel.ToLower());

        if (duplicate)
        {
            ModelState.AddModelError(
                string.Empty,
                $"A pixel named '{pixel}' already exists on this substrate."
            );

            return await ReloadPageAsync(id);
        }

        device.Pixel = pixel;

        await _database.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    // Reloads the batch after a validation error so the same page can be shown again.
    private async Task<IActionResult> ReloadPageAsync(Guid id)
    {
        if (!await LoadBatchAsync(id))
            return NotFound();

        return Page();
    }
}