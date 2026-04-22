# TaskFlow

A task management web app built with ASP.NET Core MVC. Users can organize work across personal tasks and shared
workspaces, leave comments, attach tags, and track changes through an activity log. Admins get a dedicated area to
manage users and roles.

---

## Features

- Cookie-based authentication via ASP.NET Core Identity
- Two roles: `Admin` and `User`
- Personal tasks — owned by the creating user, only visible/editable by them (or an admin)
- Workspaces — shared boards; any authenticated user can access workspace tasks, only the owner or an admin can manage
  the workspace itself
- Task board (`BoardController`) with search, status filter, and pagination (6 per page)
- Comments on tasks — add or remove per task
- Tags — global tag pool; assigning an existing tag name reuses it instead of creating a duplicate
- Activity log — every create, update, delete, comment, and tag action is recorded automatically
- Admin area — list all users, change roles, delete other accounts

---

## Architecture

The project uses a layered approach:

```
Request → Controller → Service → Repository → EF Core / PostgreSQL
```

**Controllers** handle HTTP, model binding, and authorization checks. They delegate all business decisions to a service.

**Services** contain the business logic: access rules, validation beyond model attributes, orchestration across multiple
repositories, and activity logging.

**Repositories** wrap EF Core queries. Each entity has its own interface and concrete implementation. This keeps EF out
of the service layer and makes unit testing straightforward with mocks.

**Models** are plain EF entities with data annotation constraints. View-specific models (`*ViewModel`) are separate.

---

## Layers and Services

| Service            | Responsibility                                                                       |
|--------------------|--------------------------------------------------------------------------------------|
| `TaskService`      | CRUD, ownership checks (`CanUserAccessTask`), filtering/pagination in `GetTasks`     |
| `WorkspaceService` | CRUD, access checks (`CanUserAccessWorkspace`, `CanUserManageWorkspace`)             |
| `CommentService`   | Add/delete comments, trims content before saving                                     |
| `TagService`       | Add/remove tags, deduplicates by name across the tag pool, skips if already assigned |
| `ActivityService`  | Writes an `ActivityLog` record after every state-changing operation                  |
| `AdminService`     | User listing, role change, account deletion — wraps `UserManager<ApplicationUser>`   |

Access rules:

- A task with a `WorkspaceId` is accessible to everyone.
- A personal task (no `WorkspaceId`) is accessible only to its owner or an admin.
- A workspace can be edited or deleted only by its creator or an admin.

---

## Validations

Model-level constraints are enforced by data annotations and `ModelState.IsValid` checks in every POST action:

| Model             | Field                   | Rule                                                                |
|-------------------|-------------------------|---------------------------------------------------------------------|
| `TaskItem`        | `Title`                 | Required, max 50 chars                                              |
| `TaskItem`        | `Description`           | Max 500 chars                                                       |
| `TaskItem`        | `Status`                | Max 20 chars, defaults to `"Todo"`                                  |
| `Workspace`       | `Name`                  | Required, max 100 chars, unique (DB constraint `IX_Workspace_Name`) |
| `Comment`         | `Content`               | Required, max 1000 chars                                            |
| `Tag`             | `Name`                  | Required, max 50 chars                                              |
| `ApplicationUser` | `FirstName`, `LastName` | Required, max 100 chars                                             |

Duplicate workspace names are caught at the DB level (`PostgresException` with `UniqueViolation`) and surfaced as a
`ModelState` error rather than a 500.

---

## Project Structure

```
task-flow/
  Areas/Admin/
    Controllers/    AdminController
    Models/         AdminUserEditViewModel
    Services/       AdminService + IAdminService
    Views/
  Controllers/      TaskController, WorkspaceController, BoardController,
                    CommentController, TagController, ActivityController
  Data/
    ApplicationDbContext.cs
    DbSeeder.cs
    Migrations/
  Models/
    TaskItem.cs, Workspace/, Comment/, Tag/, Activity/
    ApplicationUser.cs
  Repositories/
    TaskRepository/, WorkspaceRepository/, CommentRepository/,
    TagRepository/, ActivityRepository/
  Services/
    TaskService/, WorkspaceService/, CommentService/,
    TagService/, ActivityService/
  Views/
  tests/
    task-flow.Tests/
      Services/       TaskServiceTests, WorkspaceServiceTests, CommentServiceTests,
                      TagServiceTests, ActivityServiceTests, AdminServiceTests
      Controllers/    TaskControllerTests, BoardControllerTests, AdminControllerTests
      Helpers/        MockHelper
  Program.cs
  task-flow.csproj
```

---

## Seeding

`Data/DbSeeder.cs` runs at startup and is idempotent:

1. Creates roles `Admin` and `User` if they don't exist.
2. Reads `ADMIN_EMAIL`, `ADMIN_PASSWORD`, `ADMIN_FIRST_NAME`, `ADMIN_LAST_NAME` from environment variables — throws
   `InvalidOperationException` if any are missing.
3. Creates the admin account and assigns the `Admin` role if the email is not already registered.
4. Creates a `"Default"` workspace for the admin if none exist.
5. Seeds three example tasks into that workspace if it is empty.

---

## Tech Stack

- .NET 8, ASP.NET Core MVC
- Entity Framework Core 8 with `Npgsql` (PostgreSQL)
- ASP.NET Core Identity
- PostgreSQL (via Docker)
- xUnit, Moq, EF Core InMemory (tests)

---

## Setup

**Prerequisites:** .NET SDK 8, Docker Desktop, `dotnet-ef` CLI.

```zsh
dotnet tool install --global dotnet-ef
```

**1. Configure environment**

```zsh
cp .env.example .env
```

Edit `.env` with your database connection string and the four `ADMIN_*` values. The app loads this file at startup via
`DotNetEnv`.

**2. Start the database and apply migrations**

```zsh
docker compose up -d
dotnet ef database update
```

**3. Run**

```zsh
dotnet run
```

Open your browser to `http://localhost:5087` and sign in with the admin credentials from your `.env` file.

---

## Testing

Tests are under `tests/task-flow.Tests` and use xUnit + Moq.

Service tests use either:

- **In-memory EF Core** (`Microsoft.EntityFrameworkCore.InMemory`) for query/filter/pagination logic that relies on
  `IQueryable`
- **Mocked repositories** for business logic paths that don't need a real database

Controller tests mock the service layer entirely and validate HTTP outcomes (redirects, view results, status codes).

**Run tests:**

```zsh
dotnet test tests/task-flow.Tests/task-flow.Tests.csproj
```

**Run with coverage:**

```zsh
dotnet test tests/task-flow.Tests/task-flow.Tests.csproj --collect:"XPlat Code Coverage"
```
