using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using task_flow.Areas.Admin.Models;
using task_flow.Models;

namespace task_flow.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
  private readonly UserManager<ApplicationUser> _userManager;
  private static readonly string[] AvailableRoles = ["Admin", "User"];

  public AdminController(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult Users()
  {
    var users = _userManager.Users.ToList();
    ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return View(users);
  }

  public async Task<IActionResult> Edit(string id)
  {
    if (string.IsNullOrWhiteSpace(id))
      return NotFound();

    var user = await _userManager.FindByIdAsync(id);
    if (user == null)
      return NotFound();

    var roles = await _userManager.GetRolesAsync(user);
    var model = new AdminUserEditViewModel
    {
      UserId = user.Id,
      Email = user.Email ?? string.Empty,
      Role = roles.FirstOrDefault() ?? "User",
      AvailableRoles = AvailableRoles
    };

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

    if (!AvailableRoles.Contains(model.Role))
    {
      ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
      model.AvailableRoles = AvailableRoles;
      return View(model);
    }

    var user = await _userManager.FindByIdAsync(model.UserId);
    if (user == null)
      return NotFound();

    var currentRoles = await _userManager.GetRolesAsync(user);

    await _userManager.RemoveFromRolesAsync(user, currentRoles);
    await _userManager.AddToRoleAsync(user, model.Role);

    return RedirectToAction(nameof(Users));
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(string id)
  {
    if (string.IsNullOrWhiteSpace(id))
      return NotFound();

    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (id == currentUserId)
    {
      TempData["ErrorMessage"] = "You cannot delete your own account.";
      return RedirectToAction(nameof(Users));
    }

    var user = await _userManager.FindByIdAsync(id);
    if (user == null)
      return NotFound();

    await _userManager.DeleteAsync(user);

    return RedirectToAction(nameof(Users));
  }
}