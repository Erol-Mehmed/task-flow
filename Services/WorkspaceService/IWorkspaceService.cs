using task_flow.Models.Workspace;

namespace task_flow.Services.WorkspaceService;

public interface IWorkspaceService
{
  Task<List<Workspace>> GetIndexWorkspacesAsync(string userId, bool isAdmin);
  Task<Workspace?> GetWorkspaceByIdAsync(int id);
  bool CanUserAccessWorkspace(Workspace workspace, string userId, bool isAdmin);
  Task CreateWorkspaceAsync(Workspace workspace, string userId);
  Task UpdateWorkspaceAsync(Workspace workspace);
  Task DeleteWorkspaceAsync(int id, string userId, bool isAdmin);
}