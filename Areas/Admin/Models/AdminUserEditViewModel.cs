using System.ComponentModel.DataAnnotations;

namespace task_flow.Areas.Admin.Models;

public class AdminUserEditViewModel
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  [Required]
  public string Role { get; set; } = "User";

  public IReadOnlyList<string> AvailableRoles { get; set; } = ["Admin", "User"];
}

