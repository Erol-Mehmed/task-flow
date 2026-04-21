using task_flow.Models.Tags;
using task_flow.Repositories.TagRepository;

namespace task_flow.Services.TagService;

public class TagService : ITagService
{
  private readonly ITagRepository _tagRepository;

  public TagService(ITagRepository tagRepository)
  {
    _tagRepository = tagRepository;
  }

  public async Task<List<Tag>> GetTagsForTaskAsync(int taskId)
  {
    return await _tagRepository.GetByTaskIdAsync(taskId);
  }

  public async Task AddTagToTaskAsync(int taskId, string tagName)
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
  }
}


