# GET Endpoints

Base development URL used in examples:

```text
http://localhost:5227/api/v1
```

## GET `/health`

Checks whether the API can reach the SQLite database.

### Request

No path, query, or JSON input.

```http
GET /api/v1/health
```

### `200 OK`

```json
{
  "status": "ok",
  "database": "available"
}
```

### `503 Service Unavailable`

```json
{
  "status": "error",
  "database": "unavailable"
}
```

---

## GET `/users`

Returns all enabled research users, ordered by name.

### Request

No path, query, or JSON input.

```http
GET /api/v1/users
```

### `200 OK`

```json
[
  {
    "id": "04f40589-a621-4cb6-aa22-12a61d2289cb",
    "name": "Alice Rossi"
  },
  {
    "id": "73465bad-30d4-4383-a304-89429083a9dc",
    "name": "Bob Bianchi"
  }
]
```

If no enabled users exist:

```json
[]
```

Only enabled users are returned. `isEnabled` itself is not included in this lightweight response.

---

## GET `/batches?userId={userId}`

Returns the batches belonging to one enabled research user.

This is a lightweight list intended for selectors/dropdowns.

### Query input

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | GUID | Yes | Research user identifier |

Example:

```http
GET /api/v1/batches?userId=04f40589-a621-4cb6-aa22-12a61d2289cb
```

### `200 OK`

```json
[
  {
    "id": "47d13af8-e28a-42bf-a091-a889f76bbab9",
    "code": "B2026-001",
    "productionDate": "2026-09-10"
  },
  {
    "id": "5eabfb5c-f89a-4c2a-9a6e-b75bb2c58b25",
    "code": "B2026-002",
    "productionDate": "2026-09-08"
  }
]
```

If the user exists but has no batches:

```json
[]
```

### `400 Bad Request` — missing `userId`

```json
{
  "error": "userId is required."
}
```

### `404 Not Found` — user missing or disabled

```json
{
  "error": "Active user not found."
}
```

A malformed GUID may instead produce an automatic ASP.NET Core model-binding `400` response.

---

## GET `/batches/{id}`

Returns all currently exposed non-measurement metadata for one batch, including:

- owning research user
- batch metadata
- device stack and layers
- substrates/samples
- devices/pixels
- notes at batch, substrate, and device level

Measurement history is intentionally not included in this endpoint.

### Path input

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | GUID | Yes | Batch identifier |

Example:

```http
GET /api/v1/batches/47d13af8-e28a-42bf-a091-a889f76bbab9
```

### `200 OK`

Example with a device stack:

```json
{
  "id": "47d13af8-e28a-42bf-a091-a889f76bbab9",
  "userId": "04f40589-a621-4cb6-aa22-12a61d2289cb",
  "code": "B2026-001",
  "productionDate": "2026-09-10",
  "area": 0.16,
  "notes": "Reference batch",
  "createdAt": "2026-09-10T08:15:42.1234567Z",
  "deviceStackId": "0b1f7832-3f19-4df4-ad29-ad31050f15cb",
  "user": {
    "id": "04f40589-a621-4cb6-aa22-12a61d2289cb",
    "name": "Alice Rossi",
    "isEnabled": true
  },
  "deviceStack": {
    "id": "0b1f7832-3f19-4df4-ad29-ad31050f15cb",
    "layers": [
      {
        "id": "e6289384-a156-4875-badb-ec0e44e70d08",
        "deviceStackId": "0b1f7832-3f19-4df4-ad29-ad31050f15cb",
        "position": 1,
        "material": "ITO"
      },
      {
        "id": "dd9c7792-c122-4561-b08d-74fcf608bcf3",
        "deviceStackId": "0b1f7832-3f19-4df4-ad29-ad31050f15cb",
        "position": 2,
        "material": "SnO2"
      },
      {
        "id": "2afcdcf7-d171-461c-b73c-e79dab7d1022",
        "deviceStackId": "0b1f7832-3f19-4df4-ad29-ad31050f15cb",
        "position": 3,
        "material": "Perovskite"
      }
    ]
  },
  "substrates": [
    {
      "id": "c5aab754-8bcd-4805-98db-71686e4d90f6",
      "batchId": "47d13af8-e28a-42bf-a091-a889f76bbab9",
      "code": "S01",
      "notes": "First substrate",
      "devices": [
        {
          "id": "cdf5bade-a41d-4653-9013-dca78c491a56",
          "sampleId": "c5aab754-8bcd-4805-98db-71686e4d90f6",
          "pixel": "P1",
          "notes": "Good pixel"
        },
        {
          "id": "54b55b20-b716-47b6-a11a-d51960b38542",
          "sampleId": "c5aab754-8bcd-4805-98db-71686e4d90f6",
          "pixel": "P2",
          "notes": ""
        }
      ]
    }
  ]
}
```

