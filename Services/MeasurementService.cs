using Cicci.SampleManager.Api;
using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Services;

public class MeasurementService
{
    private readonly SampleDbContext _database;

    // Gives the service access to the measurement database.
    public MeasurementService(SampleDbContext database)
    {
        _database = database;
    }

    // Checks whether a Device GUID exists before a measurement is created.
    public async Task<bool> DeviceExistsAsync(Guid deviceId)
    {
        return await _database.Devices
            .AsNoTracking()
            .AnyAsync(device => device.Id == deviceId);
    }

    // Creates the common Measurement record shared by every measurement type.
    public Measurement CreateBaseMeasurement(
        Guid deviceId,
        MeasurementType type,
        CommonMeasurementRequest common)
    {
        return new Measurement
        {
            DeviceId = deviceId,
            Type = type,

            MeasuredAt = common.MeasuredAt?.UtcDateTime
                ?? DateTime.UtcNow,

            DataPath = NormalizeOptionalText(common.DataPath),
            Notes = NormalizeOptionalText(common.Notes)
        };
    }

    // Saves a complete measurement after its type-specific data has been attached.
    public async Task SaveAsync(Measurement measurement)
    {
        _database.Measurements.Add(measurement);
        await _database.SaveChangesAsync();
    }

    // Normalizes optional text so blank strings are stored as NULL.
    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}