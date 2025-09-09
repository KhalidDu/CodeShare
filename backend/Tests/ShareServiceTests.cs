using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CodeSnippetManager.Api.Services;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using ValidationResult = CodeSnippetManager.Api.Services.ValidationResult;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 分享服务单元测试 - 测试分享令牌生成、验证、撤销等核心功能
/// </summary>
[TestClass]
public class ShareServiceTests
{
    private readonly Mock<IShareTokenRepository> _mockShareTokenRepository;
    private readonly Mock<IShareAccessLogRepository> _mockShareAccessLogRepository;
    private readonly Mock<ICodeSnippetService> _mockCodeSnippetService;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IInputValidationService> _mockValidationService;
    private readonly Mock<ILogger<ShareService>> _mockLogger;
    private readonly ShareService _service;
    
    // 测试数据
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testSnippetId = Guid.NewGuid();
    private readonly Guid _testShareTokenId = Guid.NewGuid();
    private readonly string _testToken = "test-token-123";

    public ShareServiceTests()
    {
        _mockShareTokenRepository = new Mock<IShareTokenRepository>();
        _mockShareAccessLogRepository = new Mock<IShareAccessLogRepository>();
        _mockCodeSnippetService = new Mock<ICodeSnippetService>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockValidationService = new Mock<IInputValidationService>();
        _mockLogger = new Mock<ILogger<ShareService>>();

        _service = new ShareService(
            _mockShareTokenRepository.Object,
            _mockShareAccessLogRepository.Object,
            _mockCodeSnippetService.Object,
            _mockPermissionService.Object,
            _mockCacheService.Object,
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// 哈希密码（与ShareService中的实现相同）
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    #region 分享令牌生成测试

    /// <summary>
    /// 测试创建分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CreateShareTokenAsync_WithValidRequest_ReturnsShareToken()
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

        var snippet = new DTOs.CodeSnippetDto
        {
            Id = _testSnippetId,
            Title = "测试代码片段",
            Language = "javascript"
        };

        var expectedShareToken = new ShareToken
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
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        // 设置Mock行为
        _mockValidationService.Setup(x => x.ValidateShareExpiryHours(24))
            .Returns(ValidationResult.Success());
        
        _mockValidationService.Setup(x => x.ValidateShareMaxAccessCount(100))
            .Returns(ValidationResult.Success());

        _mockCodeSnippetService.Setup(x => x.GetSnippetAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(new DTOs.CodeSnippetDto
            {
                Id = _testSnippetId,
                Title = "测试代码片段",
                Language = "javascript",
                CreatedBy = _testUserId,
                CreatorName = "测试用户",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublic = true,
                ViewCount = 0,
                CopyCount = 0
            });

        _mockPermissionService.Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.CreateAsync(It.IsAny<ShareToken>()))
            .ReturnsAsync(expectedShareToken);

        // Act
        var result = await _service.CreateShareTokenAsync(createShareDto, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual(_testToken, result.Token);
        Assert.AreEqual(_testSnippetId, result.CodeSnippetId);
        Assert.AreEqual(_testUserId, result.CreatedBy);
        Assert.AreEqual(SharePermission.ReadOnly, result.Permission);
        Assert.AreEqual("测试分享", result.Description);
        Assert.AreEqual(100, result.MaxAccessCount);
        Assert.IsTrue(result.AllowDownload);
        Assert.IsTrue(result.AllowCopy);

        // 验证调用
        _mockValidationService.Verify(x => x.ValidateShareExpiryHours(24), Times.Once);
        _mockValidationService.Verify(x => x.ValidateShareMaxAccessCount(100), Times.Once);
        _mockCodeSnippetService.Verify(x => x.GetSnippetAsync(_testSnippetId, _testUserId), Times.Once);
        _mockPermissionService.Verify(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read), Times.Once);
        _mockShareTokenRepository.Verify(x => x.CreateAsync(It.IsAny<ShareToken>()), Times.Once);
    }

    /// <summary>
    /// 测试创建分享令牌 - 代码片段不存在
    /// </summary>
    [TestMethod]
    public async Task CreateShareTokenAsync_WithInvalidSnippet_ThrowsException()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        _mockValidationService.Setup(x => x.ValidateShareExpiryHours(24))
            .Returns(ValidationResult.Success());
        
        _mockValidationService.Setup(x => x.ValidateShareMaxAccessCount(0))
            .Returns(ValidationResult.Success());

        _mockCodeSnippetService.Setup(x => x.GetSnippetAsync(_testSnippetId, _testUserId))
            .ReturnsAsync((CodeSnippetDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.CreateShareTokenAsync(createShareDto, _testUserId));
        
        Assert.AreEqual("代码片段不存在或无权访问", exception.Message);
    }

    /// <summary>
    /// 测试创建分享令牌 - 用户无权限
    /// </summary>
    [TestMethod]
    public async Task CreateShareTokenAsync_WithoutPermission_ThrowsException()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly
        };

        var snippet = new DTOs.CodeSnippetDto
        {
            Id = _testSnippetId,
            Title = "测试代码片段",
            Language = "javascript"
        };

        _mockValidationService.Setup(x => x.ValidateShareExpiryHours(24))
            .Returns(ValidationResult.Success());
        
        _mockValidationService.Setup(x => x.ValidateShareMaxAccessCount(0))
            .Returns(ValidationResult.Success());

        _mockCodeSnippetService.Setup(x => x.GetSnippetAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(snippet);

        _mockPermissionService.Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.CreateShareTokenAsync(createShareDto, _testUserId));
        
