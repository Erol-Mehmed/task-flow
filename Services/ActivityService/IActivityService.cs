using task_flow.Models.Activity;

namespace task_flow.Services.ActivityService;

public interface IActivityService
{
  Task LogAsync(int? taskId, string? userId, string action, string? details = null);
  Task<(List<ActivityLog> Logs, int TotalPages)> GetLogsAsync(int? taskId, int page, int pageSize);
}

