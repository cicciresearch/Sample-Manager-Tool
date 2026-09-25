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

    // Validates that a measurement can be added to the requested session.
    private async Task<IActionResult?> ValidateMeasurementSessionAsync(
        Guid deviceId,
        Guid? measurementSessionId)
    {
        if (!measurementSessionId.HasValue)
            return null;

        var session = await _database.MeasurementSessions
            .AsNoTracking()
            .Where(session =>
                session.Id == measurementSessionId.Value)
            .Select(session => new
            {
                session.Status,

                DeviceIsParticipant =
                    session.Participants.Any(participant =>
                        participant.DeviceId == deviceId)
            })
            .FirstOrDefaultAsync();

        if (session == null)
        {
            return NotFound(new
            {
                error = "Measurement session not found."
            });
        }

        // if (session.Status != MeasurementSessionStatus.Running)
        // {
        //     return Conflict(new
        //     {
        //         error = "Measurement session is not running."
        //     });
        // }

        if (!session.DeviceIsParticipant)
        {
            return BadRequest(new
            {
                error = "Device does not belong to this measurement session."
            });
        }

        return null;
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

    // Returns all Sample Manager metadata belonging to one batch.
    // Measurements are intentionally excluded because this endpoint describes
    // the batch structure and metadata rather than measurement history.
    [HttpGet("batches/{id:guid}")]
    public async Task<IActionResult> GetBatchAsync(Guid id)
    {
        var batch = await _database.Batches
            .AsNoTracking()
            .Where(batch => batch.Id == id)
            .Select(batch => new
            {
                id = batch.Id,
                userId = batch.UserId,
                code = batch.Code,
                productionDate = batch.ProductionDate,
                area = batch.Area,
                notes = batch.Notes,
                createdAt = batch.CreatedAt,
                deviceStackId = batch.DeviceStackId,

                user = new
                {
                    id = batch.User.Id,
                    name = batch.User.Name,
                    isEnabled = batch.User.IsEnabled
                },

                deviceStack = batch.DeviceStack == null
                    ? null
                    : new
                    {
                        id = batch.DeviceStack.Id,

                        layers = batch.DeviceStack.Layers
                            .OrderBy(layer => layer.Position)
                            .Select(layer => new
                            {
                                id = layer.Id,
                                deviceStackId = layer.DeviceStackId,
                                position = layer.Position,
                                material = layer.Material
                            })
                            .ToList()
                    },

                // The nested structure mirrors the relational hierarchy:
                // one batch contains substrates, and each substrate contains devices.
                substrates = batch.Samples
                    .OrderBy(sample => sample.Code)
                    .Select(sample => new
                    {
                        id = sample.Id,
                        batchId = sample.BatchId,
                        code = sample.Code,
                        notes = sample.Notes,

                        devices = sample.Devices
                            .OrderBy(device => device.Pixel)
                            .Select(device => new
                            {
                                id = device.Id,
                                sampleId = device.SampleId,
                                pixel = device.Pixel,
                                notes = device.Notes
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

    // Returns the lightweight device list for one selected batch.
    // Since a batch already belongs to one user, no separate userId is needed.
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevicesAsync(
        [FromQuery(Name = "batchID")] Guid? batchId)
    {
        if (!batchId.HasValue)
        {
            return BadRequest(new
            {
                error = "batchID is required."
            });
        }

        var batchExists = await _database.Batches
            .AsNoTracking()
            .AnyAsync(batch => batch.Id == batchId.Value);

        if (!batchExists)
        {
            return NotFound(new
            {
                error = "Batch not found."
            });
        }

        // The batch is already selected, so the display name only needs
        // the substrate code and pixel name to remain unique within that batch.
        var devices = await _database.Devices
            .AsNoTracking()
            .Where(device =>
                device.Sample.BatchId == batchId.Value)
            .OrderBy(device => device.Sample.Code)
            .ThenBy(device => device.Pixel)
            .Select(device => new
            {
                id = device.Id,
                name =
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

    // Creates a measurement session for one or more selected devices.
    [HttpPost("measurement-sessions")]
    public async Task<IActionResult> CreateMeasurementSessionAsync(
        [FromBody] CreateMeasurementSessionRequest request)
    {
        var deviceIds = request.DeviceIds
            .Distinct()
            .ToList();

        if (deviceIds.Count == 0)
        {
            return BadRequest(new
            {
                error = "At least one device is required."
            });
        }

        var existingDeviceIds = await _database.Devices
            .AsNoTracking()
            .Where(device =>
                deviceIds.Contains(device.Id))
            .Select(device =>
                device.Id)
            .ToListAsync();

        var missingDeviceIds = deviceIds
            .Except(existingDeviceIds)
            .ToList();

        if (missingDeviceIds.Count > 0)
        {
            return BadRequest(new
            {
                error = "One or more devices do not exist.",
                missingDeviceIds
            });
        }

        var session = new MeasurementSession
        {
            Name = string.IsNullOrWhiteSpace(request.Name)
                ? null
                : request.Name.Trim(),

            Type = request.Type,

            Status =
                MeasurementSessionStatus.Running,

            StartedAt =
                request.StartedAt?.UtcDateTime
                    ?? DateTime.UtcNow,

            Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim()
        };

        foreach (var deviceId in deviceIds)
        {
            session.Participants.Add(
                new MeasurementSessionDevice
                {
                    DeviceId = deviceId
                });
        }

        _database.MeasurementSessions.Add(session);

        await _database.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                id = session.Id,
                type = session.Type.ToString(),
                status = session.Status.ToString(),
                startedAt = session.StartedAt,
                deviceIds
            });
    }

    // Returns one measurement session and its participating devices.
    [HttpGet("measurement-sessions/{id:guid}")]
    public async Task<IActionResult> GetMeasurementSessionAsync(
        Guid id)
    {
        var session = await _database.MeasurementSessions
            .AsNoTracking()
            .Where(session =>
                session.Id == id)
            .Select(session => new
            {
                id = session.Id,
                name = session.Name,
                type = session.Type,
                status = session.Status,
                startedAt = session.StartedAt,
                endedAt = session.EndedAt,
                notes = session.Notes,

                devices = session.Participants
                    .OrderBy(participant =>
                        participant.Device.Sample.Code)
                    .ThenBy(participant =>
                        participant.Device.Pixel)
                    .Select(participant => new
                    {
                        id = participant.Device.Id,
                        sampleId =
                            participant.Device.SampleId,

                        sampleCode =
                            participant.Device.Sample.Code,

                        pixel =
                            participant.Device.Pixel
                    })
                    .ToList(),

                measurementCount =
                    session.Measurements.Count
            })
            .FirstOrDefaultAsync();

        if (session == null)
        {
            return NotFound(new
            {
                error = "Measurement session not found."
            });
        }

        return Ok(session);
    }

    // Marks a running measurement session as successfully completed.
    [HttpPost("measurement-sessions/{id:guid}/complete")]
    public async Task<IActionResult> CompleteMeasurementSessionAsync(
        Guid id)
    {
        var session = await _database.MeasurementSessions
            .FirstOrDefaultAsync(session =>
                session.Id == id);

        if (session == null)
            return NotFound();

        if (session.Status == MeasurementSessionStatus.Completed)
        {
            return Ok(new
            {
                id = session.Id,
                status = session.Status.ToString(),
                endedAt = session.EndedAt
            });
        }

        if (session.Status == MeasurementSessionStatus.Aborted)
        {
            return Conflict(new
            {
                error = "An aborted session cannot be completed."
            });
        }

        session.Status =
            MeasurementSessionStatus.Completed;

        session.EndedAt =
            DateTime.UtcNow;

        await _database.SaveChangesAsync();

        return Ok(new
        {
            id = session.Id,
            status = session.Status.ToString(),
            endedAt = session.EndedAt
        });
    }

    // Marks a running measurement session as aborted.
    [HttpPost("measurement-sessions/{id:guid}/abort")]
    public async Task<IActionResult> AbortMeasurementSessionAsync(
        Guid id)
    {
        var session = await _database.MeasurementSessions
            .FirstOrDefaultAsync(session =>
                session.Id == id);

        if (session == null)
            return NotFound();

        if (session.Status == MeasurementSessionStatus.Aborted)
        {
            return Ok(new
            {
                id = session.Id,
                status = session.Status.ToString(),
                endedAt = session.EndedAt
            });
        }

        if (session.Status == MeasurementSessionStatus.Completed)
        {
            return Conflict(new
            {
                error = "A completed session cannot be aborted."
            });
        }

        session.Status =
            MeasurementSessionStatus.Aborted;

        session.EndedAt =
            DateTime.UtcNow;

        await _database.SaveChangesAsync();

        return Ok(new
        {
            id = session.Id,
            status = session.Status.ToString(),
            endedAt = session.EndedAt
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

        var sessionError =
            await ValidateMeasurementSessionAsync(
                deviceId,
                request.Common.MeasurementSessionId);

        if (sessionError != null)
            return sessionError;

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

    // Stores one EQE measurement with common and EQE-specific data.
    [HttpPost("devices/{deviceId:guid}/measurements/eqe")]
    public async Task<IActionResult> CreateEqeMeasurementAsync(
        Guid deviceId,
        [FromBody] CreateMeasurementRequest<EqeMeasurementRequest> request)
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
            MeasurementType.EQE,
            request.Common
        );

        measurement.Eqe = new EqeMeasurement
        {
            Jsc = request.Specific.Jsc,
            PeakEQE = request.Specific.PeakEQE
        };

        await _measurementService.SaveAsync(measurement);

        return MeasurementCreated(measurement);
    }

    // Stores one EIS measurement with common and EIS-specific data.
    [HttpPost("devices/{deviceId:guid}/measurements/eis")]
    public async Task<IActionResult> CreateEisMeasurementAsync(
        Guid deviceId,
        [FromBody] CreateMeasurementRequest<EisMeasurementRequest> request)
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
            MeasurementType.EIS,
            request.Common
        );

        measurement.Eis = new EisMeasurement
        {
            PeakFrequencyHz = request.Specific.PeakFrequencyHz,
        };

        await _measurementService.SaveAsync(measurement);

        return MeasurementCreated(measurement);
    }

    // Stores one DarkJV measurement with common and EIS-specific data.
    [HttpPost("devices/{deviceId:guid}/measurements/darkjv")]
    public async Task<IActionResult> CreateDarkJvMeasurementAsync(
        Guid deviceId,
        [FromBody] CreateMeasurementRequest<DarkJvMeasurementRequest> request)
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
            MeasurementType.DarkJV,
            request.Common
        );

        // measurement.Eis = new EisMeasurement
        // {
        //     PeakFrequencyHz = request.Specific.PeakFrequencyHz,
        // };

        await _measurementService.SaveAsync(measurement);

        return MeasurementCreated(measurement);
    }
}