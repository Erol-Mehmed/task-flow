using task_flow.Models.Tags;

namespace task_flow.Services.TagService;

public interface ITagService
{
  Task<List<Tag>> GetTagsForTaskAsync(int taskId);
  Task AddTagToTaskAsync(int taskId, string tagName, string userId);
  Task RemoveTagFromTaskAsync(int taskId, int tagId, string userId);
}


