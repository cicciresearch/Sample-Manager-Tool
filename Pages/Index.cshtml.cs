using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class IndexModel : PageModel
{
    private readonly SampleDbContext _database;

    public IndexModel(SampleDbContext database)
    {
        _database = database;
    }

    public List<Batch> Batches { get; set; } = [];

    public async Task OnGetAsync()
    {
            Batches = await _database.Batches
                .Include(batch => batch.Samples)
                    .ThenInclude(sample => sample.Devices)
                .Include(batch => batch.DeviceStack)
                    .ThenInclude(stack => stack!.Layers)
                .AsSplitQuery()
                .OrderByDescending(batch => batch.CreatedAt)
                .ToListAsync();
    }
}