namespace task_flow.Models;

public class TaskDetailsViewModel
{
  public required TaskItem Task { get; set; }
  public List<task_flow.Models.Comments.Comment> Comments { get; set; } = new();
  public task_flow.Models.Comments.CommentCreateViewModel NewComment { get; set; } = new();
  public List<task_flow.Models.Tags.Tag> Tags { get; set; } = new();
  public task_flow.Models.Tags.TagCreateViewModel NewTag { get; set; } = new();
}