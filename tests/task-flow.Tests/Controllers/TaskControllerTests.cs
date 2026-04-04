using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using task_flow.Controllers;
using task_flow.Data;
using task_flow.Models;
using task_flow.Tests.Helpers;

namespace task_flow.Tests.Controllers;

public class TaskControllerTests
{
    private static ApplicationDbContext CreateDbContext(string name) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    private static TaskController BuildController(
        ApplicationDbContext db, ApplicationUser? user, bool isAdmin = false)
    {
        var mockUserMgr = MockHelper.MockUserManager();
        mockUserMgr
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var controller = new TaskController(db, mockUserMgr.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext(user?.Id, isAdmin);
        return controller;
    }

    // ── Index ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_WhenUserNull_ReturnsUnauthorized()
    {
        var db = CreateDbContext(nameof(Index_WhenUserNull_ReturnsUnauthorized));
        var controller = BuildController(db, user: null);

        var result = await controller.Index();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Index_WhenAdmin_ReturnsAllTasks()
    {
        var db = CreateDbContext(nameof(Index_WhenAdmin_ReturnsAllTasks));
        db.Task.AddRange(
            new TaskItem { Id = 1, Title = "T1", UserId = "u1" },
            new TaskItem { Id = 2, Title = "T2", UserId = "u2" });
        await db.SaveChangesAsync();

        var admin = new ApplicationUser { Id = "admin", FirstName = "Admin", LastName = "User" };
        var controller = BuildController(db, admin, isAdmin: true);

        var result = await controller.Index() as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.Equal(2, model?.Count);
    }

    [Fact]
    public async Task Index_WhenRegularUser_ReturnsOnlyOwnTasks()
    {
        var db = CreateDbContext(nameof(Index_WhenRegularUser_ReturnsOnlyOwnTasks));
        db.Task.AddRange(
            new TaskItem { Id = 1, Title = "Mine", UserId = "u1" },
            new TaskItem { Id = 2, Title = "Other", UserId = "u2" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Index() as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.Single(model!);
        Assert.Equal("Mine", model![0].Title);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_GET_ReturnsView()
    {
        var db = CreateDbContext(nameof(Create_GET_ReturnsView));
        var controller = BuildController(db, user: null);

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_POST_WhenModelInvalid_ReturnsView()
    {
        var db = CreateDbContext(nameof(Create_POST_WhenModelInvalid_ReturnsView));
        var controller = BuildController(db, user: null);
        controller.ModelState.AddModelError("Title", "Required");

        var task = new TaskItem { Title = "" };
        var result = await controller.Create(task) as ViewResult;

        Assert.NotNull(result);
        Assert.Equal(task, result.Model);
    }

    [Fact]
    public async Task Create_POST_WhenUserNull_ReturnsUnauthorized()
    {
        var db = CreateDbContext(nameof(Create_POST_WhenUserNull_ReturnsUnauthorized));
        var controller = BuildController(db, user: null);

        var result = await controller.Create(new TaskItem { Title = "Test" });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_POST_WhenValid_SavesAndRedirectsToIndex()
    {
        var db = CreateDbContext(nameof(Create_POST_WhenValid_SavesAndRedirectsToIndex));
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Create(new TaskItem { Title = "New Task" }) as RedirectToActionResult;

        Assert.Equal("Index", result?.ActionName);
        Assert.Equal(1, await db.Task.CountAsync());
        Assert.Equal("u1", (await db.Task.FirstAsync()).UserId);
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_GET_WhenTaskNotFound_ReturnsNotFound()
    {
        var db = CreateDbContext(nameof(Edit_GET_WhenTaskNotFound_ReturnsNotFound));
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_GET_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
    {
        var db = CreateDbContext(nameof(Edit_GET_WhenTaskBelongsToOtherUser_ReturnsUnauthorized));
        db.Task.Add(new TaskItem { Id = 1, Title = "T", UserId = "other" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Edit(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Edit_GET_WhenValid_ReturnsViewWithTask()
    {
        var db = CreateDbContext(nameof(Edit_GET_WhenValid_ReturnsViewWithTask));
        db.Task.Add(new TaskItem { Id = 1, Title = "My Task", UserId = "u1" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Edit(1) as ViewResult;
        var model = result?.Model as TaskItem;

        Assert.Equal("My Task", model?.Title);
    }

    [Fact]
    public async Task Edit_POST_WhenModelInvalid_ReturnsView()
    {
        var db = CreateDbContext(nameof(Edit_POST_WhenModelInvalid_ReturnsView));
        var controller = BuildController(db, user: null);
        controller.ModelState.AddModelError("Title", "Required");

        var task = new TaskItem { Id = 1, Title = "" };
        var result = await controller.Edit(task) as ViewResult;

        Assert.NotNull(result);
        Assert.Equal(task, result.Model);
    }

    [Fact]
    public async Task Edit_POST_WhenTaskNotFound_ReturnsNotFound()
    {
        var db = CreateDbContext(nameof(Edit_POST_WhenTaskNotFound_ReturnsNotFound));
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Edit(new TaskItem { Id = 999, Title = "Ghost" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
    {
        var db = CreateDbContext(nameof(Edit_POST_WhenTaskBelongsToOtherUser_ReturnsUnauthorized));
        db.Task.Add(new TaskItem { Id = 1, Title = "T", UserId = "other" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Edit(new TaskItem { Id = 1, Title = "Hacked" });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Edit_POST_WhenValid_UpdatesTaskAndRedirectsToIndex()
    {
        var db = CreateDbContext(nameof(Edit_POST_WhenValid_UpdatesTaskAndRedirectsToIndex));
        db.Task.Add(new TaskItem { Id = 1, Title = "Old", Description = "D", Status = "Todo", UserId = "u1" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Edit(
            new TaskItem { Id = 1, Title = "New Title", Description = "New D", Status = "Done" })
            as RedirectToActionResult;

        Assert.Equal("Index", result?.ActionName);

        var saved = await db.Task.FindAsync(1);
        Assert.Equal("New Title", saved?.Title);
        Assert.Equal("Done", saved?.Status);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WhenTaskNotFound_ReturnsNotFound()
    {
        var db = CreateDbContext(nameof(Delete_WhenTaskNotFound_ReturnsNotFound));
        var controller = BuildController(db, user: null);

        var result = await controller.Delete(999, null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenUserNull_ReturnsUnauthorized()
    {
        var db = CreateDbContext(nameof(Delete_WhenUserNull_ReturnsUnauthorized));
        db.Task.Add(new TaskItem { Id = 1, Title = "T", UserId = "u1" });
        await db.SaveChangesAsync();

        var controller = BuildController(db, user: null);

        var result = await controller.Delete(1, null);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_WhenTaskBelongsToOtherUser_ReturnsUnauthorized()
    {
        var db = CreateDbContext(nameof(Delete_WhenTaskBelongsToOtherUser_ReturnsUnauthorized));
        db.Task.Add(new TaskItem { Id = 1, Title = "T", UserId = "other" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var result = await controller.Delete(1, null);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_WhenValid_DeletesAndRedirectsToIndex()
    {
        var db = CreateDbContext(nameof(Delete_WhenValid_DeletesAndRedirectsToIndex));
        db.Task.Add(new TaskItem { Id = 1, Title = "T", UserId = "u1" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var mockUrl = new Mock<IUrlHelper>();
        mockUrl.Setup(u => u.IsLocalUrl(It.IsAny<string?>())).Returns(false);
        controller.Url = mockUrl.Object;

        var result = await controller.Delete(1, null) as RedirectToActionResult;

        Assert.Equal("Index", result?.ActionName);
        Assert.Equal(0, await db.Task.CountAsync());
    }

    [Fact]
    public async Task Delete_WithLocalReturnUrl_RedirectsToReturnUrl()
    {
        var db = CreateDbContext(nameof(Delete_WithLocalReturnUrl_RedirectsToReturnUrl));
        db.Task.Add(new TaskItem { Id = 1, Title = "T", UserId = "u1" });
        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var controller = BuildController(db, user);

        var mockUrl = new Mock<IUrlHelper>();
        mockUrl.Setup(u => u.IsLocalUrl("/")).Returns(true);
        controller.Url = mockUrl.Object;

        var result = await controller.Delete(1, "/") as LocalRedirectResult;

        Assert.Equal("/", result?.Url);
    }
}

