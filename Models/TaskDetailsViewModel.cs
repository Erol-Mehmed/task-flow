namespace task_flow.Models;

public class TaskDetailsViewModel
{
  public required TaskItem Task { get; set; }
  public List<Comment> Comments { get; set; } = new();
  public CommentCreateViewModel NewComment { get; set; } = new();
}

