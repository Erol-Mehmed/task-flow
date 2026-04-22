using Moq;
using task_flow.Models.Tags;
using task_flow.Repositories.TagRepository;
using task_flow.Services.ActivityService;
using task_flow.Services.TagService;

namespace task_flow.Tests.Services;

public class TagServiceTests
{
  [Fact]
  public async Task GetTagsForTaskAsync_ReturnsTagsFromRepository()
  {
    var tagRepositoryMock = new Mock<ITagRepository>();
    var activityServiceMock = new Mock<IActivityService>();

    tagRepositoryMock
      .Setup(x => x.GetByTaskIdAsync(3))
      .ReturnsAsync(new List<Tag> { new() { Id = 1, Name = "urgent" } });

    var service = new TagService(tagRepositoryMock.Object, activityServiceMock.Object);

    var tags = await service.GetTagsForTaskAsync(3);

    Assert.Single(tags);
    Assert.Equal("urgent", tags[0].Name);
  }

  [Fact]
  public async Task AddTagToTaskAsync_WhenTagMissing_CreatesAssignsAndLogs()
  {
    var tagRepositoryMock = new Mock<ITagRepository>();
    var activityServiceMock = new Mock<IActivityService>();
    TaskTag? addedTaskTag = null;

    tagRepositoryMock.Setup(x => x.GetByNameAsync("backend")).ReturnsAsync((Tag?)null);
    tagRepositoryMock
      .Setup(x => x.AddTagAsync(It.IsAny<Tag>()))
      .Callback<Tag>(tag => tag.Id = 42)
      .Returns(Task.CompletedTask);
    tagRepositoryMock.Setup(x => x.TaskHasTagAsync(8, 42)).ReturnsAsync(false);
    tagRepositoryMock
      .Setup(x => x.AddTaskTagAsync(It.IsAny<TaskTag>()))
      .Callback<TaskTag>(taskTag => addedTaskTag = taskTag)
      .Returns(Task.CompletedTask);
    tagRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

    activityServiceMock
      .Setup(x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(Task.CompletedTask);

    var service = new TagService(tagRepositoryMock.Object, activityServiceMock.Object);

    await service.AddTagToTaskAsync(8, "  backend  ", "u1");

    Assert.NotNull(addedTaskTag);
    Assert.Equal(8, addedTaskTag!.TaskItemId);
    Assert.Equal(42, addedTaskTag.TagId);
    tagRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    activityServiceMock.Verify(x => x.LogAsync(8, "u1", "TagAdded", "Tag 'backend' added to task."), Times.Once);
  }

  [Fact]
  public async Task AddTagToTaskAsync_WhenAlreadyAssigned_DoesNotDuplicateOrLog()
  {
    var tagRepositoryMock = new Mock<ITagRepository>();
    var activityServiceMock = new Mock<IActivityService>();

    tagRepositoryMock.Setup(x => x.GetByNameAsync("frontend")).ReturnsAsync(new Tag { Id = 5, Name = "frontend" });
    tagRepositoryMock.Setup(x => x.TaskHasTagAsync(4, 5)).ReturnsAsync(true);

    var service = new TagService(tagRepositoryMock.Object, activityServiceMock.Object);

    await service.AddTagToTaskAsync(4, "frontend", "u1");

    tagRepositoryMock.Verify(x => x.AddTaskTagAsync(It.IsAny<TaskTag>()), Times.Never);
    tagRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    activityServiceMock.Verify(
      x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()),
      Times.Never);
  }

  [Fact]
  public async Task RemoveTagFromTaskAsync_WhenAssignmentMissing_Throws()
  {
    var tagRepositoryMock = new Mock<ITagRepository>();
    var activityServiceMock = new Mock<IActivityService>();
    tagRepositoryMock.Setup(x => x.GetTaskTagAsync(2, 99)).ReturnsAsync((TaskTag?)null);

    var service = new TagService(tagRepositoryMock.Object, activityServiceMock.Object);

    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RemoveTagFromTaskAsync(2, 99, "u1"));
  }

  [Fact]
  public async Task RemoveTagFromTaskAsync_WhenAssignmentExists_RemovesAndLogs()
  {
    var tagRepositoryMock = new Mock<ITagRepository>();
    var activityServiceMock = new Mock<IActivityService>();
    var taskTag = new TaskTag { TaskItemId = 2, TagId = 3 };

    tagRepositoryMock.Setup(x => x.GetTaskTagAsync(2, 3)).ReturnsAsync(taskTag);
    tagRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
    activityServiceMock
      .Setup(x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(Task.CompletedTask);

    var service = new TagService(tagRepositoryMock.Object, activityServiceMock.Object);

    await service.RemoveTagFromTaskAsync(2, 3, "u9");

    tagRepositoryMock.Verify(x => x.RemoveTaskTag(taskTag), Times.Once);
    tagRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    activityServiceMock.Verify(x => x.LogAsync(2, "u9", "TagRemoved", "Tag removed from task."), Times.Once);
  }
}
