using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 内容安全服务接口 - 提供内容验证和安全检查功能
/// 遵循接口隔离原则和单一职责原则
/// </summary>
public interface IContentSecurityService
{
    // 内容验证
    #region 内容验证

    /// <summary>
    /// 验证评论内容安全性
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>内容安全验证结果</returns>
    Task<ContentSecurityResult> ValidateContentAsync(string content, string contentType);

    /// <summary>
    /// 验证消息内容安全性
    /// </summary>
    /// <param name="content">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <returns>内容安全验证结果</returns>
    Task<ContentSecurityResult> ValidateMessageContentAsync(string content, string messageType);

    /// <summary>
    /// 验证通知内容安全性
    /// </summary>
    /// <param name="title">通知标题</param>
    /// <param name="content">通知内容</param>
    /// <returns>内容安全验证结果</returns>
    Task<ContentSecurityResult> ValidateNotificationContentAsync(string title, string content);

    /// <summary>
    /// 验证代码片段内容安全性
    /// </summary>
    /// <param name="code">代码内容</param>
    /// <param name="language">编程语言</param>
    /// <returns>代码安全验证结果</returns>
    Task<CodeSecurityResult> ValidateCodeContentAsync(string code, string language);

    /// <summary>
    /// 批量验证内容安全性
    /// </summary>
    /// <param name="contents">内容列表</param>
    /// <returns>批量验证结果</returns>
    Task<BatchContentSecurityResult> ValidateContentBatchAsync(IEnumerable<ContentValidationRequest> contents);

    /// <summary>
    /// 检查内容是否包含敏感词
    /// </summary>
    /// <param name="content">内容</param>
    /// <returns>敏感词检查结果</returns>
    Task<SensitiveWordResult> CheckSensitiveWordsAsync(string content);

    /// <summary>
    /// 获取内容安全评分
    /// </summary>
    /// <param name="content">内容</param>
    /// <returns>安全评分</returns>
    Task<int> GetContentSafetyScoreAsync(string content);

    #endregion

    // XSS防护
    #region XSS防护

    /// <summary>
    /// 清理HTML内容，防止XSS攻击
    /// </summary>
    /// <param name="html">HTML内容</param>
    /// <param name="allowedTags">允许的HTML标签</param>
    /// <returns>清理后的HTML内容</returns>
    Task<string> SanitizeHtmlAsync(string html, IEnumerable<string>? allowedTags = null);

    /// <summary>
    /// 清理用户输入，防止XSS攻击
    /// </summary>
    /// <param name="input">用户输入</param>
    /// <param name="inputType">输入类型</param>
    /// <returns>清理后的输入</returns>
    Task<string> SanitizeUserInputAsync(string input, string inputType);

    /// <summary>
    /// 编码HTML内容
    /// </summary>
    /// <param name="content">内容</param>
    /// <returns>编码后的内容</returns>
    Task<string> EncodeHtmlAsync(string content);

    /// <summary>
    /// 检测潜在的XSS攻击
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>XSS检测结果</returns>
    Task<XssDetectionResult> DetectXssAsync(string input);

    /// <summary>
    /// 验证URL安全性
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>URL安全验证结果</returns>
    Task<UrlSecurityResult> ValidateUrlAsync(string url);

    /// <summary>
    /// 清理Markdown内容
    /// </summary>
    /// <param name="markdown">Markdown内容</param>
    /// <returns>清理后的Markdown内容</returns>
    Task<string> SanitizeMarkdownAsync(string markdown);

    #endregion

    // SQL注入防护
    #region SQL注入防护

    /// <summary>
    /// 检测SQL注入攻击
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>SQL注入检测结果</returns>
    Task<SqlInjectionResult> DetectSqlInjectionAsync(string input);

    /// <summary>
    /// 清理SQL查询参数
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>清理后的内容</returns>
    Task<string> SanitizeSqlParameterAsync(string input);

