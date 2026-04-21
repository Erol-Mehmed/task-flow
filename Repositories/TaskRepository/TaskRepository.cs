using task_flow.Data;
using task_flow.Models;
using Microsoft.EntityFrameworkCore;

namespace task_flow.Repositories.TaskRepository;

public class TaskRepository : ITaskRepository
{
  private readonly ApplicationDbContext _context;

  public TaskRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public IQueryable<TaskItem> GetVisibleTasks()
  {
    return _context.Tasks
      .Include(t => t.Workspace)
      .Include(t => t.TaskTags)
      .ThenInclude(tt => tt.Tag);
  }

  public async Task<TaskItem?> GetByIdAsync(int id)
  {
    return await _context.Tasks.FindAsync(id);
  }

  public async Task<IEnumerable<TaskItem>> GetAllAsync()
  {
    return await _context.Tasks
      .Include(t => t.Workspace)
      .Include(t => t.TaskTags)
      .ThenInclude(tt => tt.Tag)
      .ToListAsync();
  }

  public async Task<TaskItem> CreateAsync(TaskItem task)
  {
    _context.Tasks.Add(task);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task<TaskItem> UpdateAsync(TaskItem task)
  {
    _context.Tasks.Update(task);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task DeleteAsync(TaskItem task)
  {
    _context.Tasks.Remove(task);
    await _context.SaveChangesAsync();
  }
}