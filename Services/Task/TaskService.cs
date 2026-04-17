using Microsoft.EntityFrameworkCore;
using task_flow.Models;
using task_flow.Repositories.Task;

namespace task_flow.Services.Task;

public class TaskService : ITaskService
{
  private readonly ITaskRepository _repo;

  public TaskService(ITaskRepository repo)
  {
    _repo = repo;
  }

  public async Task<(List<TaskItem> Tasks, int TotalPages)> GetTasks(
    string userId,
    string? search,
    string? status,
    int page)
  {
    var query = _repo.GetUserTasks(userId);

    if (!string.IsNullOrWhiteSpace(search))
    {
      var normalized = search.Trim().ToLower();
      query = query.Where(t => t.Title.ToLower().Contains(normalized));
    }

    if (!string.IsNullOrWhiteSpace(status))
      query = query.Where(t => t.Status == status);

    query = query.OrderBy(t => t.Id);

    int pageSize = 6;

    var totalItems = await query.CountAsync();

    var tasks = await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    return (tasks, totalPages);
  }
}