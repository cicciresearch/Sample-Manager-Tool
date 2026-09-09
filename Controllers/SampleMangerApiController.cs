using Cicci.SampleManager.Data;
using Cicci.SampleManager.Api;
using Cicci.SampleManager.Models;
using Cicci.SampleManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cicci.SampleManager.Controllers;

[ApiController]
[Route("api/v1")]
public class SampleManagerApiController : ControllerBase
{
    private readonly SampleDbContext _database;
    private readonly MeasurementService _measurementService;

    // Gives the controller access to the database and shared measurement logic.
    public SampleManagerApiController(
        SampleDbContext database,
        MeasurementService measurementService)
    {
        _database = database;
        _measurementService = measurementService;
    }

    // Creates the standard HTTP response returned after any measurement is stored.
    private IActionResult MeasurementCreated(Measurement measurement)
    {
        return StatusCode(StatusCodes.Status201Created, new
        {
            id = measurement.Id,
            deviceId = measurement.DeviceId,
            type = measurement.Type.ToString(),
            measuredAt = measurement.MeasuredAt
        });
    }

    // Checks that both the web API and the SQLite database are reachable.
    [HttpGet("health")]
    public async Task<IActionResult> GetHealthAsync()
    {
        // CanConnectAsync performs a lightweight database connectivity check.
        var databaseAvailable = await _database.Database.CanConnectAsync();

        if (!databaseAvailable)
        {
            return StatusCode(503, new
            {
                status = "error",
                database = "unavailable"
            });
        }

        return Ok(new
        {
            status = "ok",
            database = "available"
        });
    }

    // Returns all enabled users that LabVIEW can show in its user selector.
    [HttpGet("users")]
    public async Task<IActionResult> GetUsersAsync()
    {
        // AsNoTracking improves read-only queries because EF does not need
        // to track these objects for later database changes.
        var users = await _database.ResearchUsers
            .AsNoTracking()
            .Where(user => user.IsEnabled)
            .OrderBy(user => user.Name)
            .Select(user => new
            {
                id = user.Id,
                name = user.Name
            })
            .ToListAsync();

        return Ok(users);
    }

    // Returns the batches belonging to one active user.
    // LabVIEW uses this response to populate the Batch dropdown.
    [HttpGet("batches")]
    public async Task<IActionResult> GetBatchesAsync([FromQuery] Guid? userId)
    {
        if (!userId.HasValue)
        {
            return BadRequest(new
            {
                error = "userId is required."
            });
        }

        var userExists = await _database.ResearchUsers
            .AsNoTracking()
            .AnyAsync(user =>
                user.Id == userId.Value &&
                user.IsEnabled);

        if (!userExists)
        {
            return NotFound(new
            {
                error = "Active user not found."
            });
        }

        var batches = await _database.Batches
            .AsNoTracking()
            .Where(batch => batch.UserId == userId.Value)
            .OrderByDescending(batch => batch.ProductionDate)
            .ThenBy(batch => batch.Code)
            .Select(batch => new
            {
                id = batch.Id,
                code = batch.Code,
                productionDate = batch.ProductionDate
            })
            .ToListAsync();

        return Ok(batches);
    }

