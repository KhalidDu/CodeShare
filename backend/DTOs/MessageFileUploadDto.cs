using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 文件上传请求DTO - 用于消息附件上传
/// </summary>
public class FileUploadRequestDto
{
    /// <summary>
    /// 文件数据（Base64编码）
    /// </summary>
    [Required(ErrorMessage = "文件数据不能为空")]
    public string FileData { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    [StringLength(255, ErrorMessage = "文件名长度不能超过255个字符")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型（MIME类型）
    /// </summary>
    [StringLength(100, ErrorMessage = "文件类型长度不能超过100个字符")]
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "文件大小必须大于等于0")]
    public long FileSize { get; set; }

    /// <summary>
    /// 附件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; } = AttachmentType.Other;

    /// <summary>
    /// 关联的消息ID（如果已有消息）
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// 关联的草稿ID（如果是草稿附件）
    /// </summary>
    public Guid? DraftId { get; set; }

    /// <summary>
    /// 文件描述
    /// </summary>
    [StringLength(500, ErrorMessage = "文件描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否生成缩略图
    /// </summary>
    public bool GenerateThumbnail { get; set; } = false;

    /// <summary>
    /// 缩略图宽度
    /// </summary>
    [Range(1, 2000, ErrorMessage = "缩略图宽度必须在1-2000像素之间")]
    public int? ThumbnailWidth { get; set; } = 200;

    /// <summary>
    /// 缩略图高度
    /// </summary>
    [Range(1, 2000, ErrorMessage = "缩略图高度必须在1-2000像素之间")]
    public int? ThumbnailHeight { get; set; } = 200;

    /// <summary>
    /// 是否压缩图片
    /// </summary>
    public bool CompressImage { get; set; } = true;

    /// <summary>
    /// 图片质量（1-100）
    /// </summary>
    [Range(1, 100, ErrorMessage = "图片质量必须在1-100之间")]
    public int ImageQuality { get; set; } = 85;

    /// <summary>
    /// 最大文件大小（字节）
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "最大文件大小必须大于0")]
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// 允许的文件类型（用逗号分隔）
    /// </summary>
    public string? AllowedFileTypes { get; set; } = "image/jpeg,image/png,image/gif,image/webp,application/pdf,text/plain,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <summary>
    /// 是否需要病毒扫描
    /// </summary>
    public bool RequireVirusScan { get; set; } = true;

