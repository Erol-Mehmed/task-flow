using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using task_flow.Models.Activity;
using task_flow.Models.Comments;
using task_flow.Models;
using task_flow.Models.Tags;
using task_flow.Models.Workspace;

namespace task_flow.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  public DbSet<TaskItem> Tasks { get; set; }
  public DbSet<ActivityLog> ActivityLogs { get; set; }
  public DbSet<Comment> Comments { get; set; }
  public DbSet<Tag> Tags { get; set; }
  public DbSet<TaskTag> TaskTags { get; set; }
  public DbSet<Workspace> Workspaces { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Workspace>()
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

    modelBuilder.Entity<ActivityLog>()
      .HasOne(a => a.TaskItem)
      .WithMany()
      .HasForeignKey(a => a.TaskItemId)
      .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<ActivityLog>()
      .HasOne(a => a.User)
      .WithMany()
      .HasForeignKey(a => a.UserId)
      .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<ActivityLog>()
      .HasIndex(a => a.CreatedAt)
      .HasDatabaseName("IX_ActivityLog_CreatedAt");

    modelBuilder.Entity<ActivityLog>()
      .HasIndex(a => new { a.TaskItemId, a.CreatedAt })
      .HasDatabaseName("IX_ActivityLog_TaskItemId_CreatedAt");

    modelBuilder.Entity<Comment>()
      .HasOne(c => c.TaskItem)
      .WithMany(t => t.Comments)
      .HasForeignKey(c => c.TaskItemId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Comment>()
      .HasIndex(c => new { c.TaskItemId, c.CreatedAt })
      .HasDatabaseName("IX_Comment_TaskItemId_CreatedAt");

    modelBuilder.Entity<Tag>()
      .HasIndex(t => t.Name)
      .IsUnique()
      .HasDatabaseName("IX_Tag_Name");

    modelBuilder.Entity<TaskTag>()
      .HasKey(tt => new { tt.TaskItemId, tt.TagId });

    modelBuilder.Entity<TaskTag>()
      .HasOne(tt => tt.TaskItem)
      .WithMany(t => t.TaskTags)
      .HasForeignKey(tt => tt.TaskItemId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<TaskTag>()
      .HasOne(tt => tt.Tag)
      .WithMany(t => t.TaskTags)
      .HasForeignKey(tt => tt.TagId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}