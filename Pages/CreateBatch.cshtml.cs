using System.ComponentModel.DataAnnotations;
using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class CreateBatchModel : PageModel
{
    private readonly SampleDbContext _database;

    public CreateBatchModel(SampleDbContext database)
    {
        _database = database;
    }


    [BindProperty]
    public Batch Batch { get; set; } = new();

    [BindProperty]
    [Range(1, 1000)]
    public int SampleCount { get; set; } = 4;


    [BindProperty]
    [Range(1, 1000)]
    public int PixelsPerSample { get; set; } = 6;

    [BindProperty]
    public string? StackLayers { get; set; }


    public void OnGet()
    {
        Batch.ProductionDate = DateOnly.FromDateTime(DateTime.Today);
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }


        var batchExists = await _database.Batches
            .AnyAsync(batch => batch.Code == Batch.Code);

        if (batchExists)
        {
            ModelState.AddModelError(
                "Batch.Code",
                "A batch with this code already exists."
            );

            return Page();
        }

        Batch.Notes = string.IsNullOrWhiteSpace(Batch.Notes)
            ? null
            : Batch.Notes.Trim();

        var layers = (StackLayers ?? "")
            .Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
            .Where(layer => !string.IsNullOrWhiteSpace(layer))
            .ToList();

        if (layers.Count > 0)
        {
            var stack = new DeviceStack();

            for (int i = 0; i < layers.Count; i++)
            {
                stack.Layers.Add(new StackLayer
                {
                    Position = i + 1,
                    Material = layers[i]
                });
            }

            Batch.DeviceStack = stack;
        }

        Batch.Area = Batch.Area == 0 ? null : Batch.Area;
        
        int sampleDigits = Math.Max(2, SampleCount.ToString().Length);

        for (int sampleNumber = 1; sampleNumber <= SampleCount; sampleNumber++)
        {
            var sample = new Sample
            {
                Code = $"S{sampleNumber.ToString($"D{sampleDigits}")}",
                BatchId = Batch.Id
            };

            for (int pixelNumber = 1;
                 pixelNumber <= PixelsPerSample;
                 pixelNumber++)
            {
                var device = new Device
                {
                    Pixel = $"P{pixelNumber}",
                    SampleId = sample.Id
                };

                sample.Devices.Add(device);
            }


            Batch.Samples.Add(sample);
        }

        _database.Batches.Add(Batch);

        await _database.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}