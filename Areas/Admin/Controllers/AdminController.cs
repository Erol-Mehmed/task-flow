using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using task_flow.Areas.Admin.Models;
using task_flow.Areas.Admin.Services;

namespace task_flow.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
  private readonly IAdminService _adminService;
  private static readonly string[] AvailableRoles = ["Admin", "User"];

  public AdminController(IAdminService adminService)
  {
    _adminService = adminService;
  }

  public IActionResult Index() => View();

  public IActionResult Users()
  {
    var users = _adminService.GetAllUsers();
    ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return View(users);
  }

  public async Task<IActionResult> Edit(string id)
  {
    if (string.IsNullOrWhiteSpace(id)) return NotFound();

    var model = await _adminService.GetUserForEditAsync(id);
    if (model == null) return NotFound();

    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(AdminUserEditViewModel model)
  {
    if (!ModelState.IsValid)
    {
      model.AvailableRoles = AvailableRoles;
      return View(model);
    }

    var success = await _adminService.UpdateUserRoleAsync(model.UserId, model.Role);
    if (!success)
    {
      ModelState.AddModelError(nameof(model.Role), "Invalid role or user not found.");
      model.AvailableRoles = AvailableRoles;
      return View(model);
    }

    return RedirectToAction(nameof(Users));
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(string id)
  {
    if (string.IsNullOrWhiteSpace(id)) return NotFound();

    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var success = await _adminService.DeleteUserAsync(id, currentUserId!);

    if (!success)
    {
      TempData["ErrorMessage"] = "You cannot delete your own account.";
    }

    return RedirectToAction(nameof(Users));
  }
}