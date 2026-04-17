using Microsoft.AspNetCore.Identity;
using Moq;
using task_flow.Models;
using task_flow.Areas.Admin.Services;
using task_flow.Tests.Helpers;

namespace task_flow.Tests.Services;

public class AdminServiceTests
{
  // ── GetAllUsers ───────────────────────────────────────────────────────────

  [Fact]
  public void GetAllUsers_ReturnsAllUsers()
  {
    var users = new List<ApplicationUser>
    {
      new() { Id = "u1", UserName = "alice", FirstName = "Alice", LastName = "A" },
      new() { Id = "u2", UserName = "bob", FirstName = "Bob", LastName = "B" }
    };

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.Users).Returns(users.AsQueryable());

    var service = new AdminService(mockUserMgr.Object);
    var result = service.GetAllUsers();

    Assert.Equal(2, result.Count);
    Assert.Contains(result, u => u.Id == "u1");
  }

  // ── GetUserForEditAsync ───────────────────────────────────────────────────

  [Fact]
  public async Task GetUserForEditAsync_WithValidId_ReturnsViewModel()
  {
    var user = new ApplicationUser
      { Id = "u1", UserName = "alice", FirstName = "Alice", LastName = "A", Email = "alice@test.com" };

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
    mockUserMgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.GetUserForEditAsync("u1");

    Assert.NotNull(result);
    Assert.Equal("u1", result.UserId);
    Assert.Equal("alice@test.com", result.Email);
    Assert.Equal("User", result.Role);
  }

  [Fact]
  public async Task GetUserForEditAsync_WithInvalidId_ReturnsNull()
  {
    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("invalid")).ReturnsAsync((ApplicationUser?)null);

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.GetUserForEditAsync("invalid");

    Assert.Null(result);
  }

  [Fact]
  public async Task GetUserForEditAsync_WithNoRoles_DefaultsToUser()
  {
    var user = new ApplicationUser { Id = "u1", UserName = "alice", FirstName = "Alice", LastName = "A" };

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
    mockUserMgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.GetUserForEditAsync("u1");

    Assert.Equal("User", result?.Role);
  }

  // ── UpdateUserRoleAsync ───────────────────────────────────────────────────

  [Fact]
  public async Task UpdateUserRoleAsync_WithValidRole_UpdatesAndReturnsTrue()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "A", LastName = "B" };

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
    mockUserMgr.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
    mockUserMgr
      .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
      .ReturnsAsync(IdentityResult.Success);
    mockUserMgr
      .Setup(x => x.AddToRoleAsync(user, "Admin"))
      .ReturnsAsync(IdentityResult.Success);

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.UpdateUserRoleAsync("u1", "Admin");

    Assert.True(result);
    mockUserMgr.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
  }

  [Fact]
  public async Task UpdateUserRoleAsync_WithInvalidRole_ReturnsFalse()
  {
    var mockUserMgr = MockHelper.MockUserManager();
    var service = new AdminService(mockUserMgr.Object);

    var result = await service.UpdateUserRoleAsync("u1", "InvalidRole");

    Assert.False(result);
    mockUserMgr.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserRoleAsync_WithNonexistentUser_ReturnsFalse()
  {
    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("invalid")).ReturnsAsync((ApplicationUser?)null);

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.UpdateUserRoleAsync("invalid", "Admin");

    Assert.False(result);
  }

  // ── DeleteUserAsync ───────────────────────────────────────────────────────

  [Fact]
  public async Task DeleteUserAsync_WithValidId_DeletesUserAndReturnsTrue()
  {
    var user = new ApplicationUser { Id = "u1", FirstName = "A", LastName = "B" };

    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
    mockUserMgr.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.DeleteUserAsync("u1", "admin-id");

    Assert.True(result);
    mockUserMgr.Verify(x => x.DeleteAsync(user), Times.Once);
  }

  [Fact]
  public async Task DeleteUserAsync_WhenTargetIsSelf_ReturnsFalse()
  {
    var mockUserMgr = MockHelper.MockUserManager();
    var service = new AdminService(mockUserMgr.Object);

    var result = await service.DeleteUserAsync("admin-id", "admin-id");

    Assert.False(result);
    mockUserMgr.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserAsync_WithNonexistentUser_ReturnsFalse()
  {
    var mockUserMgr = MockHelper.MockUserManager();
    mockUserMgr.Setup(x => x.FindByIdAsync("invalid")).ReturnsAsync((ApplicationUser?)null);

    var service = new AdminService(mockUserMgr.Object);
    var result = await service.DeleteUserAsync("invalid", "admin-id");

    Assert.False(result);
  }
}