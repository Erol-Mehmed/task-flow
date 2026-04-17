using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using task_flow.Controllers;
using task_flow.Models;
using task_flow.Services.TaskService;
using task_flow.Tests.Helpers;

namespace task_flow.Tests.Controllers;

public class HomeControllerTests
{
  private static (HomeController controller, Mock<ITaskService> mockService) BuildController(
    ApplicationUser? user,
    (List<TaskItem> Tasks, int TotalPages) serviceResult = default)
  {
    var mockService = new Mock<ITaskService>();
    mockService
      .Setup(x => x.GetTasks(
        It.IsAny<string>(),
        It.IsAny<string?>(),
        It.IsAny<string?>(),
        It.IsAny<int>()))
      .ReturnsAsync(serviceResult == default
        ? (new List<TaskItem>(), 0)
        : serviceResult);

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr
      .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
      .ReturnsAsync(user);

    var logger = new Mock<ILogger<HomeController>>();
    var controller = new HomeController(mockService.Object, mockUserMgr.Object, logger.Object);
    controller.ControllerContext = MockHelper.CreateControllerContext(user?.Id);
    return (controller, mockService);
  }

  // ── Index ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Index_WhenUserNotAuthenticated_ReturnsEmptyList()
  {
    var (controller, _) = BuildController(user: null);

    var result = await controller.Index(null, null) as ViewResult;
    var model = result?.Model as List<TaskItem>;

    Assert.NotNull(model);
    Assert.Empty(model);
  }

  [Fact]
  public async Task Index_WhenAuthenticated_DelegatesToService()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var tasks = new List<TaskItem> { new() { Id = 1, Title = "Mine", UserId = "u1" } };
    var (controller, mockService) = BuildController(user, (tasks, 1));

    var result = await controller.Index(null, null) as ViewResult;
    var model = result?.Model as List<TaskItem>;

    Assert.Single(model!);
    Assert.Equal("Mine", model![0].Title);
    mockService.Verify(s => s.GetTasks("u1", null, null, 1), Times.Once);
  }

  [Fact]
  public async Task Index_PassesSearchAndStatusToService()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var (controller, mockService) = BuildController(user, (new List<TaskItem>(), 1));

    await controller.Index("Buy", "Done");

    mockService.Verify(s => s.GetTasks("u1", "Buy", "Done", 1), Times.Once);
  }

  [Fact]
  public async Task Index_SetsCorrectPaginationViewBag()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
    var (controller, _) = BuildController(user, (new List<TaskItem>(), 3));

    await controller.Index(null, null, 2);

    Assert.Equal(2, (int)controller.ViewBag.CurrentPage);
    Assert.Equal(3, (int)controller.ViewBag.TotalPages);
  }

  // ── Error / NotFound ─────────────────────────────────────────────────────

  [Fact]
  public void Error_ReturnsViewWithRequestId()
  {
    var (controller, _) = BuildController(user: null);

    var result = controller.Error() as ViewResult;
    var model = result?.Model as task_flow.Models.ErrorViewModel;

    Assert.NotNull(model);
  }

  [Fact]
  public void NotFoundPage_Returns404View()
  {
    var (controller, _) = BuildController(user: null);
    controller.ControllerContext = MockHelper.CreateControllerContext();

    var result = controller.NotFoundPage() as ViewResult;

    Assert.NotNull(result);
    Assert.Equal("NotFound", result!.ViewName);
  }
}