    /// <summary>
    /// 验证查询参数安全性
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>参数验证结果</returns>
    Task<SqlParameterValidationResult> ValidateSqlParametersAsync(Dictionary<string, object> parameters);

    /// <summary>
    /// 检查命令注入风险
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>命令注入检测结果</returns>
    Task<CommandInjectionResult> DetectCommandInjectionAsync(string input);

    #endregion

    // 内容分析和报告
    #region 内容分析和报告

    /// <summary>
    /// 分析内容风险
    /// </summary>
    /// <param name="content">内容</param>
    /// <returns>内容风险分析结果</returns>
    Task<ContentRiskAnalysisResult> AnalyzeContentRiskAsync(string content);

    /// <summary>
    /// 生成内容安全报告
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="analysisResults">分析结果</param>
    /// <returns>安全报告</returns>
    Task<ContentSecurityReport> GenerateSecurityReportAsync(string content, IEnumerable<ContentSecurityResult> analysisResults);

    /// <summary>
    /// 获取内容安全统计
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>安全统计信息</returns>
    Task<ContentSecurityStats> GetSecurityStatsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 记录安全事件
    /// </summary>
    /// <param name="securityEvent">安全事件</param>
    /// <returns>是否记录成功</returns>
    Task<bool> LogSecurityEventAsync(SecurityEvent securityEvent);

    /// <summary>
    /// 获取安全事件日志
    /// </summary>
    /// <param name="filter">事件筛选条件</param>
    /// <returns>安全事件列表</returns>
    Task<PaginatedResult<SecurityEvent>> GetSecurityEventsAsync(SecurityEventFilter filter);

    #endregion

    // 缓存和性能优化
    #region 缓存和性能优化

    /// <summary>
    /// 清理内容安全缓存
    /// </summary>
    /// <param name="contentHash">内容哈希</param>
    /// <returns>是否清理成功</returns>
    Task<bool> ClearSecurityCacheAsync(string contentHash);

    /// <summary>
    /// 预热安全检查缓存
    /// </summary>
    /// <param name="contents">内容列表</param>
    /// <returns>是否预热成功</returns>
    Task<bool> WarmupSecurityCacheAsync(IEnumerable<string> contents);

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    /// <returns>缓存统计</returns>
    Task<SecurityCacheStats> GetCacheStatsAsync();

    #endregion

    // 配置管理
    #region 配置管理

    /// <summary>
    /// 获取安全策略配置
    /// </summary>
    /// <returns>安全策略配置</returns>
    Task<SecurityPolicyConfig> GetSecurityPolicyAsync();

    /// <summary>
    /// 更新安全策略配置
    /// </summary>
    /// <param name="config">安全策略配置</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateSecurityPolicyAsync(SecurityPolicyConfig config);

    /// <summary>
    /// 重置安全策略到默认配置
    /// </summary>
    /// <returns>是否重置成功</returns>
    Task<bool> ResetSecurityPolicyAsync();

    /// <summary>
    /// 获取敏感词列表
    /// </summary>
    /// <returns>敏感词列表</returns>
    Task<IEnumerable<string>> GetSensitiveWordsAsync();

    /// <summary>
    /// 添加敏感词
    /// </summary>
    /// <param name="word">敏感词</param>
    /// <param name="category">分类</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddSensitiveWordAsync(string word, string category);

    /// <summary>
    /// 移除敏感词
    /// </summary>
    /// <param name="word">敏感词</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveSensitiveWordAsync(string word);

    #endregion
}

/// <summary>
/// 内容安全验证结果
/// </summary>
public class ContentSecurityResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public SecurityLevel SecurityLevel { get; set; }
    public IEnumerable<string> DetectedIssues { get; set; } = new List<string>();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public string ContentHash { get; set; } = string.Empty;
}

