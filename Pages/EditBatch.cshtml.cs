using System.ComponentModel.DataAnnotations;
using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class EditBatchModel : PageModel
{
    private readonly SampleDbContext _database;

    public EditBatchModel(SampleDbContext database)
    {
        _database = database;
    }

    public List<ResearchUser> Users { get; set; } = [];

    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public Guid UserId { get; set; }

    [BindProperty]
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = "";

    [BindProperty]
    public DateOnly ProductionDate { get; set; }

    [BindProperty]
    [Range(0, 1000)]
    public float? Area { get; set; }

    [BindProperty]
    public string? StackLayers { get; set; }

    [BindProperty]
    [MaxLength(2000)]
    public string? Notes { get; set; }

    private async Task LoadUsersAsync()
    {
        Users = await _database.ResearchUsers
            .Where(user => user.IsEnabled)
            .OrderBy(user => user.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var batch = await _database.Batches
            .Include(batch => batch.DeviceStack)
                .ThenInclude(stack => stack!.Layers)
            .FirstOrDefaultAsync(batch => batch.Id == id);

        if (batch == null)
            return NotFound();

        await LoadUsersAsync();

        Id = batch.Id;
        UserId = batch.UserId;
        Code = batch.Code;
        ProductionDate = batch.ProductionDate;
        Area = batch.Area;
        Notes = batch.Notes;

        if (batch.DeviceStack != null)
        {
            StackLayers = string.Join(
                Environment.NewLine,
                batch.DeviceStack.Layers
                    .OrderBy(layer => layer.Position)
                    .Select(layer => layer.Material)
            );
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var batch = await _database.Batches
            .Include(batch => batch.DeviceStack)
                .ThenInclude(stack => stack!.Layers)
            .FirstOrDefaultAsync(batch => batch.Id == Id);

        if (batch == null)
            return NotFound();

        var userExists = await _database.ResearchUsers
            .AnyAsync(user => user.Id == UserId && user.IsEnabled);

        if (!userExists)
        {
            ModelState.AddModelError(
                nameof(UserId),
                "Please select a valid user."
            );

            await LoadUsersAsync();
            return Page();
        }

        var duplicateCode = await _database.Batches
            .AnyAsync(b => b.Code == Code && b.Id != Id);

        if (duplicateCode)
        {
            ModelState.AddModelError(
                nameof(Code),
                "A batch with this code already exists."
            );

            await LoadUsersAsync();
            return Page();
        }

        batch.UserId = UserId;
        batch.Code = Code.Trim();
        batch.ProductionDate = ProductionDate;
        batch.Area = Area == 0 ? null : Area;
        batch.Notes = string.IsNullOrWhiteSpace(Notes)
            ? null
            : Notes.Trim();

        var layers = (StackLayers ?? "")
            .Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
            .Where(layer => !string.IsNullOrWhiteSpace(layer))
            .ToList();

        var oldStack = batch.DeviceStack;

        if (layers.Count == 0)
        {
            batch.DeviceStack = null;
            batch.DeviceStackId = null;
        }
        else
        {
            var newStack = new DeviceStack();

            for (int i = 0; i < layers.Count; i++)
            {
                newStack.Layers.Add(new StackLayer
                {
                    Position = i + 1,
                    Material = layers[i]
                });
            }

            batch.DeviceStack = newStack;
        }

        if (oldStack != null)
        {
            var usedByOtherBatch = await _database.Batches
                .AnyAsync(b =>
                    b.DeviceStackId == oldStack.Id &&
                    b.Id != batch.Id
                );

            if (!usedByOtherBatch)
                _database.DeviceStacks.Remove(oldStack);
        }

        await _database.SaveChangesAsync();

        return RedirectToPage("/Index", new { userId = UserId });
    }
}