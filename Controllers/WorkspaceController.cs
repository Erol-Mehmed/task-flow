using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

  private static bool IsDuplicateWorkspaceName(DbUpdateException ex)
  {
    if (ex.InnerException is not PostgresException pgEx)
      return false;

    return pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
           pgEx.ConstraintName == "IX_Workspace_Name";
  }

  public async Task<IActionResult> Index(int page = 1)
  {
    var (user, _) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    const int pageSize = 6;
    if (page < 1)
      page = 1;

    var result = await _workspaceService.GetIndexWorkspacesAsync(page, pageSize);

    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = result.TotalPages;

    return View(result.Workspaces);
  }

  public IActionResult Create()
  {
    return View(new WorkspaceCreateViewModel());
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

    var workspace = new Workspace
    {
      Name = model.Name
    };

    try
    {
      await _workspaceService.CreateWorkspaceAsync(workspace, user.Id);
    }
    catch (DbUpdateException ex) when (IsDuplicateWorkspaceName(ex))
    {
      ModelState.AddModelError(nameof(model.Name), "A workspace with this name already exists.");
      return View(model);
    }

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

    if (!_workspaceService.CanUserManageWorkspace(workspace, user.Id, isAdmin))
      return Unauthorized();

    var model = new WorkspaceEditViewModel
    {
      Id = workspace.Id,
      Name = workspace.Name
    };

    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, WorkspaceEditViewModel model)
  {
    if (id != model.Id)
      return NotFound();

    if (!ModelState.IsValid)
      return View(model);

    var existingWorkspace = await _workspaceService.GetWorkspaceByIdAsync(id);

    if (existingWorkspace == null)
      return NotFound();

    var (user, isAdmin) = await GetUserContextAsync();

    if (user == null)
      return Unauthorized();

    if (!_workspaceService.CanUserManageWorkspace(existingWorkspace, user.Id, isAdmin))
      return Unauthorized();

    existingWorkspace.Name = model.Name;

    try
    {
      await _workspaceService.UpdateWorkspaceAsync(existingWorkspace);
    }
    catch (DbUpdateException ex) when (IsDuplicateWorkspaceName(ex))
    {
      ModelState.AddModelError(nameof(model.Name), "A workspace with this name already exists.");
      return View(model);
    }

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