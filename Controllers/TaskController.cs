using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;
using task_flow.Services.TaskService;

namespace task_flow.Controllers;

[Authorize]
public class TaskController : Controller
{
  private readonly ITaskService _taskService;
  private readonly UserManager<ApplicationUser> _userManager;

  public TaskController(
    ITaskService taskService,
    UserManager<ApplicationUser> userManager)
  {
    _taskService = taskService;
    _userManager = userManager;
  }

  private async Task<(ApplicationUser? User, bool IsAdmin)> GetUserContextAsync()
  {
    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return (null, false);

    return (user, User.IsInRole("Admin"));
  }

  public async Task<IActionResult> Index()
  {
    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    var tasks = await _taskService.GetIndexTasksAsync(user.Id, isAdmin);

    return View(tasks);
  }

  public IActionResult Create()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(TaskItem task)
  {
    if (!ModelState.IsValid)
      return View(task);

    var (user, _) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    await _taskService.CreateTaskAsync(task, user.Id);

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Edit(int id)
  {
    var task = await _taskService.GetTaskByIdAsync(id);

    if (task == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_taskService.CanUserAccessTask(task, user.Id, isAdmin))
      return Unauthorized();

    return View(task);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(TaskItem task)
  {
    if (!ModelState.IsValid)
      return View(task);

    var existingTask = await _taskService.GetTaskByIdAsync(task.Id);

    if (existingTask == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_taskService.CanUserAccessTask(existingTask, user.Id, isAdmin))
      return Unauthorized();

    existingTask.Title = task.Title;
    existingTask.Description = task.Description;
    existingTask.Status = task.Status;

    await _taskService.UpdateTaskAsync(existingTask, user.Id, isAdmin);

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Details(int id)
  {
    var task = await _taskService.GetTaskByIdAsync(id);

    if (task == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_taskService.CanUserAccessTask(task, user.Id, isAdmin))
      return Unauthorized();

    return View(task);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id, string? returnUrl)
  {
    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    try
    {
      await _taskService.DeleteTaskAsync(id, user.Id, isAdmin);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
    catch (UnauthorizedAccessException)
    {
      return Unauthorized();
    }

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return LocalRedirect(returnUrl);

    return RedirectToAction(nameof(Index));
  }
}