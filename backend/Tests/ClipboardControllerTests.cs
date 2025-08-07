using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CodeSnippetManager.Api.Controllers;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 剪贴板控制器测试 - 验证复制功能 API 实现
/// </summary>
[TestClass]
public class ClipboardControllerTests
{
    private Mock<IClipboardService> _mockClipboardService;
    private Mock<ICodeSnippetService> _mockCodeSnippetService;
    private Mock<ILogger<ClipboardController>> _mockLogger;
    private ClipboardController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testSnippetId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _mockClipboardService = new Mock<IClipboardService>();
        _mockCodeSnippetService = new Mock<ICodeSnippetService>();
        _mockLogger = new Mock<ILogger<ClipboardController>>();

        _controller = new ClipboardController(
            _mockClipboardService.Object,
            _mockCodeSnippetService.Object,
            _mockLogger.Object);

        // 设置用户身份
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    /// <summary>
    /// 测试记录复制操作 - 已登录用户
    /// </summary>
    [TestMethod]
    public async Task RecordCopy_AuthenticatedUser_ShouldRecordCopyAndReturnHistory()
    {
        // Arrange
        var expectedHistory = new ClipboardHistoryDto
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            SnippetId = _testSnippetId,
            CopiedAt = DateTime.UtcNow
        };

