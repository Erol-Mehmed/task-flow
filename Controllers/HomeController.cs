using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;
using task_flow.Services.Task;
using System.Diagnostics;

namespace task_flow.Controllers;

public class HomeController : Controller
{
  private readonly ITaskService _taskService;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<HomeController> _logger;

  public HomeController(
    ITaskService taskService,
    UserManager<ApplicationUser> userManager,
    ILogger<HomeController> logger)
  {
    _taskService = taskService;
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

    var result = await _taskService.GetTasks(user.Id, search, status, page);

    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = result.TotalPages;

    return View(result.Tasks);
  }
}