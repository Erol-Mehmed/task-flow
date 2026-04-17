using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using task_flow.Areas.Admin.Controllers;
using task_flow.Areas.Admin.Models;
using task_flow.Models;
using task_flow.Areas.Admin.Services;
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

    var mockService = new Mock<IAdminService>();
    mockService.Setup(x => x.GetAllUsers()).Returns(users);

    var controller = new AdminController(mockService.Object);
    controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

    var result = controller.Users() as ViewResult;
    var model = result?.Model as List<ApplicationUser>;

    Assert.Equal(2, model?.Count);
  }

  // ── Edit ─────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Edit_GET_ReturnsViewWithUserAndCurrentRole()
  {
    var editModel = new AdminUserEditViewModel
    {
      UserId = "u1",
      Email = "alice@test.com",
      Role = "User",
      AvailableRoles = ["Admin", "User"]
    };

    var mockService = new Mock<IAdminService>();
    mockService.Setup(x => x.GetUserForEditAsync("u1")).ReturnsAsync(editModel);

    var controller = new AdminController(mockService.Object);
    controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

    var result = await controller.Edit("u1") as ViewResult;
    var model = result?.Model as AdminUserEditViewModel;

    Assert.NotNull(model);
    Assert.Equal("u1", model.UserId);
    Assert.Equal("User", model.Role);
  }

  [Fact]
  public async Task Edit_POST_UpdatesRoleAndRedirectsToUsers()
  {
    var mockService = new Mock<IAdminService>();
    mockService.Setup(x => x.UpdateUserRoleAsync("u1", "Admin")).ReturnsAsync(true);

    var controller = new AdminController(mockService.Object);
    controller.ControllerContext = MockHelper.CreateControllerContext("admin", isAdmin: true);

    var model = new AdminUserEditViewModel { UserId = "u1", Role = "Admin" };

    var result = await controller.Edit(model) as RedirectToActionResult;

    Assert.Equal("Users", result?.ActionName);
    mockService.Verify(x => x.UpdateUserRoleAsync("u1", "Admin"), Times.Once);
  }

  // ── Delete ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task Delete_RemovesUserAndRedirectsToUsers()
  {
    var mockService = new Mock<IAdminService>();
    mockService.Setup(x => x.DeleteUserAsync("u1", "admin-id")).ReturnsAsync(true);

    var controller = new AdminController(mockService.Object);
    controller.ControllerContext = MockHelper.CreateControllerContext("admin-id", isAdmin: true);

    var result = await controller.Delete("u1") as RedirectToActionResult;

    Assert.Equal("Users", result?.ActionName);
    mockService.Verify(x => x.DeleteUserAsync("u1", "admin-id"), Times.Once);
  }

  [Fact]
  public async Task Delete_WhenTargetIsCurrentUser_DoesNotDeleteAndRedirectsWithError()
  {
    var mockService = new Mock<IAdminService>();
    mockService.Setup(x => x.DeleteUserAsync("admin-id", "admin-id")).ReturnsAsync(false);

    var controller = new AdminController(mockService.Object);
    controller.ControllerContext = MockHelper.CreateControllerContext("admin-id", isAdmin: true);
    controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());

    var result = await controller.Delete("admin-id") as RedirectToActionResult;

    Assert.Equal("Users", result?.ActionName);
    Assert.Equal("You cannot delete your own account.", controller.TempData["ErrorMessage"]);
    mockService.Verify(x => x.DeleteUserAsync("admin-id", "admin-id"), Times.Once);
  }
}