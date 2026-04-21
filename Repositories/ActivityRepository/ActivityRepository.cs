using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models.Activity;

namespace task_flow.Repositories.ActivityRepository;

public class ActivityRepository : IActivityRepository
{
  private readonly ApplicationDbContext _context;

  public ActivityRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task AddAsync(ActivityLog activityLog)
  {
    await _context.ActivityLogs.AddAsync(activityLog);
  }

  public async Task SaveChangesAsync()
  {
    await _context.SaveChangesAsync();
  }

  public async Task<(List<ActivityLog> Logs, int TotalPages)> GetPagedAsync(int? taskId, int page, int pageSize)
  {
    IQueryable<ActivityLog> query = _context.ActivityLogs
      .Include(a => a.User)
      .Include(a => a.TaskItem)
      .OrderByDescending(a => a.CreatedAt)
      .ThenByDescending(a => a.Id);

    if (taskId.HasValue)
      query = query.Where(a => a.TaskItemId == taskId.Value);

    var totalItems = await query.CountAsync();

    var logs = await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    if (totalPages == 0)
      totalPages = 1;

    return (logs, totalPages);
  }
}

