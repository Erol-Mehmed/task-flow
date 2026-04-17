using task_flow.Models;

namespace task_flow.Services.Task;

public interface ITaskService
{
  Task<(List<TaskItem> Tasks, int TotalPages)> GetTasks(
    string userId,
    string? search,
    string? status,
    int page);
}