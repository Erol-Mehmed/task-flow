using task_flow.Models.Activity;

namespace task_flow.Repositories.ActivityRepository;

public interface IActivityRepository
{
  Task AddAsync(ActivityLog activityLog);
  Task SaveChangesAsync();
  Task<(List<ActivityLog> Logs, int TotalPages)> GetPagedAsync(int? taskId, int page, int pageSize);
}

