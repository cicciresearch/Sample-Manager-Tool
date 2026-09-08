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

    public List<ResearchUser> Users { get; set; } = [];
    public List<Batch> Batches { get; set; } = [];
    public Guid? SelectedUserId { get; set; }

    public async Task OnGetAsync(Guid? userId)
    {
        Users = await _database.ResearchUsers
            .Where(user => user.IsEnabled)
            .OrderBy(user => user.Name)
            .ToListAsync();

        SelectedUserId = userId;

        if (SelectedUserId == null && Users.Count > 0)
            SelectedUserId = Users[0].Id;

        if (SelectedUserId == null)
            return;

        Batches = await _database.Batches
            .Where(batch => batch.UserId == SelectedUserId)
            .Include(batch => batch.Samples)
                .ThenInclude(sample => sample.Devices)
            .Include(batch => batch.DeviceStack)
                .ThenInclude(stack => stack!.Layers)
            .AsSplitQuery()
            .OrderByDescending(batch => batch.CreatedAt)
            .ToListAsync();
    }
}