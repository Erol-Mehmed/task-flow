using task_flow.Data;
using task_flow.Models;

namespace task_flow.Repositories.Task;

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
}