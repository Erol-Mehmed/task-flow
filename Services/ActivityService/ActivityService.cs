using task_flow.Models.Activity;
using task_flow.Repositories.ActivityRepository;

namespace task_flow.Services.ActivityService;

public class ActivityService : IActivityService
{
  private readonly IActivityRepository _activityRepository;

  public ActivityService(IActivityRepository activityRepository)
  {
    _activityRepository = activityRepository;
  }

  public async Task LogAsync(int? taskId, string? userId, string action, string? details = null)
  {
    var log = new ActivityLog
    {
      TaskItemId = taskId,
      UserId = userId,
      Action = action,
      Details = details,
      CreatedAt = DateTime.UtcNow
    };

    await _activityRepository.AddAsync(log);
    await _activityRepository.SaveChangesAsync();
  }

  public async Task<(List<ActivityLog> Logs, int TotalPages)> GetLogsAsync(int? taskId, int page, int pageSize)
  {
    return await _activityRepository.GetPagedAsync(taskId, page, pageSize);
  }
}

