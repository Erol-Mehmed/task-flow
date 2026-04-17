using task_flow.Areas.Admin.Models;
using task_flow.Models;

namespace task_flow.Areas.Admin.Services;

public interface IAdminService
{
  List<ApplicationUser> GetAllUsers();
  Task<AdminUserEditViewModel?> GetUserForEditAsync(string id);
  Task<bool> UpdateUserRoleAsync(string userId, string newRole);
  Task<bool> DeleteUserAsync(string id, string currentUserId);
}