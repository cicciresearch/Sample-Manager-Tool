using Cicci.SampleManager.Data;
using Cicci.SampleManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Measurements.Common;

public static class MeasurementLoader
{
    // Adds all registered measurement-specific includes to a Measurement query.
    public static IQueryable<Measurement> IncludeDetails(
        IQueryable<Measurement> query)
    {
        foreach (var definition in MeasurementRegistry.All)
        {
            query = definition.IncludeData(query);
        }

        // Temporary legacy support until EQE is migrated
        // to the MeasurementRegistry.
        query = query.Include(
            measurement => measurement.Eqe);

        return query;
    }

    // Loads all measurements for a collection of devices and attaches them
    // to each Device.Measurements collection.
    public static async Task LoadForDevicesAsync(
        SampleDbContext database,
        IEnumerable<Device> devices)
    {
        var deviceList =
            devices.ToList();

        if (deviceList.Count == 0)
            return;

        var deviceIds = deviceList
            .Select(device => device.Id)
            .ToList();

        IQueryable<Measurement> query =
            database.Measurements
                .AsNoTracking()
                .Where(measurement =>
                    deviceIds.Contains(
                        measurement.DeviceId));

        query = IncludeDetails(query);

        var measurements =
            await query.ToListAsync();

        var measurementsByDevice =
            measurements
                .GroupBy(measurement =>
                    measurement.DeviceId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

        foreach (var device in deviceList)
        {
            if (measurementsByDevice.TryGetValue(
                device.Id,
                out var deviceMeasurements))
            {
                device.Measurements =
                    deviceMeasurements;
            }
            else
            {
                device.Measurements =
                    new List<Measurement>();
            }
        }
    }
}