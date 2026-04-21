using task_flow.Models.Tags;
using task_flow.Repositories.TagRepository;
using task_flow.Services.ActivityService;

namespace task_flow.Services.TagService;

public class TagService : ITagService
{
  private readonly ITagRepository _tagRepository;
  private readonly IActivityService _activityService;

  public TagService(ITagRepository tagRepository, IActivityService activityService)
  {
    _tagRepository = tagRepository;
    _activityService = activityService;
  }

  public async Task<List<Tag>> GetTagsForTaskAsync(int taskId)
  {
    return await _tagRepository.GetByTaskIdAsync(taskId);
  }

  public async Task AddTagToTaskAsync(int taskId, string tagName, string userId)
  {
    var normalized = tagName.Trim();

    var existingTag = await _tagRepository.GetByNameAsync(normalized);

    if (existingTag == null)
    {
      existingTag = new Tag { Name = normalized };
      await _tagRepository.AddTagAsync(existingTag);
      await _tagRepository.SaveChangesAsync();
    }

    var alreadyAssigned = await _tagRepository.TaskHasTagAsync(taskId, existingTag.Id);

    if (alreadyAssigned)
      return;

    await _tagRepository.AddTaskTagAsync(new TaskTag
    {
      TaskItemId = taskId,
      TagId = existingTag.Id
    });

    await _tagRepository.SaveChangesAsync();

    await _activityService.LogAsync(taskId, userId, "TagAdded", $"Tag '{existingTag.Name}' added to task.");
  }

  public async Task RemoveTagFromTaskAsync(int taskId, int tagId, string userId)
  {
    var taskTag = await _tagRepository.GetTaskTagAsync(taskId, tagId);

    if (taskTag == null)
      throw new KeyNotFoundException("Tag assignment not found.");

    _tagRepository.RemoveTaskTag(taskTag);
    await _tagRepository.SaveChangesAsync();

    await _activityService.LogAsync(taskId, userId, "TagRemoved", "Tag removed from task.");
  }
}


