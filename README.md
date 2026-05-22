# portfolio-household

Household management service. Groups of users (housemates, families, flatmates) form a household and use it to coordinate shared life: chore assignments and rotation schedules, a shared calendar for events, and membership management with owner/admin/member roles.

Intentionally separate from the finance service — tracking who needs to clean the kitchen is a different domain from tracking who owes $42 for utilities.

## What it does

- **Households** — create a named household with a currency and description, invite members via join code, manage roles (Owner, Admin, Member)
- **Chores** — create chores with a name, recurrence, and assigned member; mark complete; supports rotation so the same person doesn't always do dishes
- **Calendar** — shared event log for the household (move-in dates, lease renewals, maintenance visits, anything with a date)
- **Memberships** — join/leave, role promotion, owner transfer

## Stack

- .NET 8 / ASP.NET Core Web API
- PostgreSQL 17 (EF Core)
- RabbitMQ (event publishing via MassTransit)
- Clean Architecture: Domain → Application → Infrastructure → Client

## Running locally

```bash
# From repo root — requires postgres + rabbitmq (see infra/)
dotnet run --project src/Client
```

Or via the full stack:

```bash
docker compose -f infra/compose.dev.yaml up household
```

## Structure

```
src/
  Domain/          Aggregates (Household, HouseholdMembership, Chore, HouseholdCalendarEvent),
                   value objects, domain events
  Application/     Managers (commands), query interfaces, repository interfaces
  Infrastructure/  EF Core, query implementations, repositories, MassTransit consumers
  Client/          ASP.NET Core controllers, FluentValidation validators, DI wiring
```

## API surface

| Controller | Routes | Purpose |
|---|---|---|
| `HouseholdsController` | `GET/POST /api/households`, `GET/PUT/DELETE /api/households/{id}` | CRUD + membership list |
| `ChoresController` | `GET/POST /api/households/{id}/chores`, `PUT/DELETE …/{choreId}` | Chore management |
| `CalendarController` | `GET/POST /api/households/{id}/calendar`, `PUT/DELETE …/{eventId}` | Shared calendar |

## CI/CD

Two workflows run on push to `main`:

| Workflow | File | What it does |
|---|---|---|
| **Build & Publish** | `.github/workflows/docker-publish.yml` | Runs `dotnet test`, builds the Docker image, pushes to `ghcr.io/hkarpinen/portfolio-household:latest` |
| **Deploy** | `.github/workflows/deploy.yml` | Triggers after Build & Publish succeeds; SSHes into the server, pulls the new image, and restarts only the `household` container |

### Required GitHub Actions secrets

| Secret | Description |
|---|---|
| `DEPLOY_HOST` | VPS IP address or hostname |
| `DEPLOY_USER` | SSH user on the server |
| `DEPLOY_KEY` | Private SSH key for that user |
| `DEPLOY_PATH` | Absolute path to the infra directory on the server |

## Environment variables

| Variable | Description |
|---|---|
| `ConnectionStrings__Household` | PostgreSQL connection string |
| `Jwt__Secret` | JWT signing key (≥ 32 chars, shared with identity service) |
| `RabbitMq__Host` | RabbitMQ hostname |
| `RabbitMq__Username` | RabbitMQ username |
| `RabbitMq__Password` | RabbitMQ password |
