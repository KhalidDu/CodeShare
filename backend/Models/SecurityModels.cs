using System.Text.Json.Serialization;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 文件上传信息
/// </summary>
public class FileUploadInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件内容类型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 文件数据
    /// </summary>
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize => FileData.Length;
}

/// <summary>
/// 上传上下文
/// </summary>
public class UploadContext
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 上传上下文类型
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// 用户IP地址
    /// </summary>
    public string? UserIP { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }
}

/// <summary>
/// 文件分享请求
/// </summary>
public class FileShareRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// 访问密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 访问级别
    /// </summary>
    public FileAccessLevel AccessLevel { get; set; } = FileAccessLevel.Read;
}

/// <summary>
/// 上传请求
/// </summary>
public class UploadRequest
{
    /// <summary>
    /// 文件数量
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// 总大小
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户IP地址
    /// </summary>
    public string? UserIP { get; set; }
}

/// <summary>
/// 病毒扫描请求
/// </summary>
public class VirusScanRequest
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件数据
    /// </summary>
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// 文件操作类型
/// </summary>
public enum FileOperation
{
    Read,
    Write,
    Delete,
    Download,
    Upload,
    Share,
    Preview
}

/// <summary>
/// 文件访问级别
/// </summary>
public enum FileAccessLevel
{
    None,
    Read,
    Write,
    Full
}

/// <summary>
/// 上传状态
/// </summary>
public enum UploadStatus
{
    Success,
    Failed,
    Blocked,
    Quarantined
}

/// <summary>
/// 请求风险级别
/// </summary>
public enum RequestRiskLevel
{
    None,
    Low,
    Medium,
    High
}

/// <summary>
/// 文件类型配置
/// </summary>
public class FileTypeConfig
{
    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// MIME类型
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// 最大文件大小
    /// </summary>
    public long MaxSize { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 允许的上下文
    /// </summary>
    public List<string> AllowedContexts { get; set; } = new();
}

/// <summary>
/// 文件大小限制
/// </summary>
public class FileSizeLimit
{
    /// <summary>
    /// 最大文件大小
    /// </summary>
    public long MaxFileSize { get; set; }

    /// <summary>
    /// 总大小限制
    /// </summary>
    public long MaxTotalSize { get; set; }

    /// <summary>
    /// 最大文件数量
    /// </summary>
    public int MaxFiles { get; set; }

    /// <summary>
    /// 上下文类型
    /// </summary>
    public string Context { get; set; } = string.Empty;
}

/// <summary>
/// 文件安全配置
/// </summary>
public class FileSecurityConfig
{
    /// <summary>
    /// 启用病毒扫描
    /// </summary>
    public bool EnableVirusScanning { get; set; } = true;

    /// <summary>
    /// 启用内容分析
    /// </summary>
    public bool EnableContentAnalysis { get; set; } = true;

    /// <summary>
    /// 启用文件哈希检查
    /// </summary>
    public bool EnableFileHashCheck { get; set; } = true;

    /// <summary>
    /// 启用大小验证
    /// </summary>
    public bool EnableSizeValidation { get; set; } = true;

    /// <summary>
    /// 启用类型验证
    /// </summary>
    public bool EnableTypeValidation { get; set; } = true;

    /// <summary>
    /// 默认最大文件大小
    /// </summary>
    public long DefaultMaxFileSize { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// 最大总上传大小
    /// </summary>
    public long MaxTotalUploadSize { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// 每次上传最大文件数
    /// </summary>
    public int MaxFilesPerUpload { get; set; } = 10;

    /// <summary>
    /// 允许的文件类型
    /// </summary>
    public List<FileTypeConfig> AllowedFileTypes { get; set; } = new();

    /// <summary>
    /// 阻止的扩展名
    /// </summary>
    public string[] BlockedExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 安全的MIME类型
    /// </summary>
    public string[] SafeMimeTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 启用日志记录
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// 启用通知
    /// </summary>
    public bool EnableNotifications { get; set; } = true;
}

/// <summary>
/// 文件上传事件
/// </summary>
public class FileUploadEvent
{
    /// <summary>
    /// 文件ID
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 用户IP地址
    /// </summary>
    public string? UserIP { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 上传状态
    /// </summary>
    public UploadStatus Status { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 文件下载事件
/// </summary>
public class FileDownloadEvent
{
    /// <summary>
    /// 文件ID
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 下载时间
    /// </summary>
    public DateTime DownloadTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 用户IP地址
    /// </summary>
    public string? UserIP { get; set; }
}

/// <summary>
/// 文件安全事件
/// </summary>
public class FileSecurityEvent
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 事件描述
    /// </summary>
    public string EventDescription { get; set; } = string.Empty;

    /// <summary>
    /// 严重程度
    /// </summary>
    public SecurityLevel Severity { get; set; }

    /// <summary>
    /// 文件ID
    /// </summary>
    public Guid? FileId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 用户IP地址
    /// </summary>
    public string? UserIP { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 文件操作日志筛选条件
/// </summary>
public class FileOperationLogFilter
{
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string? OperationType { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 文件操作日志
/// </summary>
public class FileOperationLog
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 文件ID
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// 操作详情
    /// </summary>
    public string OperationDetails { get; set; } = string.Empty;

    /// <summary>
    /// 用户IP地址
    /// </summary>
    public string? UserIP { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime OperationTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 操作状态
    /// </summary>
    public string Status { get; set; } = "Success";
}

/// <summary>
/// 文件安全统计
/// </summary>
public class FileSecurityStats
{
    /// <summary>
    /// 总上传数
    /// </summary>
    public int TotalUploads { get; set; }

    /// <summary>
    /// 成功上传数
    /// </summary>
    public int SuccessfulUploads { get; set; }

    /// <summary>
    /// 失败上传数
    /// </summary>
    public int FailedUploads { get; set; }

    /// <summary>
    /// 总下载数
    /// </summary>
    public int TotalDownloads { get; set; }

    /// <summary>
    /// 总上传数据量
    /// </summary>
    public long TotalDataUploaded { get; set; }

    /// <summary>
    /// 总下载数据量
    /// </summary>
    public long TotalDataDownloaded { get; set; }

    /// <summary>
    /// 安全事件数
    /// </summary>
    public int SecurityEvents { get; set; }

    /// <summary>
    /// 病毒检测数
    /// </summary>
    public int VirusDetections { get; set; }

    /// <summary>
    /// 文件类型统计
    /// </summary>
    public List<FileTypeStats> FileTypeStats { get; set; } = new();

    /// <summary>
    /// 每日统计
    /// </summary>
    public List<DailyFileStats> DailyStats { get; set; } = new();
}

/// <summary>
/// 文件类型统计
/// </summary>
public class FileTypeStats
{
    /// <summary>
    /// 文件类型
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// 上传次数
    /// </summary>
    public int UploadCount { get; set; }

    /// <summary>
    /// 总大小
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 平均大小
    /// </summary>
    public long AverageSize { get; set; }

    /// <summary>
    /// 安全事件数
    /// </summary>
    public int SecurityEvents { get; set; }
}

/// <summary>
/// 每日文件统计
/// </summary>
public class DailyFileStats
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 上传数
    /// </summary>
    public int Uploads { get; set; }

    /// <summary>
    /// 下载数
    /// </summary>
    public int Downloads { get; set; }

    /// <summary>
    /// 数据传输量
    /// </summary>
    public long DataTransferred { get; set; }

    /// <summary>
    /// 安全事件数
    /// </summary>
    public int SecurityEvents { get; set; }
}

/// <summary>
/// 分页结果
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>
    /// 项目列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}