using Microsoft.AspNetCore.Identity;
using task_flow.Models;
using task_flow.Models.Workspace;

namespace task_flow.Data;

public static class DbSeeder
{
  public static async Task SeedRolesAndAdmin(IServiceProvider services)
  {
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    string[] roles = ["Admin", "User"];

    foreach (var role in roles)
    {
      if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = GetRequiredEnv("ADMIN_EMAIL");
    var adminPassword = GetRequiredEnv("ADMIN_PASSWORD");
    var adminFirstName = GetRequiredEnv("ADMIN_FIRST_NAME");
    var adminLastName = GetRequiredEnv("ADMIN_LAST_NAME");

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
      var user = new ApplicationUser
      {
        UserName = adminEmail,
        Email = adminEmail,
        FirstName = adminFirstName,
        LastName = adminLastName,
        EmailConfirmed = true
      };

      var result = await userManager.CreateAsync(user, adminPassword);

      if (result.Succeeded)
      {
        await userManager.AddToRoleAsync(user, "Admin");
        adminUser = user;
      }
    }

    if (adminUser != null)
    {
      await SeedWorkspacesAndTasks(context, adminUser);
    }
  }

  private static async Task SeedWorkspacesAndTasks(
    ApplicationDbContext context,
    ApplicationUser adminUser)
  {
    // Use any existing workspace for admin; create "Default" only if none exist.
    var defaultWorkspace = context.Workspaces
      .FirstOrDefault(w => w.UserId == adminUser.Id);

    if (defaultWorkspace == null)
    {
      defaultWorkspace = new Workspace
      {
        Name = "Default",
        UserId = adminUser.Id,
        User = adminUser
      };

      context.Workspaces.Add(defaultWorkspace);
      await context.SaveChangesAsync();
    }

    // Seed sample tasks for default workspace
    if (!context.Tasks.Any(t => t.WorkspaceId == defaultWorkspace.Id))
    {
      var tasks = new List<TaskItem>
      {
        new()
        {
          Title = "[SEED] Setup project structure",
          Description = "Organize folders and create initial project files.",
          Status = "Done",
          UserId = adminUser.Id,
          WorkspaceId = defaultWorkspace.Id
        },
        new()
        {
          Title = "[SEED] Design database schema",
          Description = "Create entity relationships and migrations for the application.",
          Status = "InProgress",
          UserId = adminUser.Id,
          WorkspaceId = defaultWorkspace.Id
        },
        new()
        {
          Title = "[SEED] Implement authentication",
          Description = "Add login and registration functionality with role-based access.",
          Status = "Todo",
          UserId = adminUser.Id,
          WorkspaceId = defaultWorkspace.Id
        }
      };

      context.Tasks.AddRange(tasks);
      await context.SaveChangesAsync();
    }
  }

  private static string GetRequiredEnv(string key)
  {
    var value = Environment.GetEnvironmentVariable(key);

    if (string.IsNullOrWhiteSpace(value))
      throw new InvalidOperationException($"Missing required environment variable: {key}");

    return value;
  }
}