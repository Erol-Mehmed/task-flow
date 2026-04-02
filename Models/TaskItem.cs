using System.ComponentModel.DataAnnotations;

namespace task_flow.Models;

public class TaskItem
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public required string Title { get; set; }

  [MaxLength(500)]
  public string? Description { get; set; }

  [MaxLength(20)]
  public string Status { get; set; } = "Todo";

  [Required]
  [MaxLength(50)]
  public required string UserId { get; set; }
  [Required] public required ApplicationUser User { get; set; }
}