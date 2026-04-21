using task_flow.Models.Tags;

namespace task_flow.Repositories.TagRepository;

public interface ITagRepository
{
  Task<List<Tag>> GetByTaskIdAsync(int taskId);
  Task<Tag?> GetByNameAsync(string name);
  Task AddTagAsync(Tag tag);
  Task<bool> TaskHasTagAsync(int taskId, int tagId);
  Task AddTaskTagAsync(TaskTag taskTag);
  Task SaveChangesAsync();
}


