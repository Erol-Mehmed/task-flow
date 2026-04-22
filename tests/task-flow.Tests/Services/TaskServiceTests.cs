using Microsoft.EntityFrameworkCore;
using Moq;
using task_flow.Data;
using task_flow.Models;
using task_flow.Repositories.TaskRepository;
using task_flow.Services.ActivityService;
using task_flow.Services.TaskService;

namespace task_flow.Tests.Services;

public class TaskServiceTests
{
  private static TaskService BuildServiceWithInMemoryDb(
    string dbName,
    out ApplicationDbContext db)
  {
    db = new ApplicationDbContext(
      new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(dbName)
        .Options);

    var activityServiceMock = new Mock<IActivityService>();
    activityServiceMock
      .Setup(x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(Task.CompletedTask);

    return new TaskService(new TaskRepository(db), activityServiceMock.Object);
  }

  private static TaskService BuildServiceWithMocks(
    out Mock<ITaskRepository> taskRepositoryMock,
    out Mock<IActivityService> activityServiceMock)
  {
    taskRepositoryMock = new Mock<ITaskRepository>();
    activityServiceMock = new Mock<IActivityService>();
    activityServiceMock
      .Setup(x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(Task.CompletedTask);

    return new TaskService(taskRepositoryMock.Object, activityServiceMock.Object);
  }

  [Fact]
  public void CanUserAccessTask_WhenTaskBelongsToWorkspace_ReturnsTrue()
  {
    var service = BuildServiceWithMocks(out _, out _);
    var task = new TaskItem { Id = 1, Title = "Task", WorkspaceId = 10, UserId = "owner" };

    var result = service.CanUserAccessTask(task, "other", false);

    Assert.True(result);
  }

  [Fact]
  public void CanUserAccessTask_WhenUserOwnsPersonalTask_ReturnsTrue()
  {
    var service = BuildServiceWithMocks(out _, out _);
    var task = new TaskItem { Id = 1, Title = "Task", WorkspaceId = null, UserId = "u1" };

    var result = service.CanUserAccessTask(task, "u1", false);

    Assert.True(result);
  }

  [Fact]
  public void CanUserAccessTask_WhenUserIsAdmin_ReturnsTrue()
  {
    var service = BuildServiceWithMocks(out _, out _);
    var task = new TaskItem { Id = 1, Title = "Task", WorkspaceId = null, UserId = "u1" };

    var result = service.CanUserAccessTask(task, "someone", true);

    Assert.True(result);
  }

  [Fact]
  public void CanUserAccessTask_WhenNotOwnerAndNotAdmin_ReturnsFalse()
  {
    var service = BuildServiceWithMocks(out _, out _);
    var task = new TaskItem { Id = 1, Title = "Task", WorkspaceId = null, UserId = "u1" };

    var result = service.CanUserAccessTask(task, "u2", false);

    Assert.False(result);
  }

  [Fact]
  public async Task GetIndexTasksAsync_ReturnsRepositoryData()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out _);
    repositoryMock
      .Setup(x => x.GetAllAsync())
      .ReturnsAsync(new List<TaskItem> { new() { Id = 1, Title = "A" }, new() { Id = 2, Title = "B" } });

    var result = await service.GetIndexTasksAsync("u1", false);

    Assert.Equal(2, result.Count());
    repositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
  }

