using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using task_flow.Areas.Admin.Controllers;
using task_flow.Areas.Admin.Models;
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
        var model = result?.Model as AdminUserEditViewModel;

        Assert.NotNull(model);
        Assert.Equal("u1", model?.UserId);
        Assert.Equal("User", model?.Role);
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

        var model = new AdminUserEditViewModel
        {
            UserId = "u1",
            Role = "Admin"
        };

        var result = await controller.Edit(model) as RedirectToActionResult;

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
        controller.ControllerContext = MockHelper.CreateControllerContext("admin-id", isAdmin: true);

        var result = await controller.Delete("u1") as RedirectToActionResult;

        Assert.Equal("Users", result?.ActionName);
        mockUserMgr.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenTargetIsCurrentUser_DoesNotDeleteAndRedirectsWithError()
    {
        var mockUserMgr = MockHelper.MockUserManager();
        var controller = new AdminController(mockUserMgr.Object);
        controller.ControllerContext = MockHelper.CreateControllerContext("admin-id", isAdmin: true);
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());

        var result = await controller.Delete("admin-id") as RedirectToActionResult;

        Assert.Equal("Users", result?.ActionName);
        Assert.Equal("You cannot delete your own account.", controller.TempData["ErrorMessage"]);
        mockUserMgr.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}

