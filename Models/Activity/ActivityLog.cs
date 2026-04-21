using System.ComponentModel.DataAnnotations;

namespace task_flow.Models.Activity;

public class ActivityLog
{
  public int Id { get; set; }

  public int? TaskItemId { get; set; }
  public TaskItem? TaskItem { get; set; }

  public string? UserId { get; set; }
  public ApplicationUser? User { get; set; }

  [Required]
  [MaxLength(100)]
  public string Action { get; set; } = string.Empty;

  [MaxLength(500)]
  public string? Details { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

