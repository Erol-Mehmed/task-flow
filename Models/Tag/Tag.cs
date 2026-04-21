using System.ComponentModel.DataAnnotations;

namespace task_flow.Models.Tags;

public class Tag
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = string.Empty;

  public ICollection<task_flow.Models.Tags.TaskTag> TaskTags { get; set; } = new List<task_flow.Models.Tags.TaskTag>();
}



