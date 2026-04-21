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

  public async Task<(List<Workspace> Workspaces, int TotalPages)> GetPagedForIndexAsync(int page, int pageSize)
  {
    IQueryable<Workspace> query = _context.Workspaces
      .Include(w => w.User);

    query = query
      .OrderBy(w => w.Name)
      .ThenBy(w => w.Id);

    var totalItems = await query.CountAsync();

    var workspaces = await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    if (totalPages == 0)
      totalPages = 1;

    return (workspaces, totalPages);
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