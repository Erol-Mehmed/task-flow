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

  public IQueryable<TaskItem> GetUserTasks(string userId)
  {
    return _context.Task.Where(t => t.UserId == userId);
  }

  public async Task<TaskItem?> GetByIdAsync(int id)
  {
    return await _context.Task.FindAsync(id);
  }

  public async Task<IEnumerable<TaskItem>> GetAllAsync()
  {
    return await _context.Task.ToListAsync();
  }

  public async Task<TaskItem> CreateAsync(TaskItem task)
  {
    _context.Task.Add(task);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task<TaskItem> UpdateAsync(TaskItem task)
  {
    _context.Task.Update(task);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task DeleteAsync(TaskItem task)
  {
    _context.Task.Remove(task);
    await _context.SaveChangesAsync();
  }
}