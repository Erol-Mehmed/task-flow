using task_flow.Models.Workspace;

namespace task_flow.Repositories.WorkspaceRepository;

public interface IWorkspaceRepository
{
  Task<List<Workspace>> GetAllForIndexAsync(string userId, bool isAdmin);
  Task<Workspace?> GetByIdAsync(int id);
  Task AddAsync(Workspace workspace);
  Task SaveChangesAsync();
  void Remove(Workspace workspace);
}