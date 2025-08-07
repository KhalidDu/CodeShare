using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CodeSnippetManager.Api.Services;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 版本管理服务单元测试
/// </summary>
[TestClass]
public class VersionManagementServiceTests
{
    private readonly Mock<ISnippetVersionRepository> _mockVersionRepository;
    private readonly Mock<ICodeSnippetRepository> _mockSnippetRepository;
    private readonly Mock<IDbConnectionFactory> _mockConnectionFactory;
    private readonly VersionManagementService _service;
    private readonly Guid _testSnippetId = Guid.NewGuid();
    private readonly Guid _testVersionId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();

    public VersionManagementServiceTests()
    {
        _mockVersionRepository = new Mock<ISnippetVersionRepository>();
        _mockSnippetRepository = new Mock<ICodeSnippetRepository>();
        _mockConnectionFactory = new Mock<IDbConnectionFactory>();
        _service = new VersionManagementService(
            _mockVersionRepository.Object,
            _mockSnippetRepository.Object,
            _mockConnectionFactory.Object);
    }

    /// <summary>
    /// 测试创建初始版本 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CreateInitialVersionAsync_WithValidSnippet_ReturnsVersion()
    {
        // Arrange
        var snippet = new CodeSnippet
        {
            Id = _testSnippetId,
            Title = "测试代码片段",
            Description = "测试描述",
            Code = "console.log('test');",
            Language = "javascript",
            CreatedBy = _testUserId
        };

        var expectedVersion = new SnippetVersion
        {
            Id = _testVersionId,
            SnippetId = _testSnippetId,
            VersionNumber = 1,
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = snippet.CreatedBy,
            ChangeDescription = "初始版本"
        };

        _mockSnippetRepository
            .Setup(x => x.GetByIdAsync(_testSnippetId))
            .ReturnsAsync(snippet);

        _mockVersionRepository
            .Setup(x => x.CreateAsync(It.IsAny<SnippetVersion>()))
            .ReturnsAsync(expectedVersion);

        // Act
        var result = await _service.CreateInitialVersionAsync(_testSnippetId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testSnippetId, result.SnippetId);
        Assert.AreEqual(1, result.VersionNumber);
        Assert.AreEqual("初始版本", result.ChangeDescription);
        Assert.AreEqual(snippet.Title, result.Title);
        Assert.AreEqual(snippet.Code, result.Code);
    }

    /// <summary>
    /// 测试创建初始版本 - 代码片段不存在
    /// </summary>
    [TestMethod]
    public async Task CreateInitialVersionAsync_WithInvalidSnippet_ThrowsException()
    {
        // Arrange
        _mockSnippetRepository
            .Setup(x => x.GetByIdAsync(_testSnippetId))
            .ReturnsAsync((CodeSnippet?)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.CreateInitialVersionAsync(_testSnippetId));
    }

    /// <summary>
    /// 测试获取版本历史 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetVersionHistoryAsync_WithValidSnippet_ReturnsVersions()
    {
        // Arrange
        var versions = new List<SnippetVersion>
        {
            new() { Id = Guid.NewGuid(), SnippetId = _testSnippetId, VersionNumber = 2 },
            new() { Id = Guid.NewGuid(), SnippetId = _testSnippetId, VersionNumber = 1 }
        };

        _mockVersionRepository
            .Setup(x => x.GetBySnippetIdAsync(_testSnippetId))
            .ReturnsAsync(versions);

        // Act
        var result = await _service.GetVersionHistoryAsync(_testSnippetId);

        // Assert
        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.AreEqual(2, resultList.Count);
        // 验证按版本号降序排列
        Assert.AreEqual(2, resultList[0].VersionNumber);
        Assert.AreEqual(1, resultList[1].VersionNumber);
    }

    /// <summary>
    /// 测试获取版本详情 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetVersionAsync_WithValidId_ReturnsVersion()
    {
        // Arrange
        var version = new SnippetVersion
        {
            Id = _testVersionId,
            SnippetId = _testSnippetId,
            VersionNumber = 1,
            Title = "测试版本",
            Code = "console.log('version 1');"
        };

        _mockVersionRepository
            .Setup(x => x.GetByIdAsync(_testVersionId))
            .ReturnsAsync(version);

        // Act
        var result = await _service.GetVersionAsync(_testVersionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testVersionId, result.Id);
        Assert.AreEqual(_testSnippetId, result.SnippetId);
        Assert.AreEqual(1, result.VersionNumber);
        Assert.AreEqual("测试版本", result.Title);
    }

