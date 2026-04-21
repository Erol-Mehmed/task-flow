using task_flow.Models.Comments;

namespace task_flow.Repositories.CommentRepository;

public interface ICommentRepository
{
  Task<List<Comment>> GetByTaskIdAsync(int taskId);
  Task<Comment?> GetByIdAsync(int id);
  Task AddAsync(Comment comment);
  void Remove(Comment comment);
  Task SaveChangesAsync();
}