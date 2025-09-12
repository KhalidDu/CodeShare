using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息导出请求DTO
/// </summary>
public class MessageExportRequestDto
{
    /// <summary>
    /// 导出格式
    /// </summary>
    [Required(ErrorMessage = "导出格式不能为空")]
    public ExportFormat Format { get; set; }

    /// <summary>
    /// 消息ID列表（空表示根据筛选条件导出）
    /// </summary>
    public List<Guid>? MessageIds { get; set; }

    /// <summary>
    /// 会话ID列表（空表示导出所有会话）
    /// </summary>
    public List<Guid>? ConversationIds { get; set; }

    /// <summary>
    /// 发送者ID列表
    /// </summary>
    public List<Guid>? SenderIds { get; set; }

    /// <summary>
    /// 接收者ID列表
    /// </summary>
    public List<Guid>? ReceiverIds { get; set; }

    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public List<MessageType>? MessageTypes { get; set; }

    /// <summary>
    /// 消息状态筛选
    /// </summary>
    public List<MessageStatus>? Statuses { get; set; }

    /// <summary>
    /// 优先级筛选
    /// </summary>
    public List<MessagePriority>? Priorities { get; set; }

    /// <summary>
    /// 标签筛选
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 是否包含附件
    /// </summary>
    public bool IncludeAttachments { get; set; } = false;

    /// <summary>
    /// 是否包含回复消息
    /// </summary>
    public bool IncludeReplies { get; set; } = true;

    /// <summary>
    /// 是否包含已删除的消息
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// 是否包含草稿
    /// </summary>
    public bool IncludeDrafts { get; set; } = false;

    /// <summary>
    /// 文件名
    /// </summary>
    [StringLength(255, ErrorMessage = "文件名长度不能超过255个字符")]
    public string? FileName { get; set; }

    /// <summary>
    /// 导出选项
    /// </summary>
    public MessageExportOptions Options { get; set; } = new();

    /// <summary>
    /// 排序方式
    /// </summary>
    public MessageSort SortBy { get; set; } = MessageSort.CreatedAtDesc;

    /// <summary>
    /// 最大导出数量
    /// </summary>
    [Range(1, 100000, ErrorMessage = "最大导出数量必须在1-100000之间")]
    public int MaxCount { get; set; } = 10000;

    /// <summary>
    /// 是否分批导出
    /// </summary>
    public bool BatchExport { get; set; } = false;

    /// <summary>
    /// 批次大小
    /// </summary>
    [Range(100, 10000, ErrorMessage = "批次大小必须在100-10000之间")]
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// 导出语言
    /// </summary>
    [StringLength(10, ErrorMessage = "语言代码长度不能超过10个字符")]
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 时区
    /// </summary>
    [StringLength(50, ErrorMessage = "时区长度不能超过50个字符")]
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// 是否压缩文件
    /// </summary>
    public bool CompressFile { get; set; } = true;

    /// <summary>
    /// 压缩格式
    /// </summary>
    public CompressionFormat CompressionFormat { get; set; } = CompressionFormat.Zip;

    /// <summary>
    /// 加密选项
    /// </summary>
    public MessageExportEncryption Encryption { get; set; } = new();

    /// <summary>
    /// 通知选项
    /// </summary>
    public MessageExportNotification Notification { get; set; } = new();

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 消息导出选项DTO
/// </summary>
public class MessageExportOptions
{
    /// <summary>
    /// 是否包含消息ID
    /// </summary>
    public bool IncludeMessageId { get; set; } = true;

    /// <summary>
    /// 是否包含发送者信息
    /// </summary>
    public bool IncludeSenderInfo { get; set; } = true;

    /// <summary>
    /// 是否包含接收者信息
    /// </summary>
    public bool IncludeReceiverInfo { get; set; } = true;

    /// <summary>
    /// 是否包含时间戳
    /// </summary>
    public bool IncludeTimestamps { get; set; } = true;

    /// <summary>
    /// 是否包含消息状态
    /// </summary>
    public bool IncludeStatus { get; set; } = true;

    /// <summary>
    /// 是否包含优先级
    /// </summary>
    public bool IncludePriority { get; set; } = true;

    /// <summary>
    /// 是否包含标签
    /// </summary>
    public bool IncludeTags { get; set; } = true;

    /// <summary>
    /// 是否包含附件信息
    /// </summary>
    public bool IncludeAttachmentInfo { get; set; } = true;

    /// <summary>
    /// 是否包含阅读状态
    /// </summary>
    public bool IncludeReadStatus { get; set; } = true;

