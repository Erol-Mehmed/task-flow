using Microsoft.AspNetCore.Identity;
using task_flow.Models;

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

    // Seed some tasks for the admin user
    if (!context.Task.Any() && adminUser != null)
    {
      context.Task.AddRange(
        new TaskItem
        {
          Title = "[SEED] Setup project structure",
          Description = "This is an example task created by the seeder to demonstrate functionality.",
          Status = "Todo"
        },
        new TaskItem
        {
          Title = "[SEED] Implement authentication",
          Description = "Example task for showing how authentication tasks appear in the system.",
          Status = "InProgress"
        },
        new TaskItem
        {
          Title = "[SEED] UI layout for dashboard",
          Description = "Example task used to demonstrate UI rendering in the task board.",
          Status = "Done"
        }
      );

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