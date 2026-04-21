using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models.Tags;

namespace task_flow.Repositories.TagRepository;

public class TagRepository : ITagRepository
{
  private readonly ApplicationDbContext _context;

  public TagRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<List<Tag>> GetByTaskIdAsync(int taskId)
  {
    return await _context.TaskTags
      .Where(tt => tt.TaskItemId == taskId)
      .Select(tt => tt.Tag!)
      .OrderBy(t => t.Name)
      .ToListAsync();
  }

  public async Task<Tag?> GetByNameAsync(string name)
  {
    return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
  }

  public async Task AddTagAsync(Tag tag)
  {
    await _context.Tags.AddAsync(tag);
  }

  public async Task<bool> TaskHasTagAsync(int taskId, int tagId)
  {
    return await _context.TaskTags.AnyAsync(tt => tt.TaskItemId == taskId && tt.TagId == tagId);
  }

  public async Task AddTaskTagAsync(TaskTag taskTag)
  {
    await _context.TaskTags.AddAsync(taskTag);
  }

  public async Task SaveChangesAsync()
  {
    await _context.SaveChangesAsync();
  }
}


