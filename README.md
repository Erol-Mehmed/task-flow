# TaskFlow

TaskFlow is an ASP.NET Core MVC course project for personal task management with authentication, role-based authorization, and an admin area.

## What It Includes

- ASP.NET Core Identity authentication (`ApplicationUser`)
- Roles: `Admin` and `User`
- Task CRUD with ownership checks (users manage only their own tasks)
- Admin-only user management (`Areas/Admin`)
- Search, status filter, and pagination on the dashboard
- PostgreSQL + EF Core migrations + startup seeding

## Tech Stack

- .NET 8 (`ASP.NET Core MVC`)
- Entity Framework Core (`Npgsql` provider)
- ASP.NET Core Identity
- PostgreSQL (Docker-friendly setup)
- xUnit + Moq + EF Core InMemory for tests

## Quick Start

1. Copy `.env.example` to `.env` and adjust values if needed.
2. Start PostgreSQL with Docker.
3. Apply EF migrations.
4. Run the app.

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
cp .env.example .env
docker compose up -d
dotnet ef database update
dotnet run
```

Default development URLs are defined in `Properties/launchSettings.json`:
- `http://localhost:5087`
- `https://localhost:7095`

## Prerequisites

- .NET SDK 8.x
- Docker Desktop (or a local PostgreSQL instance)
- EF Core CLI tools (`dotnet-ef`)

If `dotnet ef` is missing:

```zsh
dotnet tool install --global dotnet-ef
```

## Environment Configuration

The application reads database and admin seed values from environment variables (`DotNetEnv.Env.Load()` in `Program.cs`).

Use `.env.example` as the canonical template:

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
cp .env.example .env
```

Then edit `.env` with your own values (database connection and `ADMIN_*` seed credentials).

## Database and Seeding

On startup, `Data/DbSeeder.cs`:

1. Ensures roles `Admin` and `User` exist.
2. Creates admin user from `ADMIN_*` variables (if missing).
3. Assigns the `Admin` role.
4. Seeds example tasks when the task table is empty.

If required `ADMIN_*` variables are missing, startup seeding throws an error.

Start database and apply schema:

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
docker compose up -d
dotnet ef database update
```

## Run the App

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet run
```

Sign in with the admin credentials from your `.env` file.

## Testing

Tests are under `tests/task-flow.Tests`.

Current coverage focus:
- `HomeController`
- `TaskController`
- `Areas/Admin/Controllers/AdminController`

Run tests:

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet test tests/task-flow.Tests/task-flow.Tests.csproj
```

Generate coverage artifact:

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet test tests/task-flow.Tests/task-flow.Tests.csproj --collect:"XPlat Code Coverage"
```

Coverage files are generated under `tests/task-flow.Tests/TestResults/*`.

## Project Structure

```text
task-flow/
  Areas/Admin/
  Controllers/
  Data/
  Models/
  Views/
  tests/task-flow.Tests/
  Program.cs
  task-flow.csproj
```

## Known Limitations

- No separate service layer yet (`controllers` contain business flow).
- No integration tests yet (`WebApplicationFactory` can be added next).

## Suggested Next Improvements

- Extract task/admin workflows into dedicated services.
- Add DTO mapping and validation-focused service methods.
- Add integration tests for auth + routing + seeding scenarios.

