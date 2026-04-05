# TaskFlow

TaskFlow is an ASP.NET Core MVC task management application with authentication, role-based authorization, admin user management, task CRUD operations, filtering, pagination, and modal-based task actions.

## Project Concept

The project is built as a practical course project for managing personal tasks in a secure multi-user environment:

- Regular users can manage only their own tasks.
- Admin users can view all tasks and manage platform users/roles.
- The app uses server-rendered Razor views for fast development and clear MVC separation.

## Features

### Authentication and Authorization

- ASP.NET Core Identity integration (`ApplicationUser`)
- Role support: `Admin`, `User`
- Protected task/admin routes with `[Authorize]`
- Role-protected admin area with `[Authorize(Roles = "Admin")]`

### Task Management

- Create, edit, delete task
- Task fields: title, description, status
- Authorization checks so users cannot edit/delete other users' tasks
- Admin can access all tasks

### Home Dashboard

- Task board view for logged-in user
- Search by title
- Filter by status (`Todo`, `InProgress`, `Done`)
- Pagination (6 items per page)
- Task details modal with read-more/less behavior
- Shared delete confirmation modal reused across pages

### Admin Area

- List users
- Change user role
- Delete users

## Design Decisions

- **MVC + Razor Pages/Views**: chosen for simple, maintainable server-side rendering.
- **Identity + Roles**: built-in secure auth/authorization with minimum custom code.
- **EF Core + PostgreSQL**: reliable relational model and migrations support.
- **Environment variables for secrets**: DB/admin credentials are not hardcoded.
- **Shared partials**: common modal UI extracted to avoid duplicated markup/logic.

## Architecture and Layers

The current architecture is a clean MVC app with clear responsibilities:

- **Presentation Layer**
  - Controllers: `Controllers/*`, `Areas/Admin/Controllers/*`
  - Views: `Views/*`, `Areas/Admin/Views/*`
- **Domain Layer**
  - Entities/Models: `Models/ApplicationUser.cs`, `Models/TaskItem.cs`
- **Data Access Layer**
  - EF Core context: `Data/ApplicationDbContext.cs`
  - Migrations: `Data/Migrations/*`
  - Seeding: `Data/DbSeeder.cs`

### Service Layer Note

There is currently **no separate custom service layer** (`Services/*`).
Business logic is in controllers + EF queries. This is acceptable for the current scope and is covered by unit tests.
A future refactor can extract task/user workflows into dedicated services.

## Data Model and Validations

### `TaskItem`

- `Title` (required, max 50)
- `Description` (optional, max 500)
- `Status` (max 20, default `Todo`)
- `UserId` (owner id)

### `ApplicationUser`

- Extends Identity user
- `FirstName` (required, max 100)
- `LastName` (required, max 100)

## Seeding Strategy

On startup, `DbSeeder.SeedRolesAndAdmin(...)`:

1. Creates roles: `Admin`, `User`.
2. Creates admin user from environment variables if missing.
3. Adds admin role to that user.
4. Seeds initial tasks when the task table is empty.

Required admin env vars:

- `ADMIN_EMAIL`
- `ADMIN_PASSWORD`
- `ADMIN_FIRST_NAME`
- `ADMIN_LAST_NAME`

## Setup Instructions

## 1) Prerequisites

- .NET SDK 8.x
- Docker Desktop (recommended for PostgreSQL)

## 2) Configure environment variables

Create a `.env` file in the project root (`task-flow/`):

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=taskflow_db
DB_USER=postgres
DB_PASSWORD=postgres

ADMIN_EMAIL=admin@taskflow.local
ADMIN_PASSWORD=Admin123!
ADMIN_FIRST_NAME=System
ADMIN_LAST_NAME=Admin
```

## 3) Start PostgreSQL

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
docker compose up -d
```

## 4) Apply migrations

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet ef database update
```

## 5) Run the app

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet run
```

## Testing

Unit tests are in `tests/task-flow.Tests` and currently validate controllers and helper behavior.

Covered areas:

- `HomeController` (filtering, pagination, user scoping)
- `TaskController` (CRUD + authorization + redirect logic)
- `AdminController` (user listing, role update, deletion)

Run tests:

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet test tests/task-flow.Tests/task-flow.Tests.csproj
```

Generate coverage report artifacts (for instructor submission):

```zsh
cd /Users/erolmehmed/Projects/GitHub/task-flow
dotnet test tests/task-flow.Tests/task-flow.Tests.csproj --collect:"XPlat Code Coverage"
```

> This command generates `coverage.cobertura.xml` under `TestResults/*`.

## Test Coverage Requirement (Course)

The project includes controller/business-flow unit tests and coverage tooling setup (`coverlet.collector`) to measure coverage.
Use the coverage command above and include the produced report in your submission package if your instructor requires a numeric threshold check (e.g., 65%+).

## Project Structure (Simplified)

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

## Future Improvements

- Extract business logic to dedicated services (`ITaskService`, `IAdminService`)
- Add DTOs and mapping for cleaner controller boundaries
- Add integration tests (`WebApplicationFactory`)
- Improve client-side accessibility and keyboard interactions

