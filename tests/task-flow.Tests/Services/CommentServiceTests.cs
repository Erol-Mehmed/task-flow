using Moq;
using task_flow.Models.Comments;
using task_flow.Repositories.CommentRepository;
using task_flow.Services.ActivityService;
using task_flow.Services.CommentService;

namespace task_flow.Tests.Services;

public class CommentServiceTests
{
  [Fact]
  public async Task GetCommentsForTaskAsync_ReturnsCommentsFromRepository()
  {
    var commentRepositoryMock = new Mock<ICommentRepository>();
    var activityServiceMock = new Mock<IActivityService>();

    commentRepositoryMock
      .Setup(x => x.GetByTaskIdAsync(4))
      .ReturnsAsync(new List<Comment> { new() { Id = 1, TaskItemId = 4, Content = "Hey", UserId = "u1" } });

    var service = new CommentService(commentRepositoryMock.Object, activityServiceMock.Object);

    var result = await service.GetCommentsForTaskAsync(4);

    Assert.Single(result);
    Assert.Equal(4, result[0].TaskItemId);
  }

  [Fact]
  public async Task AddCommentAsync_TrimsContentAndLogsActivity()
  {
    var commentRepositoryMock = new Mock<ICommentRepository>();
    var activityServiceMock = new Mock<IActivityService>();
    Comment? addedComment = null;

    commentRepositoryMock
      .Setup(x => x.AddAsync(It.IsAny<Comment>()))
      .Callback<Comment>(comment => addedComment = comment)
      .Returns(Task.CompletedTask);
    commentRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

    activityServiceMock
      .Setup(x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(Task.CompletedTask);

    var service = new CommentService(commentRepositoryMock.Object, activityServiceMock.Object);

    await service.AddCommentAsync(7, "u2", "   hello world   ");

    Assert.NotNull(addedComment);
    Assert.Equal(7, addedComment!.TaskItemId);
    Assert.Equal("u2", addedComment.UserId);
    Assert.Equal("hello world", addedComment.Content);
    commentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    activityServiceMock.Verify(x => x.LogAsync(7, "u2", "CommentAdded", "Comment added to task."), Times.Once);
  }

  [Fact]
  public async Task DeleteCommentAsync_WhenMissing_ThrowsKeyNotFoundException()
  {
    var commentRepositoryMock = new Mock<ICommentRepository>();
    var activityServiceMock = new Mock<IActivityService>();
    commentRepositoryMock.Setup(x => x.GetByIdAsync(9)).ReturnsAsync((Comment?)null);

    var service = new CommentService(commentRepositoryMock.Object, activityServiceMock.Object);

    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteCommentAsync(9, "u1"));
  }

  [Fact]
  public async Task DeleteCommentAsync_WhenFound_RemovesAndLogs()
  {
    var commentRepositoryMock = new Mock<ICommentRepository>();
    var activityServiceMock = new Mock<IActivityService>();
    var comment = new Comment { Id = 11, TaskItemId = 6, Content = "obsolete", UserId = "u1" };

    commentRepositoryMock.Setup(x => x.GetByIdAsync(11)).ReturnsAsync(comment);
    commentRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
    activityServiceMock
      .Setup(x => x.LogAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>()))
      .Returns(Task.CompletedTask);

    var service = new CommentService(commentRepositoryMock.Object, activityServiceMock.Object);

    await service.DeleteCommentAsync(11, "deleter");

    commentRepositoryMock.Verify(x => x.Remove(comment), Times.Once);
    commentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    activityServiceMock.Verify(
      x => x.LogAsync(6, "deleter", "CommentDeleted", "Comment deleted from task."),
      Times.Once);
  }
}
