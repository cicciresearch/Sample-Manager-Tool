using System.ComponentModel.DataAnnotations;
using Cicci.SampleManager.Data;
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

    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = "";

    [BindProperty]
    public DateOnly ProductionDate { get; set; }

    [BindProperty]
    [MaxLength(2000)]
    public string Notes { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var batch = await _database.Batches.FindAsync(id);

        if (batch == null)
            return NotFound();

        Id = batch.Id;
        Code = batch.Code;
        ProductionDate = batch.ProductionDate;
        Notes = batch.Notes;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var batch = await _database.Batches.FindAsync(Id);

        if (batch == null)
            return NotFound();

        var duplicateCode = await _database.Batches
            .AnyAsync(b => b.Code == Code && b.Id != Id);

        if (duplicateCode)
        {
            ModelState.AddModelError(
                nameof(Code),
                "A batch with this code already exists."
            );
            return Page();
        }

        batch.Code = Code;
        batch.ProductionDate = ProductionDate;
        batch.Notes = Notes;

        await _database.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}