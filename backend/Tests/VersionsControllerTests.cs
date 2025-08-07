using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using CodeSnippetManager.Api.Controllers;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 版本控制器单元测试
/// </summary>
[TestClass]
public class VersionsControllerTests
{
    private readonly Mock<IVersionManagementService> _mockVersionService;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly VersionsController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testSnippetId = Guid.NewGuid();
    private readonly Guid _testVersionId = Guid.NewGuid();

    public VersionsControllerTests()
    {
        _mockVersionService = new Mock<IVersionManagementService>();
        _mockPermissionService = new Mock<IPermissionService>();
        _controller = new VersionsController(_mockVersionService.Object, _mockPermissionService.Object);

        // 设置用户身份
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    /// <summary>
    /// 测试获取版本历史 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetVersionHistory_WithValidPermission_ReturnsVersions()
    {
        // Arrange
        var expectedVersions = new List<SnippetVersionDto>
        {
            new() { Id = _testVersionId, SnippetId = _testSnippetId, VersionNumber = 1 }
        };

        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(true);

        _mockVersionService
            .Setup(x => x.GetVersionHistoryAsync(_testSnippetId))
            .ReturnsAsync(expectedVersions);

        // Act
        var result = await _controller.GetVersionHistory(_testSnippetId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedVersions, okResult.Value);
    }

    /// <summary>
    /// 测试获取版本历史 - 权限不足
    /// </summary>
    [TestMethod]
    public async Task GetVersionHistory_WithoutPermission_ReturnsForbid()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetVersionHistory(_testSnippetId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试获取版本详情 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetVersion_WithValidVersion_ReturnsVersion()
    {
        // Arrange
        var expectedVersion = new SnippetVersionDto 
        { 
            Id = _testVersionId, 
            SnippetId = _testSnippetId, 
            VersionNumber = 1 
        };

        _mockVersionService
            .Setup(x => x.GetVersionAsync(_testVersionId))
            .ReturnsAsync(expectedVersion);

        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetVersion(_testVersionId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedVersion, okResult.Value);
    }

    /// <summary>
    /// 测试获取版本详情 - 版本不存在
    /// </summary>
    [TestMethod]
    public async Task GetVersion_WithInvalidVersion_ReturnsNotFound()
    {
        // Arrange
        _mockVersionService
            .Setup(x => x.GetVersionAsync(_testVersionId))
            .ReturnsAsync((SnippetVersionDto?)null);

        // Act
        var result = await _controller.GetVersion(_testVersionId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// 测试版本恢复 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task RestoreVersion_WithValidPermission_ReturnsOk()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Edit))
            .ReturnsAsync(true);

        _mockVersionService
            .Setup(x => x.RestoreVersionAsync(_testSnippetId, _testVersionId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RestoreVersion(_testSnippetId, _testVersionId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    /// <summary>
    /// 测试版本恢复 - 权限不足
    /// </summary>
    [TestMethod]
    public async Task RestoreVersion_WithoutPermission_ReturnsForbid()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Edit))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RestoreVersion(_testSnippetId, _testVersionId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试版本比较 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CompareVersions_WithValidVersions_ReturnsComparison()
    {
        // Arrange
        var fromVersionId = Guid.NewGuid();
        var toVersionId = Guid.NewGuid();
        var expectedComparison = new VersionComparisonDto
        {
            FromVersion = new SnippetVersionDto { Id = fromVersionId, SnippetId = _testSnippetId },
            ToVersion = new SnippetVersionDto { Id = toVersionId, SnippetId = _testSnippetId },
            CodeChanged = true
        };

        _mockVersionService
            .Setup(x => x.CompareVersionsAsync(fromVersionId, toVersionId))
            .ReturnsAsync(expectedComparison);

        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CompareVersions(fromVersionId, toVersionId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedComparison, okResult.Value);
    }

    /// <summary>
    /// 测试创建版本 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CreateVersion_WithValidPermission_ReturnsCreated()
    {
        // Arrange
        var request = new CreateVersionRequest { ChangeDescription = "测试变更" };
        var expectedVersion = new SnippetVersionDto 
        { 
            Id = _testVersionId, 
            SnippetId = _testSnippetId, 
            VersionNumber = 2 
        };

        _mockPermissionService
            .Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Edit))
            .ReturnsAsync(true);

        _mockVersionService
            .Setup(x => x.CreateVersionAsync(_testSnippetId, request.ChangeDescription))
            .ReturnsAsync(expectedVersion);

        // Act
        var result = await _controller.CreateVersion(_testSnippetId, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdResult);
        Assert.AreEqual(expectedVersion, createdResult.Value);
    }
}