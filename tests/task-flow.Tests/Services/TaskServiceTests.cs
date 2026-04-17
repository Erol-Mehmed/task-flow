using Microsoft.EntityFrameworkCore;
using task_flow.Data;
using task_flow.Models;
using task_flow.Repositories.Task;
using task_flow.Services.Task;

namespace task_flow.Tests.Services;

public class TaskServiceTests
{
  private static TaskService BuildService(string dbName, out ApplicationDbContext db)
  {
    db = new ApplicationDbContext(
      new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(dbName)
        .Options);
    var repo = new TaskRepository(db);
    return new TaskService(repo);
  }

  // ── Filtering ────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetTasks_ReturnsOnlyTasksForGivenUser()
  {
    var svc = BuildService(nameof(GetTasks_ReturnsOnlyTasksForGivenUser), out var db);
    db.Task.AddRange(
      new TaskItem { Id = 1, Title = "Mine", UserId = "u1" },
      new TaskItem { Id = 2, Title = "Other", UserId = "u2" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", null, null, 1);

    Assert.Single(tasks);
    Assert.Equal("Mine", tasks[0].Title);
  }

  [Fact]
  public async Task GetTasks_WithSearch_ReturnsMatchingTasks()
  {
    var svc = BuildService(nameof(GetTasks_WithSearch_ReturnsMatchingTasks), out var db);
    db.Task.AddRange(
      new TaskItem { Id = 1, Title = "Buy Milk", UserId = "u1" },
      new TaskItem { Id = 2, Title = "Read Book", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", "Buy", null, 1);

    Assert.Single(tasks);
    Assert.Equal("Buy Milk", tasks[0].Title);
  }

  [Fact]
  public async Task GetTasks_WithSearch_IsCaseInsensitive()
  {
    var svc = BuildService(nameof(GetTasks_WithSearch_IsCaseInsensitive), out var db);
    db.Task.AddRange(
      new TaskItem { Id = 1, Title = "First Task", UserId = "u1" },
      new TaskItem { Id = 2, Title = "second task", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", "FIRST", null, 1);

    Assert.Single(tasks);
    Assert.Equal("First Task", tasks[0].Title);
  }

  [Fact]
  public async Task GetTasks_WithStatus_ReturnsMatchingTasks()
  {
    var svc = BuildService(nameof(GetTasks_WithStatus_ReturnsMatchingTasks), out var db);
    db.Task.AddRange(
      new TaskItem { Id = 1, Title = "T1", Status = "Todo", UserId = "u1" },
      new TaskItem { Id = 2, Title = "T2", Status = "Done", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", null, "Done", 1);

    Assert.Single(tasks);
    Assert.Equal("Done", tasks[0].Status);
  }

  [Fact]
  public async Task GetTasks_WithSearchAndStatus_AppliesBothFilters()
  {
    var svc = BuildService(nameof(GetTasks_WithSearchAndStatus_AppliesBothFilters), out var db);
    db.Task.AddRange(
      new TaskItem { Id = 1, Title = "Buy Milk", Status = "Todo", UserId = "u1" },
      new TaskItem { Id = 2, Title = "Buy Bread", Status = "Done", UserId = "u1" },
      new TaskItem { Id = 3, Title = "Read Book", Status = "Done", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", "Buy", "Done", 1);

    Assert.Single(tasks);
    Assert.Equal("Buy Bread", tasks[0].Title);
  }

  [Fact]
  public async Task GetTasks_NoMatch_ReturnsEmptyList()
  {
    var svc = BuildService(nameof(GetTasks_NoMatch_ReturnsEmptyList), out var db);
    db.Task.Add(new TaskItem { Id = 1, Title = "Buy Milk", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", "xyz", null, 1);

    Assert.Empty(tasks);
  }

  // ── Pagination ───────────────────────────────────────────────────────────

  [Fact]
  public async Task GetTasks_CalculatesCorrectTotalPages()
  {
    var svc = BuildService(nameof(GetTasks_CalculatesCorrectTotalPages), out var db);
    for (int i = 1; i <= 7; i++)
      db.Task.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });
    await db.SaveChangesAsync();

    var (_, totalPages) = await svc.GetTasks("u1", null, null, 1);

    Assert.Equal(2, totalPages); // pageSize=6, 7 tasks → 2 pages
  }

  [Fact]
  public async Task GetTasks_FirstPage_ReturnsSixItems()
  {
    var svc = BuildService(nameof(GetTasks_FirstPage_ReturnsSixItems), out var db);
    for (int i = 1; i <= 7; i++)
      db.Task.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", null, null, 1);

    Assert.Equal(6, tasks.Count);
  }

  [Fact]
  public async Task GetTasks_SecondPage_ReturnsRemainingItems()
  {
    var svc = BuildService(nameof(GetTasks_SecondPage_ReturnsRemainingItems), out var db);
    for (int i = 1; i <= 7; i++)
      db.Task.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", null, null, 2);

    Assert.Single(tasks);
  }

  [Fact]
  public async Task GetTasks_ExactlyOnePage_TotalPagesIsOne()
  {
    var svc = BuildService(nameof(GetTasks_ExactlyOnePage_TotalPagesIsOne), out var db);
    for (int i = 1; i <= 6; i++)
      db.Task.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, totalPages) = await svc.GetTasks("u1", null, null, 1);

    Assert.Equal(6, tasks.Count);
    Assert.Equal(1, totalPages);
  }

  [Fact]
  public async Task GetTasks_EmptyDb_ReturnsZeroPages()
  {
    var svc = BuildService(nameof(GetTasks_EmptyDb_ReturnsZeroPages), out var db);

    var (tasks, totalPages) = await svc.GetTasks("u1", null, null, 1);

    Assert.Empty(tasks);
    Assert.Equal(0, totalPages);
  }

  // ── Ordering ─────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetTasks_ReturnsTasksOrderedById()
  {
    var svc = BuildService(nameof(GetTasks_ReturnsTasksOrderedById), out var db);
    db.Task.AddRange(
      new TaskItem { Id = 3, Title = "C", UserId = "u1" },
      new TaskItem { Id = 1, Title = "A", UserId = "u1" },
      new TaskItem { Id = 2, Title = "B", UserId = "u1" });
    await db.SaveChangesAsync();

    var (tasks, _) = await svc.GetTasks("u1", null, null, 1);

    Assert.Equal(new[] { 1, 2, 3 }, tasks.Select(t => t.Id));
  }
}