using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;

namespace task_flow.Controllers;

public class HomeController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<HomeController> _logger;

  public HomeController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<HomeController> logger)
  {
    _context = context;
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<IActionResult> Index(string? search, string? status)
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
      return View(new List<TaskItem>());

    var query = _context.Task.Where(t => t.UserId == user.Id);

    if (!string.IsNullOrWhiteSpace(search))
      query = query.Where(t => t.Title.Contains(search));

    if (!string.IsNullOrWhiteSpace(status))
      query = query.Where(t => t.Status == status);

    var tasks = await query.ToListAsync();

    return View(tasks);
  }
}
