using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CodeSnippetManager.Api.Controllers;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 分享控制器测试 - 验证分享功能 API 实现
/// </summary>
[TestClass]
public class ShareControllerTests
{
    private Mock<IShareService> _mockShareService = null!;
    private Mock<ILogger<ShareController>> _mockLogger = null!;
    private ShareController _controller = null!;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testSnippetId = Guid.NewGuid();
    private readonly Guid _testShareTokenId = Guid.NewGuid();
    private readonly string _testToken = "test-token-123";

    [TestInitialize]
    public void Setup()
    {
        _mockShareService = new Mock<IShareService>();
        _mockLogger = new Mock<ILogger<ShareController>>();

        _controller = new ShareController(
            _mockShareService.Object,
            _mockLogger.Object);

        // 设置用户身份
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "User")
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

    #region 创建分享链接测试

    /// <summary>
    /// 测试创建分享链接 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CreateShare_ValidRequest_ShouldReturnCreatedShareToken()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            ExpiresInHours = 24,
            MaxAccessCount = 100,
            AllowDownload = true,
            AllowCopy = true
        };

        var expectedShareToken = new ShareTokenDto
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 0,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            HasPassword = false,
            AllowDownload = true,
            AllowCopy = true,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareService
            .Setup(s => s.CreateShareTokenAsync(createShareDto, _testUserId))
            .ReturnsAsync(expectedShareToken);

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        var createdResult = result.Result as CreatedAtActionResult;
        var returnedToken = createdResult?.Value as ShareTokenDto;

        Assert.IsNotNull(returnedToken);
        Assert.AreEqual(expectedShareToken.Id, returnedToken.Id);
        Assert.AreEqual(expectedShareToken.Token, returnedToken.Token);
        Assert.AreEqual(expectedShareToken.CodeSnippetId, returnedToken.CodeSnippetId);

        _mockShareService.Verify(s => s.CreateShareTokenAsync(createShareDto, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试创建分享链接 - 无效的代码片段ID
    /// </summary>
    [TestMethod]
    public async Task CreateShare_EmptySnippetId_ShouldReturnBadRequest()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = Guid.Empty,
            Permission = SharePermission.ReadOnly
        };

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.CreateShareTokenAsync(It.IsAny<CreateShareDto>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试创建分享链接 - 无权限访问代码片段
    /// </summary>
    [TestMethod]
    public async Task CreateShare_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        _mockShareService
            .Setup(s => s.CreateShareTokenAsync(createShareDto, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限访问此代码片段"));

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试创建分享链接 - 代码片段不存在
    /// </summary>
    [TestMethod]
    public async Task CreateShare_SnippetNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        _mockShareService
            .Setup(s => s.CreateShareTokenAsync(createShareDto, _testUserId))
            .ThrowsAsync(new ArgumentException("代码片段不存在"));

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.IsNotNull(badRequestResult?.Value);
    }

    /// <summary>
    /// 测试创建分享链接 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task CreateShare_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户
        
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 获取分享信息测试

    /// <summary>
    /// 测试通过分享令牌获取分享信息 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetShare_ValidToken_ShouldReturnShareToken()
    {
        // Arrange
        var expectedShareToken = new ShareTokenDto
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 0,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            HasPassword = false,
            AllowDownload = true,
            AllowCopy = true,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareService
            .Setup(s => s.GetShareTokenByTokenAsync(_testToken, null))
            .ReturnsAsync(expectedShareToken);

        _mockShareService
            .Setup(s => s.LogShareAccessAsync(_testShareTokenId, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.GetShare(_testToken);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedToken = okResult?.Value as ShareTokenDto;

        Assert.IsNotNull(returnedToken);
        Assert.AreEqual(expectedShareToken.Id, returnedToken.Id);
        Assert.AreEqual(expectedShareToken.Token, returnedToken.Token);

        _mockShareService.Verify(s => s.GetShareTokenByTokenAsync(_testToken, null), Times.Once);
        _mockShareService.Verify(s => s.LogShareAccessAsync(_testShareTokenId, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// 测试通过分享令牌获取分享信息 - 令牌为空
    /// </summary>
    [TestMethod]
    public async Task GetShare_EmptyToken_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetShare("");

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.GetShareTokenByTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// 测试通过分享令牌获取分享信息 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task GetShare_TokenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.GetShareTokenByTokenAsync(_testToken, null))
            .ReturnsAsync((ShareTokenDto?)null);

        // Act
        var result = await _controller.GetShare(_testToken);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// 测试通过分享令牌获取分享信息 - 带密码保护
    /// </summary>
    [TestMethod]
    public async Task GetShare_WithPassword_ShouldReturnShareToken()
    {
        // Arrange
        var password = "testpassword";
        var expectedShareToken = new ShareTokenDto
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 0,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            HasPassword = true,
            AllowDownload = true,
            AllowCopy = true,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareService
            .Setup(s => s.GetShareTokenByTokenAsync(_testToken, password))
            .ReturnsAsync(expectedShareToken);

        _mockShareService
            .Setup(s => s.LogShareAccessAsync(_testShareTokenId, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.GetShare(_testToken, password);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedToken = okResult?.Value as ShareTokenDto;

        Assert.IsNotNull(returnedToken);
        Assert.AreEqual(expectedShareToken.Id, returnedToken.Id);
        Assert.IsTrue(returnedToken.HasPassword);
    }

    #endregion

    #region 验证分享令牌测试

    /// <summary>
    /// 测试验证分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ValidateShare_ValidToken_ShouldReturnValidResult()
    {
        // Arrange
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Message = "分享令牌有效"
        };

        _mockShareService
            .Setup(s => s.ValidateShareTokenAsync(_testToken, null))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.ValidateShare(_testToken);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as dynamic;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(true, returnedResult?.isValid);
        Assert.AreEqual("分享令牌有效", returnedResult?.message);

        _mockShareService.Verify(s => s.ValidateShareTokenAsync(_testToken, null), Times.Once);
    }

    /// <summary>
    /// 测试验证分享令牌 - 无效令牌
    /// </summary>
    [TestMethod]
    public async Task ValidateShare_InvalidToken_ShouldReturnInvalidResult()
    {
        // Arrange
        var validationResult = new ValidationResult
        {
            IsValid = false,
            Message = "分享令牌已过期"
        };

        _mockShareService
            .Setup(s => s.ValidateShareTokenAsync(_testToken, null))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.ValidateShare(_testToken);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        var badRequestResult = result.Result as BadRequestObjectResult;
        var returnedResult = badRequestResult?.Value as dynamic;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(false, returnedResult?.isValid);
        Assert.AreEqual("分享令牌已过期", returnedResult?.message);
    }

    /// <summary>
    /// 测试验证分享令牌 - 带密码
    /// </summary>
    [TestMethod]
    public async Task ValidateShare_WithPassword_ShouldReturnValidResult()
    {
        // Arrange
        var password = "testpassword";
        var request = new ValidateShareRequest { Password = password };
        
        var validationResult = new ValidationResult
        {
            IsValid = true,
            Message = "分享令牌有效"
        };

        _mockShareService
            .Setup(s => s.ValidateShareTokenAsync(_testToken, password))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.ValidateShare(_testToken, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as dynamic;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(true, returnedResult?.isValid);
    }

    #endregion

    #region 获取用户分享列表测试

    /// <summary>
    /// 测试获取用户分享列表 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetMyShares_ValidRequest_ShouldReturnPaginatedResult()
    {
        // Arrange
        var shareTokens = new List<ShareTokenDto>
        {
            new ShareTokenDto
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = _testUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 0,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                HasPassword = false,
                AllowDownload = true,
                AllowCopy = true,
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        var expectedResult = new PaginatedResult<ShareTokenDto>
        {
            Items = shareTokens,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockShareService
            .Setup(s => s.GetUserShareTokensPaginatedAsync(_testUserId, 1, 10, _testUserId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetMyShares(1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as PaginatedResult<ShareTokenDto>;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(1, returnedResult.TotalCount);
        Assert.AreEqual(1, returnedResult.Items.Count());
        Assert.AreEqual(_testShareTokenId, returnedResult.Items.First().Id);

        _mockShareService.Verify(s => s.GetUserShareTokensPaginatedAsync(_testUserId, 1, 10, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取用户分享列表 - 无效页码
    /// </summary>
    [TestMethod]
    public async Task GetMyShares_InvalidPage_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetMyShares(0, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.GetUserShareTokensPaginatedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试获取用户分享列表 - 无效页面大小
    /// </summary>
    [TestMethod]
    public async Task GetMyShares_InvalidPageSize_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetMyShares(1, 0);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试获取用户分享列表 - 页面大小过大
    /// </summary>
    [TestMethod]
    public async Task GetMyShares_PageSizeTooLarge_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetMyShares(1, 101);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试获取用户分享列表 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task GetMyShares_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户

        // Act
        var result = await _controller.GetMyShares(1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 获取分享统计测试

    /// <summary>
    /// 测试获取分享统计 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetShareStats_ValidShareTokenId_ShouldReturnStats()
    {
        // Arrange
        var expectedStats = new ShareStatsDto
        {
            ShareTokenId = _testShareTokenId,
            Token = _testToken,
            TotalAccessCount = 50,
            TodayAccessCount = 5,
            ThisWeekAccessCount = 20,
            ThisMonthAccessCount = 35,
            RemainingAccessCount = 50,
            LastAccessedAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-24),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsExpired = false,
            IsAccessLimitReached = false,
            DailyStats = new List<DailyAccessStatDto>(),
            SourceStats = new List<AccessSourceStatDto>()
        };

        _mockShareService
            .Setup(s => s.GetShareStatsAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedStats = okResult?.Value as ShareStatsDto;

        Assert.IsNotNull(returnedStats);
        Assert.AreEqual(expectedStats.ShareTokenId, returnedStats.ShareTokenId);
        Assert.AreEqual(expectedStats.TotalAccessCount, returnedStats.TotalAccessCount);
        Assert.AreEqual(expectedStats.TodayAccessCount, returnedStats.TodayAccessCount);

        _mockShareService.Verify(s => s.GetShareStatsAsync(_testShareTokenId, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取分享统计 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task GetShareStats_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetShareStats(Guid.Empty);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.GetShareStatsAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Never);
    }

    /// <summary>
    /// 测试获取分享统计 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task GetShareStats_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.GetShareStatsAsync(_testShareTokenId, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限访问此分享统计"));

        // Act
        var result = await _controller.GetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试获取分享统计 - 分享令牌不存在
    /// </summary>
    [TestMethod]
    public async Task GetShareStats_ShareTokenNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.GetShareStatsAsync(_testShareTokenId, _testUserId))
            .ThrowsAsync(new ArgumentException("分享令牌不存在"));

        // Act
        var result = await _controller.GetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    #endregion

    #region 更新分享设置测试

    /// <summary>
    /// 测试更新分享设置 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task UpdateShare_ValidRequest_ShouldReturnUpdatedShareToken()
    {
        // Arrange
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新后的描述",
            Permission = SharePermission.ReadWrite,
            AllowDownload = false,
            AllowCopy = true,
            MaxAccessCount = 50,
            ExtendHours = 24
        };

        var expectedShareToken = new ShareTokenDto
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow.AddHours(-24),
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 10,
            MaxAccessCount = 50,
            Permission = SharePermission.ReadWrite,
            Description = "更新后的描述",
            HasPassword = false,
            AllowDownload = false,
            AllowCopy = true,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareService
            .Setup(s => s.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId))
            .ReturnsAsync(expectedShareToken);

        // Act
        var result = await _controller.UpdateShare(_testShareTokenId, updateShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedToken = okResult?.Value as ShareTokenDto;

        Assert.IsNotNull(returnedToken);
        Assert.AreEqual(expectedShareToken.Id, returnedToken.Id);
        Assert.AreEqual(expectedShareToken.Description, returnedToken.Description);
        Assert.AreEqual(expectedShareToken.Permission, returnedToken.Permission);

        _mockShareService.Verify(s => s.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试更新分享设置 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task UpdateShare_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Arrange
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新后的描述"
        };

        // Act
        var result = await _controller.UpdateShare(Guid.Empty, updateShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.UpdateShareTokenAsync(It.IsAny<Guid>(), It.IsAny<UpdateShareDto>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试更新分享设置 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task UpdateShare_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新后的描述"
        };

        _mockShareService
            .Setup(s => s.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限更新此分享"));

        // Act
        var result = await _controller.UpdateShare(_testShareTokenId, updateShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试更新分享设置 - 分享令牌不存在
    /// </summary>
    [TestMethod]
    public async Task UpdateShare_ShareTokenNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新后的描述"
        };

        _mockShareService
            .Setup(s => s.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId))
            .ThrowsAsync(new ArgumentException("分享令牌不存在"));

        // Act
        var result = await _controller.UpdateShare(_testShareTokenId, updateShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试更新分享设置 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task UpdateShare_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户
        
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新后的描述"
        };

        // Act
        var result = await _controller.UpdateShare(_testShareTokenId, updateShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 撤销分享链接测试

    /// <summary>
    /// 测试撤销分享链接 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task RevokeShare_ValidShareTokenId_ShouldReturnNoContent()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.RevokeShareTokenAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RevokeShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));

        _mockShareService.Verify(s => s.RevokeShareTokenAsync(_testShareTokenId, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试撤销分享链接 - 分享令牌不存在
    /// </summary>
    [TestMethod]
    public async Task RevokeShare_ShareTokenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.RevokeShareTokenAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RevokeShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// 测试撤销分享链接 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task RevokeShare_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.RevokeShare(Guid.Empty);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.RevokeShareTokenAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试撤销分享链接 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task RevokeShare_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.RevokeShareTokenAsync(_testShareTokenId, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限撤销此分享"));

        // Act
        var result = await _controller.RevokeShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试撤销分享链接 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task RevokeShare_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户

        // Act
        var result = await _controller.RevokeShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 删除分享链接测试

    /// <summary>
    /// 测试删除分享链接 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task DeleteShare_ValidShareTokenId_ShouldReturnNoContent()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.DeleteShareTokenAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));

        _mockShareService.Verify(s => s.DeleteShareTokenAsync(_testShareTokenId, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试删除分享链接 - 分享令牌不存在
    /// </summary>
    [TestMethod]
    public async Task DeleteShare_ShareTokenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.DeleteShareTokenAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// 测试删除分享链接 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task DeleteShare_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.DeleteShare(Guid.Empty);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.DeleteShareTokenAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试删除分享链接 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task DeleteShare_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.DeleteShareTokenAsync(_testShareTokenId, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限删除此分享"));

        // Act
        var result = await _controller.DeleteShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试删除分享链接 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task DeleteShare_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户

        // Act
        var result = await _controller.DeleteShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 获取分享访问日志测试

    /// <summary>
    /// 测试获取分享访问日志 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogs_ValidRequest_ShouldReturnPaginatedResult()
    {
        // Arrange
        var accessLogs = new List<ShareAccessLogDto>
        {
            new ShareAccessLogDto
            {
                Id = Guid.NewGuid(),
                ShareTokenId = _testShareTokenId,
                AccessToken = _testToken,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                AccessedAt = DateTime.UtcNow.AddHours(-1),
                IsSuccess = true,
                ErrorMessage = null
            }
        };

        var expectedResult = new PaginatedResult<ShareAccessLogDto>
        {
            Items = accessLogs,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        var filter = new AccessLogFilter
        {
            ShareTokenId = _testShareTokenId,
            Page = 1,
            PageSize = 10
        };

        _mockShareService
            .Setup(s => s.GetShareAccessLogsAsync(filter, _testUserId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetShareAccessLogs(_testShareTokenId, 1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as PaginatedResult<ShareAccessLogDto>;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(1, returnedResult.TotalCount);
        Assert.AreEqual(1, returnedResult.Items.Count());
        Assert.AreEqual(_testShareTokenId, returnedResult.Items.First().ShareTokenId);

        _mockShareService.Verify(s => s.GetShareAccessLogsAsync(It.Is<AccessLogFilter>(f => 
            f.ShareTokenId == _testShareTokenId && f.Page == 1 && f.PageSize == 10), _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取分享访问日志 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogs_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetShareAccessLogs(Guid.Empty, 1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.GetShareAccessLogsAsync(It.IsAny<AccessLogFilter>(), It.IsAny<Guid?>()), Times.Never);
    }

    /// <summary>
    /// 测试获取分享访问日志 - 无效页码
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogs_InvalidPage_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetShareAccessLogs(_testShareTokenId, 0, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试获取分享访问日志 - 无效页面大小
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogs_InvalidPageSize_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetShareAccessLogs(_testShareTokenId, 1, 0);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试获取分享访问日志 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogs_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        var filter = new AccessLogFilter
        {
            ShareTokenId = _testShareTokenId,
            Page = 1,
            PageSize = 10
        };

        _mockShareService
            .Setup(s => s.GetShareAccessLogsAsync(filter, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限访问此分享日志"));

        // Act
        var result = await _controller.GetShareAccessLogs(_testShareTokenId, 1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试获取分享访问日志 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogs_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户

        // Act
        var result = await _controller.GetShareAccessLogs(_testShareTokenId, 1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 延长分享有效期测试

    /// <summary>
    /// 测试延长分享有效期 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ExtendShareExpiry_ValidRequest_ShouldReturnUpdatedShareToken()
    {
        // Arrange
        var request = new ExtendShareExpiryRequest
        {
            ExtendHours = 24
        };

        var expectedShareToken = new ShareTokenDto
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow.AddHours(-24),
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 10,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            HasPassword = false,
            AllowDownload = true,
            AllowCopy = true,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareService
            .Setup(s => s.ExtendShareTokenExpiryAsync(_testShareTokenId, 24, _testUserId))
            .ReturnsAsync(expectedShareToken);

        // Act
        var result = await _controller.ExtendShareExpiry(_testShareTokenId, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedToken = okResult?.Value as ShareTokenDto;

        Assert.IsNotNull(returnedToken);
        Assert.AreEqual(expectedShareToken.Id, returnedToken.Id);
        Assert.AreEqual(expectedShareToken.ExpiresAt, returnedToken.ExpiresAt);

        _mockShareService.Verify(s => s.ExtendShareTokenExpiryAsync(_testShareTokenId, 24, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试延长分享有效期 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task ExtendShareExpiry_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ExtendShareExpiryRequest
        {
            ExtendHours = 24
        };

        // Act
        var result = await _controller.ExtendShareExpiry(Guid.Empty, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.ExtendShareTokenExpiryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试延长分享有效期 - 无效的延长时间
    /// </summary>
    [TestMethod]
    public async Task ExtendShareExpiry_InvalidExtendHours_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ExtendShareExpiryRequest
        {
            ExtendHours = 0
        };

        // Act
        var result = await _controller.ExtendShareExpiry(_testShareTokenId, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试延长分享有效期 - 延长时间过长
    /// </summary>
    [TestMethod]
    public async Task ExtendShareExpiry_ExtendHoursTooLarge_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ExtendShareExpiryRequest
        {
            ExtendHours = 8761 // 超过1年
        };

        // Act
        var result = await _controller.ExtendShareExpiry(_testShareTokenId, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    /// <summary>
    /// 测试延长分享有效期 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task ExtendShareExpiry_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        var request = new ExtendShareExpiryRequest
        {
            ExtendHours = 24
        };

        _mockShareService
            .Setup(s => s.ExtendShareTokenExpiryAsync(_testShareTokenId, 24, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限延长此分享有效期"));

        // Act
        var result = await _controller.ExtendShareExpiry(_testShareTokenId, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试延长分享有效期 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task ExtendShareExpiry_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户
        
        var request = new ExtendShareExpiryRequest
        {
            ExtendHours = 24
        };

        // Act
        var result = await _controller.ExtendShareExpiry(_testShareTokenId, request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 重置分享统计测试

    /// <summary>
    /// 测试重置分享统计 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ResetShareStats_ValidShareTokenId_ShouldReturnNoContent()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.ResetShareAccessStatsAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));

        _mockShareService.Verify(s => s.ResetShareAccessStatsAsync(_testShareTokenId, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试重置分享统计 - 分享令牌不存在
    /// </summary>
    [TestMethod]
    public async Task ResetShareStats_ShareTokenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.ResetShareAccessStatsAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ResetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// 测试重置分享统计 - 无效的分享令牌ID
    /// </summary>
    [TestMethod]
    public async Task ResetShareStats_EmptyShareTokenId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ResetShareStats(Guid.Empty);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.ResetShareAccessStatsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试重置分享统计 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task ResetShareStats_UnauthorizedAccess_ShouldReturnForbid()
    {
        // Arrange
        _mockShareService
            .Setup(s => s.ResetShareAccessStatsAsync(_testShareTokenId, _testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("用户无权限重置此分享统计"));

        // Act
        var result = await _controller.ResetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    /// <summary>
    /// 测试重置分享统计 - 未登录用户
    /// </summary>
    [TestMethod]
    public async Task ResetShareStats_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // 匿名用户

        // Act
        var result = await _controller.ResetShareStats(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    #endregion

    #region 管理员功能测试

    /// <summary>
    /// 测试获取所有分享链接 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetAllShares_AdminUser_ShouldReturnPaginatedResult()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        var filter = new AdminShareFilter
        {
            Page = 1,
            PageSize = 20,
            Search = "test"
        };

        var shareTokens = new List<ShareTokenDto>
        {
            new ShareTokenDto
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = _testUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 0,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                HasPassword = false,
                AllowDownload = true,
                AllowCopy = true,
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        var expectedResult = new PaginatedResult<ShareTokenDto>
        {
            Items = shareTokens,
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };

        _mockShareService
            .Setup(s => s.GetAllSharesAdminAsync(filter, _testUserId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAllShares(filter);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as PaginatedResult<ShareTokenDto>;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(1, returnedResult.TotalCount);
        Assert.AreEqual(1, returnedResult.Items.Count());

        _mockShareService.Verify(s => s.GetAllSharesAdminAsync(filter, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取所有分享链接 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task GetAllShares_NonAdminUser_ShouldReturnForbid()
    {
        // Arrange
        var filter = new AdminShareFilter
        {
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await _controller.GetAllShares(filter);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.GetAllSharesAdminAsync(It.IsAny<AdminShareFilter>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试获取系统分享统计 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetSystemShareStats_AdminUser_ShouldReturnStats()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        var expectedStats = new SystemShareStatsDto
        {
            TotalShares = 100,
            ActiveShares = 80,
            ExpiredShares = 20,
            TotalAccessCount = 5000,
            TodayAccessCount = 50,
            ThisWeekAccessCount = 300,
            ThisMonthAccessCount = 1000,
            ActiveUserCount = 25,
            PopularShares = new List<PopularShareDto>(),
            ActiveUsers = new List<ActiveUserDto>(),
            DailyStats = new List<DailyShareStatDto>(),
            PermissionStats = new List<PermissionStatDto>(),
            LanguageStats = new List<LanguageStatDto>()
        };

        _mockShareService
            .Setup(s => s.GetSystemShareStatsAsync(_testUserId))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetSystemShareStats();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedStats = okResult?.Value as SystemShareStatsDto;

        Assert.IsNotNull(returnedStats);
        Assert.AreEqual(expectedStats.TotalShares, returnedStats.TotalShares);
        Assert.AreEqual(expectedStats.ActiveShares, returnedStats.ActiveShares);
        Assert.AreEqual(expectedStats.TotalAccessCount, returnedStats.TotalAccessCount);

        _mockShareService.Verify(s => s.GetSystemShareStatsAsync(_testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取系统分享统计 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task GetSystemShareStats_NonAdminUser_ShouldReturnForbid()
    {
        // Act
        var result = await _controller.GetSystemShareStats();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.GetSystemShareStatsAsync(It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试批量操作分享链接 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task BulkOperationShares_AdminUser_ShouldReturnOperationResult()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        var request = new BulkShareOperationRequest
        {
            ShareTokenIds = new List<Guid> { _testShareTokenId },
            Operation = "revoke",
            OperationParam = null
        };

        var expectedResult = new BulkOperationResultDto
        {
            TotalCount = 1,
            SuccessCount = 1,
            FailureCount = 0,
            FailedShareTokenIds = new List<Guid>(),
            FailureReasons = new List<string>()
        };

        _mockShareService
            .Setup(s => s.BulkOperationSharesAsync(request, _testUserId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.BulkOperationShares(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as BulkOperationResultDto;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(expectedResult.TotalCount, returnedResult.TotalCount);
        Assert.AreEqual(expectedResult.SuccessCount, returnedResult.SuccessCount);
        Assert.AreEqual(expectedResult.FailureCount, returnedResult.FailureCount);

        _mockShareService.Verify(s => s.BulkOperationSharesAsync(request, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试批量操作分享链接 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task BulkOperationShares_NonAdminUser_ShouldReturnForbid()
    {
        // Arrange
        var request = new BulkShareOperationRequest
        {
            ShareTokenIds = new List<Guid> { _testShareTokenId },
            Operation = "revoke"
        };

        // Act
        var result = await _controller.BulkOperationShares(request);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.BulkOperationSharesAsync(It.IsAny<BulkShareOperationRequest>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试强制撤销分享链接 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task ForceRevokeShare_AdminUser_ShouldReturnNoContent()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        _mockShareService
            .Setup(s => s.ForceRevokeShareTokenAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForceRevokeShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));

        _mockShareService.Verify(s => s.ForceRevokeShareTokenAsync(_testShareTokenId, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试强制撤销分享链接 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task ForceRevokeShare_NonAdminUser_ShouldReturnForbid()
    {
        // Act
        var result = await _controller.ForceRevokeShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.ForceRevokeShareTokenAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试强制删除分享链接 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task ForceDeleteShare_AdminUser_ShouldReturnNoContent()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        _mockShareService
            .Setup(s => s.ForceDeleteShareTokenAsync(_testShareTokenId, _testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForceDeleteShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));

        _mockShareService.Verify(s => s.ForceDeleteShareTokenAsync(_testShareTokenId, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试强制删除分享链接 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task ForceDeleteShare_NonAdminUser_ShouldReturnForbid()
    {
        // Act
        var result = await _controller.ForceDeleteShare(_testShareTokenId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.ForceDeleteShareTokenAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试获取用户分享链接 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetUserSharesAdmin_AdminUser_ShouldReturnPaginatedResult()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        var targetUserId = Guid.NewGuid();

        var shareTokens = new List<ShareTokenDto>
        {
            new ShareTokenDto
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = targetUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 0,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                HasPassword = false,
                AllowDownload = true,
                AllowCopy = true,
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        var expectedResult = new PaginatedResult<ShareTokenDto>
        {
            Items = shareTokens,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockShareService
            .Setup(s => s.GetUserShareTokensPaginatedAsync(targetUserId, 1, 10, _testUserId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetUserSharesAdmin(targetUserId, 1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as PaginatedResult<ShareTokenDto>;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(1, returnedResult.TotalCount);
        Assert.AreEqual(1, returnedResult.Items.Count());

        _mockShareService.Verify(s => s.GetUserShareTokensPaginatedAsync(targetUserId, 1, 10, _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取用户分享链接 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task GetUserSharesAdmin_NonAdminUser_ShouldReturnForbid()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();

        // Act
        var result = await _controller.GetUserSharesAdmin(targetUserId, 1, 10);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.GetUserShareTokensPaginatedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试获取分享访问日志 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogsAdmin_AdminUser_ShouldReturnPaginatedResult()
    {
        // Arrange
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuth");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        _controller.ControllerContext.HttpContext.User = adminPrincipal;

        var accessLogs = new List<ShareAccessLogDto>
        {
            new ShareAccessLogDto
            {
                Id = Guid.NewGuid(),
                ShareTokenId = _testShareTokenId,
                AccessToken = _testToken,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                AccessedAt = DateTime.UtcNow.AddHours(-1),
                IsSuccess = true,
                ErrorMessage = null
            }
        };

        var expectedResult = new PaginatedResult<ShareAccessLogDto>
        {
            Items = accessLogs,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        var filter = new AccessLogFilter
        {
            ShareTokenId = _testShareTokenId,
            Page = 1,
            PageSize = 10
        };

        _mockShareService
            .Setup(s => s.GetShareAccessLogsAsync(filter, _testUserId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetShareAccessLogsAdmin(_testShareTokenId, filter);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as PaginatedResult<ShareAccessLogDto>;

        Assert.IsNotNull(returnedResult);
        Assert.AreEqual(1, returnedResult.TotalCount);
        Assert.AreEqual(1, returnedResult.Items.Count());
        Assert.AreEqual(_testShareTokenId, returnedResult.Items.First().ShareTokenId);

        _mockShareService.Verify(s => s.GetShareAccessLogsAsync(It.Is<AccessLogFilter>(f => 
            f.ShareTokenId == _testShareTokenId && f.Page == 1 && f.PageSize == 10), _testUserId), Times.Once);
    }

    /// <summary>
    /// 测试获取分享访问日志 - 非管理员用户
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogsAdmin_NonAdminUser_ShouldReturnForbid()
    {
        // Arrange
        var filter = new AccessLogFilter
        {
            ShareTokenId = _testShareTokenId,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _controller.GetShareAccessLogsAdmin(_testShareTokenId, filter);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
        
        _mockShareService.Verify(s => s.GetShareAccessLogsAsync(It.IsAny<AccessLogFilter>(), It.IsAny<Guid?>()), Times.Never);
    }

    #endregion

    #region 边界条件和异常处理测试

    /// <summary>
    /// 测试服务异常处理
    /// </summary>
    [TestMethod]
    public async Task CreateShare_ServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        _mockShareService
            .Setup(s => s.CreateShareTokenAsync(createShareDto, _testUserId))
            .ThrowsAsync(new Exception("数据库连接失败"));

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        var objectResult = result.Result as ObjectResult;
        
        Assert.AreEqual(500, objectResult?.StatusCode);
    }

    /// <summary>
    /// 测试模型状态验证
    /// </summary>
    [TestMethod]
    public async Task CreateShare_InvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Permission", "权限级别不能为空");

        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        // Act
        var result = await _controller.CreateShare(createShareDto);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.CreateShareTokenAsync(It.IsAny<CreateShareDto>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// 测试空请求处理
    /// </summary>
    [TestMethod]
    public async Task CreateShare_NullRequest_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.CreateShare(null!);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        _mockShareService.Verify(s => s.CreateShareTokenAsync(It.IsAny<CreateShareDto>(), It.IsAny<Guid>()), Times.Never);
    }

    #endregion
}