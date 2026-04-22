using Moq;
using task_flow.Models.Workspace;
using task_flow.Repositories.WorkspaceRepository;
using task_flow.Services.WorkspaceService;

namespace task_flow.Tests.Services;

public class WorkspaceServiceTests
{
  [Fact]
  public async Task GetIndexWorkspacesAsync_ReturnsRepositoryPagedResult()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    var expected = new List<Workspace> { new() { Id = 1, Name = "Main", UserId = "u1" } };

    workspaceRepositoryMock
      .Setup(x => x.GetPagedForIndexAsync(1, 6))
      .ReturnsAsync((expected, 1));

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    var (workspaces, totalPages) = await service.GetIndexWorkspacesAsync(1, 6);

    Assert.Single(workspaces);
    Assert.Equal(1, totalPages);
  }

  [Fact]
  public async Task GetWorkspaceByIdAsync_ReturnsWorkspaceFromRepository()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    workspaceRepositoryMock
      .Setup(x => x.GetByIdAsync(11))
      .ReturnsAsync(new Workspace { Id = 11, Name = "Engineering", UserId = "u1" });

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    var result = await service.GetWorkspaceByIdAsync(11);

    Assert.NotNull(result);
    Assert.Equal("Engineering", result.Name);
  }

  [Fact]
  public void CanUserAccessWorkspace_AlwaysReturnsTrue()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    var service = new WorkspaceService(workspaceRepositoryMock.Object);
    var workspace = new Workspace { Id = 1, Name = "Shared", UserId = "owner" };

    Assert.True(service.CanUserAccessWorkspace(workspace, "someone", false));
    Assert.True(service.CanUserAccessWorkspace(workspace, "someone", true));
  }

  [Fact]
  public void CanUserManageWorkspace_ReturnsTrueForAdminOrOwner()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    var service = new WorkspaceService(workspaceRepositoryMock.Object);
    var workspace = new Workspace { Id = 1, Name = "Shared", UserId = "owner" };

    Assert.True(service.CanUserManageWorkspace(workspace, "owner", false));
    Assert.True(service.CanUserManageWorkspace(workspace, "other", true));
    Assert.False(service.CanUserManageWorkspace(workspace, "other", false));
  }

  [Fact]
  public async Task CreateWorkspaceAsync_AssignsUserAndSaves()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    var workspace = new Workspace { Name = "New workspace" };

    workspaceRepositoryMock.Setup(x => x.AddAsync(workspace)).Returns(Task.CompletedTask);
    workspaceRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    await service.CreateWorkspaceAsync(workspace, "creator-id");

    Assert.Equal("creator-id", workspace.UserId);
    workspaceRepositoryMock.Verify(x => x.AddAsync(workspace), Times.Once);
    workspaceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
  }

  [Fact]
  public async Task UpdateWorkspaceAsync_OnlySavesChanges()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    workspaceRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    await service.UpdateWorkspaceAsync(new Workspace { Id = 2, Name = "Updated", UserId = "u1" });

    workspaceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
  }

  [Fact]
  public async Task DeleteWorkspaceAsync_WhenWorkspaceMissing_Throws()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    workspaceRepositoryMock.Setup(x => x.GetByIdAsync(88)).ReturnsAsync((Workspace?)null);

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteWorkspaceAsync(88, "u1", false));
  }

  [Fact]
  public async Task DeleteWorkspaceAsync_WhenUnauthorized_Throws()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    var workspace = new Workspace { Id = 5, Name = "Restricted", UserId = "owner" };
    workspaceRepositoryMock.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(workspace);

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteWorkspaceAsync(5, "other", false));
    workspaceRepositoryMock.Verify(x => x.Remove(It.IsAny<Workspace>()), Times.Never);
  }

  [Fact]
  public async Task DeleteWorkspaceAsync_WhenAuthorized_RemovesAndSaves()
  {
    var workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
    var workspace = new Workspace { Id = 5, Name = "Owned", UserId = "owner" };
    workspaceRepositoryMock.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(workspace);
    workspaceRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

    var service = new WorkspaceService(workspaceRepositoryMock.Object);

    await service.DeleteWorkspaceAsync(5, "owner", false);

    workspaceRepositoryMock.Verify(x => x.Remove(workspace), Times.Once);
    workspaceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
  }
}

