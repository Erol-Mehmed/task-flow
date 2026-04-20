// using System.Security.Claims;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using task_flow.Controllers;
// using task_flow.Models;
// using task_flow.Services.TaskService;
// using task_flow.Services.WorkspaceService;
// using task_flow.Tests.Helpers;
//
// namespace task_flow.Tests.Controllers;
//
// public class TaskControllerTests
// {
//   private static ApplicationUser User(string id = "u1") =>
//     new() { Id = id, FirstName = "Test", LastName = "User" };
//
//   private static Mock<ITaskService> TaskService() => new();
//
//   private static Mock<IWorkspaceService> WorkspaceService() => new();
//
//   private static TaskController BuildController(
//     Mock<ITaskService> taskServiceMock,
//     Mock<IWorkspaceService> workspaceServiceMock,
//     ApplicationUser? user = null,
//     bool isAdmin = false)
//   {
//     var userMgr = MockHelper.MockUserManager();
//     userMgr
//       .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
//       .ReturnsAsync(user);
//
//     var controller = new TaskController(
//       taskServiceMock.Object,
//       workspaceServiceMock.Object,
//       userMgr.Object)
//     {
//       ControllerContext = MockHelper.CreateControllerContext(user?.Id, isAdmin)
//     };
//
//     return controller;
//   }
//
//   // ── Index ────────────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Index_WhenUserNull_ReturnsUnauthorized()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, user: null);
//
//     var result = await sut.Index();
//
//     Assert.IsType<UnauthorizedResult>(result);
//     taskSvc.Verify(x => x.GetIndexTasksAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
//   }
//
//   [Fact]
//   public async Task Index_WhenAdmin_ReturnsAllTasks()
//   {
//     var tasks = new List<TaskItem>
//     {
//       new() { Id = 1, Title = "T1", UserId = "u1" },
//       new() { Id = 2, Title = "T2", UserId = "u2" }
//     };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetIndexTasksAsync("admin", true)).ReturnsAsync(tasks);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("admin"), isAdmin: true);
//
//     var result = await sut.Index() as ViewResult;
//     var model = Assert.IsType<List<TaskItem>>(result?.Model);
//
//     Assert.Equal(2, model.Count);
//     taskSvc.Verify(x => x.GetIndexTasksAsync("admin", true), Times.Once);
//   }
//
//   [Fact]
//   public async Task Index_WhenRegularUser_ReturnsOnlyOwnTasks()
//   {
//     var tasks = new List<TaskItem>
//     {
//       new() { Id = 1, Title = "Mine", UserId = "u1" }
//     };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetIndexTasksAsync("u1", false)).ReturnsAsync(tasks);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Index() as ViewResult;
//     var model = Assert.IsType<List<TaskItem>>(result?.Model);
//
//     Assert.Single(model);
//     Assert.Equal("Mine", model[0].Title);
//     taskSvc.Verify(x => x.GetIndexTasksAsync("u1", false), Times.Once);
//   }
//
//   // ── Create GET ───────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Create_GET_WhenUserNull_ReturnsUnauthorized()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, user: null);
//
//     var result = await sut.Create((int?)null);
//
//     Assert.IsType<UnauthorizedResult>(result);
//   }
//
//   [Fact]
//   public async Task Create_GET_WithoutWorkspace_ReturnsView()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Create((int?)null) as ViewResult;
//
//     Assert.NotNull(result);
//     Assert.Null(sut.ViewBag.SelectedWorkspaceId);
//   }
//
//   [Fact]
//   public async Task Create_GET_WithWorkspace_ReturnsViewWithWorkspaceContext()
//   {
//     var workspace = new task_flow.Models.Workspace.Workspace
//     {
//       Id = 5,
//       Name = "My Workspace",
//       UserId = "u1"
//     };
//
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     workspaceSvc.Setup(x => x.GetWorkspaceByIdAsync(5)).ReturnsAsync(workspace);
//     workspaceSvc.Setup(x => x.CanUserAccessWorkspace(workspace, "u1", false)).Returns(true);
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Create(5) as ViewResult;
//
//     Assert.NotNull(result);
//     Assert.Equal(5, (int)sut.ViewBag.SelectedWorkspaceId);
//     Assert.Equal("My Workspace", (string)sut.ViewBag.SelectedWorkspaceName);
//   }
//
//   [Fact]
//   public async Task Create_GET_WhenWorkspaceNotFound_ReturnsNotFound()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     workspaceSvc.Setup(x => x.GetWorkspaceByIdAsync(99)).ReturnsAsync((task_flow.Models.Workspace.Workspace?)null);
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Create(99);
//
//     Assert.IsType<NotFoundResult>(result);
//   }
//
//   [Fact]
//   public async Task Create_GET_WhenWorkspaceUnauthorized_ReturnsUnauthorized()
//   {
//     var workspace = new task_flow.Models.Workspace.Workspace
//     {
//       Id = 5,
//       Name = "Other's",
//       UserId = "other"
//     };
//
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     workspaceSvc.Setup(x => x.GetWorkspaceByIdAsync(5)).ReturnsAsync(workspace);
//     workspaceSvc.Setup(x => x.CanUserAccessWorkspace(workspace, "u1", false)).Returns(false);
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Create(5);
//
//     Assert.IsType<UnauthorizedResult>(result);
//   }
//
//   // ── Create POST ──────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Create_POST_WhenModelInvalid_ReturnsView()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, user: null);
//     sut.ModelState.AddModelError("Title", "Required");
//
//     var task = new TaskItem { Title = "" };
//
//     var result = await sut.Create(task) as ViewResult;
//
//     Assert.NotNull(result);
//     Assert.Same(task, result.Model);
//     taskSvc.Verify(x => x.CreateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<string>()), Times.Never);
//   }
//
//   [Fact]
//   public async Task Create_POST_WhenUserNull_ReturnsUnauthorized()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, user: null);
//
//     var result = await sut.Create(new TaskItem { Title = "Test" });
//
//     Assert.IsType<UnauthorizedResult>(result);
//     taskSvc.Verify(x => x.CreateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<string>()), Times.Never);
//   }
//
//   [Fact]
//   public async Task Create_POST_WhenValid_CallsServiceAndRedirectsToIndex()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//     var task = new TaskItem { Title = "New Task" };
//
//     var result = await sut.Create(task) as RedirectToActionResult;
//
//     Assert.Equal("Index", result?.ActionName);
//     taskSvc.Verify(x => x.CreateTaskAsync(task, "u1"), Times.Once);
//   }
//
//   [Fact]
//   public async Task Create_POST_WithWorkspace_RedirectsToBoard()
//   {
//     var workspace = new task_flow.Models.Workspace.Workspace
//     {
//       Id = 5,
//       Name = "Test",
//       UserId = "u1"
//     };
//
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     workspaceSvc.Setup(x => x.GetWorkspaceByIdAsync(5)).ReturnsAsync(workspace);
//     workspaceSvc.Setup(x => x.CanUserAccessWorkspace(workspace, "u1", false)).Returns(true);
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//     var task = new TaskItem { Title = "New", WorkspaceId = 5 };
//
//     var result = await sut.Create(task) as RedirectToActionResult;
//
//     Assert.Equal("Index", result?.ActionName);
//     Assert.Equal("Board", result?.ControllerName);
//     Assert.Equal(5, result?.RouteValues?["workspaceId"]);
//   }
//
//   // ── Edit GET ─────────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Edit_GET_WhenTaskNotFound_ReturnsNotFound()
//   {
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(999)).ReturnsAsync((TaskItem?)null);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Edit(999);
//
//     Assert.IsType<NotFoundResult>(result);
//   }
//
//   [Fact]
//   public async Task Edit_GET_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
//   {
//     var existing = new TaskItem { Id = 1, Title = "T", UserId = "other" };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(existing);
//     taskSvc.Setup(x => x.CanUserAccessTask(existing, "u1", false)).Returns(false);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Edit(1);
//
//     Assert.IsType<UnauthorizedResult>(result);
//   }
//
//   [Fact]
//   public async Task Edit_GET_WhenValid_ReturnsViewWithTask()
//   {
//     var existing = new TaskItem { Id = 1, Title = "My Task", UserId = "u1" };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(existing);
//     taskSvc.Setup(x => x.CanUserAccessTask(existing, "u1", false)).Returns(true);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Edit(1) as ViewResult;
//     var model = Assert.IsType<TaskItem>(result?.Model);
//
//     Assert.Equal("My Task", model.Title);
//   }
//
//   // ── Edit POST ────────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Edit_POST_WhenModelInvalid_ReturnsView()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, user: null);
//     sut.ModelState.AddModelError("Title", "Required");
//
//     var posted = new TaskItem { Id = 1, Title = "" };
//
//     var result = await sut.Edit(posted) as ViewResult;
//
//     Assert.NotNull(result);
//     Assert.Same(posted, result.Model);
//     taskSvc.Verify(
//       x => x.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<string>(), It.IsAny<bool>()),
//       Times.Never);
//   }
//
//   [Fact]
//   public async Task Edit_POST_WhenTaskNotFound_ReturnsNotFound()
//   {
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(999)).ReturnsAsync((TaskItem?)null);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Edit(new TaskItem { Id = 999, Title = "Ghost" });
//
//     Assert.IsType<NotFoundResult>(result);
//   }
//
//   [Fact]
//   public async Task Edit_POST_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
//   {
//     var existing = new TaskItem { Id = 1, Title = "T", UserId = "other" };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(existing);
//     taskSvc.Setup(x => x.CanUserAccessTask(existing, "u1", false)).Returns(false);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Edit(new TaskItem { Id = 1, Title = "Hacked" });
//
//     Assert.IsType<UnauthorizedResult>(result);
//     taskSvc.Verify(
//       x => x.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<string>(), It.IsAny<bool>()),
//       Times.Never);
//   }
//
//   [Fact]
//   public async Task Edit_POST_WhenValid_UpdatesTaskAndRedirectsToIndex()
//   {
//     var existing = new TaskItem
//     {
//       Id = 1,
//       Title = "Old",
//       Description = "D",
//       Status = "Todo",
//       UserId = "u1"
//     };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(existing);
//     taskSvc.Setup(x => x.CanUserAccessTask(existing, "u1", false)).Returns(true);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Edit(new TaskItem
//     {
//       Id = 1,
//       Title = "New Title",
//       Description = "New D",
//       Status = "Done"
//     }) as RedirectToActionResult;
//
//     Assert.Equal("Index", result?.ActionName);
//     Assert.Equal("New Title", existing.Title);
//     Assert.Equal("New D", existing.Description);
//     Assert.Equal("Done", existing.Status);
//     taskSvc.Verify(x => x.UpdateTaskAsync(existing, "u1", false), Times.Once);
//   }
//
//   // ── Details ──────────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Details_WhenTaskNotFound_ReturnsNotFound()
//   {
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(999)).ReturnsAsync((TaskItem?)null);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Details(999);
//
//     Assert.IsType<NotFoundResult>(result);
//   }
//
//   [Fact]
//   public async Task Details_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
//   {
//     var existing = new TaskItem { Id = 1, Title = "T", UserId = "other" };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(existing);
//     taskSvc.Setup(x => x.CanUserAccessTask(existing, "u1", false)).Returns(false);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Details(1);
//
//     Assert.IsType<UnauthorizedResult>(result);
//   }
//
//   [Fact]
//   public async Task Details_WhenValid_ReturnsViewWithTask()
//   {
//     var existing = new TaskItem { Id = 1, Title = "My Task", UserId = "u1" };
//
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(existing);
//     taskSvc.Setup(x => x.CanUserAccessTask(existing, "u1", false)).Returns(true);
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Details(1) as ViewResult;
//     var model = Assert.IsType<TaskItem>(result?.Model);
//
//     Assert.Equal("My Task", model.Title);
//   }
//
//   // ── Delete ───────────────────────────────────────────────────────────────
//
//   [Fact]
//   public async Task Delete_WhenTaskNotFound_ReturnsNotFound()
//   {
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.DeleteTaskAsync(999, "u1", false))
//       .ThrowsAsync(new KeyNotFoundException());
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Delete(999, null);
//
//     Assert.IsType<NotFoundResult>(result);
//   }
//
//   [Fact]
//   public async Task Delete_WhenUserNull_ReturnsUnauthorized()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, user: null);
//
//     var result = await sut.Delete(1, null);
//
//     Assert.IsType<UnauthorizedResult>(result);
//     taskSvc.Verify(
//       x => x.DeleteTaskAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()),
//       Times.Never);
//   }
//
//   [Fact]
//   public async Task Delete_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
//   {
//     var taskSvc = TaskService();
//     taskSvc.Setup(x => x.DeleteTaskAsync(1, "u1", false))
//       .ThrowsAsync(new UnauthorizedAccessException());
//     var workspaceSvc = WorkspaceService();
//
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var result = await sut.Delete(1, null);
//
//     Assert.IsType<UnauthorizedResult>(result);
//   }
//
//   [Fact]
//   public async Task Delete_WhenValid_DeletesAndRedirectsToIndex()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var url = new Mock<IUrlHelper>();
//     url.Setup(u => u.IsLocalUrl(It.IsAny<string?>())).Returns(false);
//     sut.Url = url.Object;
//
//     var result = await sut.Delete(1, null) as RedirectToActionResult;
//
//     Assert.Equal("Index", result?.ActionName);
//     taskSvc.Verify(x => x.DeleteTaskAsync(1, "u1", false), Times.Once);
//   }
//
//   [Fact]
//   public async Task Delete_WithLocalReturnUrl_RedirectsToReturnUrl()
//   {
//     var taskSvc = TaskService();
//     var workspaceSvc = WorkspaceService();
//     var sut = BuildController(taskSvc, workspaceSvc, User("u1"));
//
//     var url = new Mock<IUrlHelper>();
//     url.Setup(u => u.IsLocalUrl("/")).Returns(true);
//     sut.Url = url.Object;
//
//     var result = await sut.Delete(1, "/") as LocalRedirectResult;
//
//     Assert.Equal("/", result?.Url);
//     taskSvc.Verify(x => x.DeleteTaskAsync(1, "u1", false), Times.Once);
//   }
// }