using task_flow.Models;

namespace task_flow.Services.CommentService;

public interface ICommentService
{
  Task<List<Comment>> GetCommentsForTaskAsync(int taskId);
  Task AddCommentAsync(int taskId, string userId, string content);
}

