using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using task_flow.Areas.Admin.Controllers;
using task_flow.Models;
using task_flow.Tests.Helpers;

namespace task_flow.Tests.Controllers;

public class AdminControllerTests
{
    // ── Users ────────────────────────────────────────────────────────────────

    [Fact]
    public void Users_ReturnsViewWithAllUsers()
    {
        var users = new List<ApplicationUser>
        {
            new() { Id = "u1", FirstName = "Alice", LastName = "A", UserName = "alice" },
            new() { Id = "u2", FirstName = "Bob", LastName = "B", UserName = "bob" }
        };

        var mockUserMgr = MockHelper.MockUserManager();
        mockUserMgr.Setup(x => x.Users).Returns(users.AsQueryable());

        var controller = new AdminController(mockUserMgr.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

        var result = controller.Users() as ViewResult;
        var model = result?.Model as List<ApplicationUser>;

        Assert.Equal(2, model?.Count);
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_GET_ReturnsViewWithUserAndCurrentRole()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };

        var mockUserMgr = MockHelper.MockUserManager();
        mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
        mockUserMgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var controller = new AdminController(mockUserMgr.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

        var result = await controller.Edit("u1") as ViewResult;

        Assert.Equal(user, result?.Model);
        Assert.Equal("User", controller.ViewBag.UserRole);
    }

    [Fact]
    public async Task Edit_POST_UpdatesRoleAndRedirectsToUsers()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };

        var mockUserMgr = MockHelper.MockUserManager();
        mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
        mockUserMgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
        mockUserMgr
            .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        mockUserMgr
            .Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(mockUserMgr.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

        var result = await controller.Edit("u1", "Admin") as RedirectToActionResult;

        Assert.Equal("Users", result?.ActionName);
        mockUserMgr.Verify(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        mockUserMgr.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_RemovesUserAndRedirectsToUsers()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "A" };

        var mockUserMgr = MockHelper.MockUserManager();
        mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
        mockUserMgr.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(mockUserMgr.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

        var result = await controller.Delete("u1") as RedirectToActionResult;

        Assert.Equal("Users", result?.ActionName);
        mockUserMgr.Verify(x => x.DeleteAsync(user), Times.Once);
    }
}

