using Microsoft.EntityFrameworkCore;
using task_flow.Models;
using task_flow.Repositories.TaskRepository;
using TaskItem = task_flow.Models.TaskItem;

namespace task_flow.Services.TaskService;

public class TaskService : ITaskService
{
  private readonly ITaskRepository _repo;

  public TaskService(ITaskRepository repo)
  {
    _repo = repo;
  }

  public bool CanUserAccessTask(TaskItem task, string userId, bool isAdmin)
  {
    return task.UserId == userId || isAdmin;
  }

  public async Task<IEnumerable<TaskItem>> GetIndexTasksAsync(string userId, bool isAdmin)
  {
    if (isAdmin)
      return await _repo.GetAllAsync();

    return await _repo.GetUserTasks(userId).ToListAsync();
  }

  public async Task<TaskItem?> GetTaskByIdAsync(int id)
  {
    return await _repo.GetByIdAsync(id);
  }

  public async Task<TaskItem> CreateTaskAsync(TaskItem task, string userId)
  {
    task.UserId = userId;
    return await _repo.CreateAsync(task);
  }

  public async Task UpdateTaskAsync(TaskItem task, string userId, bool isAdmin)
  {
    if (!CanUserAccessTask(task, userId, isAdmin))
      throw new UnauthorizedAccessException("You don't have permission to update this task.");

    await _repo.UpdateAsync(task);
  }

  public async Task DeleteTaskAsync(int taskId, string userId, bool isAdmin)
  {
    var task = await _repo.GetByIdAsync(taskId);

    if (task == null)
      throw new KeyNotFoundException("Task not found.");

    if (!CanUserAccessTask(task, userId, isAdmin))
      throw new UnauthorizedAccessException("You don't have permission to delete this task.");

    await _repo.DeleteAsync(task);
  }

  public async Task<(List<TaskItem> Tasks, int TotalPages)> GetTasks(
    string userId,
    string? search,
    string? status,
    int page,
    int? workspaceId = null)
  {
    var query = _repo.GetUserTasks(userId);

    if (workspaceId.HasValue)
      query = query.Where(t => t.WorkspaceId == workspaceId.Value);

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