using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 系统设置DTO - 主要数据传输对象
/// </summary>
public class SystemSettingsDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public SiteSettingsDto SiteSettings { get; set; } = new();
    public SecuritySettingsDto SecuritySettings { get; set; } = new();
    public FeatureSettingsDto FeatureSettings { get; set; } = new();
    public EmailSettingsDto EmailSettings { get; set; } = new();
}

/// <summary>
/// 站点设置DTO
/// </summary>
public class SiteSettingsDto
{
    public string SiteName { get; set; } = "代码片段管理";
    public string SiteDescription { get; set; } = "专业的代码片段管理平台";
    public string LogoUrl { get; set; } = "/logo.png";
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "zh-CN";
    public int PageSize { get; set; } = 20;
    public bool AllowRegistration { get; set; } = true;
    public string Announcement { get; set; } = "";
    public string CustomCss { get; set; } = "";
    public string CustomJs { get; set; } = "";
}

/// <summary>
/// 安全设置DTO
/// </summary>
public class SecuritySettingsDto
{
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialChars { get; set; } = true;
    public int MaxLoginAttempts { get; set; } = 5;
    public int AccountLockoutDuration { get; set; } = 30;
    public int SessionTimeout { get; set; } = 120;
    public bool EnableTwoFactorAuth { get; set; } = false;
    public bool EnableCors { get; set; } = true;
    public string[] AllowedCorsOrigins { get; set; } = Array.Empty<string>();
    public bool EnableHttpsRedirection { get; set; } = true;
    public int ApiRateLimit { get; set; } = 100;
    public bool EnableLoginLogging { get; set; } = true;
    public bool EnableActionLogging { get; set; } = true;
}

/// <summary>
/// 功能设置DTO
/// </summary>
public class FeatureSettingsDto
{
    public bool EnableCodeSnippets { get; set; } = true;
    public bool EnableSharing { get; set; } = true;
    public bool EnableTags { get; set; } = true;
    public bool EnableComments { get; set; } = true;
    public bool EnableFavorites { get; set; } = true;
    public bool EnableSearch { get; set; } = true;
    public bool EnableExport { get; set; } = true;
    public bool EnableImport { get; set; } = true;
    public bool EnableApi { get; set; } = true;
    public bool EnableWebHooks { get; set; } = false;
    public bool EnableFileUpload { get; set; } = true;
    public int MaxFileSize { get; set; } = 10;
    public string[] AllowedFileTypes { get; set; } = new[] { ".txt", ".md", ".json", ".xml", ".csv" };
    public bool EnableRealTimeNotifications { get; set; } = true;
    public bool EnableAnalytics { get; set; } = false;
}

/// <summary>
/// 邮件设置DTO
/// </summary>
public class EmailSettingsDto
{
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";
    public bool EnableSsl { get; set; } = true;
    public bool EnableTls { get; set; } = true;
    public string TemplatePath { get; set; } = "/templates/emails";
    public bool EnableEmailQueue { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int EmailTimeout { get; set; } = 30;
    public bool EnableEmailLogging { get; set; } = true;
    public string TestEmailRecipient { get; set; } = "";
}

/// <summary>
/// 更新站点设置请求DTO
/// </summary>
public class UpdateSiteSettingsRequest
{
    public string? SiteName { get; set; }
    public string? SiteDescription { get; set; }
    public string? LogoUrl { get; set; }
    public string? Theme { get; set; }
    public string? Language { get; set; }
    public int? PageSize { get; set; }
    public bool? AllowRegistration { get; set; }
    public string? Announcement { get; set; }
    public string? CustomCss { get; set; }
    public string? CustomJs { get; set; }
}

/// <summary>
/// 更新安全设置请求DTO
/// </summary>
public class UpdateSecuritySettingsRequest
{
    public int? MinPasswordLength { get; set; }
    public bool? RequireUppercase { get; set; }
    public bool? RequireLowercase { get; set; }
    public bool? RequireNumbers { get; set; }
    public bool? RequireSpecialChars { get; set; }
    public int? MaxLoginAttempts { get; set; }
    public int? AccountLockoutDuration { get; set; }
    public int? SessionTimeout { get; set; }
    public bool? EnableTwoFactorAuth { get; set; }
    public bool? EnableCors { get; set; }
    public string[]? AllowedCorsOrigins { get; set; }
    public bool? EnableHttpsRedirection { get; set; }
    public int? ApiRateLimit { get; set; }
    public bool? EnableLoginLogging { get; set; }
    public bool? EnableActionLogging { get; set; }
}

/// <summary>
/// 更新功能设置请求DTO
/// </summary>
public class UpdateFeatureSettingsRequest
{
    public bool? EnableCodeSnippets { get; set; }
    public bool? EnableSharing { get; set; }
    public bool? EnableTags { get; set; }
    public bool? EnableComments { get; set; }
    public bool? EnableFavorites { get; set; }
    public bool? EnableSearch { get; set; }
    public bool? EnableExport { get; set; }
    public bool? EnableImport { get; set; }
    public bool? EnableApi { get; set; }
    public bool? EnableWebHooks { get; set; }
    public bool? EnableFileUpload { get; set; }
    public int? MaxFileSize { get; set; }
    public string[]? AllowedFileTypes { get; set; }
    public bool? EnableRealTimeNotifications { get; set; }
    public bool? EnableAnalytics { get; set; }
}

/// <summary>
/// 更新邮件设置请求DTO
/// </summary>
public class UpdateEmailSettingsRequest
{
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public bool? EnableSsl { get; set; }
    public bool? EnableTls { get; set; }
    public string? TemplatePath { get; set; }
    public bool? EnableEmailQueue { get; set; }
    public int? MaxRetryAttempts { get; set; }
    public int? EmailTimeout { get; set; }
    public bool? EnableEmailLogging { get; set; }
    public string? TestEmailRecipient { get; set; }
}

/// <summary>
/// 设置验证结果DTO
/// </summary>
public class SettingsValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public Dictionary<string, string[]> Warnings { get; set; } = new();
}

/// <summary>
/// 测试邮件发送请求DTO
/// </summary>
public class TestEmailRequest
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = "系统设置测试邮件";
    public string Body { get; set; } = "这是一封测试邮件，用于验证邮件设置配置是否正确。";
}

/// <summary>
/// 测试邮件发送响应DTO
/// </summary>
public class TestEmailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string? ErrorDetails { get; set; }
}