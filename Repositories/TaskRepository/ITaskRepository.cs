using task_flow.Models;

namespace task_flow.Repositories.TaskRepository;

public interface ITaskRepository
{
  IQueryable<TaskItem> GetUserTasks(string userId);
  Task<TaskItem?> GetByIdAsync(int id);
  Task<IEnumerable<TaskItem>> GetAllAsync(); // For Admin
  Task<TaskItem> CreateAsync(TaskItem task);
  Task<TaskItem> UpdateAsync(TaskItem task);
  Task DeleteAsync(TaskItem task);
}