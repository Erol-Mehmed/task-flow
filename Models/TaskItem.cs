using System.ComponentModel.DataAnnotations;
using task_flow.Models;

public class TaskItem
{
  public int Id { get; set; }

  [Required] public required string Title { get; set; }

  public string? Description { get; set; }

  public string Status { get; set; } = "Todo";

  [Required] public required string UserId { get; set; }
  [Required] public required ApplicationUser User { get; set; }
}