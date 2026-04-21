using System.ComponentModel.DataAnnotations;

namespace task_flow.Models.Workspace;

public class WorkspaceEditViewModel
{
  public int Id { get; set; }

  [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
}