    // Returns the substrate/device hierarchy of one batch.
    // LabVIEW uses the substrates for its second dropdown and the nested
    // devices for the Pixel dropdown after a substrate is selected.
    [HttpGet("batches/{id:guid}")]
    public async Task<IActionResult> GetBatchAsync(Guid id)
    {
        var batch = await _database.Batches
            .AsNoTracking()
            .Where(batch => batch.Id == id)
            .Select(batch => new
            {
                id = batch.Id,
                code = batch.Code,
                productionDate = batch.ProductionDate,
                area = batch.Area,

                // A nested JSON structure mirrors the relational hierarchy:
                // one batch contains substrates, and each substrate contains devices.
                substrates = batch.Samples
                    .OrderBy(sample => sample.Code)
                    .Select(sample => new
                    {
                        id = sample.Id,
                        code = sample.Code,

                        devices = sample.Devices
                            .OrderBy(device => device.Pixel)
                            .Select(device => new
                            {
                                id = device.Id,
                                pixel = device.Pixel
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (batch == null)
        {
            return NotFound(new
            {
                error = "Batch not found."
            });
        }

        return Ok(batch);
    }

    // Returns the lightweight device list used to populate the LabVIEW dropdown.
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevicesAsync([FromQuery] Guid? userId)
    {
        if (!userId.HasValue)
        {
            return BadRequest(new
            {
                error = "userId is required."
            });
        }

        var userExists = await _database.ResearchUsers
            .AsNoTracking()
            .AnyAsync(user =>
                user.Id == userId.Value &&
                user.IsEnabled);

        if (!userExists)
        {
            return NotFound(new
            {
                error = "Active user not found."
            });
        }

        // We project directly to the small JSON structure LabVIEW needs
        // instead of returning the EF database entities themselves.
        var devices = await _database.Devices
            .AsNoTracking()
            .Where(device =>
                device.Sample.Batch.UserId == userId.Value)
            .OrderByDescending(device =>
                device.Sample.Batch.ProductionDate)
            .ThenBy(device =>
                device.Sample.Batch.Code)
            .ThenBy(device =>
                device.Sample.Code)
            .ThenBy(device =>
                device.Pixel)
            .Select(device => new
            {
                id = device.Id,
                label =
                    device.Sample.Batch.Code + " - " +
                    device.Sample.Code + " - " +
                    device.Pixel
            })
            .ToListAsync();

        return Ok(devices);
    }

    // Returns the complete Sample Manager metadata for one stable Device GUID.
    [HttpGet("devices/{id:guid}")]
    public async Task<IActionResult> GetDeviceAsync(Guid id)
    {
        var device = await _database.Devices
            .AsNoTracking()
            .Include(device => device.Sample)
                .ThenInclude(sample => sample.Batch)
                    .ThenInclude(batch => batch.User)
            .Include(device => device.Sample)
                .ThenInclude(sample => sample.Batch)
                    .ThenInclude(batch => batch.DeviceStack)
                        .ThenInclude(stack => stack!.Layers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(device => device.Id == id);

        if (device == null)
        {
            return NotFound(new
            {
                error = "Device not found."
            });
        }

        var batch = device.Sample.Batch;

        // This is a DTO-style response: we explicitly define the public API
        // instead of serializing the complete relational EF object graph.
        var result = new
        {
            id = device.Id,

            label =
                batch.Code + " - " +
                device.Sample.Code + " - " +
                device.Pixel,

            userId = batch.UserId,
            userName = batch.User.Name,

            batchId = batch.Id,
            batchCode = batch.Code,
            productionDate = batch.ProductionDate,
            area = batch.Area,

            sampleId = device.Sample.Id,
            sampleCode = device.Sample.Code,

            pixel = device.Pixel,

            stack = batch.DeviceStack?.Layers
                .OrderBy(layer => layer.Position)
                .Select(layer => layer.Material)
                .ToList() ?? [],

            batchNotes = batch.Notes,
            sampleNotes = device.Sample.Notes,
            deviceNotes = device.Notes
        };

        return Ok(result);
    }

    // Creates a new enabled ResearchUser from a JSON request sent by client.
    [HttpPost("users")]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                error = "User name is required."
            });
        }

        var existingUser = await _database.ResearchUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user =>
                user.Name.ToLower() == name.ToLower());

        if (existingUser != null)
        {
            // 409 Conflict means that the request is valid, but the resource
            // already exists and therefore cannot be created again.
            return Conflict(new
            {
                error = "A user with this name already exists.",
                id = existingUser.Id,
                name = existingUser.Name,
                isEnabled = existingUser.IsEnabled
            });
        }

        var user = new ResearchUser
        {
            Name = name,
            IsEnabled = true
        };

        _database.ResearchUsers.Add(user);
        await _database.SaveChangesAsync();

        // HTTP 201 means that a new resource was successfully created.
        return StatusCode(StatusCodes.Status201Created, new
        {
            id = user.Id,
            name = user.Name,
            isEnabled = user.IsEnabled
        });
    }

    // Stores one JV measurement with common and JV-specific data.
    [HttpPost("devices/{deviceId:guid}/measurements/jv")]
    public async Task<IActionResult> CreateJvMeasurementAsync(
        Guid deviceId,
        [FromBody] CreateMeasurementRequest<JvMeasurementRequest> request)
    {
        if (!await _measurementService.DeviceExistsAsync(deviceId))
        {
            return NotFound(new
            {
                error = "Device not found."
            });
        }

        var measurement = _measurementService.CreateBaseMeasurement(
            deviceId,
            MeasurementType.JV,
            request.Common
        );

        measurement.Jv = new JvMeasurement
        {
            VocV = request.Specific.VocV,
            JscMilliampPerCm2 = request.Specific.JscMilliampPerCm2,
            FillFactorPercent = request.Specific.FillFactorPercent,
            EfficiencyPercent = request.Specific.EfficiencyPercent,
            VmppV = request.Specific.VmppV,
            JmppMilliampPerCm2 = request.Specific.JmppMilliampPerCm2,
            PmppMilliwattPerCm2 = request.Specific.PmppMilliwattPerCm2
        };

        await _measurementService.SaveAsync(measurement);

        return MeasurementCreated(measurement);
    }
}