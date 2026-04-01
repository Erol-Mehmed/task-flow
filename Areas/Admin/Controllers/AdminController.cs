using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;

namespace task_flow.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
  private readonly UserManager<ApplicationUser> _userManager;

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
    return View(users);
  }

  public async Task<IActionResult> Edit(string id)
  {
    var user = await _userManager.FindByIdAsync(id);
    var roles = await _userManager.GetRolesAsync(user);

    ViewBag.Roles = new[] { "Admin", "User" };
    ViewBag.UserRole = roles.FirstOrDefault();

    return View(user);
  }

  [HttpPost]
  public async Task<IActionResult> Edit(string id, string role)
  {
    var user = await _userManager.FindByIdAsync(id);
    var currentRoles = await _userManager.GetRolesAsync(user);

    await _userManager.RemoveFromRolesAsync(user, currentRoles);
    await _userManager.AddToRoleAsync(user, role);

    return RedirectToAction("Users");
  }

  public async Task<IActionResult> Delete(string id)
  {
    var user = await _userManager.FindByIdAsync(id);
    await _userManager.DeleteAsync(user);

    return RedirectToAction("Users");
  }
}