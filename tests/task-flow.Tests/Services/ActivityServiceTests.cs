using Moq;
using task_flow.Models.Activity;
using task_flow.Repositories.ActivityRepository;
using task_flow.Services.ActivityService;

namespace task_flow.Tests.Services;

public class ActivityServiceTests
{
  [Fact]
  public async Task LogAsync_PersistsActivityLogWithProvidedData()
  {
    var repositoryMock = new Mock<IActivityRepository>();
    ActivityLog? capturedLog = null;

    repositoryMock
      .Setup(x => x.AddAsync(It.IsAny<ActivityLog>()))
      .Callback<ActivityLog>(log => capturedLog = log)
      .Returns(Task.CompletedTask);

    repositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

    var service = new ActivityService(repositoryMock.Object);
    var before = DateTime.UtcNow;

    await service.LogAsync(5, "u1", "TaskUpdated", "Updated title");

    var after = DateTime.UtcNow;
    Assert.NotNull(capturedLog);
    Assert.Equal(5, capturedLog!.TaskItemId);
    Assert.Equal("u1", capturedLog.UserId);
    Assert.Equal("TaskUpdated", capturedLog.Action);
    Assert.Equal("Updated title", capturedLog.Details);
    Assert.InRange(capturedLog.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
    repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
  }

  [Fact]
  public async Task GetLogsAsync_ReturnsRepositoryPagedResult()
  {
    var repositoryMock = new Mock<IActivityRepository>();
    var expectedLogs = new List<ActivityLog> { new() { Id = 1, Action = "TaskCreated" } };

    repositoryMock
      .Setup(x => x.GetPagedAsync(10, 2, 20))
      .ReturnsAsync((expectedLogs, 3));

    var service = new ActivityService(repositoryMock.Object);

    var (logs, totalPages) = await service.GetLogsAsync(10, 2, 20);

    Assert.Single(logs);
    Assert.Equal(3, totalPages);
    Assert.Equal("TaskCreated", logs[0].Action);
  }
}
