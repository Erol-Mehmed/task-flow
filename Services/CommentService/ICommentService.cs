using task_flow.Models.Comments;

namespace task_flow.Services.CommentService;

public interface ICommentService
{
  Task<List<Comment>> GetCommentsForTaskAsync(int taskId);
  Task AddCommentAsync(int taskId, string userId, string content);
  Task DeleteCommentAsync(int commentId, string userId);
}