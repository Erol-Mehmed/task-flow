using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task_flow.Services.ActivityService;

namespace task_flow.Controllers;

[Authorize]
public class ActivityController : Controller
{
  private readonly IActivityService _activityService;

  public ActivityController(IActivityService activityService)
  {
    _activityService = activityService;
  }

  public async Task<IActionResult> Index(int? taskId, int page = 1)
  {
    const int pageSize = 10;

    if (page < 1)
      page = 1;

    var result = await _activityService.GetLogsAsync(taskId, page, pageSize);

    ViewBag.TaskId = taskId;
    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = result.TotalPages;

    return View("~/Views/Activity/Index.cshtml", result.Logs);
  }
}


