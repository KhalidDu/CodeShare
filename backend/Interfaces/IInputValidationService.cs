using CodeSnippetManager.Api.Services;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 输入验证服务接口 - 提供统一的输入验证和数据清理功能
/// </summary>
public interface IInputValidationService
{
    /// <summary>
    /// 验证邮箱格式
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateEmail(string email);

    /// <summary>
    /// 验证用户名格式
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateUsername(string username);

    /// <summary>
    /// 验证密码强度
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidatePassword(string password);

    /// <summary>
    /// 验证代码片段标题
    /// </summary>
    /// <param name="title">标题</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateSnippetTitle(string title);

    /// <summary>
    /// 验证代码片段描述
    /// </summary>
    /// <param name="description">描述</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateSnippetDescription(string? description);

    /// <summary>
    /// 验证代码内容
    /// </summary>
    /// <param name="code">代码内容</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateCodeContent(string code);

    /// <summary>
    /// 验证编程语言
    /// </summary>
    /// <param name="language">编程语言</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateLanguage(string language);

    /// <summary>
    /// 验证标签名称
    /// </summary>
    /// <param name="tagName">标签名称</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateTagName(string tagName);

    /// <summary>
    /// 验证分享令牌描述
    /// </summary>
    /// <param name="description">分享描述</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateShareDescription(string? description);

    /// <summary>
    /// 验证分享密码
    /// </summary>
    /// <param name="password">分享密码</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateSharePassword(string password);

    /// <summary>
    /// 验证分享有效期（小时）
    /// </summary>
    /// <param name="hours">有效期小时数</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateShareExpiryHours(int hours);

    /// <summary>
    /// 验证分享访问次数限制
    /// </summary>
    /// <param name="maxAccessCount">最大访问次数</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateShareMaxAccessCount(int maxAccessCount);

    /// <summary>
    /// 清理HTML内容
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>清理后的内容</returns>
    string SanitizeHtml(string input);

    /// <summary>
    /// 清理用户输入
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>清理后的内容</returns>
    string SanitizeUserInput(string input);
}