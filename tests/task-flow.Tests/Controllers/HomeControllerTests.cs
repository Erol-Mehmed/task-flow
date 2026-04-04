using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using task_flow.Controllers;
using task_flow.Data;
using task_flow.Models;
using task_flow.Tests.Helpers;

namespace task_flow.Tests.Controllers;

public class HomeControllerTests
{
    private static ApplicationDbContext CreateDbContext(string name) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options);

    private static (HomeController controller, ApplicationDbContext db) BuildController(
        string dbName, ApplicationUser? user)
    {
        var db = CreateDbContext(dbName);
        var mockUserMgr = MockHelper.MockUserManager();
        mockUserMgr
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        var logger = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(db, mockUserMgr.Object, logger.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext(user?.Id);
        return (controller, db);
    }

    // ── Index ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_WhenUserNotAuthenticated_ReturnsEmptyList()
    {
        var (controller, _) = BuildController(
            nameof(Index_WhenUserNotAuthenticated_ReturnsEmptyList), user: null);

        var result = await controller.Index(null, null) as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.NotNull(model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Index_WhenAuthenticated_ReturnsOnlyCurrentUserTasks()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var (controller, db) = BuildController(
            nameof(Index_WhenAuthenticated_ReturnsOnlyCurrentUserTasks), user);

        db.Task.AddRange(
            new TaskItem { Id = 1, Title = "Mine", UserId = "u1" },
            new TaskItem { Id = 2, Title = "Other", UserId = "u2" });
        await db.SaveChangesAsync();

        var result = await controller.Index(null, null) as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.Single(model!);
        Assert.Equal("Mine", model![0].Title);
    }

    [Fact]
    public async Task Index_WithSearchFilter_ReturnsMatchingTasks()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var (controller, db) = BuildController(
            nameof(Index_WithSearchFilter_ReturnsMatchingTasks), user);

        db.Task.AddRange(
            new TaskItem { Id = 1, Title = "Buy Milk", UserId = "u1" },
            new TaskItem { Id = 2, Title = "Read Book", UserId = "u1" });
        await db.SaveChangesAsync();

        var result = await controller.Index("Buy", null) as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.Single(model!);
        Assert.Equal("Buy Milk", model![0].Title);
    }

    [Fact]
    public async Task Index_WithStatusFilter_ReturnsMatchingTasks()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var (controller, db) = BuildController(
            nameof(Index_WithStatusFilter_ReturnsMatchingTasks), user);

        db.Task.AddRange(
            new TaskItem { Id = 1, Title = "T1", Status = "Todo", UserId = "u1" },
            new TaskItem { Id = 2, Title = "T2", Status = "Done", UserId = "u1" });
        await db.SaveChangesAsync();

        var result = await controller.Index(null, "Done") as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.Single(model!);
        Assert.Equal("Done", model![0].Status);
    }

    [Fact]
    public async Task Index_SetsCorrectPaginationViewBag()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var (controller, db) = BuildController(
            nameof(Index_SetsCorrectPaginationViewBag), user);

        for (int i = 1; i <= 7; i++)
            db.Task.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });
        await db.SaveChangesAsync();

        await controller.Index(null, null, 1);

        Assert.Equal(1, (int)controller.ViewBag.CurrentPage);
        Assert.Equal(2, (int)controller.ViewBag.TotalPages);  // pageSize=6, 7 tasks → 2 pages
    }

    [Fact]
    public async Task Index_SecondPage_ReturnsRemainingItems()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };
        var (controller, db) = BuildController(
            nameof(Index_SecondPage_ReturnsRemainingItems), user);

        for (int i = 1; i <= 7; i++)
            db.Task.Add(new TaskItem { Id = i, Title = $"Task {i}", UserId = "u1" });
        await db.SaveChangesAsync();

        var result = await controller.Index(null, null, 2) as ViewResult;
        var model = result?.Model as List<TaskItem>;

        Assert.Single(model!);  // 7 tasks, page 2 has only 1
    }

    // ── Error / NotFound ─────────────────────────────────────────────────────

    [Fact]
    public void Error_ReturnsViewWithRequestId()
    {
        var (controller, _) = BuildController(nameof(Error_ReturnsViewWithRequestId), null);

        var result = controller.Error() as ViewResult;
        var model = result?.Model as task_flow.Models.ErrorViewModel;

        Assert.NotNull(model);
    }
}

