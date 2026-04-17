using Microsoft.AspNetCore.Identity;
using task_flow.Areas.Admin.Models;
using task_flow.Models;

namespace task_flow.Areas.Admin.Services;

public class AdminService : IAdminService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private static readonly string[] AvailableRoles = ["Admin", "User"];

  public AdminService(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public List<ApplicationUser> GetAllUsers()
  {
    return _userManager.Users.ToList();
  }

  public async Task<AdminUserEditViewModel?> GetUserForEditAsync(string id)
  {
    var user = await _userManager.FindByIdAsync(id);
    if (user == null) return null;

    var roles = await _userManager.GetRolesAsync(user);
    return new AdminUserEditViewModel
    {
      UserId = user.Id,
      Email = user.Email ?? string.Empty,
      Role = roles.FirstOrDefault() ?? "User",
      AvailableRoles = AvailableRoles
    };
  }

  public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
  {
    if (!AvailableRoles.Contains(newRole)) return false;

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return false;

    var currentRoles = await _userManager.GetRolesAsync(user);
    await _userManager.RemoveFromRolesAsync(user, currentRoles);
    await _userManager.AddToRoleAsync(user, newRole);
    return true;
  }

  public async Task<bool> DeleteUserAsync(string id, string currentUserId)
  {
    if (id == currentUserId) return false; // can't delete yourself

    var user = await _userManager.FindByIdAsync(id);
    if (user == null) return false;

    await _userManager.DeleteAsync(user);
    return true;
  }
}