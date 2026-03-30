using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace task_flow.Models;

public class ApplicationUser : IdentityUser
{
  [MaxLength(100)]
  public required string FirstName { get; set; }
  
  [MaxLength(100)]
  public required string LastName { get; set; }
}