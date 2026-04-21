using System.ComponentModel.DataAnnotations;

namespace task_flow.Models;

public class Comment
{
  public int Id { get; set; }

  public int TaskItemId { get; set; }
  public TaskItem? TaskItem { get; set; }

  [Required]
  [MaxLength(1000)]
  public string Content { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;
  public ApplicationUser? User { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

