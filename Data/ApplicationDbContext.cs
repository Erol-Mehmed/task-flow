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
  public DbSet<task_flow.Models.Comments.Comment> Comments { get; set; }
  public DbSet<task_flow.Models.Tags.Tag> Tags { get; set; }
  public DbSet<task_flow.Models.Tags.TaskTag> TaskTags { get; set; }
  public DbSet<task_flow.Models.Workspace.Workspace> Workspaces { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<task_flow.Models.Workspace.Workspace>()
      .HasIndex(w => w.Name)
      .IsUnique()
      .HasDatabaseName("IX_Workspace_Name");

    modelBuilder.Entity<TaskItem>()
      .HasIndex(t => new { t.WorkspaceId, t.Title })
      .IsUnique()
      .HasDatabaseName("IX_TaskItem_WorkspaceId_Title");

    modelBuilder.Entity<TaskItem>()
      .HasOne(t => t.Workspace)
      .WithMany(w => w.Tasks)
      .HasForeignKey(t => t.WorkspaceId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<task_flow.Models.Comments.Comment>()
      .HasOne(c => c.TaskItem)
      .WithMany(t => t.Comments)
      .HasForeignKey(c => c.TaskItemId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<task_flow.Models.Comments.Comment>()
      .HasIndex(c => new { c.TaskItemId, c.CreatedAt })
      .HasDatabaseName("IX_Comment_TaskItemId_CreatedAt");

    modelBuilder.Entity<task_flow.Models.Tags.Tag>()
      .HasIndex(t => t.Name)
      .IsUnique()
      .HasDatabaseName("IX_Tag_Name");

    modelBuilder.Entity<task_flow.Models.Tags.TaskTag>()
      .HasKey(tt => new { tt.TaskItemId, tt.TagId });

    modelBuilder.Entity<task_flow.Models.Tags.TaskTag>()
      .HasOne(tt => tt.TaskItem)
      .WithMany(t => t.TaskTags)
      .HasForeignKey(tt => tt.TaskItemId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<task_flow.Models.Tags.TaskTag>()
      .HasOne(tt => tt.Tag)
      .WithMany(t => t.TaskTags)
      .HasForeignKey(tt => tt.TagId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}