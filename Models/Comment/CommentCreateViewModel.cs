using System.ComponentModel.DataAnnotations;

namespace task_flow.Models.Comments;

public class CommentCreateViewModel
{
  [Required] public int TaskId { get; set; }

  [Required] [MaxLength(1000)] public string Content { get; set; } = string.Empty;
}

