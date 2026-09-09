using Cicci.SampleManager.Data;
using Microsoft.EntityFrameworkCore;
using Cicci.SampleManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Controllers handle HTTP API routes such as /api/v1/devices.
builder.Services.AddControllers();
builder.Services.AddScoped<MeasurementService>();
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite("Data Source=DataStore/samples.db"));

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
app.MapControllers(); // Maps routes declared by API controllers.

// Create the database automatically if it does not exist.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    await database.Database.MigrateAsync();
}

app.Run();