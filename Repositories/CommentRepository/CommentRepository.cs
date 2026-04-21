using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;

namespace task_flow.Repositories.CommentRepository;

public class CommentRepository : ICommentRepository
{
  private readonly ApplicationDbContext _context;

  public CommentRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<List<Comment>> GetByTaskIdAsync(int taskId)
  {
    return await _context.Comments
      .Include(c => c.User)
      .Where(c => c.TaskItemId == taskId)
      .OrderByDescending(c => c.CreatedAt)
      .ToListAsync();
  }

  public async Task AddAsync(Comment comment)
  {
    await _context.Comments.AddAsync(comment);
  }

  public async Task SaveChangesAsync()
  {
    await _context.SaveChangesAsync();
  }
}

