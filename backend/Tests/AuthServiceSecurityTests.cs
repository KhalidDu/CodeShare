using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CodeSnippetManager.Api.Services;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 认证服务安全测试 - 测试面向接口的安全实现
/// </summary>
[TestClass]
public class AuthServiceSecurityTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IInputValidationService> _mockValidationService;
    private readonly AuthService _authService;

    public AuthServiceSecurityTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockValidationService = new Mock<IInputValidationService>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["JwtSettings:SecretKey"])
            .Returns("test-secret-key-that-is-at-least-32-characters-long");

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockConfiguration.Object,
            _mockValidationService.Object);
    }

    #region Login Security Tests

    [TestMethod]
    public async Task LoginAsync_ValidatesUsername()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "password" };
        
        _mockValidationService.Setup(v => v.ValidateUsername(loginDto.Username))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.SanitizeUserInput(loginDto.Username))
            .Returns(loginDto.Username);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            Role = UserRole.Viewer
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(loginDto.Username))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        _mockValidationService.Verify(v => v.ValidateUsername(loginDto.Username), Times.Once);
        _mockValidationService.Verify(v => v.SanitizeUserInput(loginDto.Username), Times.Once);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Token);
    }

    [TestMethod]
    public async Task LoginAsync_InvalidUsername_ThrowsException()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "invalid@user", Password = "password" };
        
        _mockValidationService.Setup(v => v.ValidateUsername(loginDto.Username))
            .Returns(ValidationResult.Failure("用户名格式无效"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _authService.LoginAsync(loginDto));

        _mockValidationService.Verify(v => v.ValidateUsername(loginDto.Username), Times.Once);
        _mockUserRepository.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task LoginAsync_SanitizesInput()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "  testuser  ", Password = "password" };
        var sanitizedUsername = "testuser";
        
        _mockValidationService.Setup(v => v.ValidateUsername(loginDto.Username))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.SanitizeUserInput(loginDto.Username))
            .Returns(sanitizedUsername);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = sanitizedUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            Role = UserRole.Viewer
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(sanitizedUsername))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        _mockUserRepository.Verify(r => r.GetByUsernameAsync(sanitizedUsername), Times.Once);
        Assert.IsNotNull(result);
    }

    #endregion

    #region Registration Security Tests

    [TestMethod]
    public async Task RegisterAsync_ValidatesAllInputs()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockValidationService.Setup(v => v.ValidateUsername(registerDto.Username))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidateEmail(registerDto.Email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidatePassword(registerDto.Password))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.SanitizeUserInput(It.IsAny<string>()))
            .Returns<string>(input => input);

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        _mockValidationService.Verify(v => v.ValidateUsername(registerDto.Username), Times.Once);
        _mockValidationService.Verify(v => v.ValidateEmail(registerDto.Email), Times.Once);
        _mockValidationService.Verify(v => v.ValidatePassword(registerDto.Password), Times.Once);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task RegisterAsync_InvalidEmail_ThrowsException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "Password123!"
        };

        _mockValidationService.Setup(v => v.ValidateUsername(registerDto.Username))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidateEmail(registerDto.Email))
            .Returns(ValidationResult.Failure("邮箱格式无效"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _authService.RegisterAsync(registerDto));

        _mockValidationService.Verify(v => v.ValidateEmail(registerDto.Email), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_WeakPassword_ThrowsException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "weak"
        };

        _mockValidationService.Setup(v => v.ValidateUsername(registerDto.Username))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidateEmail(registerDto.Email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidatePassword(registerDto.Password))
            .Returns(ValidationResult.Failure("密码强度不足"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _authService.RegisterAsync(registerDto));

        _mockValidationService.Verify(v => v.ValidatePassword(registerDto.Password), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_SanitizesInputs()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "  testuser  ",
            Email = "  test@example.com  ",
            Password = "Password123!"
        };

        var sanitizedUsername = "testuser";
        var sanitizedEmail = "test@example.com";

        _mockValidationService.Setup(v => v.ValidateUsername(registerDto.Username))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidateEmail(registerDto.Email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.ValidatePassword(registerDto.Password))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(v => v.SanitizeUserInput(registerDto.Username))
            .Returns(sanitizedUsername);
        _mockValidationService.Setup(v => v.SanitizeUserInput(registerDto.Email))
            .Returns(sanitizedEmail);

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(sanitizedUsername))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByEmailAsync(sanitizedEmail))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        _mockUserRepository.Verify(r => r.GetByUsernameAsync(sanitizedUsername), Times.Once);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(sanitizedEmail), Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(sanitizedUsername, result.Username);
        Assert.AreEqual(sanitizedEmail, result.Email);
    }

    #endregion

    #region Password Change Security Tests

    [TestMethod]
    public async Task ChangePasswordAsync_ValidatesNewPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";

        var user = new User
        {
            Id = userId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword)
        };

        _mockValidationService.Setup(v => v.ValidatePassword(newPassword))
            .Returns(ValidationResult.Success());
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        _mockValidationService.Verify(v => v.ValidatePassword(newPassword), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_WeakNewPassword_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "OldPassword123!";
        var newPassword = "weak";

        _mockValidationService.Setup(v => v.ValidatePassword(newPassword))
            .Returns(ValidationResult.Failure("密码强度不足"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _authService.ChangePasswordAsync(userId, currentPassword, newPassword));

        _mockValidationService.Verify(v => v.ValidatePassword(newPassword), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_IncorrectCurrentPassword_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "WrongPassword";
        var newPassword = "NewPassword123!";

        var user = new User
        {
            Id = userId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!")
        };

        _mockValidationService.Setup(v => v.ValidatePassword(newPassword))
            .Returns(ValidationResult.Success());
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => _authService.ChangePasswordAsync(userId, currentPassword, newPassword));

        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion
}