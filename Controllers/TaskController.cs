using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;
using task_flow.Models.Comments;
using task_flow.Models.Tags;
using task_flow.Services.CommentService;
using task_flow.Services.TagService;
using task_flow.Services.TaskService;
using task_flow.Services.WorkspaceService;

namespace task_flow.Controllers;

[Authorize]
public class TaskController : Controller
{
  private readonly ITaskService _taskService;
  private readonly ICommentService _commentService;
  private readonly ITagService _tagService;
  private readonly IWorkspaceService _workspaceService;
  private readonly UserManager<ApplicationUser> _userManager;

  public TaskController(
    ITaskService taskService,
    ICommentService commentService,
    ITagService tagService,
    IWorkspaceService workspaceService,
    UserManager<ApplicationUser> userManager)
  {
    _taskService = taskService;
    _commentService = commentService;
    _tagService = tagService;
    _workspaceService = workspaceService;
    _userManager = userManager;
  }

  private async Task<(ApplicationUser? User, bool IsAdmin)> GetUserContextAsync()
  {
    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return (null, false);

    return (user, User.IsInRole("Admin"));
  }

  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Index()
  {
    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    var tasks = await _taskService.GetIndexTasksAsync(user.Id, isAdmin);
    return View(tasks);
  }

  public async Task<IActionResult> Create(int? workspaceId, string? returnUrl)
  {
    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (workspaceId.HasValue)
    {
      var workspace = await _workspaceService.GetWorkspaceByIdAsync(workspaceId.Value);

      if (workspace == null)
        return NotFound();

      if (!_workspaceService.CanUserAccessWorkspace(workspace, user.Id, isAdmin))
        return Unauthorized();

      ViewBag.SelectedWorkspaceId = workspace.Id;
      ViewBag.SelectedWorkspaceName = workspace.Name;
    }

    ViewBag.ReturnUrl = returnUrl;

    var model = new TaskItem
    {
      Title = string.Empty,
      WorkspaceId = workspaceId
    };

    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(TaskItem task, string? returnUrl)
  {
    if (!ModelState.IsValid)
    {
      ViewBag.ReturnUrl = returnUrl;

      if (task.WorkspaceId.HasValue)
      {
        var workspaceForView = await _workspaceService.GetWorkspaceByIdAsync(task.WorkspaceId.Value);
        if (workspaceForView != null)
        {
          ViewBag.SelectedWorkspaceId = workspaceForView.Id;
          ViewBag.SelectedWorkspaceName = workspaceForView.Name;
        }
      }

      return View(task);
    }

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (task.WorkspaceId.HasValue)
    {
      var workspace = await _workspaceService.GetWorkspaceByIdAsync(task.WorkspaceId.Value);

      if (workspace == null)
        return NotFound();

      if (!_workspaceService.CanUserAccessWorkspace(workspace, user.Id, isAdmin))
        return Unauthorized();
    }

    await _taskService.CreateTaskAsync(task, user.Id);

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return LocalRedirect(returnUrl);

    if (task.WorkspaceId.HasValue)
      return RedirectToAction("Index", "Board", new { workspaceId = task.WorkspaceId });

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Edit(int id, string? returnUrl)
  {
    var task = await _taskService.GetTaskByIdAsync(id);

    if (task == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_taskService.CanUserAccessTask(task, user.Id, isAdmin))
      return Unauthorized();

    ViewBag.ReturnUrl = returnUrl;

    if (task.WorkspaceId.HasValue)
    {
      var workspace = await _workspaceService.GetWorkspaceByIdAsync(task.WorkspaceId.Value);
      if (workspace != null)
      {
        ViewBag.SelectedWorkspaceId = workspace.Id;
        ViewBag.SelectedWorkspaceName = workspace.Name;
      }
    }

    return View(task);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(TaskItem task, string? returnUrl)
  {
    if (!ModelState.IsValid)
    {
      ViewBag.ReturnUrl = returnUrl;

      if (task.WorkspaceId.HasValue)
      {
        var workspaceForView = await _workspaceService.GetWorkspaceByIdAsync(task.WorkspaceId.Value);
        if (workspaceForView != null)
        {
          ViewBag.SelectedWorkspaceId = workspaceForView.Id;
          ViewBag.SelectedWorkspaceName = workspaceForView.Name;
        }
      }

      return View(task);
    }

    var existingTask = await _taskService.GetTaskByIdAsync(task.Id);

    if (existingTask == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_taskService.CanUserAccessTask(existingTask, user.Id, isAdmin))
      return Unauthorized();

    if (task.WorkspaceId.HasValue)
    {
      var workspace = await _workspaceService.GetWorkspaceByIdAsync(task.WorkspaceId.Value);

      if (workspace == null)
        return NotFound();

      if (!_workspaceService.CanUserAccessWorkspace(workspace, user.Id, isAdmin))
        return Unauthorized();
    }

    existingTask.Title = task.Title;
    existingTask.Description = task.Description;
    existingTask.Status = task.Status;
    existingTask.WorkspaceId = task.WorkspaceId;

    await _taskService.UpdateTaskAsync(existingTask, user.Id, isAdmin);

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return LocalRedirect(returnUrl);

    if (existingTask.WorkspaceId.HasValue)
      return RedirectToAction("Index", "Board", new { workspaceId = existingTask.WorkspaceId });

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

    var comments = await _commentService.GetCommentsForTaskAsync(task.Id);
    var tags = await _tagService.GetTagsForTaskAsync(task.Id);

    var model = new TaskDetailsViewModel
    {
      Task = task,
      Comments = comments,
      NewComment = new CommentCreateViewModel { TaskId = task.Id },
      Tags = tags,
      NewTag = new TagCreateViewModel { TaskId = task.Id }
    };

    return View(model);
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