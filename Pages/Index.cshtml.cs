using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class IndexModel : PageModel
{
    private readonly SampleDbContext _database;

    // Creates the page model and gives it access to the EF Core database context.
    public IndexModel(SampleDbContext database)
    {
        _database = database;
    }

    public List<ResearchUser> Users { get; set; } = [];
    public List<Batch> Batches { get; set; } = [];

    public Guid? SelectedUserId { get; set; }
    public string Search { get; set; } = "";
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string StackFilter { get; set; } = "all";

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Search) ||
        FromDate.HasValue ||
        ToDate.HasValue;

    // Loads the active user and returns only batches matching the selected filters.
    public async Task OnGetAsync(
        Guid? userId,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        Users = await _database.ResearchUsers
            .Where(user => user.IsEnabled)
            .OrderBy(user => user.Name)
            .ToListAsync();

        SelectedUserId = userId;
        Search = search?.Trim() ?? "";
        FromDate = fromDate;
        ToDate = toDate;

        if (SelectedUserId == null && Users.Count > 0)
            SelectedUserId = Users[0].Id;

        if (SelectedUserId == null)
            return;

        // IQueryable builds the SQL query step-by-step.
        // SQLite is not queried until ToListAsync() is called at the end.
        var query = _database.Batches
            .Where(batch => batch.UserId == SelectedUserId);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            // % means "any text before or after" in a SQL LIKE search.
            var searchPattern = $"%{Search}%";

            query = query.Where(batch =>
                EF.Functions.Like(batch.Code, searchPattern) ||
                (batch.Notes != null &&
                    EF.Functions.Like(batch.Notes, searchPattern)) ||
                batch.Samples.Any(sample =>
                    EF.Functions.Like(sample.Code, searchPattern) ||
                    sample.Devices.Any(device =>
                        EF.Functions.Like(device.Pixel, searchPattern))) ||
                (batch.DeviceStack != null &&
                    batch.DeviceStack.Layers.Any(layer =>
                        EF.Functions.Like(layer.Material, searchPattern)))
            );
        }

        if (FromDate.HasValue)
        {
            query = query.Where(
                batch => batch.ProductionDate >= FromDate.Value
            );
        }

        if (ToDate.HasValue)
        {
            query = query.Where(
                batch => batch.ProductionDate <= ToDate.Value
            );
        }

        Batches = await query
            .Include(batch => batch.Samples)
                .ThenInclude(sample => sample.Devices)
            .Include(batch => batch.DeviceStack)
                .ThenInclude(stack => stack!.Layers)
            .AsSplitQuery()
            .OrderByDescending(batch => batch.ProductionDate)
            .ThenByDescending(batch => batch.CreatedAt)
            .ToListAsync();
    }
}