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

        return query;
    }

    // Returns the number of measurements stored for each selected pixel.
    public static async Task<Dictionary<Guid, int>> LoadCountsForDevicesAsync(
        SampleDbContext database,
        IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return [];

        return await database.Measurements
            .AsNoTracking()
            .Where(measurement =>
                ids.Contains(measurement.DeviceId))
            .GroupBy(measurement =>
                measurement.DeviceId)
            .Select(group => new
            {
                DeviceId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.DeviceId,
                item => item.Count);
    }

    // Loads only the newest measurement of each type for every selected pixel.
    public static async Task<List<Measurement>> LoadLatestForDevicesAsync(
        SampleDbContext database,
        IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return [];

        var latestMeasurementIds =
            database.Measurements
                .AsNoTracking()
                .Where(measurement =>
                    ids.Contains(measurement.DeviceId))
                .GroupBy(measurement => new
                {
                    measurement.DeviceId,
                    measurement.Type
                })
                .Select(group =>
                    group
                        .OrderByDescending(measurement =>
                            measurement.MeasuredAt)
                        .ThenByDescending(measurement =>
                            measurement.Id)
                        .Select(measurement =>
                            measurement.Id)
                        .First());

        IQueryable<Measurement> query =
            database.Measurements
                .AsNoTracking()
                .Where(measurement =>
                    latestMeasurementIds.Contains(
                        measurement.Id));

        query = IncludeDetails(query);

        return await query.ToListAsync();
    }
}