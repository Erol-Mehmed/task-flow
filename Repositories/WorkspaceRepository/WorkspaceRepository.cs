using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models.Workspace;

namespace task_flow.Repositories.WorkspaceRepository;

public class WorkspaceRepository : IWorkspaceRepository
{
  private readonly ApplicationDbContext _context;

  public WorkspaceRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<List<Workspace>> GetAllForIndexAsync(string userId, bool isAdmin)
  {
    IQueryable<Workspace> query = _context.Workspaces
      .Include(w => w.User);

    if (!isAdmin)
      query = query.Where(w => w.UserId == userId);

    return await query
      .OrderBy(w => w.Name)
      .ToListAsync();
  }

  public async Task<Workspace?> GetByIdAsync(int id)
  {
    return await _context.Workspaces.FindAsync(id);
  }

  public async Task AddAsync(Workspace workspace)
  {
    await _context.Workspaces.AddAsync(workspace);
  }

  public async Task SaveChangesAsync()
  {
    await _context.SaveChangesAsync();
  }

  public void Remove(Workspace workspace)
  {
    _context.Workspaces.Remove(workspace);
  }
}