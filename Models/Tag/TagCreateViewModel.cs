using System.ComponentModel.DataAnnotations;

namespace task_flow.Models.Tags;

public class TagCreateViewModel
{
  [Required]
  public int TaskId { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = string.Empty;
}


