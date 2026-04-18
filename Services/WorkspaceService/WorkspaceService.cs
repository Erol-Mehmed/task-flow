using task_flow.Models;
using task_flow.Repositories.WorkspaceRepository;

namespace task_flow.Services.WorkspaceService;

public class WorkspaceService : IWorkspaceService
{
  private readonly IWorkspaceRepository _workspaceRepository;

  public WorkspaceService(IWorkspaceRepository workspaceRepository)
  {
    _workspaceRepository = workspaceRepository;
  }

  public async Task<List<Workspace>> GetIndexWorkspacesAsync(string userId, bool isAdmin)
  {
    return await _workspaceRepository.GetAllForIndexAsync(userId, isAdmin);
  }

  public async Task<Workspace?> GetWorkspaceByIdAsync(int id)
  {
    return await _workspaceRepository.GetByIdAsync(id);
  }

  public bool CanUserAccessWorkspace(Workspace workspace, string userId, bool isAdmin)
  {
    return isAdmin || workspace.UserId == userId;
  }

  public async Task CreateWorkspaceAsync(Workspace workspace, string userId)
  {
    workspace.UserId = userId;
    await _workspaceRepository.AddAsync(workspace);
    await _workspaceRepository.SaveChangesAsync();
  }

  public async Task UpdateWorkspaceAsync(Workspace workspace)
  {
    // Entity is already tracked from repository lookup in controller flow.
    await _workspaceRepository.SaveChangesAsync();
  }

  public async Task DeleteWorkspaceAsync(int id, string userId, bool isAdmin)
  {
    var workspace = await _workspaceRepository.GetByIdAsync(id);

    if (workspace == null)
      throw new KeyNotFoundException("Workspace not found.");

    if (!CanUserAccessWorkspace(workspace, userId, isAdmin))
      throw new UnauthorizedAccessException("User cannot delete this workspace.");

    _workspaceRepository.Remove(workspace);
    await _workspaceRepository.SaveChangesAsync();
  }
}