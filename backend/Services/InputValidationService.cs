using System.Text.RegularExpressions;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 输入验证服务 - 提供统一的输入验证和数据清理功能
/// </summary>
public class InputValidationService : IInputValidationService
{
    private readonly ILogger<InputValidationService> _logger;

    // 常用的验证正则表达式
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9_-]{3,30}$", 
        RegexOptions.Compiled);
    
    private static readonly Regex PasswordRegex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        RegexOptions.Compiled);
    
    private static readonly Regex HtmlTagRegex = new(@"<[^>]*>", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex SqlInjectionRegex = new(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE){0,1}|INSERT( +INTO){0,1}|MERGE|SELECT|UPDATE|UNION( +ALL){0,1})\b)", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // 危险字符模式
    private static readonly string[] DangerousPatterns = new[]
    {
        "<script", "</script>", "javascript:", "vbscript:", "onload=", "onerror=", 
        "onclick=", "onmouseover=", "eval(", "expression(", "url(", "import("
    };

    public InputValidationService(ILogger<InputValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 验证邮箱格式
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValidationResult.Failure("邮箱地址不能为空");
        }

        if (email.Length > 254)
        {
            return ValidationResult.Failure("邮箱地址长度不能超过254个字符");
        }

        if (!EmailRegex.IsMatch(email))
        {
            return ValidationResult.Failure("邮箱地址格式无效");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证用户名格式
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ValidationResult.Failure("用户名不能为空");
        }

        if (!UsernameRegex.IsMatch(username))
        {
            return ValidationResult.Failure("用户名只能包含字母、数字、下划线和连字符，长度为3-30个字符");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证密码强度
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ValidationResult.Failure("密码不能为空");
        }

        if (password.Length < 8)
        {
            return ValidationResult.Failure("密码长度至少为8个字符");
        }

        if (password.Length > 128)
        {
            return ValidationResult.Failure("密码长度不能超过128个字符");
        }

        if (!PasswordRegex.IsMatch(password))
        {
            return ValidationResult.Failure("密码必须包含至少一个大写字母、一个小写字母、一个数字和一个特殊字符");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证代码片段标题
    /// </summary>
    /// <param name="title">标题</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateSnippetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return ValidationResult.Failure("代码片段标题不能为空");
        }

        if (title.Length > 200)
        {
            return ValidationResult.Failure("代码片段标题长度不能超过200个字符");
        }

        if (ContainsDangerousContent(title))
        {
            return ValidationResult.Failure("代码片段标题包含不安全的内容");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证代码片段描述
    /// </summary>
    /// <param name="description">描述</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateSnippetDescription(string? description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return ValidationResult.Success(); // 描述可以为空
        }

        if (description.Length > 1000)
        {
            return ValidationResult.Failure("代码片段描述长度不能超过1000个字符");
        }

        if (ContainsDangerousContent(description))
        {
            return ValidationResult.Failure("代码片段描述包含不安全的内容");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证代码内容
    /// </summary>
    /// <param name="code">代码内容</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateCodeContent(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ValidationResult.Failure("代码内容不能为空");
        }

        if (code.Length > 100000) // 100KB限制
        {
            return ValidationResult.Failure("代码内容长度不能超过100KB");
        }

        // 检查是否包含SQL注入模式（对于SQL代码片段可能误报，需要特殊处理）
        if (ContainsSqlInjection(code))
        {
            _logger.LogWarning("代码内容可能包含SQL注入模式，需要人工审核");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证编程语言
    /// </summary>
    /// <param name="language">编程语言</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return ValidationResult.Failure("编程语言不能为空");
        }

        if (language.Length > 50)
        {
            return ValidationResult.Failure("编程语言名称长度不能超过50个字符");
        }

        // 允许的编程语言列表
        var allowedLanguages = new[]
        {
            "javascript", "typescript", "python", "java", "csharp", "cpp", "c", 
            "go", "rust", "php", "ruby", "swift", "kotlin", "scala", "html", 
            "css", "scss", "less", "sql", "bash", "powershell", "yaml", "json", 
            "xml", "markdown", "dockerfile", "nginx", "apache", "regex", "text"
        };

        if (!allowedLanguages.Contains(language.ToLower()))
        {
            return ValidationResult.Failure($"不支持的编程语言: {language}");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证标签名称
    /// </summary>
    /// <param name="tagName">标签名称</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateTagName(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return ValidationResult.Failure("标签名称不能为空");
        }

        if (tagName.Length > 50)
        {
            return ValidationResult.Failure("标签名称长度不能超过50个字符");
        }

        if (ContainsDangerousContent(tagName))
        {
            return ValidationResult.Failure("标签名称包含不安全的内容");
        }

        // 标签名称只允许字母、数字、中文、连字符和下划线
        if (!Regex.IsMatch(tagName, @"^[\w\u4e00-\u9fa5-]+$"))
        {
            return ValidationResult.Failure("标签名称只能包含字母、数字、中文、连字符和下划线");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证分享令牌描述
    /// </summary>
    /// <param name="description">分享描述</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateShareDescription(string? description)
    {
        if (description == null)
        {
            return ValidationResult.Success(); // 描述可以为空
        }

        if (description.Length > 500)
        {
            return ValidationResult.Failure("分享描述长度不能超过500个字符");
        }

        if (ContainsDangerousContent(description))
        {
            return ValidationResult.Failure("分享描述包含不安全的内容");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证分享密码
    /// </summary>
    /// <param name="password">分享密码</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateSharePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ValidationResult.Failure("分享密码不能为空");
        }

        if (password.Length < 6 || password.Length > 64)
        {
            return ValidationResult.Failure("分享密码长度必须在6-64个字符之间");
        }

        if (ContainsDangerousContent(password))
        {
            return ValidationResult.Failure("分享密码包含不安全的内容");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证分享有效期（小时）
    /// </summary>
    /// <param name="hours">有效期小时数</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateShareExpiryHours(int hours)
    {
        if (hours < 0)
        {
            return ValidationResult.Failure("有效期不能为负数");
        }

        if (hours > 8760) // 365天 * 24小时
        {
            return ValidationResult.Failure("有效期不能超过365天（8760小时）");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 验证分享访问次数限制
    /// </summary>
    /// <param name="maxAccessCount">最大访问次数</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateShareMaxAccessCount(int maxAccessCount)
    {
        if (maxAccessCount < 0)
        {
            return ValidationResult.Failure("访问次数限制不能为负数");
        }

        if (maxAccessCount > 10000)
        {
            return ValidationResult.Failure("访问次数限制不能超过10000次");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 清理HTML内容
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>清理后的内容</returns>
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // 移除HTML标签
        var sanitized = HtmlTagRegex.Replace(input, string.Empty);
        
        // HTML解码
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);
        
        // 移除危险字符
        foreach (var pattern in DangerousPatterns)
        {
            sanitized = sanitized.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return sanitized.Trim();
    }

    /// <summary>
    /// 清理用户输入
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>清理后的内容</returns>
    public string SanitizeUserInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // 移除控制字符
        var sanitized = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);
        
        // 标准化空白字符
        sanitized = Regex.Replace(sanitized, @"\s+", " ");
        
        // 移除前后空白
        sanitized = sanitized.Trim();

        return sanitized;
    }

    /// <summary>
    /// 检查是否包含危险内容
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>是否包含危险内容</returns>
    private bool ContainsDangerousContent(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var lowerInput = input.ToLower();
        
        foreach (var pattern in DangerousPatterns)
        {
            if (lowerInput.Contains(pattern.ToLower()))
            {
                _logger.LogWarning("检测到危险内容模式: {Pattern} 在输入: {Input}", pattern, input);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否包含SQL注入模式
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>是否包含SQL注入模式</returns>
    private bool ContainsSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        return SqlInjectionRegex.IsMatch(input);
    }
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
}