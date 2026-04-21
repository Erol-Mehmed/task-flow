using task_flow.Models.Workspace;

namespace task_flow.Repositories.WorkspaceRepository;

public interface IWorkspaceRepository
{
  Task<(List<Workspace> Workspaces, int TotalPages)> GetPagedForIndexAsync(int page, int pageSize);
  Task<Workspace?> GetByIdAsync(int id);
  Task AddAsync(Workspace workspace);
  Task SaveChangesAsync();
  void Remove(Workspace workspace);
}