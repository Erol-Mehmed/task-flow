using System.ComponentModel.DataAnnotations;

namespace task_flow.Models;

public class TaskItem
{
  public int Id { get; set; }
  public int? WorkspaceId { get; set; }

  public task_flow.Models.Workspace.Workspace? Workspace { get; set; }

  [Required] [MaxLength(50)] public required string Title { get; set; }

  [MaxLength(500)] public string? Description { get; set; }

  [MaxLength(20)] public string Status { get; set; } = "Todo";

  [MaxLength(36)] public string? UserId { get; set; }
  public ApplicationUser? User { get; set; }

  public ICollection<task_flow.Models.Comments.Comment> Comments { get; set; } = new List<task_flow.Models.Comments.Comment>();
  public ICollection<task_flow.Models.Tags.TaskTag> TaskTags { get; set; } = new List<task_flow.Models.Tags.TaskTag>();
}