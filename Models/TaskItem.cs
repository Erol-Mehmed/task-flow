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
  
  [MaxLength(36)]
  public string? UserId { get; set; }
  public ApplicationUser? User { get; set; }
}