  [Fact]
  public async Task GetTaskByIdAsync_ReturnsTaskFromRepository()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out _);
    repositoryMock.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(new TaskItem { Id = 3, Title = "Read" });

    var result = await service.GetTaskByIdAsync(3);

    Assert.NotNull(result);
    Assert.Equal(3, result.Id);
  }

  [Fact]
  public async Task CreateTaskAsync_SetsUserAndLogsActivity()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out var activityServiceMock);
    var task = new TaskItem { Title = "Create me" };

    repositoryMock
      .Setup(x => x.CreateAsync(It.IsAny<TaskItem>()))
      .ReturnsAsync((TaskItem created) =>
      {
        created.Id = 15;
        return created;
      });

    var created = await service.CreateTaskAsync(task, "u1");

    Assert.Equal("u1", created.UserId);
    Assert.Equal(15, created.Id);
    activityServiceMock.Verify(x => x.LogAsync(15, "u1", "TaskCreated", "Task 'Create me' created."), Times.Once);
  }

  [Fact]
  public async Task UpdateTaskAsync_WhenUnauthorized_ThrowsAndDoesNotUpdate()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out var activityServiceMock);
    var task = new TaskItem { Id = 8, Title = "Restricted", UserId = "owner" };

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateTaskAsync(task, "other", false));

    repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    activityServiceMock.Verify(
      x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()),
      Times.Never);
  }

  [Fact]
  public async Task UpdateTaskAsync_WhenAuthorized_UpdatesAndLogs()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out var activityServiceMock);
    var task = new TaskItem { Id = 8, Title = "Updated", UserId = "owner" };

    repositoryMock.Setup(x => x.UpdateAsync(task)).ReturnsAsync(task);

    await service.UpdateTaskAsync(task, "owner", false);

    repositoryMock.Verify(x => x.UpdateAsync(task), Times.Once);
    activityServiceMock.Verify(x => x.LogAsync(8, "owner", "TaskUpdated", "Task 'Updated' updated."), Times.Once);
  }

  [Fact]
  public async Task DeleteTaskAsync_WhenTaskNotFound_Throws()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out _);
    repositoryMock.Setup(x => x.GetByIdAsync(123)).ReturnsAsync((TaskItem?)null);

    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteTaskAsync(123, "u1", false));
  }

  [Fact]
  public async Task DeleteTaskAsync_WhenUnauthorized_ThrowsAndDoesNotDelete()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out var activityServiceMock);
    var task = new TaskItem { Id = 4, Title = "Secret", UserId = "owner" };
    repositoryMock.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(task);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteTaskAsync(4, "other", false));

    repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    activityServiceMock.Verify(
      x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()),
      Times.Never);
  }

  [Fact]
  public async Task DeleteTaskAsync_WhenAuthorized_DeletesAndLogs()
  {
    var service = BuildServiceWithMocks(out var repositoryMock, out var activityServiceMock);
    var task = new TaskItem { Id = 9, Title = "Cleanup", UserId = "owner" };
    repositoryMock.Setup(x => x.GetByIdAsync(9)).ReturnsAsync(task);
    repositoryMock.Setup(x => x.DeleteAsync(task)).Returns(Task.CompletedTask);

    await service.DeleteTaskAsync(9, "owner", false);

    repositoryMock.Verify(x => x.DeleteAsync(task), Times.Once);
    activityServiceMock.Verify(x => x.LogAsync(9, "owner", "TaskDeleted", "Task 'Cleanup' deleted."), Times.Once);
  }

  [Fact]
  public async Task GetTasks_WithWorkspaceSearchStatusAndPaging_AppliesAllRules()
  {
    var service = BuildServiceWithInMemoryDb(
      nameof(GetTasks_WithWorkspaceSearchStatusAndPaging_AppliesAllRules),
      out var db);

    db.Tasks.AddRange(
      new TaskItem { Id = 1, Title = "  feature one", Status = "Todo", WorkspaceId = 1, UserId = "u1" },
      new TaskItem { Id = 2, Title = "Feature two", Status = "Done", WorkspaceId = 1, UserId = "u1" },
      new TaskItem { Id = 3, Title = "Feature three", Status = "Done", WorkspaceId = 2, UserId = "u2" });
    await db.SaveChangesAsync();

    var (tasks, totalPages) = await service.GetTasks("ignored-user", " FEATURE ", "Done", 1, workspaceId: 1);

    Assert.Single(tasks);
    Assert.Equal(2, tasks[0].Id);
    Assert.Equal(1, totalPages);
  }

  [Fact]
  public async Task GetTasks_IgnoresUserFilter_AndReturnsAllVisibleTasks()
  {
    var service = BuildServiceWithInMemoryDb(
      nameof(GetTasks_IgnoresUserFilter_AndReturnsAllVisibleTasks),
      out var db);

    db.Tasks.AddRange(
      new TaskItem { Id = 1, Title = "Mine", UserId = "u1" },
      new TaskItem { Id = 2, Title = "Other", UserId = "u2" });
    await db.SaveChangesAsync();

    var (tasks, _) = await service.GetTasks("u1", null, null, 1);

    Assert.Equal(2, tasks.Count);
    Assert.Equal(new[] { 1, 2 }, tasks.Select(t => t.Id));
  }

  [Fact]
  public async Task GetTasks_WithSevenRecords_ReturnsSecondPageAndTotalPagesTwo()
  {
    var service = BuildServiceWithInMemoryDb(
      nameof(GetTasks_WithSevenRecords_ReturnsSecondPageAndTotalPagesTwo),
      out var db);

    for (int i = 1; i <= 7; i++)
      db.Tasks.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });

    await db.SaveChangesAsync();

    var (tasks, totalPages) = await service.GetTasks("u1", null, null, page: 2);

    Assert.Single(tasks);
    Assert.Equal(7, tasks[0].Id);
    Assert.Equal(2, totalPages);
  }
}