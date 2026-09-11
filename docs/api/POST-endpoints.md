# POST Endpoints

Base development URL used in examples:

```text
http://localhost:5227/api/v1
```

All POST requests documented here use:

```http
Content-Type: application/json
```

## POST `/users`

Creates a new enabled research user.

### Request JSON

```json
{
  "name": "Alice Rossi"
}
```

### Input fields

| Field | Type | Required | Validation |
| --- | --- | --- | --- |
| `name` | string | Yes | Maximum 100 characters; blank/whitespace-only names are rejected |

The controller trims surrounding whitespace before storing the name.

Example request:

```http
POST /api/v1/users
Content-Type: application/json
```

```json
{
  "name": "Alice Rossi"
}
```

### `201 Created`

```json
{
  "id": "04f40589-a621-4cb6-aa22-12a61d2289cb",
  "name": "Alice Rossi",
  "isEnabled": true
}
```

### `400 Bad Request` — blank name

```json
{
  "error": "User name is required."
}
```

If the `name` property is missing or exceeds its validation constraints, `[ApiController]` may return an automatic validation response instead.

Example automatic validation response:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "The Name field is required."
    ]
  },
  "traceId": "..."
}
```

### `409 Conflict` — duplicate user name

The duplicate check is case-insensitive.

```json
{
  "error": "A user with this name already exists.",
  "id": "04f40589-a621-4cb6-aa22-12a61d2289cb",
  "name": "Alice Rossi",
  "isEnabled": true
}
```

This response also occurs when the existing user is disabled, because the name already exists in the database.

---

## Measurement request structure

Both measurement POST endpoints use the same top-level structure:

```json
{
  "common": {
    "measuredAt": "2026-09-10T09:35:00+02:00",
    "dataPath": "C:\\Arkeo\\Results\\example.csv",
    "notes": "Optional note"
  },
  "specific": {
  }
}
```

Both `common` and `specific` are required JSON objects.

### Common fields

| Field | JSON type | Required | Stored behavior |
| --- | --- | --- | --- |
| `measuredAt` | string/date-time or `null` | No | Converted to UTC; current UTC time is used if omitted/null |
| `dataPath` | string or `null` | No | Maximum 2000 characters; blank text is stored as `null` |
| `notes` | string or `null` | No | Maximum 2000 characters; blank text is stored as `null` |

Example minimal common object:

```json
{
  "common": {},
  "specific": {}
}
```

This is structurally valid. Scientific result fields are currently nullable, so each measurement-specific object can also contain null/omitted result fields.

### Standard measurement creation response

Both JV and EQE return the same response shape after successful storage:

```json
{
  "id": "180b1aa9-643c-4b83-a0e2-62ccb1fb124c",
  "deviceId": "cdf5bade-a41d-4653-9013-dca78c491a56",
  "type": "JV",
  "measuredAt": "2026-09-10T07:35:00Z"
}
```

The response confirms the created base measurement. It does not echo all submitted scientific results.

---

## POST `/devices/{deviceId}/measurements/jv`

Stores one JV measurement against an existing device/pixel.

### Path input

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `deviceId` | GUID | Yes | Device/pixel identifier |

Example endpoint:

```text
/api/v1/devices/cdf5bade-a41d-4653-9013-dca78c491a56/measurements/jv
```

### Full request JSON

```json
{
  "common": {
    "measuredAt": "2026-09-10T09:35:00+02:00",
    "dataPath": "C:\\Arkeo\\Results\\B2026-001\\S01\\P1\\JV_001.csv",
    "notes": "Forward scan"
  },
  "specific": {
    "vocV": 1.12,
    "jscMilliampPerCm2": 23.4,
    "fillFactorPercent": 78.2,
    "efficiencyPercent": 20.5,
    "vmppV": 0.94,
    "jmppMilliampPerCm2": 21.8,
    "pmppMilliwattPerCm2": 20.5
  }
}
```

### JV-specific fields

| Field | JSON type | Required | Meaning |
| --- | --- | --- | --- |
| `vocV` | number or `null` | No | Open-circuit voltage in V |
| `jscMilliampPerCm2` | number or `null` | No | Short-circuit current density in mA/cm² |
| `fillFactorPercent` | number or `null` | No | Fill factor in % |
| `efficiencyPercent` | number or `null` | No | Power-conversion efficiency in % |
| `vmppV` | number or `null` | No | Voltage at maximum power point in V |
| `jmppMilliampPerCm2` | number or `null` | No | Current density at maximum power point in mA/cm² |
| `pmppMilliwattPerCm2` | number or `null` | No | Maximum power density in mW/cm² |

### Minimal request JSON

```json
{
  "common": {},
  "specific": {}
}
```

All JV result properties are currently nullable, so this request can create a JV measurement with no result values.

### Example request with explicit nulls

```json
{
  "common": {
    "measuredAt": null,
    "dataPath": null,
    "notes": null
  },
  "specific": {
    "vocV": null,
    "jscMilliampPerCm2": null,
    "fillFactorPercent": null,
    "efficiencyPercent": null,
    "vmppV": null,
    "jmppMilliampPerCm2": null,
    "pmppMilliwattPerCm2": null
  }
}
```

### `201 Created`

```json
{
  "id": "180b1aa9-643c-4b83-a0e2-62ccb1fb124c",
  "deviceId": "cdf5bade-a41d-4653-9013-dca78c491a56",
  "type": "JV",
  "measuredAt": "2026-09-10T07:35:00Z"
}
```

### `404 Not Found`

```json
{
  "error": "Device not found."
}
```

### `400 Bad Request`

Examples include:

- missing `common`
- missing `specific`
- malformed JSON
- invalid date/time text
- `dataPath` longer than 2000 characters
- `notes` longer than 2000 characters

These are normally returned as automatic ASP.NET Core validation/model-binding responses.

---

## POST `/devices/{deviceId}/measurements/eqe`

Stores one EQE measurement against an existing device/pixel.

### Path input

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `deviceId` | GUID | Yes | Device/pixel identifier |

Example endpoint:

```text
/api/v1/devices/cdf5bade-a41d-4653-9013-dca78c491a56/measurements/eqe
```

### Full request JSON

```json
{
  "common": {
    "measuredAt": "2026-09-10T11:20:00+02:00",
    "dataPath": "C:\\Arkeo\\Results\\B2026-001\\S01\\P1\\EQE_001.csv",
    "notes": "EQE scan"
  },
  "specific": {
    "jsc": 22.8,
    "peakEQE": 91.5
  }
}
```

### EQE-specific fields

| Field | JSON type | Required | Meaning |
| --- | --- | --- | --- |
| `jsc` | number or `null` | No | Integrated Jsc; UI currently displays it in mA/cm² |
| `peakEQE` | number or `null` | No | Peak EQE value |

Note the exact JSON casing of `peakEQE`. ASP.NET Core deserialization is generally case-insensitive, but this spelling matches the current serialized/property naming convention and should be preferred.

### Minimal request JSON

```json
{
  "common": {},
  "specific": {}
}
```

### Example request with explicit nulls

```json
{
  "common": {
    "measuredAt": null,
    "dataPath": null,
    "notes": null
  },
  "specific": {
    "jsc": null,
    "peakEQE": null
  }
}
```

### `201 Created`

```json
{
  "id": "86bdd555-a117-4503-8742-48f0cd7eb623",
  "deviceId": "cdf5bade-a41d-4653-9013-dca78c491a56",
  "type": "EQE",
  "measuredAt": "2026-09-10T09:20:00Z"
}
```

### `404 Not Found`

```json
{
  "error": "Device not found."
}
```

### `400 Bad Request`

The same model-binding/validation rules described for JV apply here.

## Measurement input summary

```text
POST /devices/{deviceId}/measurements/jv
{
  common: CommonMeasurementRequest,
  specific: JvMeasurementRequest
}

POST /devices/{deviceId}/measurements/eqe
{
  common: CommonMeasurementRequest,
  specific: EqeMeasurementRequest
}
```

Current result properties do not have numeric range validation. For example, the API model itself currently does not reject a negative efficiency or EQE value based on scientific plausibility. Any such validation would need to be added explicitly in the future.
