using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Pages;

public class CreateUserModel : PageModel
{
    private readonly SampleDbContext _database;

    public CreateUserModel(SampleDbContext database)
    {
        _database = database;
    }

    [BindProperty]
    public ResearchUser ResearchUser { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        ResearchUser.Name = ResearchUser.Name.Trim();

        var userExists = await _database.ResearchUsers
            .AnyAsync(user => user.Name.ToLower() == ResearchUser.Name.ToLower());

        if (userExists)
        {
            ModelState.AddModelError(
                "ResearchUser.Name",
                "A user with this name already exists."
            );
            return Page();
        }

        _database.ResearchUsers.Add(ResearchUser);
        await _database.SaveChangesAsync();

        return RedirectToPage("/Index", new { userId = ResearchUser.Id });
    }
}