    /// <summary>
    /// 测试获取版本详情 - 版本不存在
    /// </summary>
    [TestMethod]
    public async Task GetVersionAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockVersionRepository
            .Setup(x => x.GetByIdAsync(_testVersionId))
            .ReturnsAsync((SnippetVersion?)null);

        // Act
        var result = await _service.GetVersionAsync(_testVersionId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// 测试版本比较 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CompareVersionsAsync_WithValidVersions_ReturnsComparison()
    {
        // Arrange
        var fromVersionId = Guid.NewGuid();
        var toVersionId = Guid.NewGuid();

        var fromVersion = new SnippetVersion
        {
            Id = fromVersionId,
            SnippetId = _testSnippetId,
            VersionNumber = 1,
            Title = "原始标题",
            Code = "console.log('version 1');",
            Language = "javascript"
        };

        var toVersion = new SnippetVersion
        {
            Id = toVersionId,
            SnippetId = _testSnippetId,
            VersionNumber = 2,
            Title = "更新标题",
            Code = "console.log('version 2');",
            Language = "javascript"
        };

        _mockVersionRepository
            .Setup(x => x.GetByIdAsync(fromVersionId))
            .ReturnsAsync(fromVersion);

        _mockVersionRepository
            .Setup(x => x.GetByIdAsync(toVersionId))
            .ReturnsAsync(toVersion);

        // Act
        var result = await _service.CompareVersionsAsync(fromVersionId, toVersionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(fromVersionId, result.FromVersion.Id);
        Assert.AreEqual(toVersionId, result.ToVersion.Id);
        Assert.IsTrue(result.TitleChanged);
        Assert.IsTrue(result.CodeChanged);
        Assert.IsFalse(result.LanguageChanged);
        Assert.IsTrue(result.CodeDifferences.Count > 0);
    }

    /// <summary>
    /// 测试版本比较 - 版本不属于同一代码片段
    /// </summary>
    [TestMethod]
    public async Task CompareVersionsAsync_WithDifferentSnippets_ThrowsException()
    {
        // Arrange
        var fromVersionId = Guid.NewGuid();
        var toVersionId = Guid.NewGuid();
        var otherSnippetId = Guid.NewGuid();

        var fromVersion = new SnippetVersion
        {
            Id = fromVersionId,
            SnippetId = _testSnippetId,
            VersionNumber = 1
        };

        var toVersion = new SnippetVersion
        {
            Id = toVersionId,
            SnippetId = otherSnippetId, // 不同的代码片段ID
            VersionNumber = 1
        };

        _mockVersionRepository
            .Setup(x => x.GetByIdAsync(fromVersionId))
            .ReturnsAsync(fromVersion);

        _mockVersionRepository
            .Setup(x => x.GetByIdAsync(toVersionId))
            .ReturnsAsync(toVersion);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.CompareVersionsAsync(fromVersionId, toVersionId));
    }

    /// <summary>
    /// 测试创建新版本 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CreateVersionAsync_WithValidSnippet_ReturnsVersion()
    {
        // Arrange
        var snippet = new CodeSnippet
        {
            Id = _testSnippetId,
            Title = "更新的代码片段",
            Description = "更新的描述",
            Code = "console.log('updated');",
            Language = "javascript",
            CreatedBy = _testUserId
        };

        var expectedVersion = new SnippetVersion
        {
            Id = _testVersionId,
            SnippetId = _testSnippetId,
            VersionNumber = 2,
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = snippet.CreatedBy,
            ChangeDescription = "功能更新"
        };

        _mockSnippetRepository
            .Setup(x => x.GetByIdAsync(_testSnippetId))
            .ReturnsAsync(snippet);

        _mockVersionRepository
            .Setup(x => x.GetNextVersionNumberAsync(_testSnippetId))
            .ReturnsAsync(2);

        _mockVersionRepository
            .Setup(x => x.CreateAsync(It.IsAny<SnippetVersion>()))
            .ReturnsAsync(expectedVersion);

        // Act
        var result = await _service.CreateVersionAsync(_testSnippetId, "功能更新");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testSnippetId, result.SnippetId);
        Assert.AreEqual(2, result.VersionNumber);
        Assert.AreEqual("功能更新", result.ChangeDescription);
        Assert.AreEqual(snippet.Title, result.Title);
        Assert.AreEqual(snippet.Code, result.Code);
    }
}