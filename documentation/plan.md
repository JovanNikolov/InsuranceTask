# Plan

Working plan for the Claims API task. Tasks are not done in README order: the premium fix
is small and independent, and the tests in Task 4 only make sense after the layering in Task 1.

1. Task 2 – Validation – **done**
2. Task 5 – Premium bugs – **done**
3. Task 1 – Layering – **done**
4. Task 3 – Async auditing – **done**
5. Task 4 – Tests – **done**
6. Pull request description – not started

---

## 1. Validation (Task 2) – done

Rules live in `src/Claims/Services`:

- `CoverValidator` – start date not in the past, end date after start date, period of at most one year.
- `ClaimValidator` – damage cost not negative and up to 100.000, cover must exist, created date inside the cover period.

Decisions:

- Validators return a dictionary of errors, so the API can answer with a standard
  `400 ValidationProblemDetails` listing every failed rule at once.
- `TimeProvider` is injected so "not in the past" can be tested with a fixed date.
- `ClaimValidator` gets the cover passed in instead of loading it, so it stays free of the database.

The services call the validators, see step 3.

## 2. Premium computation (Task 5) – done

Bugs in `CoversController.ComputePremium`:

- The `if` blocks are not exclusive, so a day in the first 30 is charged three times
  and a day between 31 and 180 is charged twice.
- Days after day 365 are not charged at all. Validation now caps the period at a year,
  but the calculation should not depend on that.

Target logic:

- Days 1 – 30: full rate for all types.
- Days 31 – 180: 5% discount for Yacht, 2% for other types.
- Days 181 – end: 8% discount for Yacht, 3% for other types.

Day rate is `1250 × multiplier` (Yacht 1.1, Passenger ship 1.2, Tanker 1.5, other 1.3).

Steps:

1. Move the calculation into its own class, e.g. `PremiumCalculator`.
2. Replace the loop with a sum per range (days in range × discounted rate).
3. Add a few tests: a short period, one crossing day 30, one crossing day 180, and a Yacht case.
4. The end date counts as an insurance day, the same as in `ClaimValidator`. `CoverValidator` now
   requires the end date to be before the same date next year, so a cover is at most 365 or 366 days.

## 3. Layering (Task 1) – done

Goal: thin controllers that only translate between HTTP and the application.

```
Controllers  →  Services  →  Repositories  →  EF Core contexts
                   ↓
              Validators, PremiumCalculator
```

Structure:

```
Claims.sln
src/Claims/
  Controllers/    HTTP only
  Services/       business logic, validators, premium calculator
  Repositories/   data access behind interfaces
  Data/           ClaimsContext, AuditContext, Migrations
  Models/         stored entities
  DTOs/           request and response records
  Extensions/     DI registration
tests/Claims.Tests/
```

Done:

1. `ClaimsContext` moved out of `ClaimsController.cs` into `Data`.
2. `IClaimsRepository`, `ICoversRepository` and `IAuditRepository` on top of the contexts.
3. `ClaimsService` / `CoversService` validate, save and audit.
4. Services return a `ServiceResult` with the validation errors, and the controllers map a
   failed result to `400 ValidationProblemDetails`.
5. Auditing goes through `IAuditRepository` from DI instead of `new Auditer(...)` in the controllers.
6. API fixes: `GET /covers/{id}` queries one cover instead of the whole collection, `POST` returns
   `201` with a location, missing items return `404`, `DELETE` returns `204`, or `404` if nothing was deleted.
7. DI registrations moved from `Program.cs` into extension methods.
8. Requests and responses use DTOs, so a client cannot set `Id` or `Premium`.

9. Documentation: `README.md` at the root, and `EndpointSummary` / `ProducesResponseType` attributes so
   Swagger describes every endpoint and its status codes without comments in the code.

## 4. Async auditing (Task 3) – done

Problem: every POST and DELETE waited for the SQL insert into the audit tables, so a slow or
unavailable SQL Server slowed down or broke requests that had already saved to MongoDB.

Options considered:

- `Task.Run` / fire-and-forget – rejected, the scoped `DbContext` is disposed after the request
  and exceptions are lost.
- Outbox – does not fit, claims and covers are in MongoDB and audits in SQL Server, so there is
  no shared transaction.
- Azure Service Bus – durable, but needs infrastructure this task does not need.
- `Channel<T>` with a `BackgroundService` – chosen, in-memory and no extra infrastructure.

Done, in `src/Claims/Auditing`:

1. Services call `IAuditQueue.Enqueue`, which writes an `AuditMessage` to an unbounded channel and
   returns right away. The timestamp is taken at that moment, in UTC from `TimeProvider`.
2. `AuditWorker` reads the channel and saves each message through `IAuditRepository` in its own scope.
3. A failed insert is logged and the worker continues with the next message.
4. On shutdown the channel is completed and the worker saves what is still queued.

Trade-offs:

- Audits still in the channel are lost if the process crashes. Service Bus or another durable
  queue would fix that.
- The channel is unbounded. A bounded one would force a choice between blocking the request and
  dropping audits.
- No retries, failures are only logged. Retrying in memory does not help much while SQL is down.

## 5. Tests (Task 4) – done

Keep it to tests that prove the rules, not every line:

- Validators – already covered.
- Premium calculator – the cases from step 2.
- Services – validation failure stops the save, success saves and enqueues an audit
  (using fake repositories from `Fakes.cs`, no database). `ClaimsServiceTests` and `CoversServiceTests`.
- Audit worker – queued audits are saved and a failure does not stop the worker.
- Integration tests in `ClaimsControllerTests` – the app runs with its real containers: create, read and
  delete a claim, and a `400` with the failed rules for an invalid claim. Controllers have no unit tests,
  they only map service results to status codes.

## 6. Pull request

Short description for reviewers:

- Where to look first: services, premium calculator, audit background worker.
- Assumptions: end-date handling in premium, UTC dates, audit loss on crash.
- What to discuss in the interview: why a channel instead of a message bus, and how the
  layering would grow with more entities.
