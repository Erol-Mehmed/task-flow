using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;
using System.Diagnostics;

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

  public IActionResult Error()
  {
    var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    return View(new ErrorViewModel { RequestId = requestId });
  }

  [Route("Home/NotFound")]
  public IActionResult NotFoundPage()
  {
    Response.StatusCode = 404;
    return View("NotFound");
  }

  public async Task<IActionResult> Index(string? search, string? status, int page = 1)
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
      return View(new List<TaskItem>());

    var query = _context.Task.Where(t => t.UserId == user.Id);

    if (!string.IsNullOrWhiteSpace(search))
    {
      var normalizedSearch = search.Trim().ToLower();
      query = query.Where(t => t.Title.ToLower().Contains(normalizedSearch));
    }

    if (!string.IsNullOrWhiteSpace(status))
      query = query.Where(t => t.Status == status);

    query = query.OrderBy(t => t.Id);

    int pageSize = 6;

    var totalItems = await query.CountAsync();

    var tasks = await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    return View(tasks);
  }
}