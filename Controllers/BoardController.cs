using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using task_flow.Models;
using task_flow.Services.TaskService;
using task_flow.Services.WorkspaceService;

namespace task_flow.Controllers;

[Authorize]
public class BoardController : Controller
{
  private readonly ITaskService _taskService;
  private readonly IWorkspaceService _workspaceService;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<BoardController> _logger;

  public BoardController(
    ITaskService taskService,
    IWorkspaceService workspaceService,
    UserManager<ApplicationUser> userManager,
    ILogger<BoardController> logger)
  {
    _taskService = taskService;
    _workspaceService = workspaceService;
    _userManager = userManager;
    _logger = logger;
  }

  public IActionResult Error()
  {
    var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    return View(new ErrorViewModel { RequestId = requestId });
  }

  [Route("Board/NotFound")]
  public IActionResult NotFoundPage()
  {
    Response.StatusCode = 404;
    return View("NotFound");
  }

  public async Task<IActionResult> Index(int? workspaceId, string? search, string? status, int page = 1)
  {
    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    if (!workspaceId.HasValue)
    {
      ViewBag.SelectedWorkspaceId = null;
      ViewBag.SelectedWorkspaceName = null;
      ViewBag.CurrentPage = 1;
      ViewBag.TotalPages = 1;

      return View(new List<TaskItem>());
    }

    var workspace = await _workspaceService.GetWorkspaceByIdAsync(workspaceId.Value);

    if (workspace == null)
      return NotFound();

    var isAdmin = User.IsInRole("Admin");

    if (!_workspaceService.CanUserAccessWorkspace(workspace, user.Id, isAdmin))
      return Unauthorized();

    ViewBag.SelectedWorkspaceId = workspace.Id;
    ViewBag.SelectedWorkspaceName = workspace.Name;

    var result = await _taskService.GetTasks(user.Id, search, status, page, workspaceId);

    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = result.TotalPages;

    return View(result.Tasks);
  }
}