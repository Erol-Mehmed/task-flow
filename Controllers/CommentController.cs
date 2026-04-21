using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;
using task_flow.Models.Comments;
using task_flow.Services.CommentService;
using task_flow.Services.TaskService;

namespace task_flow.Controllers;

[Authorize]
public class CommentController : Controller
{
  private readonly ICommentService _commentService;
  private readonly ITaskService _taskService;
  private readonly UserManager<ApplicationUser> _userManager;

  public CommentController(
    ICommentService commentService,
    ITaskService taskService,
    UserManager<ApplicationUser> userManager)
  {
    _commentService = commentService;
    _taskService = taskService;
    _userManager = userManager;
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(CommentCreateViewModel model, string? returnUrl)
  {
    if (!ModelState.IsValid)
      return RedirectToAction("Details", "Task", new { id = model.TaskId });

    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    var task = await _taskService.GetTaskByIdAsync(model.TaskId);

    if (task == null)
      return NotFound();

    var isAdmin = User.IsInRole("Admin");

    if (!_taskService.CanUserAccessTask(task, user.Id, isAdmin))
      return Unauthorized();

    await _commentService.AddCommentAsync(model.TaskId, user.Id, model.Content);

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return LocalRedirect(returnUrl);

    return RedirectToAction("Details", "Task", new { id = model.TaskId });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id, int taskId, string? returnUrl)
  {
    var user = await _userManager.GetUserAsync(User);

    if (user == null)
      return Unauthorized();

    var task = await _taskService.GetTaskByIdAsync(taskId);

    if (task == null)
      return NotFound();

    var isAdmin = User.IsInRole("Admin");

    if (!_taskService.CanUserAccessTask(task, user.Id, isAdmin))
      return Unauthorized();

    try
    {
      await _commentService.DeleteCommentAsync(id, user.Id);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return LocalRedirect(returnUrl);

    return RedirectToAction("Details", "Task", new { id = taskId });
  }
}