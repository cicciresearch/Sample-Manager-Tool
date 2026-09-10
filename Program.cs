using Cicci.SampleManager.Data;
using Cicci.SampleManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Allow the application to run as a Windows Service.
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Cicci Research Sample Manager";
});

builder.Services.AddRazorPages();

// Controllers handle HTTP API routes such as /api/v1/devices.
builder.Services.AddControllers();
builder.Services.AddScoped<MeasurementService>();

var databasePath = builder.Configuration["DatabasePath"]
    ?? @"C:\Arkeo\data\db\samples.db";

var databaseDirectory = Path.GetDirectoryName(databasePath);

if (!string.IsNullOrEmpty(databaseDirectory))
{
    Directory.CreateDirectory(databaseDirectory);
}

builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Create/update the database automatically.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    await database.Database.MigrateAsync();
}

app.Run();