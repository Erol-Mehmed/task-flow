using task_flow.Models;

namespace task_flow.Repositories.CommentRepository;

public interface ICommentRepository
{
  Task<List<Comment>> GetByTaskIdAsync(int taskId);
  Task AddAsync(Comment comment);
  Task SaveChangesAsync();
}