        _mockCodeSnippetService
            .Setup(s => s.IncrementCopyCountAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(true);

        _mockClipboardService
            .Setup(s => s.RecordCopyAsync(_testUserId, _testSnippetId))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.RecordCopy(_testSnippetId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedHistory = okResult?.Value as ClipboardHistoryDto;

        Assert.IsNotNull(returnedHistory);
        Assert.AreEqual(expectedHistory.Id, returnedHistory.Id);
        Assert.AreEqual(expectedHistory.SnippetId, returnedHistory.SnippetId);

        _mockCodeSnippetService.Verify(s => s.IncrementCopyCountAsync(_testSnippetId, _testUserId), Times.Once);
        _mockClipboardService.Verify(s => s.RecordCopyAsync(_testUserId, _testSnippetId), Times.Once);
    }

    /// <summary>
    /// 测试记录复制操作 - 匿名用户
    /// </summary>
    [TestMethod]
    public async Task RecordCopy_AnonymousUser_ShouldOnlyIncrementCopyCount()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户

        _mockCodeSnippetService
            .Setup(s => s.IncrementCopyCountAsync(_testSnippetId, null))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RecordCopy(_testSnippetId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        
        Assert.IsNotNull(okResult?.Value);

        _mockCodeSnippetService.Verify(s => s.IncrementCopyCountAsync(_testSnippetId, null), Times.Once);
        _mockClipboardService.Verify(s => s.RecordCopyAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试记录复制操作 - 代码片段不存在
    /// </summary>
    [TestMethod]
    public async Task RecordCopy_SnippetNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockCodeSnippetService
            .Setup(s => s.IncrementCopyCountAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RecordCopy(_testSnippetId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));

        _mockClipboardService.Verify(s => s.RecordCopyAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试获取剪贴板历史
    /// </summary>
    [TestMethod]
    public async Task GetClipboardHistory_ValidRequest_ShouldReturnHistoryWithSnippets()
    {
        // Arrange
        var histories = new List<ClipboardHistoryDto>
        {
            new ClipboardHistoryDto
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                SnippetId = _testSnippetId,
                CopiedAt = DateTime.UtcNow
            }
        };

        var snippet = new CodeSnippetDto
        {
            Id = _testSnippetId,
            Title = "Test Snippet",
            Code = "console.log('test');",
            Language = "javascript"
        };

        _mockClipboardService
            .Setup(s => s.GetUserClipboardHistoryAsync(_testUserId, 50))
            .ReturnsAsync(histories);

        _mockCodeSnippetService
            .Setup(s => s.GetSnippetAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(snippet);

        // Act
        var result = await _controller.GetClipboardHistory();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedHistories = okResult?.Value as List<ClipboardHistoryWithSnippetDto>;

        Assert.IsNotNull(returnedHistories);
        Assert.AreEqual(1, returnedHistories.Count);
        Assert.AreEqual(snippet.Title, returnedHistories[0].Snippet?.Title);
    }

    /// <summary>
    /// 测试获取剪贴板历史记录数量
    /// </summary>
    [TestMethod]
    public async Task GetClipboardHistoryCount_ValidRequest_ShouldReturnCount()
    {
        // Arrange
        const int expectedCount = 25;
        _mockClipboardService
            .Setup(s => s.GetUserHistoryCountAsync(_testUserId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _controller.GetClipboardHistoryCount();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        
        Assert.AreEqual(expectedCount, okResult?.Value);
    }

    /// <summary>
    /// 测试清空剪贴板历史
    /// </summary>
    [TestMethod]
    public async Task ClearClipboardHistory_ValidRequest_ShouldClearHistory()
    {
        // Arrange
        _mockClipboardService
            .Setup(s => s.ClearUserClipboardHistoryAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ClearClipboardHistory();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        
        _mockClipboardService.Verify(s => s.ClearUserClipboardHistoryAsync(_testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取复制统计
    /// </summary>
    [TestMethod]
    public async Task GetCopyStats_ValidRequest_ShouldReturnStats()
    {
        // Arrange
        var snippet = new CodeSnippetDto
        {
            Id = _testSnippetId,
            Title = "Test Snippet",
            CopyCount = 10,
            ViewCount = 25
        };

        _mockCodeSnippetService
            .Setup(s => s.GetSnippetAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(snippet);

        // Act
        var result = await _controller.GetCopyStats(_testSnippetId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var stats = okResult?.Value as CopyStatsDto;

        Assert.IsNotNull(stats);
        Assert.AreEqual(_testSnippetId, stats.SnippetId);
        Assert.AreEqual(10, stats.TotalCopyCount);
        Assert.AreEqual(25, stats.ViewCount);
    }

    /// <summary>
    /// 测试批量获取复制统计
    /// </summary>
    [TestMethod]
    public async Task GetBatchCopyStats_ValidRequest_ShouldReturnBatchStats()
    {
        // Arrange
        var snippetIds = new List<Guid> { _testSnippetId, Guid.NewGuid() };
        var request = new BatchCopyStatsRequestDto { SnippetIds = snippetIds };
        
        var expectedStats = new Dictionary<Guid, int>
        {
            { snippetIds[0], 10 },
            { snippetIds[1], 5 }
        };

        _mockClipboardService
            .Setup(s => s.GetCopyCountsBatchAsync(snippetIds))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetBatchCopyStats(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var stats = okResult?.Value as Dictionary<Guid, int>;

        Assert.IsNotNull(stats);
        Assert.AreEqual(2, stats.Count);
        Assert.AreEqual(10, stats[snippetIds[0]]);
        Assert.AreEqual(5, stats[snippetIds[1]]);
    }

    /// <summary>
    /// 测试重新复制历史记录
    /// </summary>
    [TestMethod]
    public async Task RecopyFromHistory_ValidRequest_ShouldCreateNewHistory()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var existingHistory = new ClipboardHistoryDto
        {
            Id = historyId,
            UserId = _testUserId,
            SnippetId = _testSnippetId,
            CopiedAt = DateTime.UtcNow.AddHours(-1)
        };

        var newHistory = new ClipboardHistoryDto
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            SnippetId = _testSnippetId,
            CopiedAt = DateTime.UtcNow
        };

        _mockClipboardService
            .Setup(s => s.GetUserClipboardHistoryAsync(_testUserId, 1000))
            .ReturnsAsync(new List<ClipboardHistoryDto> { existingHistory });

        _mockClipboardService
            .Setup(s => s.RecordCopyAsync(_testUserId, _testSnippetId))
            .ReturnsAsync(newHistory);

        // Act
        var result = await _controller.RecopyFromHistory(historyId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedHistory = okResult?.Value as ClipboardHistoryDto;

        Assert.IsNotNull(returnedHistory);
        Assert.AreEqual(newHistory.Id, returnedHistory.Id);
        Assert.AreEqual(_testSnippetId, returnedHistory.SnippetId);

        _mockClipboardService.Verify(s => s.RecordCopyAsync(_testUserId, _testSnippetId), Times.Once);
    }

    /// <summary>
    /// 测试输入验证 - 空的代码片段ID
    /// </summary>
    [TestMethod]
    public async Task RecordCopy_EmptySnippetId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.RecordCopy(Guid.Empty);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试批量统计输入验证 - 空列表
    /// </summary>
    [TestMethod]
    public async Task GetBatchCopyStats_EmptyList_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new BatchCopyStatsRequestDto { SnippetIds = new List<Guid>() };

        // Act
        var result = await _controller.GetBatchCopyStats(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }
}