using task_flow.Models.Workspace;

namespace task_flow.Services.WorkspaceService;

public interface IWorkspaceService
{
  Task<(List<Workspace> Workspaces, int TotalPages)> GetIndexWorkspacesAsync(int page, int pageSize);
  Task<Workspace?> GetWorkspaceByIdAsync(int id);
  bool CanUserAccessWorkspace(Workspace workspace, string userId, bool isAdmin);
  bool CanUserManageWorkspace(Workspace workspace, string userId, bool isAdmin);
  Task CreateWorkspaceAsync(Workspace workspace, string userId);
  Task UpdateWorkspaceAsync(Workspace workspace);
  Task DeleteWorkspaceAsync(int id, string userId, bool isAdmin);
}