    /// <summary>
    /// 是否包含回复信息
    /// </summary>
    public bool IncludeReplyInfo { get; set; } = true;

    /// <summary>
    /// 是否包含会话信息
    /// </summary>
    public bool IncludeConversationInfo { get; set; } = true;

    /// <summary>
    /// 日期格式
    /// </summary>
    [StringLength(50, ErrorMessage = "日期格式长度不能超过50个字符")]
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 数字格式
    /// </summary>
    [StringLength(20, ErrorMessage = "数字格式长度不能超过20个字符")]
    public string NumberFormat { get; set; } = "0.00";

    /// <summary>
    /// 是否包含页眉
    /// </summary>
    public bool IncludeHeader { get; set; } = true;

    /// <summary>
    /// 是否包含页脚
    /// </summary>
    public bool IncludeFooter { get; set; } = true;

    /// <summary>
    /// 页眉文本
    /// </summary>
    [StringLength(500, ErrorMessage = "页眉文本长度不能超过500个字符")]
    public string? HeaderText { get; set; }

    /// <summary>
    /// 页脚文本
    /// </summary>
    [StringLength(500, ErrorMessage = "页脚文本长度不能超过500个字符")]
    public string? FooterText { get; set; }

    /// <summary>
    /// 自定义字段
    /// </summary>
    public List<string> CustomFields { get; set; } = new();

    /// <summary>
    /// 排除字段
    /// </summary>
    public List<string> ExcludedFields { get; set; } = new();
}

/// <summary>
/// 消息导出加密选项DTO
/// </summary>
public class MessageExportEncryption
{
    /// <summary>
    /// 是否启用加密
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 加密算法
    /// </summary>
    public EncryptionAlgorithm Algorithm { get; set; } = EncryptionAlgorithm.AES256;

    /// <summary>
    /// 加密密码
    /// </summary>
    [StringLength(128, ErrorMessage = "加密密码长度不能超过128个字符")]
    public string? Password { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [StringLength(128, ErrorMessage = "确认密码长度不能超过128个字符")]
    public string? ConfirmPassword { get; set; }

    /// <summary>
    /// 加密密钥
    /// </summary>
    [StringLength(512, ErrorMessage = "加密密钥长度不能超过512个字符")]
    public string? EncryptionKey { get; set; }

    /// <summary>
    /// 加密盐值
    /// </summary>
    [StringLength(128, ErrorMessage = "加密盐值长度不能超过128个字符")]
    public string? Salt { get; set; }

    /// <summary>
    /// 加密轮数
    /// </summary>
    [Range(1000, 100000, ErrorMessage = "加密轮数必须在1000-100000之间")]
    public int Iterations { get; set; } = 10000;

    /// <summary>
    /// 是否使用压缩
    /// </summary>
    public bool UseCompression { get; set; } = true;
}

/// <summary>
/// 消息导出通知选项DTO
/// </summary>
public class MessageExportNotification
{
    /// <summary>
    /// 是否启用通知
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 通知方式
    /// </summary>
    public List<NotificationMethod> Methods { get; set; } = new() { NotificationMethod.Email };

    /// <summary>
    /// 通知邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 通知手机号
    /// </summary>
    [StringLength(20, ErrorMessage = "手机号长度不能超过20个字符")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 通知Webhook URL
    /// </summary>
    [StringLength(500, ErrorMessage = "Webhook URL长度不能超过500个字符")]
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// 通知模板
    /// </summary>
    [StringLength(1000, ErrorMessage = "通知模板长度不能超过1000个字符")]
    public string? Template { get; set; }

    /// <summary>
    /// 是否在导出开始时通知
    /// </summary>
    public bool NotifyOnStart { get; set; } = true;

    /// <summary>
    /// 是否在导出完成时通知
    /// </summary>
    public bool NotifyOnComplete { get; set; } = true;

    /// <summary>
    /// 是否在导出失败时通知
    /// </summary>
    public bool NotifyOnFailure { get; set; } = true;
}

/// <summary>
/// 消息导出响应DTO
/// </summary>
public class MessageExportResponseDto
{
    /// <summary>
    /// 导出任务ID
    /// </summary>
    public Guid ExportId { get; set; }

    /// <summary>
    /// 导出状态
    /// </summary>
    public ExportStatus Status { get; set; }

    /// <summary>
    /// 导出进度（0-100）
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 导出格式
    /// </summary>
    public ExportFormat Format { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件URL
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// 下载链接
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; }

