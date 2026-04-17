using task_flow.Models;

namespace task_flow.Repositories.Task;

public interface ITaskRepository
{
  IQueryable<TaskItem> GetUserTasks(string userId);
}