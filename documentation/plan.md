# Plan

Working plan for the Claims API task. Tasks are not done in README order: the premium fix
is small and independent, and the tests in Task 4 only make sense after the layering in Task 1.

1. Task 2 – Validation – **done**
2. Task 5 – Premium bugs – **done**
3. Task 1 – Layering – not started
4. Task 3 – Async auditing – not started
5. Task 4 – Tests – not started
6. Pull request description – not started

---

## 1. Validation (Task 2) – done

Rules live in `Claims/Validation`:

- `CoverValidator` – start date not in the past, end date after start date, period of at most one year.
- `ClaimValidator` – damage cost greater than 0 and up to 100.000, cover must exist, created date inside the cover period.

Decisions:

- Validators return a dictionary of errors, so the API can answer with a standard
  `400 ValidationProblemDetails` listing every failed rule at once.
- `TimeProvider` is injected so "not in the past" can be tested with a fixed date.
- `ClaimValidator` gets the cover passed in instead of loading it, so it stays free of the database.

Currently the controllers call the validators. This moves into the service layer in step 3.

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

## 3. Layering (Task 1)

Goal: thin controllers that only translate between HTTP and the application.

```
Controllers  →  Services  →  Repositories  →  EF Core contexts
                   ↓
              Validators, PremiumCalculator, Auditer
```

Steps:

1. Move `ClaimsContext` out of `ClaimsController.cs` into a data folder.
2. Add `IClaimsRepository` / `ICoversRepository` on top of the contexts.
3. Add `ClaimsService` / `CoversService` that validate, save and audit.
4. Services return a result with the validation errors instead of throwing, and the
   controllers map a failed result to `400 ValidationProblemDetails`.
5. Register the auditer through DI instead of `new Auditer(...)` in the controllers.
6. Fix small API issues on the way: `GET /covers/{id}` loads the whole collection,
   `POST` returns `200` instead of `201`, missing items return `200` with null instead of `404`.
7. Move the DI registrations from `Program.cs` into extension methods to keep it short.
8. Add XML summaries on the public API so Swagger shows proper documentation.

## 4. Async auditing (Task 3)

Problem: every request waits for the SQL insert into the audit tables.

Approach, in-memory and without extra infrastructure:

- The service writes an audit message to a `Channel<T>` and returns right away.
- A `BackgroundService` reads from the channel and writes to `AuditContext` in its own scope.

Trade-off to mention: audits waiting in the channel are lost if the process crashes.
A queue such as Azure Service Bus would fix that, but is more than this task needs.

## 5. Tests (Task 4)

Keep it to tests that prove the rules, not every line:

- Validators – already covered.
- Premium calculator – the cases from step 2.
- Services – validation failure stops the save, success saves and enqueues an audit
  (using fake repositories, no database).
- Keep the existing integration test working.

## 6. Pull request

Short description for reviewers:

- Where to look first: services, premium calculator, audit background worker.
- Assumptions: end-date handling in premium, UTC dates, audit loss on crash.
- What to discuss in the interview: why a channel instead of a message bus, and how the
  layering would grow with more entities.
