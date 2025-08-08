using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CodeSnippetManager.Api.Services;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 输入验证服务单元测试 - 测试面向接口的设计
/// </summary>
[TestClass]
public class InputValidationServiceTests
{
    private readonly Mock<ILogger<InputValidationService>> _mockLogger;
    private readonly InputValidationService _service;

    public InputValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<InputValidationService>>();
        _service = new InputValidationService(_mockLogger.Object);
    }

    #region Email Validation Tests

    [TestMethod]
    public void ValidateEmail_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = _service.ValidateEmail(email);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateEmail_EmptyEmail_ReturnsFailure()
    {
        // Arrange
        var email = "";

        // Act
        var result = _service.ValidateEmail(email);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("邮箱地址不能为空", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateEmail_InvalidFormat_ReturnsFailure()
    {
        // Arrange
        var email = "invalid-email";

        // Act
        var result = _service.ValidateEmail(email);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("邮箱地址格式无效", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateEmail_TooLong_ReturnsFailure()
    {
        // Arrange
        var email = new string('a', 250) + "@example.com";

        // Act
        var result = _service.ValidateEmail(email);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("邮箱地址长度不能超过254个字符", result.ErrorMessage);
    }

    #endregion

    #region Username Validation Tests

    [TestMethod]
    public void ValidateUsername_ValidUsername_ReturnsSuccess()
    {
        // Arrange
        var username = "testuser123";

        // Act
        var result = _service.ValidateUsername(username);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateUsername_EmptyUsername_ReturnsFailure()
    {
        // Arrange
        var username = "";

        // Act
        var result = _service.ValidateUsername(username);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("用户名不能为空", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateUsername_InvalidCharacters_ReturnsFailure()
    {
        // Arrange
        var username = "test@user";

        // Act
        var result = _service.ValidateUsername(username);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("用户名只能包含字母、数字、下划线和连字符，长度为3-30个字符", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateUsername_TooShort_ReturnsFailure()
    {
        // Arrange
        var username = "ab";

        // Act
        var result = _service.ValidateUsername(username);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("用户名只能包含字母、数字、下划线和连字符，长度为3-30个字符", result.ErrorMessage);
    }

    #endregion

    #region Password Validation Tests

    [TestMethod]
    public void ValidatePassword_ValidPassword_ReturnsSuccess()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var result = _service.ValidatePassword(password);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ValidatePassword_EmptyPassword_ReturnsFailure()
    {
        // Arrange
        var password = "";

        // Act
        var result = _service.ValidatePassword(password);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("密码不能为空", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidatePassword_TooShort_ReturnsFailure()
    {
        // Arrange
        var password = "Pass1!";

        // Act
        var result = _service.ValidatePassword(password);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("密码长度至少为8个字符", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidatePassword_NoUppercase_ReturnsFailure()
    {
        // Arrange
        var password = "password123!";

        // Act
        var result = _service.ValidatePassword(password);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("密码必须包含至少一个大写字母、一个小写字母、一个数字和一个特殊字符", result.ErrorMessage);
    }

    #endregion

    #region Code Content Validation Tests

    [TestMethod]
    public void ValidateCodeContent_ValidCode_ReturnsSuccess()
    {
        // Arrange
        var code = "console.log('Hello World');";

        // Act
        var result = _service.ValidateCodeContent(code);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateCodeContent_EmptyCode_ReturnsFailure()
    {
        // Arrange
        var code = "";

        // Act
        var result = _service.ValidateCodeContent(code);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("代码内容不能为空", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateCodeContent_TooLarge_ReturnsFailure()
    {
        // Arrange
        var code = new string('a', 100001);

        // Act
        var result = _service.ValidateCodeContent(code);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("代码内容长度不能超过100KB", result.ErrorMessage);
    }

    #endregion

    #region Language Validation Tests

    [TestMethod]
    public void ValidateLanguage_ValidLanguage_ReturnsSuccess()
    {
        // Arrange
        var language = "javascript";

        // Act
        var result = _service.ValidateLanguage(language);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateLanguage_UnsupportedLanguage_ReturnsFailure()
    {
        // Arrange
        var language = "unsupported";

        // Act
        var result = _service.ValidateLanguage(language);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("不支持的编程语言: unsupported", result.ErrorMessage);
    }

    #endregion

    #region Tag Validation Tests

    [TestMethod]
    public void ValidateTagName_ValidTag_ReturnsSuccess()
    {
        // Arrange
        var tagName = "frontend";

        // Act
        var result = _service.ValidateTagName(tagName);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateTagName_EmptyTag_ReturnsFailure()
    {
        // Arrange
        var tagName = "";

        // Act
        var result = _service.ValidateTagName(tagName);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("标签名称不能为空", result.ErrorMessage);
    }

    [TestMethod]
    public void ValidateTagName_InvalidCharacters_ReturnsFailure()
    {
        // Arrange
        var tagName = "tag@name";

        // Act
        var result = _service.ValidateTagName(tagName);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("标签名称只能包含字母、数字、中文、连字符和下划线", result.ErrorMessage);
    }

    #endregion

    #region Sanitization Tests

    [TestMethod]
    public void SanitizeHtml_RemovesHtmlTags()
    {
        // Arrange
        var input = "<script>alert('xss')</script>Hello World";

        // Act
        var result = _service.SanitizeHtml(input);

        // Assert
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void SanitizeHtml_RemovesDangerousPatterns()
    {
        // Arrange
        var input = "javascript:alert('xss')";

        // Act
        var result = _service.SanitizeHtml(input);

        // Assert
        Assert.IsFalse(result.Contains("javascript:"));
    }

    [TestMethod]
    public void SanitizeUserInput_NormalizesWhitespace()
    {
        // Arrange
        var input = "  Hello    World  ";

        // Act
        var result = _service.SanitizeUserInput(input);

        // Assert
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void SanitizeUserInput_RemovesControlCharacters()
    {
        // Arrange
        var input = "Hello\x00\x01World";

        // Act
        var result = _service.SanitizeUserInput(input);

        // Assert
        Assert.AreEqual("HelloWorld", result);
    }

    #endregion
}