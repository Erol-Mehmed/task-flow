using task_flow.Models.Comments;
using task_flow.Repositories.CommentRepository;
using task_flow.Services.ActivityService;

namespace task_flow.Services.CommentService;

public class CommentService : ICommentService
{
  private readonly ICommentRepository _commentRepository;
  private readonly IActivityService _activityService;

  public CommentService(ICommentRepository commentRepository, IActivityService activityService)
  {
    _commentRepository = commentRepository;
    _activityService = activityService;
  }

  public async Task<List<Comment>> GetCommentsForTaskAsync(int taskId)
  {
    return await _commentRepository.GetByTaskIdAsync(taskId);
  }

  public async Task AddCommentAsync(int taskId, string userId, string content)
  {
    var trimmed = content.Trim();

    var comment = new Comment
    {
      TaskItemId = taskId,
      UserId = userId,
      Content = trimmed,
      CreatedAt = DateTime.UtcNow
    };

    await _commentRepository.AddAsync(comment);
    await _commentRepository.SaveChangesAsync();

    await _activityService.LogAsync(taskId, userId, "CommentAdded", "Comment added to task.");
  }

  public async Task DeleteCommentAsync(int commentId, string userId)
  {
    var comment = await _commentRepository.GetByIdAsync(commentId);

    if (comment == null)
      throw new KeyNotFoundException("Comment not found.");

    var taskId = comment.TaskItemId;

    _commentRepository.Remove(comment);
    await _commentRepository.SaveChangesAsync();

    await _activityService.LogAsync(taskId, userId, "CommentDeleted", "Comment deleted from task.");
  }
}