using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using task_flow.Models;
using task_flow.Models.Workspace;
using task_flow.Services.WorkspaceService;

namespace task_flow.Controllers;

[Authorize]
public class WorkspaceController : Controller
{
  private readonly IWorkspaceService _workspaceService;
  private readonly UserManager<ApplicationUser> _userManager;

  public WorkspaceController(
    IWorkspaceService workspaceService,
    UserManager<ApplicationUser> userManager)
  {
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

  public async Task<IActionResult> Index()
  {
    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    var workspaces = await _workspaceService.GetIndexWorkspacesAsync(user.Id, isAdmin);
    return View(workspaces);
  }

  public IActionResult Create()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(WorkspaceCreateViewModel model)
  {
    if (!ModelState.IsValid)
      return View(model);

    var (user, _) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    var workspace = new task_flow.Models.Workspace.Workspace
    {
      Name = model.Name
    };

    await _workspaceService.CreateWorkspaceAsync(workspace, user.Id);

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> Edit(int id)
  {
    var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);

    if (workspace == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_workspaceService.CanUserAccessWorkspace(workspace, user.Id, isAdmin))
      return Unauthorized();

    return View(workspace);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, task_flow.Models.Workspace.Workspace workspace)
  {
    if (id != workspace.Id)
      return NotFound();

    if (!ModelState.IsValid)
      return View(workspace);

    var existingWorkspace = await _workspaceService.GetWorkspaceByIdAsync(id);

    if (existingWorkspace == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_workspaceService.CanUserAccessWorkspace(existingWorkspace, user.Id, isAdmin))
      return Unauthorized();

    existingWorkspace.Name = workspace.Name;
    await _workspaceService.UpdateWorkspaceAsync(existingWorkspace);

    return RedirectToAction(nameof(Index));
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
      await _workspaceService.DeleteWorkspaceAsync(id, user.Id, isAdmin);

      if (Request.Cookies.TryGetValue("SelectedWorkspaceId", out var selectedWorkspaceId) &&
          int.TryParse(selectedWorkspaceId, out var selectedId) &&
          selectedId == id)
      {
        Response.Cookies.Delete("SelectedWorkspaceId");
      }
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