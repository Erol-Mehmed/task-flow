using System.ComponentModel.DataAnnotations;

namespace task_flow.Models;

public class Workspace
{
  public int Id { get; set; }

  [Required] [MaxLength(100)] public string Name { get; set; } = null!;

  [Required] public string UserId { get; set; } = string.Empty;

  public ApplicationUser User { get; set; } = null!;

  public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}