    /// <summary>
    /// 导出消息数量
    /// </summary>
    public int ExportedMessageCount { get; set; }

    /// <summary>
    /// 总消息数量
    /// </summary>
    public int TotalMessageCount { get; set; }

    /// <summary>
    /// 导出会话数量
    /// </summary>
    public int ExportedConversationCount { get; set; }

    /// <summary>
    /// 导出附件数量
    /// </summary>
    public int ExportedAttachmentCount { get; set; }

    /// <summary>
    /// 总附件大小（字节）
    /// </summary>
    public long TotalAttachmentSize { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 处理时间（秒）
    /// </summary>
    public double ProcessingTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 警告信息
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 消息报告生成请求DTO
/// </summary>
public class MessageReportRequestDto
{
    /// <summary>
    /// 报告类型
    /// </summary>
    [Required(ErrorMessage = "报告类型不能为空")]
    public ReportType ReportType { get; set; }

    /// <summary>
    /// 报告标题
    /// </summary>
    [StringLength(200, ErrorMessage = "报告标题长度不能超过200个字符")]
    public string? Title { get; set; }

    /// <summary>
    /// 报告描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "报告描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 时间范围
    /// </summary>
    public DateRangeDto DateRange { get; set; } = new();

    /// <summary>
    /// 用户ID列表
    /// </summary>
    public List<Guid>? UserIds { get; set; }

    /// <summary>
    /// 会话ID列表
    /// </summary>
    public List<Guid>? ConversationIds { get; set; }

    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public List<MessageType>? MessageTypes { get; set; }

    /// <summary>
    /// 优先级筛选
    /// </summary>
    public List<MessagePriority>? Priorities { get; set; }

    /// <summary>
    /// 标签筛选
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 分组方式
    /// </summary>
    public List<ReportGroupBy> GroupBy { get; set; } = new();

    /// <summary>
    /// 聚合函数
    /// </summary>
    public List<ReportAggregateFunction> Aggregates { get; set; } = new();

    /// <summary>
    /// 排序字段
    /// </summary>
    public List<ReportSortField> SortFields { get; set; } = new();

    /// <summary>
    /// 报告选项
    /// </summary>
    public MessageReportOptions Options { get; set; } = new();

    /// <summary>
    /// 输出格式
    /// </summary>
    public ExportFormat OutputFormat { get; set; } = ExportFormat.Json;

    /// <summary>
    /// 是否生成图表
    /// </summary>
    public bool GenerateCharts { get; set; } = false;

    /// <summary>
    /// 图表类型
    /// </summary>
    public List<ChartType> ChartTypes { get; set; } = new();

    /// <summary>
    /// 调度选项
    /// </summary>
    public ReportScheduleOptions Schedule { get; set; } = new();

    /// <summary>
    /// 通知选项
    /// </summary>
    public MessageReportNotificationOptions Notification { get; set; } = new();
}

/// <summary>
/// 消息报告选项DTO
/// </summary>
public class MessageReportOptions
{
    /// <summary>
    /// 是否包含总览统计
    /// </summary>
    public bool IncludeSummary { get; set; } = true;

    /// <summary>
    /// 是否包含趋势分析
    /// </summary>
    public bool IncludeTrends { get; set; } = true;

    /// <summary>
    /// 是否包含用户分析
    /// </summary>
    public bool IncludeUserAnalysis { get; set; } = true;

    /// <summary>
    /// 是否包含会话分析
    /// </summary>
    public bool IncludeConversationAnalysis { get; set; } = true;

    /// <summary>
    /// 是否包含附件统计
    /// </summary>
    public bool IncludeAttachmentStats { get; set; } = true;

    /// <summary>
    /// 是否包含性能指标
    /// </summary>
    public bool IncludePerformanceMetrics { get; set; } = false;

    /// <summary>
    /// 是否包含自定义指标
    /// </summary>
    public bool IncludeCustomMetrics { get; set; } = false;

    /// <summary>
    /// 自定义指标定义
    /// </summary>
    public List<CustomMetricDefinition> CustomMetrics { get; set; } = new();

    /// <summary>
    /// 数据采样方式
    /// </summary>
    public DataSamplingMethod SamplingMethod { get; set; } = DataSamplingMethod.None;

    /// <summary>
    /// 采样率（0-1）
    /// </summary>
    [Range(0, 1, ErrorMessage = "采样率必须在0-1之间")]
    public double SamplingRate { get; set; } = 1.0;

    /// <summary>
    /// 数据精度
    /// </summary>
    public DataPrecision Precision { get; set; } = DataPrecision.Normal;

    /// <summary>
    /// 缓存选项
    /// </summary>
    public ReportCacheOptions Cache { get; set; } = new();
}

/// <summary>
/// 消息报告响应DTO
/// </summary>
public class MessageReportResponseDto
{
    /// <summary>
    /// 报告ID
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// 报告类型
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// 报告标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 报告描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 报告状态
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// 生成进度（0-100）
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 报告数据
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 报告图表
    /// </summary>
    public List<ReportChart> Charts { get; set; } = new();

    /// <summary>
    /// 报告摘要
    /// </summary>
    public ReportSummary Summary { get; set; } = new();

    /// <summary>
    /// 时间范围
    /// </summary>
    public DateRangeDto DateRange { get; set; } = new();

    /// <summary>
    /// 生成时间
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// 文件URL
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// 下载链接
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 处理时间（秒）
    /// </summary>
    public double ProcessingTime { get; set; }

    /// <summary>
    /// 数据行数
    /// </summary>
    public int DataRows { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 警告信息
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

// 辅助类和枚举
/// <summary>
/// 压缩格式枚举
/// </summary>
public enum CompressionFormat
{
    /// <summary>
    /// ZIP格式
    /// </summary>
    Zip = 0,

    /// <summary>
    /// GZIP格式
    /// </summary>
    Gzip = 1,

    /// <summary>
    /// RAR格式
    /// </summary>
    Rar = 2,

    /// <summary>
    /// 7Z格式
    /// </summary>
    SevenZip = 3
}

/// <summary>
/// 加密算法枚举
/// </summary>
public enum EncryptionAlgorithm
{
    /// <summary>
    /// AES-256
    /// </summary>
    AES256 = 0,

    /// <summary>
    /// AES-128
    /// </summary>
    AES128 = 1,

    /// <summary>
    /// RSA-2048
    /// </summary>
    RSA2048 = 2,

    /// <summary>
    /// RSA-4096
    /// </summary>
    RSA4096 = 3
}

/// <summary>
/// 通知方式枚举
/// </summary>
public enum NotificationMethod
{
    /// <summary>
    /// 邮件通知
    /// </summary>
    Email = 0,

    /// <summary>
    /// 短信通知
    /// </summary>
    SMS = 1,

    /// <summary>
    /// Webhook通知
    /// </summary>
    Webhook = 2,

    /// <summary>
    /// 推送通知
    /// </summary>
    Push = 3
}

/// <summary>
/// 导出状态枚举
/// </summary>
public enum ExportStatus
{
    /// <summary>
    /// 等待中
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 处理中
    /// </summary>
    Processing = 1,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 5
}

/// <summary>
/// 报告类型枚举
/// </summary>
public enum ReportType
{
    /// <summary>
    /// 消息统计报告
    /// </summary>
    MessageStatistics = 0,

    /// <summary>
    /// 用户活跃度报告
    /// </summary>
    UserActivity = 1,

    /// <summary>
    /// 会话分析报告
    /// </summary>
    ConversationAnalysis = 2,

    /// <summary>
    /// 附件统计报告
    /// </summary>
    AttachmentStatistics = 3,

    /// <summary>
    /// 性能分析报告
    /// </summary>
    PerformanceAnalysis = 4,

    /// <summary>
    /// 自定义报告
    /// </summary>
    Custom = 99
}

/// <summary>
/// 报告分组方式枚举
/// </summary>
public enum ReportGroupBy
{
    /// <summary>
    /// 按日期分组
    /// </summary>
    Date = 0,

    /// <summary>
    /// 按用户分组
    /// </summary>
    User = 1,

    /// <summary>
    /// 按会话分组
    /// </summary>
    Conversation = 2,

    /// <summary>
    /// 按消息类型分组
    /// </summary>
    MessageType = 3,

    /// <summary>
    /// 按优先级分组
    /// </summary>
    Priority = 4,

    /// <summary>
    /// 按标签分组
    /// </summary>
    Tag = 5
}

/// <summary>
/// 报告聚合函数枚举
/// </summary>
public enum ReportAggregateFunction
{
    /// <summary>
    /// 计数
    /// </summary>
    Count = 0,

    /// <summary>
    /// 求和
    /// </summary>
    Sum = 1,

    /// <summary>
    /// 平均值
    /// </summary>
    Average = 2,

    /// <summary>
    /// 最大值
    /// </summary>
    Max = 3,

    /// <summary>
    /// 最小值
    /// </summary>
    Min = 4
}

/// <summary>
/// 报告排序字段枚举
/// </summary>
public enum ReportSortField
{
    /// <summary>
    /// 按日期排序
    /// </summary>
    Date = 0,

    /// <summary>
    /// 按用户排序
    /// </summary>
    User = 1,

    /// <summary>
    /// 按消息数量排序
    /// </summary>
    MessageCount = 2,

    /// <summary>
    /// 按总大小排序
    /// </summary>
    TotalSize = 3
}

/// <summary>
/// 图表类型枚举
/// </summary>
public enum ChartType
{
    /// <summary>
    /// 柱状图
    /// </summary>
    Bar = 0,

    /// <summary>
    /// 折线图
    /// </summary>
    Line = 1,

    /// <summary>
    /// 饼图
    /// </summary>
    Pie = 2,

    /// <summary>
    /// 散点图
    /// </summary>
    Scatter = 3,

    /// <summary>
    /// 面积图
    /// </summary>
    Area = 4
}

/// <summary>
/// 报告状态枚举
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// 等待中
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 生成中
    /// </summary>
    Generating = 1,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// 自定义指标定义DTO
/// </summary>
public class CustomMetricDefinition
{
    /// <summary>
    /// 指标名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 指标描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 计算公式
    /// </summary>
    public string Formula { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; } = "number";

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }
}

/// <summary>
/// 数据采样方式枚举
/// </summary>
public enum DataSamplingMethod
{
    /// <summary>
    /// 无采样
    /// </summary>
    None = 0,

    /// <summary>
    /// 随机采样
    /// </summary>
    Random = 1,

    /// <summary>
    /// 系统采样
    /// </summary>
    Systematic = 2,

    /// <summary>
    /// 分层采样
    /// </summary>
    Stratified = 3
}

/// <summary>
/// 数据精度枚举
/// </summary>
public enum DataPrecision
{
    /// <summary>
    /// 低精度
    /// </summary>
    Low = 0,

    /// <summary>
    /// 普通精度
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 高精度
    /// </summary>
    High = 2
}

/// <summary>
/// 报告缓存选项DTO
/// </summary>
public class ReportCacheOptions
{
    /// <summary>
    /// 是否启用缓存
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 缓存时间（分钟）
    /// </summary>
    [Range(1, 1440, ErrorMessage = "缓存时间必须在1-1440分钟之间")]
    public int CacheDuration { get; set; } = 60;

    /// <summary>
    /// 缓存键
    /// </summary>
    public string? CacheKey { get; set; }
}

/// <summary>
/// 调度选项DTO
/// </summary>
public class ReportScheduleOptions
{
    /// <summary>
    /// 是否启用调度
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 调度类型
    /// </summary>
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Once;

    /// <summary>
    /// 执行时间
    /// </summary>
    public DateTime? ExecuteAt { get; set; }

    /// <summary>
    /// 重复间隔
    /// </summary>
    public TimeSpan? Interval { get; set; }

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string? CronExpression { get; set; }
}

/// <summary>
/// 消息报告通知选项DTO
/// </summary>
public class MessageReportNotificationOptions
{
    /// <summary>
    /// 是否启用通知
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 通知邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 通知Webhook URL
    /// </summary>
    [StringLength(500, ErrorMessage = "Webhook URL长度不能超过500个字符")]
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// 报告图表DTO
/// </summary>
public class ReportChart
{
    /// <summary>
    /// 图表标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 图表类型
    /// </summary>
    public ChartType Type { get; set; }

    /// <summary>
    /// 图表数据
    /// </summary>
    public object Data { get; set; } = new();

    /// <summary>
    /// 图表配置
    /// </summary>
    public object Config { get; set; } = new();
}

/// <summary>
/// 报告摘要DTO
/// </summary>
public class ReportSummary
{
    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 总用户数
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// 总会话数
    /// </summary>
    public int TotalConversations { get; set; }

    /// <summary>
    /// 总附件数
    /// </summary>
    public int TotalAttachments { get; set; }

    /// <summary>
    /// 总大小（字节）
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 平均响应时间（秒）
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// 峰值时段
    /// </summary>
    public string PeakHours { get; set; } = string.Empty;
}

/// <summary>
/// 调度类型枚举
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// 执行一次
    /// </summary>
    Once = 0,

    /// <summary>
    /// 每日执行
    /// </summary>
    Daily = 1,

    /// <summary>
    /// 每周执行
    /// </summary>
    Weekly = 2,

    /// <summary>
    /// 每月执行
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// 自定义调度
    /// </summary>
    Custom = 4
}