        Assert.AreEqual("用户没有分享此代码片段的权限", exception.Message);
    }

    /// <summary>
    /// 测试创建分享令牌 - 验证失败
    /// </summary>
    [TestMethod]
    public async Task CreateShareTokenAsync_WithInvalidValidation_ThrowsException()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly,
            ExpiresInHours = 10000 // 超出范围
        };

        _mockValidationService.Setup(x => x.ValidateShareExpiryHours(10000))
            .Returns(ValidationResult.Failure("有效期必须在0-8760小时之间"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.CreateShareTokenAsync(createShareDto, _testUserId));
        
        Assert.AreEqual("有效期必须在0-8760小时之间", exception.Message);
    }

    /// <summary>
    /// 测试创建分享令牌 - 带密码保护
    /// </summary>
    [TestMethod]
    public async Task CreateShareTokenAsync_WithPassword_ReturnsShareTokenWithPassword()
    {
        // Arrange
        var createShareDto = new CreateShareDto
        {
            CodeSnippetId = _testSnippetId,
            Permission = SharePermission.ReadOnly,
            Password = "test-password-123"
        };

        var snippet = new DTOs.CodeSnippetDto
        {
            Id = _testSnippetId,
            Title = "测试代码片段",
            Language = "javascript"
        };

        var expectedShareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            Password = "hashed-password", // 应该被哈希
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 0,
            MaxAccessCount = 0,
            Permission = SharePermission.ReadOnly,
            Description = "",
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockValidationService.Setup(x => x.ValidateShareExpiryHours(24))
            .Returns(ValidationResult.Success());
        
        _mockValidationService.Setup(x => x.ValidateShareMaxAccessCount(0))
            .Returns(ValidationResult.Success());

        _mockCodeSnippetService.Setup(x => x.GetSnippetAsync(_testSnippetId, _testUserId))
            .ReturnsAsync(snippet);

        _mockPermissionService.Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.CreateAsync(It.IsAny<ShareToken>()))
            .ReturnsAsync(expectedShareToken);

        // Act
        var result = await _service.CreateShareTokenAsync(createShareDto, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasPassword);
        Assert.AreEqual(_testShareTokenId, result.Id);

        // 验证密码被哈希处理
        _mockShareTokenRepository.Verify(x => x.CreateAsync(It.Is<ShareToken>(t => 
            t.Password != null && t.Password != "test-password-123")), Times.Once);
    }

    #endregion

    #region 分享令牌验证测试

    /// <summary>
    /// 测试验证分享令牌 - 有效令牌
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.Message);
        Assert.IsNotNull(result.ShareToken);
        Assert.AreEqual(_testShareTokenId, result.ShareToken.Id);
        Assert.AreEqual(_testToken, result.ShareToken.Token);
    }

    /// <summary>
    /// 测试验证分享令牌 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithNonexistentToken_ReturnsFailure()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("分享令牌不存在", result.Message);
        Assert.IsNull(result.ShareToken);
    }

    /// <summary>
    /// 测试验证分享令牌 - 令牌已过期
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // 已过期
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("分享令牌已失效", result.Message);
        Assert.IsNull(result.ShareToken);
    }

    /// <summary>
    /// 测试验证分享令牌 - 令牌未激活
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithInactiveToken_ReturnsFailure()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = false, // 未激活
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("分享令牌已失效", result.Message);
        Assert.IsNull(result.ShareToken);
    }

    /// <summary>
    /// 测试验证分享令牌 - 达到访问次数限制
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithAccessLimitReached_ReturnsFailure()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 100, // 已达到限制
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("分享令牌已失效", result.Message);
        Assert.IsNull(result.ShareToken);
    }

    /// <summary>
    /// 测试验证分享令牌 - 需要密码但未提供
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithPasswordProtectedTokenButNoPassword_ReturnsFailure()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = "hashed-password", // 有密码保护
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("需要访问密码", result.Message);
        Assert.IsNull(result.ShareToken);
    }

    /// <summary>
    /// 测试验证分享令牌 - 密码错误
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = "hashed-correct-password",
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken, "wrong-password");

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("密码错误", result.Message);
        Assert.IsNull(result.ShareToken);
    }

    /// <summary>
    /// 测试验证分享令牌 - 正确密码
    /// </summary>
    [TestMethod]
    public async Task ValidateShareTokenAsync_WithCorrectPassword_ReturnsSuccess()
    {
        // Arrange
        var correctPassword = "correct-password";
        var hashedPassword = HashPassword(correctPassword);
        
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = hashedPassword,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ValidateShareTokenAsync(_testToken, correctPassword);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.Message);
        Assert.IsNotNull(result.ShareToken);
        Assert.AreEqual(_testShareTokenId, result.ShareToken.Id);
    }

    #endregion

    #region 分享令牌撤销测试

    /// <summary>
    /// 测试撤销分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task RevokeShareTokenAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.DeactivateTokenAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RevokeShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);
        _mockShareTokenRepository.Verify(x => x.DeactivateTokenAsync(_testShareTokenId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains(_testToken))), Times.Once);
    }

    /// <summary>
    /// 测试撤销分享令牌 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task RevokeShareTokenAsync_WithNonexistentToken_ThrowsException()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RevokeShareTokenAsync(_testShareTokenId, _testUserId));
        
        Assert.AreEqual("分享令牌不存在", exception.Message);
    }

    /// <summary>
    /// 测试撤销分享令牌 - 无权限
    /// </summary>
    [TestMethod]
    public async Task RevokeShareTokenAsync_WithoutPermission_ThrowsException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId, // 其他用户创建的
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.RevokeShareTokenAsync(_testShareTokenId, _testUserId));
        
        Assert.AreEqual("无权撤销此分享令牌", exception.Message);
    }

    /// <summary>
    /// 测试撤销分享令牌 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task RevokeShareTokenAsync_WithAdminPermission_ReturnsSuccess()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId, // 其他用户创建的
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.DeactivateTokenAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RevokeShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);
        _mockShareTokenRepository.Verify(x => x.DeactivateTokenAsync(_testShareTokenId), Times.Once);
    }

    /// <summary>
    /// 测试撤销分享令牌 - 已经被撤销
    /// </summary>
    [TestMethod]
    public async Task RevokeShareTokenAsync_WithAlreadyRevokedToken_ReturnsTrue()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = false, // 已经被撤销
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.RevokeShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result); // 已经被撤销，返回true
        _mockShareTokenRepository.Verify(x => x.DeactivateTokenAsync(_testShareTokenId), Times.Never); // 不应该再次调用
    }

    #endregion

    #region 权限验证测试

    /// <summary>
    /// 测试检查分享访问权限 - 创建者有权限
    /// </summary>
    [TestMethod]
    public async Task HasShareAccessPermissionAsync_WithCreator_ReturnsTrue()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.HasShareAccessPermissionAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// 测试检查分享访问权限 - 管理员有权限
    /// </summary>
    [TestMethod]
    public async Task HasShareAccessPermissionAsync_WithAdmin_ReturnsTrue()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.HasShareAccessPermissionAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// 测试检查分享访问权限 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task HasShareAccessPermissionAsync_WithNonexistentToken_ReturnsFalse()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.HasShareAccessPermissionAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// 测试检查分享访问权限 - 有效分享令牌
    /// </summary>
    [TestMethod]
    public async Task HasShareAccessPermissionAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.HasShareAccessPermissionAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result); // 有效的分享令牌，任何人都应该有权限
    }

    /// <summary>
    /// 测试检查分享访问权限 - 过期分享令牌
    /// </summary>
    [TestMethod]
    public async Task HasShareAccessPermissionAsync_WithExpiredToken_ReturnsFalse()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // 已过期
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.HasShareAccessPermissionAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion

    #region 异常情况处理测试

    /// <summary>
    /// 测试获取分享令牌 - 缓存命中
    /// </summary>
    [TestMethod]
    public async Task GetShareTokenByTokenAsync_WithCacheHit_ReturnsCachedToken()
    {
        // Arrange
        var cachedToken = new ShareTokenDto
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            HasPassword = false,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockCacheService.Setup(x => x.GetAsync<ShareTokenDto>(It.Is<string>(key => key.Contains(_testToken))))
            .ReturnsAsync(cachedToken);

        // Act
        var result = await _service.GetShareTokenByTokenAsync(_testToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual(_testToken, result.Token);
        
        // 验证不应该调用数据库
        _mockShareTokenRepository.Verify(x => x.GetByTokenAsync(_testToken), Times.Never);
    }

    /// <summary>
    /// 测试获取分享令牌 - 缓存未命中但数据库命中
    /// </summary>
    [TestMethod]
    public async Task GetShareTokenByTokenAsync_WithCacheMissButDatabaseHit_ReturnsTokenAndCaches()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockCacheService.Setup(x => x.GetAsync<ShareTokenDto>(It.Is<string>(key => key.Contains(_testToken))))
            .ReturnsAsync((ShareTokenDto?)null);

        _mockShareTokenRepository.Setup(x => x.GetByTokenAsync(_testToken))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.GetShareTokenByTokenAsync(_testToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual(_testToken, result.Token);
        
        // 验证调用了数据库并缓存了结果
        _mockShareTokenRepository.Verify(x => x.GetByTokenAsync(_testToken), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.Is<string>(key => key.Contains(_testToken)), 
            It.IsAny<ShareTokenDto>(), TimeSpan.FromMinutes(30)), Times.Once);
    }

    /// <summary>
    /// 测试生成分享令牌 - 令牌冲突时重试
    /// </summary>
    [TestMethod]
    public async Task GenerateShareTokenAsync_WithTokenConflict_RetriesAndReturnsUniqueToken()
    {
        // Arrange
        var existingToken = new ShareToken
        {
            Id = Guid.NewGuid(),
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        // 第一次调用返回已存在的令牌，第二次调用返回null（表示新生成的令牌唯一）
        _mockShareTokenRepository.SetupSequence(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(existingToken)
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.GenerateShareTokenAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(_testToken, result); // 应该返回不同的令牌
        
        // 验证调用了两次检查（第一次冲突，第二次成功）
        _mockShareTokenRepository.Verify(x => x.GetByTokenAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    /// <summary>
    /// 测试记录分享访问日志 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task LogShareAccessAsync_WithValidRequest_ReturnsLogId()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        var createdLog = new ShareAccessLog
        {
            Id = Guid.NewGuid(),
            ShareTokenId = _testShareTokenId,
            CodeSnippetId = _testSnippetId,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AccessedAt = DateTime.UtcNow,
            IsSuccess = true,
            Duration = 0,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript",
            CreatorName = "测试用户"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // 使用回调来捕获实际的ShareAccessLog对象
        ShareAccessLog capturedLog = null;
        _mockShareAccessLogRepository.Setup(x => x.CreateAsync(It.IsAny<ShareAccessLog>()))
            .Callback<ShareAccessLog>(log => capturedLog = log)
            .ReturnsAsync(createdLog);

        _mockShareTokenRepository.Setup(x => x.IncrementAccessCountAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.LogShareAccessAsync(_testShareTokenId, "192.168.1.1", "Mozilla/5.0");

        // Assert
        Assert.AreEqual(createdLog.Id, result);
        Assert.IsNotNull(capturedLog);
        Assert.AreEqual(_testShareTokenId, capturedLog.ShareTokenId);
        Assert.AreEqual(_testSnippetId, capturedLog.CodeSnippetId);
        Assert.AreEqual("192.168.1.1", capturedLog.IpAddress);
        Assert.AreEqual("Mozilla/5.0", capturedLog.UserAgent);
        Assert.IsTrue(capturedLog.IsSuccess);
        
        // 验证调用了创建日志和增加访问计数
        _mockShareAccessLogRepository.Verify(x => x.CreateAsync(It.IsAny<ShareAccessLog>()), Times.Once);
        _mockShareTokenRepository.Verify(x => x.IncrementAccessCountAsync(_testShareTokenId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains($"share_stats_{_testShareTokenId}"))), Times.Once);
    }

    /// <summary>
    /// 测试记录分享访问日志 - 访问失败
    /// </summary>
    [TestMethod]
    public async Task LogShareAccessAsync_WithFailedAccess_DoesNotIncrementCount()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        var failedLog = new ShareAccessLog
        {
            Id = Guid.NewGuid(),
            ShareTokenId = _testShareTokenId,
            CodeSnippetId = _testSnippetId,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            AccessedAt = DateTime.UtcNow,
            IsSuccess = false,
            FailureReason = "密码错误",
            Duration = 0,
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript",
            CreatorName = "测试用户"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareAccessLogRepository.Setup(x => x.CreateAsync(It.IsAny<ShareAccessLog>()))
            .ReturnsAsync(failedLog);

        // Act
        var result = await _service.LogShareAccessAsync(_testShareTokenId, "192.168.1.1", "Mozilla/5.0", false, "密码错误");

        // Assert
        Assert.AreEqual(failedLog.Id, result);
        
        // 验证只调用了创建日志，没有增加访问计数
        _mockShareAccessLogRepository.Verify(x => x.CreateAsync(It.Is<ShareAccessLog>(log => 
            log.IsSuccess == false && log.FailureReason == "密码错误")), Times.Once);
        _mockShareTokenRepository.Verify(x => x.IncrementAccessCountAsync(_testShareTokenId), Times.Never);
    }

    /// <summary>
    /// 测试获取分享统计信息 - 缓存命中
    /// </summary>
    [TestMethod]
    public async Task GetShareStatsAsync_WithCacheHit_ReturnsCachedStats()
    {
        // Arrange
        var cachedStats = new ShareStatsDto
        {
            ShareTokenId = _testShareTokenId,
            Token = _testToken,
            TotalAccessCount = 100,
            TodayAccessCount = 10,
            ThisWeekAccessCount = 50,
            ThisMonthAccessCount = 80,
            RemainingAccessCount = 900,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsExpired = false,
            IsAccessLimitReached = false
        };

        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 100,
            MaxAccessCount = 1000,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockCacheService.Setup(x => x.GetAsync<ShareStatsDto>(It.Is<string>(key => key.Contains($"share_stats_{_testShareTokenId}"))))
            .ReturnsAsync(cachedStats);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.GetShareStatsAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.ShareTokenId);
        Assert.AreEqual(100, result.TotalAccessCount);
        Assert.AreEqual(10, result.TodayAccessCount);
        
        // 验证没有调用数据库获取统计信息
        _mockShareTokenRepository.Verify(x => x.GetShareStatsAsync(_testShareTokenId), Times.Never);
    }

    #endregion

    #region 分享令牌获取测试

    /// <summary>
    /// 测试根据ID获取分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetShareTokenByIdAsync_WithValidId_ReturnsShareToken()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.GetShareTokenByIdAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual(_testToken, result.Token);
        Assert.AreEqual(_testSnippetId, result.CodeSnippetId);
        Assert.AreEqual("测试用户", result.CreatorName);
        Assert.AreEqual("测试代码片段", result.CodeSnippetTitle);
        Assert.AreEqual("javascript", result.CodeSnippetLanguage);
    }

    /// <summary>
    /// 测试根据ID获取分享令牌 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task GetShareTokenByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.GetShareTokenByIdAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// 测试根据ID获取分享令牌 - 无权限访问
    /// </summary>
    [TestMethod]
    public async Task GetShareTokenByIdAsync_WithoutPermission_ReturnsNull()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(false);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetShareTokenByIdAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// 测试获取用户分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetUserShareTokensAsync_WithValidUser_ReturnsShareTokens()
    {
        // Arrange
        var shareTokens = new List<ShareToken>
        {
            new ShareToken
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = _testUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 5,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                Password = null,
                AllowDownload = true,
                AllowCopy = true,
                CreatorName = "测试用户",
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        _mockShareTokenRepository.Setup(x => x.GetByUserIdAsync(_testUserId))
            .ReturnsAsync(shareTokens);

        // Act
        var result = await _service.GetUserShareTokensAsync(_testUserId, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(_testShareTokenId, result.First().Id);
    }

    /// <summary>
    /// 测试获取用户分享令牌 - 管理员查看其他用户
    /// </summary>
    [TestMethod]
    public async Task GetUserShareTokensAsync_WithAdminViewingOtherUser_ReturnsShareTokens()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareTokens = new List<ShareToken>
        {
            new ShareToken
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = otherUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 5,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                Password = null,
                AllowDownload = true,
                AllowCopy = true,
                CreatorName = "其他用户",
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        _mockShareTokenRepository.Setup(x => x.GetByUserIdAsync(otherUserId))
            .ReturnsAsync(shareTokens);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GetUserShareTokensAsync(otherUserId, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(_testShareTokenId, result.First().Id);
    }

    /// <summary>
    /// 测试获取用户分享令牌 - 无权限查看其他用户
    /// </summary>
    [TestMethod]
    public async Task GetUserShareTokensAsync_WithoutPermission_ThrowsException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.GetUserShareTokensAsync(otherUserId, _testUserId));
        
        Assert.AreEqual("无权查看其他用户的分享令牌", exception.Message);
    }

    /// <summary>
    /// 测试分页获取用户分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetUserShareTokensPaginatedAsync_WithValidRequest_ReturnsPaginatedResult()
    {
        // Arrange
        var shareTokens = new List<ShareToken>
        {
            new ShareToken
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = _testUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 5,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                Password = null,
                AllowDownload = true,
                AllowCopy = true,
                CreatorName = "测试用户",
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        var pagedResult = new PaginatedResult<ShareToken>
        {
            Items = shareTokens,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockShareTokenRepository.Setup(x => x.GetPagedAsync(It.Is<ShareTokenFilter>(f => 
            f.CreatedBy == _testUserId && f.Page == 1 && f.PageSize == 10)))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetUserShareTokensPaginatedAsync(_testUserId, 1, 10, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.Page);
        Assert.AreEqual(10, result.PageSize);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(_testShareTokenId, result.Items.First().Id);
    }

    #endregion

    #region 分享令牌更新测试

    /// <summary>
    /// 测试更新分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task UpdateShareTokenAsync_WithValidRequest_ReturnsUpdatedToken()
    {
        // Arrange
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新的描述",
            Permission = SharePermission.Edit,
            AllowDownload = false,
            AllowCopy = false,
            MaxAccessCount = 50
        };

        var existingShareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "原始描述",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        var updatedShareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 50,
            Permission = SharePermission.Edit,
            Description = "更新的描述",
            Password = null,
            AllowDownload = false,
            AllowCopy = false,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(existingShareToken);

        _mockValidationService.Setup(x => x.ValidateShareDescription("更新的描述"))
            .Returns(ValidationResult.Success());

        _mockValidationService.Setup(x => x.ValidateShareMaxAccessCount(50))
            .Returns(ValidationResult.Success());

        _mockShareTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<ShareToken>()))
            .ReturnsAsync(updatedShareToken);

        // Act
        var result = await _service.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual("更新的描述", result.Description);
        Assert.AreEqual(SharePermission.Edit, result.Permission);
        Assert.AreEqual(50, result.MaxAccessCount);
        Assert.IsFalse(result.AllowDownload);
        Assert.IsFalse(result.AllowCopy);

        // 验证调用
        _mockShareTokenRepository.Verify(x => x.UpdateAsync(It.Is<ShareToken>(t => 
            t.Description == "更新的描述" && t.Permission == SharePermission.Edit)), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains(_testToken))), Times.Once);
    }

    /// <summary>
    /// 测试更新分享令牌 - 无权限
    /// </summary>
    [TestMethod]
    public async Task UpdateShareTokenAsync_WithoutPermission_ThrowsException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var updateShareDto = new UpdateShareDto
        {
            Description = "更新的描述"
        };

        var existingShareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "原始描述",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(existingShareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId));
        
        Assert.AreEqual("无权修改此分享令牌", exception.Message);
    }

    /// <summary>
    /// 测试更新分享令牌 - 没有变化
    /// </summary>
    [TestMethod]
    public async Task UpdateShareTokenAsync_WithNoChanges_ReturnsOriginalToken()
    {
        // Arrange
        var updateShareDto = new UpdateShareDto
        {
            Description = "原始描述" // 与原始描述相同
        };

        var existingShareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "原始描述",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(existingShareToken);

        _mockValidationService.Setup(x => x.ValidateShareDescription("原始描述"))
            .Returns(ValidationResult.Success());

        // Act
        var result = await _service.UpdateShareTokenAsync(_testShareTokenId, updateShareDto, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual("原始描述", result.Description);

        // 验证没有调用更新
        _mockShareTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<ShareToken>()), Times.Never);
    }

    #endregion

    #region 分享令牌删除测试

    /// <summary>
    /// 测试删除分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task DeleteShareTokenAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.DeleteAsync(_testShareTokenId))
            .ReturnsAsync(true);

        _mockShareAccessLogRepository.Setup(x => x.DeleteByShareTokenIdAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);
        
        // 验证调用
        _mockShareAccessLogRepository.Verify(x => x.DeleteByShareTokenIdAsync(_testShareTokenId), Times.Once);
        _mockShareTokenRepository.Verify(x => x.DeleteAsync(_testShareTokenId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains(_testToken))), Times.Once);
    }

    /// <summary>
    /// 测试删除分享令牌 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task DeleteShareTokenAsync_WithNonexistentToken_ThrowsException()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.DeleteShareTokenAsync(_testShareTokenId, _testUserId));
        
        Assert.AreEqual("分享令牌不存在", exception.Message);
    }

    /// <summary>
    /// 测试删除分享令牌 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task DeleteShareTokenAsync_WithAdminPermission_ReturnsSuccess()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.DeleteAsync(_testShareTokenId))
            .ReturnsAsync(true);

        _mockShareAccessLogRepository.Setup(x => x.DeleteByShareTokenIdAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);
        _mockShareTokenRepository.Verify(x => x.DeleteAsync(_testShareTokenId), Times.Once);
    }

    #endregion

    #region 分享令牌扩展功能测试

    /// <summary>
    /// 测试增加分享访问次数 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task IncrementShareAccessCountAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.IncrementAccessCountAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IncrementShareAccessCountAsync(_testShareTokenId);

        // Assert
        Assert.IsTrue(result);
        _mockShareTokenRepository.Verify(x => x.IncrementAccessCountAsync(_testShareTokenId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains($"share_stats_{_testShareTokenId}"))), Times.Once);
    }

    /// <summary>
    /// 测试增加分享访问次数 - 失败场景
    /// </summary>
    [TestMethod]
    public async Task IncrementShareAccessCountAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.IncrementAccessCountAsync(_testShareTokenId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IncrementShareAccessCountAsync(_testShareTokenId);

        // Assert
        Assert.IsFalse(result);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains($"share_stats_{_testShareTokenId}"))), Times.Never);
    }

    /// <summary>
    /// 测试检查分享令牌是否过期 - 未过期
    /// </summary>
    [TestMethod]
    public async Task IsShareTokenExpiredAsync_WithValidToken_ReturnsFalse()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // 未过期
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.IsShareTokenExpiredAsync(_testShareTokenId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// 测试检查分享令牌是否过期 - 已过期
    /// </summary>
    [TestMethod]
    public async Task IsShareTokenExpiredAsync_WithExpiredToken_ReturnsTrue()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // 已过期
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.IsShareTokenExpiredAsync(_testShareTokenId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// 测试检查分享令牌是否过期 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task IsShareTokenExpiredAsync_WithNonexistentToken_ReturnsTrue()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.IsShareTokenExpiredAsync(_testShareTokenId);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// 测试延长分享令牌有效期 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ExtendShareTokenExpiryAsync_WithValidRequest_ReturnsUpdatedToken()
    {
        // Arrange
        var originalExpiry = DateTime.UtcNow.AddHours(1);
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = originalExpiry,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        var updatedShareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = originalExpiry.AddHours(24), // 延长24小时
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.ExtendTokenExpirationAsync(_testShareTokenId, TimeSpan.FromHours(24)))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(updatedShareToken);

        // Act
        var result = await _service.ExtendShareTokenExpiryAsync(_testShareTokenId, 24, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testShareTokenId, result.Id);
        Assert.AreEqual(updatedShareToken.ExpiresAt, result.ExpiresAt);

        // 验证调用
        _mockShareTokenRepository.Verify(x => x.ExtendTokenExpirationAsync(_testShareTokenId, TimeSpan.FromHours(24)), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains(_testToken))), Times.Once);
    }

    /// <summary>
    /// 测试延长分享令牌有效期 - 无效的小时数
    /// </summary>
    [TestMethod]
    public async Task ExtendShareTokenExpiryAsync_WithInvalidHours_ThrowsException()
    {
        // Arrange - 先设置一个存在的分享令牌
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.ExtendShareTokenExpiryAsync(_testShareTokenId, 0, _testUserId));
        
        Assert.AreEqual("延长时间必须大于0", exception.Message);
    }

    /// <summary>
    /// 测试重置分享访问统计 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ResetShareAccessStatsAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 100, // 有访问记录
            MaxAccessCount = 1000,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<ShareToken>()))
            .ReturnsAsync(shareToken);

        _mockShareAccessLogRepository.Setup(x => x.DeleteByShareTokenIdAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResetShareAccessStatsAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);

        // 验证调用
        _mockShareTokenRepository.Verify(x => x.UpdateAsync(It.Is<ShareToken>(t => 
            t.AccessCount == 0 && t.LastAccessedAt == null)), Times.Once);
        _mockShareAccessLogRepository.Verify(x => x.DeleteByShareTokenIdAsync(_testShareTokenId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains(_testToken))), Times.Once);
    }

    /// <summary>
    /// 测试获取分享URL - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetShareUrlAsync_WithValidToken_ReturnsShareUrl()
    {
        // Arrange
        var expectedUrl = "https://codeshare.example.com/share/test-token-123";

        _mockCacheService.Setup(x => x.GetAsync<string>(It.Is<string>(key => key.Contains($"share_url_{_testToken}"))))
            .ReturnsAsync((string?)null);

        _mockCacheService.Setup(x => x.SetAsync(It.Is<string>(key => key.Contains($"share_url_{_testToken}")), 
            expectedUrl, TimeSpan.FromHours(1)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetShareUrlAsync(_testToken);

        // Assert
        Assert.AreEqual(expectedUrl, result);

        // 验证缓存
        _mockCacheService.Verify(x => x.SetAsync(It.Is<string>(key => key.Contains($"share_url_{_testToken}")), 
            expectedUrl, TimeSpan.FromHours(1)), Times.Once);
    }

    /// <summary>
    /// 测试获取分享URL - 缓存命中
    /// </summary>
    [TestMethod]
    public async Task GetShareUrlAsync_WithCacheHit_ReturnsCachedUrl()
    {
        // Arrange
        var cachedUrl = "https://codeshare.example.com/share/test-token-123";

        _mockCacheService.Setup(x => x.GetAsync<string>(It.Is<string>(key => key.Contains($"share_url_{_testToken}"))))
            .ReturnsAsync(cachedUrl);

        // Act
        var result = await _service.GetShareUrlAsync(_testToken);

        // Assert
        Assert.AreEqual(cachedUrl, result);

        // 验证没有重新缓存
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    /// <summary>
    /// 测试验证分享密码 - 正确密码
    /// </summary>
    [TestMethod]
    public async Task VerifySharePasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var correctPassword = "correct-password";
        var hashedPassword = HashPassword(correctPassword);
        
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = hashedPassword, // SHA256哈希
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.VerifySharePasswordAsync(_testShareTokenId, correctPassword);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// 测试验证分享密码 - 错误密码
    /// </summary>
    [TestMethod]
    public async Task VerifySharePasswordAsync_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = "hashed-correct-password",
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.VerifySharePasswordAsync(_testShareTokenId, "wrong-password");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// 测试验证分享密码 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task VerifySharePasswordAsync_WithNonexistentToken_ReturnsFalse()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.VerifySharePasswordAsync(_testShareTokenId, "any-password");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// 测试验证分享密码 - 无密码保护
    /// </summary>
    [TestMethod]
    public async Task VerifySharePasswordAsync_WithNoPassword_ReturnsFalse()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null, // 无密码
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.VerifySharePasswordAsync(_testShareTokenId, "any-password");

        // Assert
        Assert.IsFalse(result);
    }

    #endregion

    #region 管理员功能测试

    /// <summary>
    /// 测试获取所有分享链接 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetAllSharesAdminAsync_WithAdminPermission_ReturnsShares()
    {
        // Arrange
        var filter = new AdminShareFilter
        {
            Page = 1,
            PageSize = 10
        };

        var pagedResult = new PaginatedResult<ShareTokenDto>
        {
            Items = new List<ShareTokenDto>
            {
                new ShareTokenDto
                {
                    Id = _testShareTokenId,
                    Token = _testToken,
                    CodeSnippetId = _testSnippetId,
                    CreatedBy = _testUserId,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    AccessCount = 5,
                    MaxAccessCount = 100,
                    Permission = SharePermission.ReadOnly,
                    Description = "测试分享",
                    HasPassword = false,
                    AllowDownload = true,
                    AllowCopy = true,
                    CreatorName = "测试用户",
                    CodeSnippetTitle = "测试代码片段",
                    CodeSnippetLanguage = "javascript"
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetAllSharesAdminAsync(filter))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetAllSharesAdminAsync(filter, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(_testShareTokenId, result.Items.First().Id);
    }

    /// <summary>
    /// 测试获取所有分享链接 - 无管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetAllSharesAdminAsync_WithoutAdminPermission_ThrowsException()
    {
        // Arrange
        var filter = new AdminShareFilter
        {
            Page = 1,
            PageSize = 10
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.GetAllSharesAdminAsync(filter, _testUserId));
        
        Assert.AreEqual("无管理员权限", exception.Message);
    }

    /// <summary>
    /// 测试获取系统分享统计 - 管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetSystemShareStatsAsync_WithAdminPermission_ReturnsStats()
    {
        // Arrange
        var expectedStats = new SystemShareStatsDto
        {
            TotalShares = 100,
            ActiveShares = 80,
            ExpiredShares = 20,
            TotalAccessCount = 1000,
            TodayAccessCount = 50,
            ThisWeekAccessCount = 300,
            ThisMonthAccessCount = 800,
            PopularShares = new List<PopularShareDto>
            {
                new PopularShareDto
                {
                    ShareTokenId = _testShareTokenId,
                    CodeSnippetTitle = "测试代码片段",
                    AccessCount = 100
                }
            }
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockCacheService.Setup(x => x.GetAsync<SystemShareStatsDto>("system_share_stats"))
            .ReturnsAsync((SystemShareStatsDto?)null);

        _mockShareTokenRepository.Setup(x => x.GetSystemShareStatsAsync())
            .ReturnsAsync(expectedStats);

        _mockCacheService.Setup(x => x.SetAsync("system_share_stats", expectedStats, TimeSpan.FromMinutes(30)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetSystemShareStatsAsync(_testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(100, result.TotalShares);
        Assert.AreEqual(80, result.ActiveShares);
        Assert.AreEqual(20, result.ExpiredShares);
        Assert.AreEqual(1000, result.TotalAccessCount);
        Assert.AreEqual(1, result.PopularShares.Count());
        Assert.AreEqual(_testShareTokenId, result.PopularShares.First().ShareTokenId);

        // 验证缓存
        _mockCacheService.Verify(x => x.SetAsync("system_share_stats", expectedStats, TimeSpan.FromMinutes(30)), Times.Once);
    }

    /// <summary>
    /// 测试获取系统分享统计 - 缓存命中
    /// </summary>
    [TestMethod]
    public async Task GetSystemShareStatsAsync_WithCacheHit_ReturnsCachedStats()
    {
        // Arrange
        var cachedStats = new SystemShareStatsDto
        {
            TotalShares = 100,
            ActiveShares = 80,
            ExpiredShares = 20,
            TotalAccessCount = 1000,
            TodayAccessCount = 50,
            ThisWeekAccessCount = 300,
            ThisMonthAccessCount = 800,
            PopularShares = new List<PopularShareDto>()
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockCacheService.Setup(x => x.GetAsync<SystemShareStatsDto>("system_share_stats"))
            .ReturnsAsync(cachedStats);

        // Act
        var result = await _service.GetSystemShareStatsAsync(_testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(100, result.TotalShares);
        Assert.AreEqual(80, result.ActiveShares);

        // 验证没有调用数据库
        _mockShareTokenRepository.Verify(x => x.GetSystemShareStatsAsync(), Times.Never);
    }

    /// <summary>
    /// 测试获取系统分享统计 - 无管理员权限
    /// </summary>
    [TestMethod]
    public async Task GetSystemShareStatsAsync_WithoutAdminPermission_ThrowsException()
    {
        // Arrange
        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.GetSystemShareStatsAsync(_testUserId));
        
        Assert.AreEqual("无管理员权限", exception.Message);
    }

    /// <summary>
    /// 测试批量操作分享链接 - 撤销操作
    /// </summary>
    [TestMethod]
    public async Task BulkOperationSharesAsync_WithRevokeOperation_ReturnsSuccessResult()
    {
        // Arrange
        var request = new BulkShareOperationRequest
        {
            ShareTokenIds = new List<Guid> { _testShareTokenId },
            Operation = "revoke"
        };

        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<ShareToken>()))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.BulkOperationSharesAsync(request, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.SuccessCount);
        Assert.AreEqual(0, result.FailureCount);
        Assert.AreEqual(0, result.FailedShareTokenIds.Count());

        // 验证调用
        _mockShareTokenRepository.Verify(x => x.UpdateAsync(It.Is<ShareToken>(t => t.IsActive == false)), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync("system_share_stats"), Times.AtLeastOnce);
    }

    /// <summary>
    /// 测试批量操作分享链接 - 删除操作
    /// </summary>
    [TestMethod]
    public async Task BulkOperationSharesAsync_WithDeleteOperation_ReturnsSuccessResult()
    {
        // Arrange
        var request = new BulkShareOperationRequest
        {
            ShareTokenIds = new List<Guid> { _testShareTokenId },
            Operation = "delete"
        };

        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareAccessLogRepository.Setup(x => x.DeleteByShareTokenIdAsync(_testShareTokenId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.DeleteAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.BulkOperationSharesAsync(request, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.SuccessCount);
        Assert.AreEqual(0, result.FailureCount);

        // 验证调用
        _mockShareAccessLogRepository.Verify(x => x.DeleteByShareTokenIdAsync(_testShareTokenId), Times.Once);
        _mockShareTokenRepository.Verify(x => x.DeleteAsync(_testShareTokenId), Times.Once);
    }

    /// <summary>
    /// 测试批量操作分享链接 - 延长操作
    /// </summary>
    [TestMethod]
    public async Task BulkOperationSharesAsync_WithExtendOperation_ReturnsSuccessResult()
    {
        // Arrange
        var request = new BulkShareOperationRequest
        {
            ShareTokenIds = new List<Guid> { _testShareTokenId },
            Operation = "extend",
            OperationParam = 24
        };

        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.ExtendTokenExpirationAsync(_testShareTokenId, TimeSpan.FromHours(24)))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.BulkOperationSharesAsync(request, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.SuccessCount);
        Assert.AreEqual(0, result.FailureCount);

        // 验证调用
        _mockShareTokenRepository.Verify(x => x.ExtendTokenExpirationAsync(_testShareTokenId, TimeSpan.FromHours(24)), Times.Once);
    }

    /// <summary>
    /// 测试批量操作分享链接 - 无效操作
    /// </summary>
    [TestMethod]
    public async Task BulkOperationSharesAsync_WithInvalidOperation_ReturnsFailureResult()
    {
        // Arrange
        var request = new BulkShareOperationRequest
        {
            ShareTokenIds = new List<Guid> { _testShareTokenId },
            Operation = "invalid_operation"
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.BulkOperationSharesAsync(request, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(0, result.SuccessCount);
        Assert.AreEqual(1, result.FailureCount);
        Assert.AreEqual(1, result.FailedShareTokenIds.Count());
        Assert.AreEqual(_testShareTokenId, result.FailedShareTokenIds.First());
        Assert.AreEqual("操作失败：invalid_operation", result.FailureReasons.First());
    }

    /// <summary>
    /// 测试强制撤销分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ForceRevokeShareTokenAsync_WithAdminPermission_ReturnsSuccess()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = Guid.NewGuid(), // 其他用户创建的
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<ShareToken>()))
            .ReturnsAsync(shareToken);

        // Act
        var result = await _service.ForceRevokeShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);

        // 验证调用
        _mockShareTokenRepository.Verify(x => x.UpdateAsync(It.Is<ShareToken>(t => t.IsActive == false)), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.Is<string>(key => key.Contains($"share_stats_{_testShareTokenId}"))), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync("system_share_stats"), Times.Once);
    }

    /// <summary>
    /// 测试强制撤销分享令牌 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task ForceRevokeShareTokenAsync_WithNonexistentToken_ReturnsFalse()
    {
        // Arrange
        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.ForceRevokeShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// 测试强制删除分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task ForceDeleteShareTokenAsync_WithAdminPermission_ReturnsSuccess()
    {
        // Arrange
        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = Guid.NewGuid(), // 其他用户创建的
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareAccessLogRepository.Setup(x => x.DeleteByShareTokenIdAsync(_testShareTokenId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.DeleteAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ForceDeleteShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsTrue(result);

        // 验证调用
        _mockShareAccessLogRepository.Verify(x => x.DeleteByShareTokenIdAsync(_testShareTokenId), Times.Once);
        _mockShareTokenRepository.Verify(x => x.DeleteAsync(_testShareTokenId), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync("system_share_stats"), Times.Once);
    }

    /// <summary>
    /// 测试强制删除分享令牌 - 令牌不存在
    /// </summary>
    [TestMethod]
    public async Task ForceDeleteShareTokenAsync_WithNonexistentToken_ReturnsFalse()
    {
        // Arrange
        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync((ShareToken?)null);

        // Act
        var result = await _service.ForceDeleteShareTokenAsync(_testShareTokenId, _testUserId);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// 测试清理过期分享令牌 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task CleanupExpiredShareTokensAsync_WithExpiredTokens_ReturnsCleanupCount()
    {
        // Arrange
        var expiredTokens = new List<ShareToken>
        {
            new ShareToken
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = _testUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(-1), // 已过期
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 5,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                Password = null,
                AllowDownload = true,
                AllowCopy = true,
                CreatorName = "测试用户",
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        _mockShareTokenRepository.Setup(x => x.GetExpiredTokensAsync())
            .ReturnsAsync(expiredTokens);

        _mockShareAccessLogRepository.Setup(x => x.DeleteByShareTokenIdAsync(_testShareTokenId))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.DeleteAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CleanupExpiredShareTokensAsync();

        // Assert
        Assert.AreEqual(1, result);

        // 验证调用
        _mockShareAccessLogRepository.Verify(x => x.DeleteByShareTokenIdAsync(_testShareTokenId), Times.Once);
        _mockShareTokenRepository.Verify(x => x.DeleteAsync(_testShareTokenId), Times.Once);
    }

    /// <summary>
    /// 测试清理过期分享令牌 - 无过期令牌
    /// </summary>
    [TestMethod]
    public async Task CleanupExpiredShareTokensAsync_WithNoExpiredTokens_ReturnsZero()
    {
        // Arrange
        _mockShareTokenRepository.Setup(x => x.GetExpiredTokensAsync())
            .ReturnsAsync(new List<ShareToken>());

        // Act
        var result = await _service.CleanupExpiredShareTokensAsync();

        // Assert
        Assert.AreEqual(0, result);

        // 验证没有调用删除
        _mockShareAccessLogRepository.Verify(x => x.DeleteByShareTokenIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockShareTokenRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region 批量和高级功能测试

    /// <summary>
    /// 测试批量记录分享访问日志 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task BulkLogShareAccessAsync_WithValidLogs_ReturnsSuccessCount()
    {
        // Arrange
        var accessLogs = new List<CreateAccessLogDto>
        {
            new CreateAccessLogDto
            {
                ShareTokenId = _testShareTokenId,
                CodeSnippetId = _testSnippetId,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                IsSuccess = true,
                Duration = 100
            },
            new CreateAccessLogDto
            {
                ShareTokenId = _testShareTokenId,
                CodeSnippetId = _testSnippetId,
                IpAddress = "192.168.1.2",
                UserAgent = "Chrome/91.0",
                IsSuccess = true,
                Duration = 150
            }
        };

        _mockShareAccessLogRepository.Setup(x => x.BulkInsertAsync(It.IsAny<List<ShareAccessLog>>()))
            .ReturnsAsync(2);

        _mockShareTokenRepository.Setup(x => x.IncrementAccessCountAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.BulkLogShareAccessAsync(accessLogs);

        // Assert
        Assert.AreEqual(2, result);

        // 验证调用
        _mockShareAccessLogRepository.Verify(x => x.BulkInsertAsync(It.Is<List<ShareAccessLog>>(logs => 
            logs.Count == 2 && logs.All(l => l.IsSuccess))), Times.Once);
        _mockShareTokenRepository.Verify(x => x.IncrementAccessCountAsync(_testShareTokenId), Times.Exactly(2)); // 两次成功的访问
    }

    /// <summary>
    /// 测试批量记录分享访问日志 - 空列表
    /// </summary>
    [TestMethod]
    public async Task BulkLogShareAccessAsync_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var accessLogs = new List<CreateAccessLogDto>();

        // Act
        var result = await _service.BulkLogShareAccessAsync(accessLogs);

        // Assert
        Assert.AreEqual(0, result);

        // 验证没有调用数据库
        _mockShareAccessLogRepository.Verify(x => x.BulkInsertAsync(It.IsAny<List<ShareAccessLog>>()), Times.Never);
    }

    /// <summary>
    /// 测试批量记录分享访问日志 - 混合成功和失败
    /// </summary>
    [TestMethod]
    public async Task BulkLogShareAccessAsync_WithMixedSuccessAndFailure_ReturnsSuccessCount()
    {
        // Arrange
        var accessLogs = new List<CreateAccessLogDto>
        {
            new CreateAccessLogDto
            {
                ShareTokenId = _testShareTokenId,
                CodeSnippetId = _testSnippetId,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                IsSuccess = true,
                Duration = 100
            },
            new CreateAccessLogDto
            {
                ShareTokenId = _testShareTokenId,
                CodeSnippetId = _testSnippetId,
                IpAddress = "192.168.1.2",
                UserAgent = "Chrome/91.0",
                IsSuccess = false, // 失败的访问
                FailureReason = "密码错误",
                Duration = 150
            }
        };

        _mockShareAccessLogRepository.Setup(x => x.BulkInsertAsync(It.IsAny<List<ShareAccessLog>>()))
            .ReturnsAsync(2);

        _mockShareTokenRepository.Setup(x => x.IncrementAccessCountAsync(_testShareTokenId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.BulkLogShareAccessAsync(accessLogs);

        // Assert
        Assert.AreEqual(2, result);

        // 验证只对成功的访问增加计数
        _mockShareTokenRepository.Verify(x => x.IncrementAccessCountAsync(_testShareTokenId), Times.Once); // 只有一次成功的访问
    }

    /// <summary>
    /// 测试获取分享访问日志 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogsAsync_WithValidFilter_ReturnsPaginatedLogs()
    {
        // Arrange
        var filter = new AccessLogFilter
        {
            ShareTokenId = _testShareTokenId,
            Page = 1,
            PageSize = 10
        };

        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = _testUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "测试用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        var accessLogs = new List<ShareAccessLog>
        {
            new ShareAccessLog
            {
                Id = Guid.NewGuid(),
                ShareTokenId = _testShareTokenId,
                CodeSnippetId = _testSnippetId,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                AccessedAt = DateTime.UtcNow,
                IsSuccess = true,
                Duration = 100,
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript",
                CreatorName = "测试用户"
            }
        };

        var pagedResult = new PaginatedResult<ShareAccessLog>
        {
            Items = accessLogs,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockShareAccessLogRepository.Setup(x => x.GetPagedAsync(filter))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetShareAccessLogsAsync(filter, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(_testShareTokenId, result.Items.First().ShareTokenId);
        Assert.AreEqual("192.168.1.1", result.Items.First().IpAddress);
    }

    /// <summary>
    /// 测试获取分享访问日志 - 无权限
    /// </summary>
    [TestMethod]
    public async Task GetShareAccessLogsAsync_WithoutPermission_ThrowsException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var filter = new AccessLogFilter
        {
            ShareTokenId = _testShareTokenId,
            Page = 1,
            PageSize = 10
        };

        var shareToken = new ShareToken
        {
            Id = _testShareTokenId,
            Token = _testToken,
            CodeSnippetId = _testSnippetId,
            CreatedBy = otherUserId,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            AccessCount = 5,
            MaxAccessCount = 100,
            Permission = SharePermission.ReadOnly,
            Description = "测试分享",
            Password = null,
            AllowDownload = true,
            AllowCopy = true,
            CreatorName = "其他用户",
            CodeSnippetTitle = "测试代码片段",
            CodeSnippetLanguage = "javascript"
        };

        _mockShareTokenRepository.Setup(x => x.GetByIdAsync(_testShareTokenId))
            .ReturnsAsync(shareToken);

        _mockPermissionService.Setup(x => x.IsAdminAsync(_testUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.GetShareAccessLogsAsync(filter, _testUserId));
        
        Assert.AreEqual("无权查看此分享访问日志", exception.Message);
    }

    /// <summary>
    /// 测试获取代码片段分享记录 - 成功场景
    /// </summary>
    [TestMethod]
    public async Task GetSnippetSharesAsync_WithValidSnippet_ReturnsShareTokens()
    {
        // Arrange
        var shareTokens = new List<ShareToken>
        {
            new ShareToken
            {
                Id = _testShareTokenId,
                Token = _testToken,
                CodeSnippetId = _testSnippetId,
                CreatedBy = _testUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 5,
                MaxAccessCount = 100,
                Permission = SharePermission.ReadOnly,
                Description = "测试分享",
                Password = null,
                AllowDownload = true,
                AllowCopy = true,
                CreatorName = "测试用户",
                CodeSnippetTitle = "测试代码片段",
                CodeSnippetLanguage = "javascript"
            }
        };

        _mockPermissionService.Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(true);

        _mockShareTokenRepository.Setup(x => x.GetByCodeSnippetIdAsync(_testSnippetId))
            .ReturnsAsync(shareTokens);

        // Act
        var result = await _service.GetSnippetSharesAsync(_testSnippetId, _testUserId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(_testShareTokenId, result.First().Id);
    }

    /// <summary>
    /// 测试获取代码片段分享记录 - 无权限
    /// </summary>
    [TestMethod]
    public async Task GetSnippetSharesAsync_WithoutPermission_ThrowsException()
    {
        // Arrange
        _mockPermissionService.Setup(x => x.CanAccessSnippetAsync(_testUserId, _testSnippetId, PermissionOperation.Read))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _service.GetSnippetSharesAsync(_testSnippetId, _testUserId));
        
        Assert.AreEqual("无权查看此代码片段的分享记录", exception.Message);
    }

    #endregion
}