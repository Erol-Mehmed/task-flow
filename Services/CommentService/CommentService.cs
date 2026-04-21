using task_flow.Models;
using task_flow.Repositories.CommentRepository;

namespace task_flow.Services.CommentService;

public class CommentService : ICommentService
{
  private readonly ICommentRepository _commentRepository;

  public CommentService(ICommentRepository commentRepository)
  {
    _commentRepository = commentRepository;
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
  }
}