/// <summary>
/// 代码安全验证结果
/// </summary>
public class CodeSecurityResult : ContentSecurityResult
{
    public string Language { get; set; } = string.Empty;
    public int CodeLines { get; set; }
    public IEnumerable<string> SecurityWarnings { get; set; } = new List<string>();
    public IEnumerable<string> AllowedPatterns { get; set; } = new List<string>();
    public bool ContainsDatabaseOperations { get; set; }
    public bool ContainsFileOperations { get; set; }
    public bool ContainsNetworkOperations { get; set; }
}

/// <summary>
/// 批量内容安全验证结果
/// </summary>
public class BatchContentSecurityResult
{
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int InvalidItems { get; set; }
    public IEnumerable<ContentSecurityResult> Results { get; set; } = new List<ContentSecurityResult>();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public double ProcessingTimeMs { get; set; }
}

/// <summary>
/// 敏感词检查结果
/// </summary>
public class SensitiveWordResult
{
    public bool ContainsSensitiveWords { get; set; }
    public IEnumerable<string> DetectedWords { get; set; } = new List<string>();
    public IEnumerable<string> WordCategories { get; set; } = new List<string>();
    public int TotalCount { get; set; }
    public double SeverityScore { get; set; }
}

/// <summary>
/// XSS检测结果
/// </summary>
public class XssDetectionResult
{
    public bool ContainsXss { get; set; }
    public IEnumerable<string> DetectedPatterns { get; set; } = new List<string>();
    public XssRiskLevel RiskLevel { get; set; }
    public IEnumerable<string> SanitizedContent { get; set; } = new List<string>();
}

/// <summary>
/// URL安全验证结果
/// </summary>
public class UrlSecurityResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Uri? ParsedUri { get; set; }
    public bool IsInternalUrl { get; set; }
    public bool HasDangerousProtocol { get; set; }
    public bool ContainsCredentials { get; set; }
    public bool IsWhitelisted { get; set; }
}

/// <summary>
/// SQL注入检测结果
/// </summary>
public class SqlInjectionResult
{
    public bool ContainsSqlInjection { get; set; }
    public IEnumerable<string> DetectedPatterns { get; set; } = new List<string>();
    public InjectionRiskLevel RiskLevel { get; set; }
    public IEnumerable<string> SanitizedContent { get; set; } = new List<string>();
}

/// <summary>
/// SQL参数验证结果
/// </summary>
public class SqlParameterValidationResult
{
    public bool AllParametersValid { get; set; }
    public IEnumerable<ParameterValidationResult> ParameterResults { get; set; } = new List<ParameterValidationResult>();
    public int InvalidParametersCount { get; set; }
}

