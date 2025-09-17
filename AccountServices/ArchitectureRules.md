### Account Management Microservice – Architecture Rules

1. **Domain-focused scope only**
- Keep this service responsible strictly for account lifecycle and balance operations.
- Integrate with other domains (Customer, Compliance) via HTTP/gRPC or messaging; avoid direct data coupling.

2. **DDD layering and dependency direction**
- Layers: `Domain` (core), `Application` (DTOs, ports), `Infrastructure` (EF Core, repositories), `API` (controllers).
- Dependencies point inward: API → Application → Domain; Infrastructure implements Application interfaces.
- Domain depends on no frameworks or persistence libraries.

3. **Rich domain model with invariants**
- Entities encapsulate behavior and enforce rules (no negative debits, freeze rules, zero balance before close).
- Use factory methods (e.g., `Account.Open(...)`) and explicit state transitions (`Credit`, `Debit`, `Freeze`, `Close`).
- Record all monetary mutations as immutable `Transaction` entries for auditability.

4. **Persistence boundaries and configuration**
- Use EF Core only in `Infrastructure`; keep mappings in `Configurations` (no data annotations in Domain).
- Enforce `AccountNumber` uniqueness; store amounts as `decimal(18,2)`; manage status transitions via domain logic.
- Repositories expose intention-revealing methods and persist using `SaveChangesAsync`.

5. **Thin API, explicit contracts, and idempotency**
- Controllers orchestrate: validate DTOs, invoke domain, persist, and return results.
- Prefer command-style endpoints for state changes; query endpoints return projections, not entities.
- Support idempotency for money movements (idempotency key), and return clear error responses for rule violations.
