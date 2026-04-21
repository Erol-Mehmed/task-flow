using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;
using task_flow.Models.Tags;
using task_flow.Services.TagService;
using task_flow.Services.TaskService;

namespace task_flow.Controllers;

[Authorize]
public class TagController : Controller
{
  private readonly ITagService _tagService;
  private readonly ITaskService _taskService;
  private readonly UserManager<ApplicationUser> _userManager;

  public TagController(
    ITagService tagService,
    ITaskService taskService,
    UserManager<ApplicationUser> userManager)
  {
    _tagService = tagService;
    _taskService = taskService;
    _userManager = userManager;
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(TagCreateViewModel model, string? returnUrl)
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

    await _tagService.AddTagToTaskAsync(model.TaskId, model.Name, user.Id);

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
      return LocalRedirect(returnUrl);

    return RedirectToAction("Details", "Task", new { id = model.TaskId });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int taskId, int tagId, string? returnUrl)
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
      await _tagService.RemoveTagFromTaskAsync(taskId, tagId, user.Id);
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