/// <summary>
/// 参数验证结果
/// </summary>
public class ParameterValidationResult
{
    public string ParameterName { get; set; } = string.Empty;
    public object? ParameterValue { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 命令注入检测结果
/// </summary>
public class CommandInjectionResult
{
    public bool ContainsCommandInjection { get; set; }
    public IEnumerable<string> DetectedCommands { get; set; } = new List<string>();
    public InjectionRiskLevel RiskLevel { get; set; }
    public IEnumerable<string> SanitizedContent { get; set; } = new List<string>();
}

/// <summary>
/// 内容风险分析结果
/// </summary>
public class ContentRiskAnalysisResult
{
    public double RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public IEnumerable<string> RiskFactors { get; set; } = new List<string>();
    public IEnumerable<string> Recommendations { get; set; } = new List<string>();
    public ContentCategory ContentCategory { get; set; }
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// 内容安全报告
/// </summary>
public class ContentSecurityReport
{
    public string ReportId { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public OverallSecurityStatus OverallStatus { get; set; }
    public IEnumerable<SecurityIssue> SecurityIssues { get; set; } = new List<SecurityIssue>();
    public IEnumerable<SecurityRecommendation> Recommendations { get; set; } = new List<SecurityRecommendation>();
    public SecurityMetrics Metrics { get; set; } = new SecurityMetrics();
}

/// <summary>
/// 安全统计信息
/// </summary>
public class ContentSecurityStats
{
    public int TotalContentChecks { get; set; }
    public int ValidContentCount { get; set; }
    public int InvalidContentCount { get; set; }
    public int SecurityEventsCount { get; set; }
    public double AverageRiskScore { get; set; }
    public IEnumerable<SecurityEventCount> EventCounts { get; set; } = new List<SecurityEventCount>();
    public IEnumerable<DailySecurityStats> DailyStats { get; set; } = new List<DailySecurityStats>();
}

/// <summary>
/// 安全事件
/// </summary>
public class SecurityEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public SecurityLevel Severity { get; set; }
    public string? SourceContent { get; set; }
    public string? UserIdentifier { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
}

/// <summary>
/// 安全事件筛选条件
/// </summary>
public class SecurityEventFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? EventType { get; set; }
    public SecurityLevel? MinSeverity { get; set; }
    public string? UserIdentifier { get; set; }
    public bool? IsResolved { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 缓存统计信息
/// </summary>
public class SecurityCacheStats
{
    public long TotalCacheEntries { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRate { get; set; }
    public long MemoryUsage { get; set; }
    public DateTime LastCleanup { get; set; }
    public IEnumerable<CacheEntryInfo> TopEntries { get; set; } = new List<CacheEntryInfo>();
}

/// <summary>
/// 安全策略配置
/// </summary>
public class SecurityPolicyConfig
{
    public bool EnableXssProtection { get; set; } = true;
    public bool EnableSqlInjectionProtection { get; set; } = true;
    public bool EnableSensitiveWordFilter { get; set; } = true;
    public bool EnableContentAnalysis { get; set; } = true;
    public SecurityLevel MinimumSecurityLevel { get; set; } = SecurityLevel.Medium;
    public IEnumerable<string> AllowedHtmlTags { get; set; } = new List<string>();
    public IEnumerable<string> AllowedUrlProtocols { get; set; } = new List<string>();
    public int MaxContentLength { get; set; } = 10000;
    public int MaxBatchSize { get; set; } = 100;
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 30;
    public bool EnableLogging { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
}

/// <summary>
/// 内容验证请求
/// </summary>
public class ContentValidationRequest
{
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string? UserId { get; set; }
}

/// <summary>
/// 安全问题
/// </summary>
public class SecurityIssue
{
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityLevel Severity { get; set; }
    public string? Location { get; set; }
    public IEnumerable<string> Recommendations { get; set; } = new List<string>();
}

/// <summary>
/// 安全建议
/// </summary>
public class SecurityRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public string? Action { get; set; }
}

/// <summary>
/// 安全指标
/// </summary>
public class SecurityMetrics
{
    public int TotalChecks { get; set; }
    public int IssuesFound { get; set; }
    public double AverageRiskScore { get; set; }
    public int HighRiskIssues { get; set; }
    public int MediumRiskIssues { get; set; }
    public int LowRiskIssues { get; set; }
}

/// <summary>
/// 安全事件计数
/// </summary>
public class SecurityEventCount
{
    public string EventType { get; set; } = string.Empty;
    public int Count { get; set; }
    public SecurityLevel Severity { get; set; }
}

/// <summary>
/// 每日安全统计
/// </summary>
public class DailySecurityStats
{
    public DateTime Date { get; set; }
    public int TotalChecks { get; set; }
    public int IssuesFound { get; set; }
    public double AverageRiskScore { get; set; }
    public int SecurityEvents { get; set; }
}

/// <summary>
/// 缓存条目信息
/// </summary>
public class CacheEntryInfo
{
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessed { get; set; }
    public long AccessCount { get; set; }
    public long Size { get; set; }
}

// 枚举定义
public enum SecurityLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum XssRiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum InjectionRiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum RiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum ContentCategory
{
    Text,
    Code,
    Html,
    Markdown,
    Json,
    Xml,
    Other
}

public enum OverallSecurityStatus
{
    Safe,
    LowRisk,
    MediumRisk,
    HighRisk,
    Critical
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}