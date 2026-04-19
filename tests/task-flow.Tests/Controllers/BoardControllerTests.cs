using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using task_flow.Controllers;
using task_flow.Models;
using task_flow.Services.TaskService;
using task_flow.Services.WorkspaceService;
using task_flow.Tests.Helpers;

namespace task_flow.Tests.Controllers;

public class BoardControllerTests
{
  private static (
    BoardController controller,
    Mock<ITaskService> mockTaskService,
    Mock<IWorkspaceService> mockWorkspaceService) BuildController(
      ApplicationUser? user,
      (List<TaskItem> Tasks, int TotalPages) serviceResult = default)
  {
    var mockTaskService = new Mock<ITaskService>();
    mockTaskService
      .Setup(x => x.GetTasks(
        It.IsAny<string>(),
        It.IsAny<string?>(),
        It.IsAny<string?>(),
        It.IsAny<int>(),
        It.IsAny<int?>()))
      .ReturnsAsync(serviceResult == default
        ? (new List<TaskItem>(), 0)
        : serviceResult);

    var mockWorkspaceService = new Mock<IWorkspaceService>();

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr
      .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
      .ReturnsAsync(user);

    var logger = new Mock<ILogger<BoardController>>();
    var controller = new BoardController(
      mockTaskService.Object,
      mockWorkspaceService.Object,
      mockUserMgr.Object,
      logger.Object);

    controller.ControllerContext = MockHelper.CreateControllerContext(user?.Id);

    return (controller, mockTaskService, mockWorkspaceService);
  }

  // ── Index ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Index_WhenUserNotAuthenticated_ReturnsUnauthorized()
  {
    var (controller, _, _) = BuildController(user: null);

    var result = await controller.Index(null, null, null, 1);

    Assert.IsType<UnauthorizedResult>(result);
  }

  [Fact]
  public async Task Index_WhenAuthenticated_DelegatesToService()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var tasks = new List<TaskItem> { new() { Id = 1, Title = "Mine", UserId = "u1" } };
    var (controller, mockTaskService, _) = BuildController(user, (tasks, 1));

    var result = await controller.Index(null, null, null, 1) as ViewResult;
    var model = result?.Model as List<TaskItem>;

    Assert.Single(model!);
    Assert.Equal("Mine", model![0].Title);
    mockTaskService.Verify(s => s.GetTasks("u1", null, null, 1, null), Times.Once);
  }

  [Fact]
  public async Task Index_PassesSearchAndStatusToService()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var (controller, mockTaskService, _) = BuildController(user, (new List<TaskItem>(), 1));

    await controller.Index(null, "Buy", "Done", 1);

    mockTaskService.Verify(s => s.GetTasks("u1", "Buy", "Done", 1, null), Times.Once);
  }

  [Fact]
  public async Task Index_PassesWorkspaceIdToService()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var workspace = new Workspace { Id = 10, Name = "W1", UserId = "u1" };

    var (controller, mockTaskService, mockWorkspaceService) =
      BuildController(user, (new List<TaskItem>(), 1));

    mockWorkspaceService
      .Setup(x => x.GetWorkspaceByIdAsync(10))
      .ReturnsAsync(workspace);

    mockWorkspaceService
      .Setup(x => x.CanUserAccessWorkspace(workspace, "u1", false))
      .Returns(true);

    await controller.Index(10, null, null, 1);

    mockTaskService.Verify(s => s.GetTasks("u1", null, null, 1, 10), Times.Once);
    Assert.Equal(10, (int)controller.ViewBag.SelectedWorkspaceId);
    Assert.Equal("W1", (string)controller.ViewBag.SelectedWorkspaceName);
  }

  [Fact]
  public async Task Index_WhenWorkspaceDoesNotExist_ReturnsNotFound()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var (controller, _, mockWorkspaceService) = BuildController(user, (new List<TaskItem>(), 1));

    mockWorkspaceService
      .Setup(x => x.GetWorkspaceByIdAsync(99))
      .ReturnsAsync((Workspace?)null);

    var result = await controller.Index(99, null, null, 1);

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task Index_WhenWorkspaceUnauthorized_ReturnsUnauthorized()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var workspace = new Workspace { Id = 10, Name = "W1", UserId = "u2" };

    var (controller, _, mockWorkspaceService) = BuildController(user, (new List<TaskItem>(), 1));

    mockWorkspaceService
      .Setup(x => x.GetWorkspaceByIdAsync(10))
      .ReturnsAsync(workspace);

    mockWorkspaceService
      .Setup(x => x.CanUserAccessWorkspace(workspace, "u1", false))
      .Returns(false);

    var result = await controller.Index(10, null, null, 1);

    Assert.IsType<UnauthorizedResult>(result);
  }

  [Fact]
  public async Task Index_SetsCorrectPaginationViewBag()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var (controller, _, _) = BuildController(user, (new List<TaskItem>(), 3));

    await controller.Index(null, null, null, 2);

    Assert.Equal(2, (int)controller.ViewBag.CurrentPage);
    Assert.Equal(3, (int)controller.ViewBag.TotalPages);
  }

  // ── Error / NotFound ─────────────────────────────────────────────────────

  [Fact]
  public void Error_ReturnsViewWithRequestId()
  {
    var (controller, _, _) = BuildController(user: null);

    var result = controller.Error() as ViewResult;
    var model = result?.Model as ErrorViewModel;

    Assert.NotNull(model);
  }

  [Fact]
  public void NotFoundPage_Returns404View()
  {
    var (controller, _, _) = BuildController(user: null);
    controller.ControllerContext = MockHelper.CreateControllerContext();

    var result = controller.NotFoundPage() as ViewResult;

    Assert.NotNull(result);
    Assert.Equal("NotFound", result!.ViewName);
  }
}