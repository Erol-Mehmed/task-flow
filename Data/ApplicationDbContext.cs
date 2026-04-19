using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using task_flow.Models;

namespace task_flow.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  public DbSet<TaskItem> Tasks { get; set; }
  public DbSet<task_flow.Models.Workspace.Workspace> Workspaces { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<task_flow.Models.Workspace.Workspace>()
      .HasIndex(w => new { w.UserId, w.Name })
      .IsUnique()
      .HasDatabaseName("IX_Workspace_UserId_Name");

    modelBuilder.Entity<TaskItem>()
      .HasIndex(t => new { t.WorkspaceId, t.Title })
      .IsUnique()
      .HasDatabaseName("IX_TaskItem_WorkspaceId_Title");

    modelBuilder.Entity<TaskItem>()
      .HasOne(t => t.Workspace)
      .WithMany(w => w.Tasks)
      .HasForeignKey(t => t.WorkspaceId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}