using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 系统设置主实体 - 遵循单一职责原则，负责系统设置的统一管理
/// </summary>
public class SystemSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// 站点设置JSON数据
    /// </summary>
    public string SiteSettingsJson { get; set; } = "{}";
    
    /// <summary>
    /// 安全设置JSON数据
    /// </summary>
    public string SecuritySettingsJson { get; set; } = "{}";
    
    /// <summary>
    /// 功能设置JSON数据
    /// </summary>
    public string FeatureSettingsJson { get; set; } = "{}";
    
    /// <summary>
    /// 邮件设置JSON数据
    /// </summary>
    public string EmailSettingsJson { get; set; } = "{}";
    
    // JSON序列化选项
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
    
    /// <summary>
    /// 获取站点设置
    /// </summary>
    public SiteSettings GetSiteSettings()
    {
        try
        {
            return string.IsNullOrEmpty(SiteSettingsJson) 
                ? new SiteSettings() 
                : JsonSerializer.Deserialize<SiteSettings>(SiteSettingsJson, JsonSerializerOptions) ?? new SiteSettings();
        }
        catch
        {
            return new SiteSettings();
        }
    }
    
    /// <summary>
    /// 设置站点设置
    /// </summary>
    public void SetSiteSettings(SiteSettings settings)
    {
        SiteSettingsJson = JsonSerializer.Serialize(settings, JsonSerializerOptions);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 获取安全设置
    /// </summary>
    public SecuritySettings GetSecuritySettings()
    {
        try
        {
            return string.IsNullOrEmpty(SecuritySettingsJson) 
                ? new SecuritySettings() 
                : JsonSerializer.Deserialize<SecuritySettings>(SecuritySettingsJson, JsonSerializerOptions) ?? new SecuritySettings();
        }
        catch
        {
            return new SecuritySettings();
        }
    }
    
    /// <summary>
    /// 设置安全设置
    /// </summary>
    public void SetSecuritySettings(SecuritySettings settings)
    {
        SecuritySettingsJson = JsonSerializer.Serialize(settings, JsonSerializerOptions);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 获取功能设置
    /// </summary>
    public FeatureSettings GetFeatureSettings()
    {
        try
        {
            return string.IsNullOrEmpty(FeatureSettingsJson) 
                ? new FeatureSettings() 
                : JsonSerializer.Deserialize<FeatureSettings>(FeatureSettingsJson, JsonSerializerOptions) ?? new FeatureSettings();
        }
        catch
        {
            return new FeatureSettings();
        }
    }
    
    /// <summary>
    /// 设置功能设置
    /// </summary>
    public void SetFeatureSettings(FeatureSettings settings)
    {
        FeatureSettingsJson = JsonSerializer.Serialize(settings, JsonSerializerOptions);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 获取邮件设置
    /// </summary>
    public EmailSettings GetEmailSettings()
    {
        try
        {
            return string.IsNullOrEmpty(EmailSettingsJson) 
                ? new EmailSettings() 
                : JsonSerializer.Deserialize<EmailSettings>(EmailSettingsJson, JsonSerializerOptions) ?? new EmailSettings();
        }
        catch
        {
            return new EmailSettings();
        }
    }
    
    /// <summary>
    /// 设置邮件设置
    /// </summary>
    public void SetEmailSettings(EmailSettings settings)
    {
        EmailSettingsJson = JsonSerializer.Serialize(settings, JsonSerializerOptions);
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// 站点设置 - 负责网站基本配置
/// </summary>
public class SiteSettings
{
    /// <summary>
    /// 站点名称
    /// </summary>
    public string SiteName { get; set; } = "代码片段管理";
    
    /// <summary>
    /// 站点描述
    /// </summary>
    public string SiteDescription { get; set; } = "专业的代码片段管理平台";
    
    /// <summary>
    /// 站点Logo URL
    /// </summary>
    public string LogoUrl { get; set; } = "/logo.png";
    
    /// <summary>
    /// 站点主题
    /// </summary>
    public string Theme { get; set; } = "light";
    
    /// <summary>
    /// 站点语言
    /// </summary>
    public string Language { get; set; } = "zh-CN";
    
    /// <summary>
    /// 每页显示数量
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否启用注册
    /// </summary>
    public bool AllowRegistration { get; set; } = true;
    
    /// <summary>
    /// 站点公告
    /// </summary>
    public string Announcement { get; set; } = "";
    
    /// <summary>
    /// 自定义CSS
    /// </summary>
    public string CustomCss { get; set; } = "";
    
    /// <summary>
    /// 自定义JavaScript
    /// </summary>
    public string CustomJs { get; set; } = "";
}

/// <summary>
/// 安全设置 - 负责系统安全配置
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// 密码最小长度
    /// </summary>
    public int MinPasswordLength { get; set; } = 8;
    
    /// <summary>
    /// 密码必须包含大写字母
    /// </summary>
    public bool RequireUppercase { get; set; } = true;
    
    /// <summary>
    /// 密码必须包含小写字母
    /// </summary>
    public bool RequireLowercase { get; set; } = true;
    
    /// <summary>
    /// 密码必须包含数字
    /// </summary>
    public bool RequireNumbers { get; set; } = true;
    
    /// <summary>
    /// 密码必须包含特殊字符
    /// </summary>
    public bool RequireSpecialChars { get; set; } = true;
    
    /// <summary>
    /// 登录尝试最大次数
    /// </summary>
    public int MaxLoginAttempts { get; set; } = 5;
    
    /// <summary>
    /// 账户锁定时间（分钟）
    /// </summary>
    public int AccountLockoutDuration { get; set; } = 30;
    
    /// <summary>
    /// 会话超时时间（分钟）
    /// </summary>
    public int SessionTimeout { get; set; } = 120;
    
    /// <summary>
    /// 是否启用双因子认证
    /// </summary>
    public bool EnableTwoFactorAuth { get; set; } = false;
    
    /// <summary>
    /// 是否启用CORS
    /// </summary>
    public bool EnableCors { get; set; } = true;
    
    /// <summary>
    /// 允许的CORS域名
    /// </summary>
    public string[] AllowedCorsOrigins { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// 是否启用HTTPS重定向
    /// </summary>
    public bool EnableHttpsRedirection { get; set; } = true;
    
    /// <summary>
    /// API速率限制（每分钟请求数）
    /// </summary>
    public int ApiRateLimit { get; set; } = 100;
    
    /// <summary>
    /// 是否启用登录日志
    /// </summary>
    public bool EnableLoginLogging { get; set; } = true;
    
    /// <summary>
    /// 是否启用操作日志
    /// </summary>
    public bool EnableActionLogging { get; set; } = true;
}

/// <summary>
/// 功能设置 - 负责功能开关配置
/// </summary>
public class FeatureSettings
{
    /// <summary>
    /// 是否启用代码片段功能
    /// </summary>
    public bool EnableCodeSnippets { get; set; } = true;
    
    /// <summary>
    /// 是否启用分享功能
    /// </summary>
    public bool EnableSharing { get; set; } = true;
    
    /// <summary>
    /// 是否启用标签功能
    /// </summary>
    public bool EnableTags { get; set; } = true;
    
    /// <summary>
    /// 是否启用评论功能
    /// </summary>
    public bool EnableComments { get; set; } = true;
    
    /// <summary>
    /// 是否启用收藏功能
    /// </summary>
    public bool EnableFavorites { get; set; } = true;
    
    /// <summary>
    /// 是否启用搜索功能
    /// </summary>
    public bool EnableSearch { get; set; } = true;
    
    /// <summary>
    /// 是否启用导出功能
    /// </summary>
    public bool EnableExport { get; set; } = true;
    
    /// <summary>
    /// 是否启用导入功能
    /// </summary>
    public bool EnableImport { get; set; } = true;
    
    /// <summary>
    /// 是否启用API功能
    /// </summary>
    public bool EnableApi { get; set; } = true;
    
    /// <summary>
    /// 是否启用WebHooks
    /// </summary>
    public bool EnableWebHooks { get; set; } = false;
    
    /// <summary>
    /// 是否启用文件上传
    /// </summary>
    public bool EnableFileUpload { get; set; } = true;
    
    /// <summary>
    /// 最大文件大小（MB）
    /// </summary>
    public int MaxFileSize { get; set; } = 10;
    
    /// <summary>
    /// 允许的文件类型
    /// </summary>
    public string[] AllowedFileTypes { get; set; } = new[] { ".txt", ".md", ".json", ".xml", ".csv" };
    
    /// <summary>
    /// 是否启用实时通知
    /// </summary>
    public bool EnableRealTimeNotifications { get; set; } = true;
    
    /// <summary>
    /// 是否启用分析统计
    /// </summary>
    public bool EnableAnalytics { get; set; } = false;
}

/// <summary>
/// 邮件设置 - 负责邮件配置
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// SMTP服务器地址
    /// </summary>
    public string SmtpHost { get; set; } = "";
    
    /// <summary>
    /// SMTP端口
    /// </summary>
    public int SmtpPort { get; set; } = 587;
    
    /// <summary>
    /// SMTP用户名
    /// </summary>
    public string SmtpUsername { get; set; } = "";
    
    /// <summary>
    /// SMTP密码
    /// </summary>
    public string SmtpPassword { get; set; } = "";
    
    /// <summary>
    /// 发件人邮箱
    /// </summary>
    public string FromEmail { get; set; } = "";
    
    /// <summary>
    /// 发件人名称
    /// </summary>
    public string FromName { get; set; } = "";
    
    /// <summary>
    /// 是否启用SSL
    /// </summary>
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>
    /// 是否启用TLS
    /// </summary>
    public bool EnableTls { get; set; } = true;
    
    /// <summary>
    /// 邮件模板路径
    /// </summary>
    public string TemplatePath { get; set; } = "/templates/emails";
    
    /// <summary>
    /// 是否启用邮件队列
    /// </summary>
    public bool EnableEmailQueue { get; set; } = true;
    
    /// <summary>
    /// 邮件发送重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// 邮件发送超时时间（秒）
    /// </summary>
    public int EmailTimeout { get; set; } = 30;
    
    /// <summary>
    /// 是否启用邮件日志
    /// </summary>
    public bool EnableEmailLogging { get; set; } = true;
    
    /// <summary>
    /// 测试邮件接收地址
    /// </summary>
    public string TestEmailRecipient { get; set; } = "";
}