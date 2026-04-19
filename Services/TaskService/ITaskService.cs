using task_flow.Models;

namespace task_flow.Services.TaskService;

public interface ITaskService
{
  Task<(List<TaskItem> Tasks, int TotalPages)> GetTasks(
    string userId,
    string? search,
    string? status,
    int page,
    int? workspaceId = null);

  Task<IEnumerable<TaskItem>> GetIndexTasksAsync(string userId, bool isAdmin);
  Task<TaskItem?> GetTaskByIdAsync(int id);
  Task<TaskItem> CreateTaskAsync(TaskItem task, string userId);
  Task UpdateTaskAsync(TaskItem task, string userId, bool isAdmin);
  Task DeleteTaskAsync(int taskId, string userId, bool isAdmin);
  bool CanUserAccessTask(TaskItem task, string userId, bool isAdmin);
}