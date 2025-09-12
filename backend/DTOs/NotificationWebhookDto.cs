using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 通知Webhook配置DTO - 用于返回Webhook配置详情
/// </summary>
public class NotificationWebhookConfigDto
{
    /// <summary>
    /// Webhook配置ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Webhook名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Webhook描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP方法
    /// </summary>
    public WebhookHttpMethod HttpMethod { get; set; } = WebhookHttpMethod.POST;

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 认证类型
    /// </summary>
    public WebhookAuthType AuthType { get; set; } = WebhookAuthType.None;

    /// <summary>
    /// 认证配置
    /// </summary>
    public Dictionary<string, string> AuthConfig { get; set; } = new();

    /// <summary>
    /// 触发条件
    /// </summary>
    public NotificationWebhookTriggerDto Trigger { get; set; } = new();

    /// <summary>
    /// 数据格式
    /// </summary>
    public WebhookDataFormat DataFormat { get; set; } = WebhookDataFormat.Json;

    /// <summary>
    /// 自定义模板
    /// </summary>
    public string? CustomTemplate { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用签名验证
    /// </summary>
    public bool EnableSignature { get; set; } = false;

    /// <summary>
    /// 签名密钥
    /// </summary>
    public string? SignatureSecret { get; set; }

    /// <summary>
    /// 签名算法
    /// </summary>
    public WebhookSignatureAlgorithm SignatureAlgorithm { get; set; } = WebhookSignatureAlgorithm.HmacSha256;

    /// <summary>
    /// 重试策略
    /// </summary>
    public WebhookRetryPolicy RetryPolicy { get; set; } = new();

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 过滤规则
    /// </summary>
    public List<NotificationWebhookFilterDto> Filters { get; set; } = new();

    /// <summary>
    /// 统计信息
    /// </summary>
    public NotificationWebhookStatsDto Stats { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最后触发时间
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>
    /// 创建者信息
    /// </summary>
    public UserInfoDto? Creator { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 是否可删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 是否可测试
    /// </summary>
    public bool CanTest { get; set; }
}

/// <summary>
/// 通知Webhook触发条件DTO
/// </summary>
public class NotificationWebhookTriggerDto
{
    /// <summary>
    /// 触发的事件类型
    /// </summary>
    public List<NotificationType> EventTypes { get; set; } = new();

    /// <summary>
    /// 触发的状态
    /// </summary>
    public List<NotificationStatus> Statuses { get; set; } = new();

    /// <summary>
    /// 触发的优先级
    /// </summary>
    public List<NotificationPriority> Priorities { get; set; } = new();

    /// <summary>
    /// 触发的渠道
    /// </summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>
    /// 是否匹配所有条件
    /// </summary>
    public bool MatchAllConditions { get; set; } = true;

    /// <summary>
    /// 自定义触发条件
    /// </summary>
    public string? CustomCondition { get; set; }
}

/// <summary>
/// 通知Webhook过滤规则DTO
/// </summary>
public class NotificationWebhookFilterDto
{
    /// <summary>
    /// 过滤字段
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// 过滤操作
    /// </summary>
    public WebhookFilterOperator Operator { get; set; } = WebhookFilterOperator.Equals;

    /// <summary>
    /// 过滤值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 是否忽略大小写
    /// </summary>
    public bool IgnoreCase { get; set; } = false;
}

/// <summary>
/// 通知Webhook统计信息DTO
/// </summary>
public class NotificationWebhookStatsDto
{
    /// <summary>
    /// 总触发次数
    /// </summary>
    public int TotalTriggered { get; set; }

    /// <summary>
    /// 成功次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败次数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均响应时间
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// 今日触发次数
    /// </summary>
    public int TodayTriggered { get; set; }

    /// <summary>
    /// 本月触发次数
    /// </summary>
    public int MonthTriggered { get; set; }

    /// <summary>
    /// 最后触发时间
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// 最后成功时间
    /// </summary>
    public DateTime? LastSuccessAt { get; set; }

    /// <summary>
    /// 最后失败时间
    /// </summary>
    public DateTime? LastFailureAt { get; set; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    public string? LastErrorMessage { get; set; }
}

/// <summary>
/// 通知Webhook重试策略DTO
/// </summary>
public class WebhookRetryPolicy
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 指数退避因子
    /// </summary>
    public double ExponentialBackoffFactor { get; set; } = 2.0;

    /// <summary>
    /// 最大重试间隔（秒）
    /// </summary>
    public int MaxRetryIntervalSeconds { get; set; } = 3600;

    /// <summary>
    /// HTTP重试状态码
    /// </summary>
    public List<int> RetryStatusCodes { get; set; } = new() { 408, 429, 500, 502, 503, 504 };

    /// <summary>
    /// 是否只在特定错误时重试
    /// </summary>
    public bool RetryOnSpecificErrorsOnly { get; set; } = false;
}

/// <summary>
/// 通知Webhook配置创建请求DTO
/// </summary>
public class NotificationWebhookConfigCreateDto
{
    /// <summary>
    /// Webhook名称
    /// </summary>
    [Required(ErrorMessage = "Webhook名称不能为空")]
    [StringLength(100, ErrorMessage = "Webhook名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Webhook描述
    /// </summary>
    [StringLength(500, ErrorMessage = "Webhook描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    [Required(ErrorMessage = "Webhook URL不能为空")]
    [StringLength(2000, ErrorMessage = "Webhook URL长度不能超过2000个字符")]
    [Url(ErrorMessage = "Webhook URL格式不正确")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP方法
    /// </summary>
    public WebhookHttpMethod HttpMethod { get; set; } = WebhookHttpMethod.POST;

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 认证类型
    /// </summary>
    public WebhookAuthType AuthType { get; set; } = WebhookAuthType.None;

    /// <summary>
    /// 认证配置
    /// </summary>
    public Dictionary<string, string> AuthConfig { get; set; } = new();

    /// <summary>
    /// 触发条件
    /// </summary>
    public NotificationWebhookTriggerDto Trigger { get; set; } = new();

    /// <summary>
    /// 数据格式
    /// </summary>
    public WebhookDataFormat DataFormat { get; set; } = WebhookDataFormat.Json;

    /// <summary>
    /// 自定义模板
    /// </summary>
    [StringLength(5000, ErrorMessage = "自定义模板长度不能超过5000个字符")]
    public string? CustomTemplate { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用签名验证
    /// </summary>
    public bool EnableSignature { get; set; } = false;

    /// <summary>
    /// 签名密钥
    /// </summary>
    [StringLength(500, ErrorMessage = "签名密钥长度不能超过500个字符")]
    public string? SignatureSecret { get; set; }

    /// <summary>
    /// 签名算法
    /// </summary>
    public WebhookSignatureAlgorithm SignatureAlgorithm { get; set; } = WebhookSignatureAlgorithm.HmacSha256;

    /// <summary>
    /// 重试策略
    /// </summary>
    public WebhookRetryPolicy RetryPolicy { get; set; } = new();

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 过滤规则
    /// </summary>
    public List<NotificationWebhookFilterDto> Filters { get; set; } = new();
}

/// <summary>
/// 通知Webhook配置更新请求DTO
/// </summary>
public class NotificationWebhookConfigUpdateDto
{
    /// <summary>
    /// Webhook名称
    /// </summary>
    [StringLength(100, ErrorMessage = "Webhook名称长度不能超过100个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// Webhook描述
    /// </summary>
    [StringLength(500, ErrorMessage = "Webhook描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    [StringLength(2000, ErrorMessage = "Webhook URL长度不能超过2000个字符")]
    [Url(ErrorMessage = "Webhook URL格式不正确")]
    public string? Url { get; set; }

    /// <summary>
    /// HTTP方法
    /// </summary>
    public WebhookHttpMethod? HttpMethod { get; set; }

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// 认证类型
    /// </summary>
    public WebhookAuthType? AuthType { get; set; }

    /// <summary>
    /// 认证配置
    /// </summary>
    public Dictionary<string, string>? AuthConfig { get; set; }

    /// <summary>
    /// 触发条件
    /// </summary>
    public NotificationWebhookTriggerDto? Trigger { get; set; }

    /// <summary>
    /// 数据格式
    /// </summary>
    public WebhookDataFormat? DataFormat { get; set; }

    /// <summary>
    /// 自定义模板
    /// </summary>
    [StringLength(5000, ErrorMessage = "自定义模板长度不能超过5000个字符")]
    public string? CustomTemplate { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 是否启用签名验证
    /// </summary>
    public bool? EnableSignature { get; set; }

    /// <summary>
    /// 签名密钥
    /// </summary>
    [StringLength(500, ErrorMessage = "签名密钥长度不能超过500个字符")]
    public string? SignatureSecret { get; set; }

    /// <summary>
    /// 签名算法
    /// </summary>
    public WebhookSignatureAlgorithm? SignatureAlgorithm { get; set; }

    /// <summary>
    /// 重试策略
    /// </summary>
    public WebhookRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// 过滤规则
    /// </summary>
    public List<NotificationWebhookFilterDto>? Filters { get; set; }
}

/// <summary>
/// 通知Webhook配置测试请求DTO
/// </summary>
public class NotificationWebhookConfigTestDto
{
    /// <summary>
    /// 配置ID
    /// </summary>
    [Required(ErrorMessage = "配置ID不能为空")]
    public Guid ConfigId { get; set; }

    /// <summary>
    /// 测试类型
    /// </summary>
    public WebhookTestType TestType { get; set; } = WebhookTestType.Connectivity;

    /// <summary>
    /// 测试数据
    /// </summary>
    public Dictionary<string, object>? TestData { get; set; }

    /// <summary>
    /// 是否使用默认测试数据
    /// </summary>
    public bool UseDefaultTestData { get; set; } = true;
}

/// <summary>
/// 通知Webhook配置测试结果DTO
/// </summary>
public class NotificationWebhookConfigTestResultDto
{
    /// <summary>
    /// 测试是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 测试消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 测试类型
    /// </summary>
    public WebhookTestType TestType { get; set; }

    /// <summary>
    /// 测试时间
    /// </summary>
    public DateTime TestAt { get; set; }

    /// <summary>
    /// 测试耗时
    /// </summary>
    public TimeSpan TestDuration { get; set; }

    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// 响应头
    /// </summary>
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();

    /// <summary>
    /// 响应内容
    /// </summary>
    public string? ResponseContent { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 错误详情
    /// </summary>
    public Dictionary<string, object>? ErrorDetails { get; set; }

    /// <summary>
    /// 建议
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// 通知Webhook配置列表DTO
/// </summary>
public class NotificationWebhookConfigListDto
{
    /// <summary>
    /// Webhook配置列表
    /// </summary>
    public List<NotificationWebhookConfigDto> Configs { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }
}

/// <summary>
/// Webhook HTTP方法枚举
/// </summary>
public enum WebhookHttpMethod
{
    /// <summary>
    /// GET方法
    /// </summary>
    GET = 0,

    /// <summary>
    /// POST方法
    /// </summary>
    POST = 1,

    /// <summary>
    /// PUT方法
    /// </summary>
    PUT = 2,

    /// <summary>
    /// DELETE方法
    /// </summary>
    DELETE = 3,

    /// <summary>
    /// PATCH方法
    /// </summary>
    PATCH = 4
}

/// <summary>
/// Webhook认证类型枚举
/// </summary>
public enum WebhookAuthType
{
    /// <summary>
    /// 无认证
    /// </summary>
    None = 0,

    /// <summary>
    /// Basic认证
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Bearer Token认证
    /// </summary>
    BearerToken = 2,

    /// <summary>
    /// API Key认证
    /// </summary>
    ApiKey = 3,

    /// <summary>
    /// OAuth2认证
    /// </summary>
    OAuth2 = 4,

    /// <summary>
    /// 自定义认证
    /// </summary>
    Custom = 5
}

/// <summary>
/// Webhook数据格式枚举
/// </summary>
public enum WebhookDataFormat
{
    /// <summary>
    /// JSON格式
    /// </summary>
    Json = 0,

    /// <summary>
    /// XML格式
    /// </summary>
    Xml = 1,

    /// <summary>
    /// Form格式
    /// </summary>
    Form = 2,

    /// <summary>
    /// 自定义格式
    /// </summary>
    Custom = 3
}

/// <summary>
/// Webhook签名算法枚举
/// </summary>
public enum WebhookSignatureAlgorithm
{
    /// <summary>
    /// HMAC-SHA256
    /// </summary>
    HmacSha256 = 0,

    /// <summary>
    /// HMAC-SHA1
    /// </summary>
    HmacSha1 = 1,

    /// <summary>
    /// HMAC-MD5
    /// </summary>
    HmacMd5 = 2,

    /// <summary>
    /// SHA256
    /// </summary>
    Sha256 = 3,

    /// <summary>
    /// SHA1
    /// </summary>
    Sha1 = 4
}

/// <summary>
/// Webhook过滤操作符枚举
/// </summary>
public enum WebhookFilterOperator
{
    /// <summary>
    /// 等于
    /// </summary>
    Equals = 0,

    /// <summary>
    /// 不等于
    /// </summary>
    NotEquals = 1,

    /// <summary>
    /// 包含
    /// </summary>
    Contains = 2,

    /// <summary>
    /// 不包含
    /// </summary>
    NotContains = 3,

    /// <summary>
    /// 开始于
    /// </summary>
    StartsWith = 4,

    /// <summary>
    /// 结束于
    /// </summary>
    EndsWith = 5,

    /// <summary>
    /// 大于
    /// </summary>
    GreaterThan = 6,

    /// <summary>
    /// 小于
    /// </summary>
    LessThan = 7,

    /// <summary>
    /// 大于等于
    /// </summary>
    GreaterThanOrEqual = 8,

    /// <summary>
    /// 小于等于
    /// </summary>
    LessThanOrEqual = 9,

    /// <summary>
    /// 正则匹配
    /// </summary>
    Regex = 10
}

/// <summary>
/// Webhook测试类型枚举
/// </summary>
public enum WebhookTestType
{
    /// <summary>
    /// 连通性测试
    /// </summary>
    Connectivity = 0,

    /// <summary>
    /// 认证测试
    /// </summary>
    Authentication = 1,

    /// <summary>
    /// 数据格式测试
    /// </summary>
    DataFormat = 2,

    /// <summary>
    /// 完整测试
    /// </summary>
    FullTest = 3
}