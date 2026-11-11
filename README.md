# Claims API

Backend for handling insurance claims. A cover insures a ship for a period and has a premium.
A claim is filed against a cover. Every create and delete is audited in the background.

## Running

Requirements: .NET 10 SDK and a running Docker daemon.

```
dotnet run --project src/Claims
```

On startup the app starts SQL Server (audits) and MongoDB (claims and covers) with Testcontainers
and applies the audit migrations. Swagger is available at `/swagger` in the Development environment.

## Tests

```
dotnet test --solution Claims.sln
```

The integration test starts the app with its containers, so Docker must be running.

## Structure

```
src/Claims/
  Controllers/    HTTP only: map requests to services and results to status codes
  Services/       business logic, validators, premium calculator
  Repositories/   data access behind interfaces
  Auditing/       audit queue and background worker
  Data/           EF Core contexts and migrations
  Models/         stored entities
  DTOs/           request and response records
  Extensions/     DI registration
tests/Claims.Tests/
```

Controllers call services, services call repositories, repositories use the EF Core contexts.

## Endpoints

| Method | Route | Result |
|---|---|---|
| GET | `/Covers` | `200` all covers |
| GET | `/Covers/{id}` | `200` cover, `404` if missing |
| POST | `/Covers` | `201` created cover, `400` validation errors |
| DELETE | `/Covers/{id}` | `204`, `404` if missing |
| POST | `/Covers/compute?startDate&endDate&coverType` | `200` premium, `400` validation errors |
| GET | `/Claims` | `200` all claims |
| GET | `/Claims/{id}` | `200` claim, `404` if missing |
| POST | `/Claims` | `201` created claim, `400` validation errors |
| DELETE | `/Claims/{id}` | `204`, `404` if missing |

Validation errors are returned as `ValidationProblemDetails` with every failed rule.

## Rules

Cover:
- StartDate cannot be in the past (UTC date).
- EndDate must be after StartDate and before the same date one year later.

Claim:
- DamageCost must be greater than 0 and at most 100.000.
- The cover must exist and Created must be within the cover period.

Dates are compared by day and the end date is inclusive.

## Auditing

Creating or deleting a claim or cover puts an audit message on an in-memory `Channel` and the
request returns without waiting for SQL Server. `AuditWorker`, a `BackgroundService`, saves the
messages to the audit tables. A failed insert is logged and skipped. On shutdown the worker
saves what is still queued, but audits that were queued when the process crashed are lost.

## Premium

Day rate is 1250 × a multiplier per type: Yacht 1.1, Passenger ship 1.2, Tanker 1.5, other types 1.3.

| Days | Yacht | Other types |
|---|---|---|
| 1 – 30 | full rate | full rate |
| 31 – 180 | 5% discount | 2% discount |
| 181 – end | 8% discount | 3% discount |