    /// <summary>
    /// 上传进度回调URL
    /// </summary>
    [StringLength(500, ErrorMessage = "回调URL长度不能超过500个字符")]
    public string? ProgressCallbackUrl { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// 文件上传响应DTO
/// </summary>
public class FileUploadResponseDto
{
    /// <summary>
    /// 上传是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 附件ID
    /// </summary>
    public Guid? AttachmentId { get; set; }

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
    /// 缩略图URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 上传进度（0-100）
    /// </summary>
    public int UploadProgress { get; set; }

    /// <summary>
    /// 上传状态
    /// </summary>
    public UploadStatus Status { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// 文件哈希值
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// 病毒扫描结果
    /// </summary>
    public VirusScanResult? VirusScanResult { get; set; }

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTime { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// 上传状态枚举
/// </summary>
public enum UploadStatus
{
    /// <summary>
    /// 上传中
    /// </summary>
    Uploading = 0,

    /// <summary>
    /// 上传成功
    /// </summary>
    Success = 1,

    /// <summary>
    /// 上传失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 上传取消
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// 文件过大
    /// </summary>
    FileTooLarge = 4,

    /// <summary>
    /// 文件类型不支持
    /// </summary>
    UnsupportedFileType = 5,

    /// <summary>
    /// 病毒检测中
    /// </summary>
    VirusScanning = 6,

    /// <summary>
    /// 检测到病毒
    /// </summary>
    VirusDetected = 7,

    /// <summary>
    /// 处理中
    /// </summary>
    Processing = 8
}

/// <summary>
/// 病毒扫描结果DTO
/// </summary>
public class VirusScanResult
{
    /// <summary>
    /// 是否扫描完成
    /// </summary>
    public bool IsScanCompleted { get; set; }

    /// <summary>
    /// 是否检测到病毒
    /// </summary>
    public bool IsVirusDetected { get; set; }

    /// <summary>
    /// 病毒名称
    /// </summary>
    public string? VirusName { get; set; }

    /// <summary>
    /// 扫描引擎
    /// </summary>
    public string? ScanEngine { get; set; }

    /// <summary>
    /// 扫描时间
    /// </summary>
    public DateTime? ScannedAt { get; set; }

    /// <summary>
    /// 扫描详情
    /// </summary>
    public string? ScanDetails { get; set; }
}

/// <summary>
/// 批量文件上传请求DTO
/// </summary>
public class BatchFileUploadRequestDto
{
    /// <summary>
    /// 文件列表
    /// </summary>
    [Required(ErrorMessage = "文件列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要上传一个文件")]
    [MaxLength(50, ErrorMessage = "一次最多上传50个文件")]
    public List<FileUploadRequestDto> Files { get; set; } = new();

    /// <summary>
    /// 关联的消息ID
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// 关联的草稿ID
    /// </summary>
    public Guid? DraftId { get; set; }

    /// <summary>
    /// 是否并行上传
    /// </summary>
    public bool ParallelUpload { get; set; } = true;

    /// <summary>
    /// 最大并发数
    /// </summary>
    [Range(1, 10, ErrorMessage = "最大并发数必须在1-10之间")]
    public int MaxConcurrency { get; set; } = 3;

    /// <summary>
    /// 上传超时时间（秒）
    /// </summary>
    [Range(30, 3600, ErrorMessage = "上传超时时间必须在30-3600秒之间")]
    public int UploadTimeout { get; set; } = 300;
}

/// <summary>
/// 批量文件上传响应DTO
/// </summary>
public class BatchFileUploadResponseDto
{
    /// <summary>
    /// 总文件数
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// 成功上传的文件数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败的文件数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 上传结果列表
    /// </summary>
    public List<FileUploadResponseDto> Results { get; set; } = new();

    /// <summary>
    /// 总上传大小（字节）
    /// </summary>
    public long TotalFileSize { get; set; }

    /// <summary>
    /// 总上传时间（毫秒）
    /// </summary>
    public long TotalUploadTime { get; set; }

    /// <summary>
    /// 平均上传速度（字节/秒）
    /// </summary>
    public double AverageUploadSpeed { get; set; }

    /// <summary>
    /// 批量上传ID
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// 上传状态
    /// </summary>
    public BatchUploadStatus Status { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// 批量上传状态枚举
/// </summary>
public enum BatchUploadStatus
{
    /// <summary>
    /// 上传中
    /// </summary>
    Uploading = 0,

    /// <summary>
    /// 上传完成
    /// </summary>
    Completed = 1,

    /// <summary>
    /// 上传失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 部分失败
    /// </summary>
    PartialFailed = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// 文件删除请求DTO
/// </summary>
public class FileDeleteRequestDto
{
    /// <summary>
    /// 附件ID列表
    /// </summary>
    [Required(ErrorMessage = "附件ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要选择一个附件")]
    public List<Guid> AttachmentIds { get; set; } = new();

    /// <summary>
    /// 删除原因
    /// </summary>
    [StringLength(500, ErrorMessage = "删除原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 是否物理删除
    /// </summary>
    public bool PermanentDelete { get; set; } = false;

    /// <summary>
    /// 当前用户ID
    /// </summary>
    public Guid CurrentUserId { get; set; }
}

/// <summary>
/// 文件删除响应DTO
/// </summary>
public class FileDeleteResponseDto
{
    /// <summary>
    /// 删除是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 成功删除的附件ID列表
    /// </summary>
    public List<Guid> DeletedAttachmentIds { get; set; } = new();

    /// <summary>
    /// 删除失败的附件ID列表
    /// </summary>
    public List<Guid> FailedAttachmentIds { get; set; } = new();

    /// <summary>
    /// 删除失败的错误信息
    /// </summary>
    public Dictionary<Guid, string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime DeletedAt { get; set; }

    /// <summary>
    /// 删除的文件总大小（字节）
    /// </summary>
    public long TotalDeletedFileSize { get; set; }
}