Example when no device stack is assigned:

```json
{
  "id": "47d13af8-e28a-42bf-a091-a889f76bbab9",
  "userId": "04f40589-a621-4cb6-aa22-12a61d2289cb",
  "code": "B2026-001",
  "productionDate": "2026-09-10",
  "area": null,
  "notes": null,
  "createdAt": "2026-09-10T08:15:42.1234567Z",
  "deviceStackId": null,
  "user": {
    "id": "04f40589-a621-4cb6-aa22-12a61d2289cb",
    "name": "Alice Rossi",
    "isEnabled": true
  },
  "deviceStack": null,
  "substrates": []
}
```

### `404 Not Found`

```json
{
  "error": "Batch not found."
}
```

A malformed batch GUID does not match the `{id:guid}` route and will not be handled by this action.

---

## GET `/devices?userId={userId}`

Returns a lightweight list of all devices/pixels belonging to batches owned by one enabled user.

This endpoint is currently intended for selector/dropdown use.

### Query input

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | GUID | Yes | Research user identifier |

Example:

```http
GET /api/v1/devices?userId=04f40589-a621-4cb6-aa22-12a61d2289cb
```

### `200 OK`

```json
[
  {
    "id": "cdf5bade-a41d-4653-9013-dca78c491a56",
    "label": "B2026-001 - S01 - P1"
  },
  {
    "id": "54b55b20-b716-47b6-a11a-d51960b38542",
    "label": "B2026-001 - S01 - P2"
  }
]
```

If the enabled user has no devices:

```json
[]
```

### `400 Bad Request` — missing `userId`

```json
{
  "error": "userId is required."
}
```

### `404 Not Found` — user missing or disabled

```json
{
  "error": "Active user not found."
}
```

---

## GET `/devices/{id}`

Returns the currently exposed metadata for one stable device/pixel GUID.

### Path input

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | GUID | Yes | Device identifier |

Example:

```http
GET /api/v1/devices/cdf5bade-a41d-4653-9013-dca78c491a56
```

### `200 OK`

```json
{
  "id": "cdf5bade-a41d-4653-9013-dca78c491a56",
  "label": "B2026-001 - S01 - P1",
  "userId": "04f40589-a621-4cb6-aa22-12a61d2289cb",
  "userName": "Alice Rossi",
  "batchId": "47d13af8-e28a-42bf-a091-a889f76bbab9",
  "batchCode": "B2026-001",
  "productionDate": "2026-09-10",
  "area": 0.16,
  "sampleId": "c5aab754-8bcd-4805-98db-71686e4d90f6",
  "sampleCode": "S01",
  "pixel": "P1",
  "stack": [
    "ITO",
    "SnO2",
    "Perovskite"
  ],
  "batchNotes": "Reference batch",
  "sampleNotes": "First substrate",
  "deviceNotes": "Good pixel"
}
```

When no stack is assigned, `stack` is an empty array:

```json
"stack": []
```

Nullable batch metadata can appear as `null`, for example:

```json
{
  "area": null,
  "batchNotes": null
}
```

`sampleNotes` and `deviceNotes` are currently modelled as non-null strings and may be returned as empty strings.

### `404 Not Found`

```json
{
  "error": "Device not found."
}
```

## Current GET response hierarchy summary

```text
/users
  -> enabled users only

/batches?userId=...
  -> lightweight batch list

/batches/{id}
  -> user
  -> batch metadata
  -> stack + stack layers
  -> substrates
      -> devices/pixels

/devices?userId=...
  -> lightweight device list with display label

/devices/{id}
  -> device metadata + batch/sample context + flattened stack materials
```
