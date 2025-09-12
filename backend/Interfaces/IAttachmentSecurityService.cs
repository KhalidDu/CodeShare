using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 附件安全服务接口 - 提供附件安全检查和文件上传安全功能
/// 遵循接口隔离原则和单一职责原则
/// </summary>
public interface IAttachmentSecurityService
{
    // 文件类型验证
    #region 文件类型验证

    /// <summary>
    /// 验证文件类型安全性
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="fileData">文件数据</param>
    /// <returns>文件类型验证结果</returns>
    Task<FileTypeValidationResult> ValidateFileTypeAsync(string fileName, string contentType, byte[] fileData);

    /// <summary>
    /// 验证文件扩展名安全性
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="allowedExtensions">允许的扩展名</param>
    /// <returns>扩展名验证结果</returns>
    Task<FileExtensionValidationResult> ValidateFileExtensionAsync(string fileName, IEnumerable<string> allowedExtensions);

    /// <summary>
    /// 检测文件真实类型
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <returns>文件真实类型检测结果</returns>
    Task<FileTypeDetectionResult> DetectRealFileTypeAsync(byte[] fileData);

    /// <summary>
    /// 验证MIME类型安全性
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <param name="fileData">文件数据</param>
    /// <returns>MIME类型验证结果</returns>
    Task<MimeTypeValidationResult> ValidateMimeTypeAsync(string contentType, byte[] fileData);

    /// <summary>
    /// 检查文件是否为可执行文件
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <returns>是否为可执行文件</returns>
    Task<bool> IsExecutableFileAsync(byte[] fileData);

    #endregion

    // 文件内容安全检查
    #region 文件内容安全检查

    /// <summary>
    /// 扫描文件内容安全性
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <param name="fileName">文件名</param>
    /// <returns>文件内容安全扫描结果</returns>
    Task<FileContentSecurityResult> ScanFileContentAsync(byte[] fileData, string fileName);

    /// <summary>
    /// 检查文件是否包含恶意代码
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <returns>恶意代码检测结果</returns>
    Task<MalwareDetectionResult> DetectMalwareAsync(byte[] fileData);

    /// <summary>
    /// 验证代码文件安全性
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <param name="language">编程语言</param>
    /// <returns>代码文件安全验证结果</returns>
    Task<CodeFileSecurityResult> ValidateCodeFileAsync(byte[] fileData, string language);

    /// <summary>
    /// 检查文件是否包含敏感信息
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <param name="fileType">文件类型</param>
    /// <returns>敏感信息检测结果</returns>
    Task<SensitiveDataResult> DetectSensitiveDataAsync(byte[] fileData, string fileType);

    /// <summary>
    /// 验证图片文件安全性
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <returns>图片文件安全验证结果</returns>
    Task<ImageSecurityResult> ValidateImageFileAsync(byte[] fileData);

    #endregion

    // 文件大小和限制检查
    #region 文件大小和限制检查

    /// <summary>
    /// 验证文件大小
    /// </summary>
    /// <param name="fileSize">文件大小</param>
    /// <param name="maxSize">最大允许大小</param>
    /// <returns>文件大小验证结果</returns>
    Task<FileSizeValidationResult> ValidateFileSizeAsync(long fileSize, long maxSize);

    /// <summary>
    /// 检查文件大小限制
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <param name="context">上传上下文</param>
    /// <returns>文件大小限制检查结果</returns>
    Task<FileSizeLimitResult> CheckFileSizeLimitAsync(byte[] fileData, UploadContext context);

    /// <summary>
    /// 验证批量上传大小限制
    /// </summary>
    /// <param name="files">文件列表</param>
    /// <param name="maxTotalSize">最大总大小</param>
    /// <returns>批量上传验证结果</returns>
    Task<BatchUploadValidationResult> ValidateBatchUploadAsync(IEnumerable<FileUploadInfo> files, long maxTotalSize);

    #endregion

    // 文件上传安全
    #region 文件上传安全

    /// <summary>
    /// 验证文件上传安全性
    /// </summary>
    /// <param name="file">文件信息</param>
    /// <param name="uploadContext">上传上下文</param>
    /// <returns>文件上传安全验证结果</returns>
    Task<FileUploadSecurityResult> ValidateFileUploadAsync(FileUploadInfo file, UploadContext uploadContext);

    /// <summary>
    /// 验证批量文件上传安全性
    /// </summary>
    /// <param name="files">文件列表</param>
    /// <param name="uploadContext">上传上下文</param>
    /// <returns>批量文件上传安全验证结果</returns>
    Task<BatchFileUploadSecurityResult> ValidateBatchFileUploadAsync(IEnumerable<FileUploadInfo> files, UploadContext uploadContext);

    /// <summary>
    /// 生成安全的文件名
    /// </summary>
    /// <param name="originalFileName">原始文件名</param>
    /// <returns>安全的文件名</returns>
    Task<string> GenerateSecureFileNameAsync(string originalFileName);

    /// <summary>
    /// 验证上传请求的合法性
    /// </summary>
    /// <param name="request">上传请求</param>
    /// <returns>上传请求验证结果</returns>
    Task<UploadRequestValidationResult> ValidateUploadRequestAsync(UploadRequest request);

    #endregion

    // 病毒扫描
    #region 病毒扫描

    /// <summary>
    /// 扫描文件病毒
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <returns>病毒扫描结果</returns>
    Task<VirusScanResult> ScanForVirusesAsync(byte[] fileData);

    /// <summary>
    /// 批量扫描文件病毒
    /// </summary>
    /// <param name="files">文件列表</param>
    /// <returns>批量病毒扫描结果</returns>
    Task<BatchVirusScanResult> BatchScanForVirusesAsync(IEnumerable<VirusScanRequest> files);

    /// <summary>
    /// 检查文件哈希是否在黑名单中
    /// </summary>
    /// <param name="fileData">文件数据</param>
    /// <returns>文件哈希检查结果</returns>
    Task<FileHashCheckResult> CheckFileHashBlacklistAsync(byte[] fileData);

    #endregion

    // 文件操作安全
    #region 文件操作安全

    /// <summary>
    /// 验证文件下载安全性
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>文件下载安全验证结果</returns>
    Task<FileDownloadSecurityResult> ValidateFileDownloadAsync(string fileId, Guid userId);

    /// <summary>
    /// 验证文件删除权限
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>文件删除权限验证结果</returns>
    Task<FileDeleteSecurityResult> ValidateFileDeleteAsync(string fileId, Guid userId);

    /// <summary>
    /// 验证文件访问权限
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="operation">操作类型</param>
    /// <returns>文件访问权限验证结果</returns>
    Task<FileAccessSecurityResult> ValidateFileAccessAsync(string fileId, Guid userId, FileOperation operation);

    /// <summary>
    /// 验证文件分享安全性
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="shareRequest">分享请求</param>
    /// <returns>文件分享安全验证结果</returns>
    Task<FileShareSecurityResult> ValidateFileShareAsync(string fileId, FileShareRequest shareRequest);

    #endregion

    // 配置和策略管理
    #region 配置和策略管理

    /// <summary>
    /// 获取文件安全配置
    /// </summary>
    /// <returns>文件安全配置</returns>
    Task<FileSecurityConfig> GetFileSecurityConfigAsync();

    /// <summary>
    /// 更新文件安全配置
    /// </summary>
    /// <param name="config">文件安全配置</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateFileSecurityConfigAsync(FileSecurityConfig config);

    /// <summary>
    /// 获取允许的文件类型
    /// </summary>
    /// <param name="context">上下文</param>
    /// <returns>允许的文件类型列表</returns>
    Task<IEnumerable<FileTypeConfig>> GetAllowedFileTypesAsync(string context);

    /// <summary>
    /// 添加允许的文件类型
    /// </summary>
    /// <param name="fileType">文件类型配置</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddAllowedFileTypeAsync(FileTypeConfig fileType);

    /// <summary>
    /// 移除允许的文件类型
    /// </summary>
    /// <param name="extension">文件扩展名</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveAllowedFileTypeAsync(string extension);

    /// <summary>
    /// 获取文件大小限制
    /// </summary>
    /// <param name="context">上下文</param>
    /// <returns>文件大小限制</returns>
    Task<FileSizeLimit> GetFileSizeLimitAsync(string context);

    #endregion

    // 审计和日志
    #region 审计和日志

    /// <summary>
    /// 记录文件上传事件
    /// </summary>
    /// <param name="uploadEvent">上传事件</param>
    /// <returns>是否记录成功</returns>
    Task<bool> LogFileUploadEventAsync(FileUploadEvent uploadEvent);

    /// <summary>
    /// 记录文件下载事件
    /// </summary>
    /// <param name="downloadEvent">下载事件</param>
    /// <returns>是否记录成功</returns>
    Task<bool> LogFileDownloadEventAsync(FileDownloadEvent downloadEvent);

    /// <summary>
    /// 记录文件安全事件
    /// </summary>
    /// <param name="securityEvent">安全事件</param>
    /// <returns>是否记录成功</returns>
    Task<bool> LogFileSecurityEventAsync(FileSecurityEvent securityEvent);

    /// <summary>
    /// 获取文件操作日志
    /// </summary>
    /// <param name="filter">日志筛选条件</param>
    /// <returns>文件操作日志</returns>
    Task<PaginatedResult<FileOperationLog>> GetFileOperationLogsAsync(FileOperationLogFilter filter);

    /// <summary>
    /// 获取文件安全统计
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>文件安全统计</returns>
    Task<FileSecurityStats> GetFileSecurityStatsAsync(DateTime startDate, DateTime endDate);

    #endregion
}

/// <summary>
/// 文件类型验证结果
/// </summary>
public class FileTypeValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string DetectedType { get; set; } = string.Empty;
    public string DeclaredType { get; set; } = string.Empty;
    public bool IsTypeMismatch { get; set; }
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
}

/// <summary>
/// 文件扩展名验证结果
/// </summary>
public class FileExtensionValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string Extension { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public bool IsDoubleExtension { get; set; }
    public IEnumerable<string> AlternativeExtensions { get; set; } = new List<string>();
}

/// <summary>
/// 文件类型检测结果
/// </summary>
public class FileTypeDetectionResult
{
    public string DetectedType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Signature { get; set; } = string.Empty;
    public IEnumerable<string> PossibleTypes { get; set; } = new List<string>();
    public bool IsReliable { get; set; }
}

/// <summary>
/// MIME类型验证结果
/// </summary>
public class MimeTypeValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string DeclaredMimeType { get; set; } = string.Empty;
    public string DetectedMimeType { get; set; } = string.Empty;
    public bool IsMismatch { get; set; }
    public bool IsWhitelisted { get; set; }
}

/// <summary>
/// 文件内容安全扫描结果
/// </summary>
public class FileContentSecurityResult
{
    public bool IsSafe { get; set; }
    public SecurityLevel RiskLevel { get; set; }
    public IEnumerable<string> SecurityIssues { get; set; } = new List<string>();
    public IEnumerable<string> Recommendations { get; set; } = new List<string>();
    public double RiskScore { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 恶意代码检测结果
/// </summary>
public class MalwareDetectionResult
{
    public bool IsClean { get; set; }
    public IEnumerable<string> DetectedMalware { get; set; } = new List<string>();
    public MalwareRiskLevel RiskLevel { get; set; }
    public IEnumerable<string> ThreatTypes { get; set; } = new List<string>();
    public string? EngineName { get; set; }
    public DateTime ScanTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 代码文件安全验证结果
/// </summary>
public class CodeFileSecurityResult
{
    public bool IsValid { get; set; }
    public string Language { get; set; } = string.Empty;
    public IEnumerable<string> SecurityWarnings { get; set; } = new List<string>();
    public IEnumerable<string> SecurityErrors { get; set; } = new List<string>();
    public CodeRiskLevel RiskLevel { get; set; }
    public bool ContainsDangerousFunctions { get; set; }
    public bool ContainsObfuscatedCode { get; set; }
    public bool ContainsSuspiciousPatterns { get; set; }
}

/// <summary>
/// 敏感信息检测结果
/// </summary>
public class SensitiveDataResult
{
    public bool ContainsSensitiveData { get; set; }
    public IEnumerable<SensitiveDataItem> DetectedItems { get; set; } = new List<SensitiveDataItem>();
    public SensitivityLevel SensitivityLevel { get; set; }
    public IEnumerable<string> DataCategories { get; set; } = new List<string>();
    public int TotalMatches { get; set; }
}

/// <summary>
/// 敏感数据项
/// </summary>
public class SensitiveDataItem
{
    public string DataType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public SensitivityLevel Sensitivity { get; set; }
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
}

/// <summary>
/// 图片文件安全验证结果
/// </summary>
public class ImageSecurityResult
{
    public bool IsValid { get; set; }
    public string ImageFormat { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize { get; set; }
    public bool ContainsExifData { get; set; }
    public bool IsAnimated { get; set; }
    public bool IsSteganographyDetected { get; set; }
    public IEnumerable<string> SecurityIssues { get; set; } = new List<string>();
}

/// <summary>
/// 文件大小验证结果
/// </summary>
public class FileSizeValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSize { get; set; }
    public long MaxAllowedSize { get; set; }
    public double SizePercentage { get; set; }
}

/// <summary>
/// 文件大小限制检查结果
/// </summary>
public class FileSizeLimitResult
{
    public bool IsWithinLimit { get; set; }
    public long CurrentSize { get; set; }
    public long Limit { get; set; }
    public long RemainingSize { get; set; }
    public string? LimitType { get; set; }
}

/// <summary>
/// 批量上传验证结果
/// </summary>
public class BatchUploadValidationResult
{
    public bool IsValid { get; set; }
    public int TotalFiles { get; set; }
    public int ValidFiles { get; set; }
    public int InvalidFiles { get; set; }
    public long TotalSize { get; set; }
    public long MaxTotalSize { get; set; }
    public IEnumerable<FileValidationResult> FileResults { get; set; } = new List<FileValidationResult>();
}

/// <summary>
/// 文件验证结果
/// </summary>
public class FileValidationResult
{
    public string FileName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// 文件上传安全验证结果
/// </summary>
public class FileUploadSecurityResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string SecureFileName { get; set; } = string.Empty;
    public IEnumerable<string> SecurityWarnings { get; set; } = new List<string>();
    public IEnumerable<string> SecurityRecommendations { get; set; } = new List<string>();
    public SecurityLevel RiskLevel { get; set; }
    public FileSecurityScore SecurityScore { get; set; }
}

/// <summary>
/// 批量文件上传安全验证结果
/// </summary>
public class BatchFileUploadSecurityResult
{
    public bool AllFilesValid { get; set; }
    public int TotalFiles { get; set; }
    public int ValidFiles { get; set; }
    public int InvalidFiles { get; set; }
    public IEnumerable<FileUploadSecurityResult> FileResults { get; set; } = new List<FileUploadSecurityResult>();
    public IEnumerable<string> GlobalWarnings { get; set; } = new List<string>();
    public IEnumerable<string> GlobalRecommendations { get; set; } = new List<string>();
}

/// <summary>
/// 上传请求验证结果
/// </summary>
public class UploadRequestValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<string> ValidationIssues { get; set; } = new List<string>();
    public RequestRiskLevel RiskLevel { get; set; }
}

/// <summary>
/// 病毒扫描结果
/// </summary>
public class VirusScanResult
{
    public bool IsClean { get; set; }
    public IEnumerable<string> DetectedViruses { get; set; } = new List<string>();
    public VirusScanStatus Status { get; set; }
    public string? EngineVersion { get; set; }
    public string? DatabaseVersion { get; set; }
    public DateTime ScanTime { get; set; } = DateTime.UtcNow;
    public int ScanDurationMs { get; set; }
}

/// <summary>
/// 批量病毒扫描结果
/// </summary>
public class BatchVirusScanResult
{
    public int TotalFiles { get; set; }
    public int CleanFiles { get; set; }
    public int InfectedFiles { get; set; }
    public int ScanErrors { get; set; }
    public IEnumerable<VirusScanResult> Results { get; set; } = new List<VirusScanResult>();
    public DateTime BatchScanTime { get; set; } = DateTime.UtcNow;
    public int TotalScanDurationMs { get; set; }
}

/// <summary>
/// 病毒扫描请求
/// </summary>
public class VirusScanRequest
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
}

/// <summary>
/// 文件哈希检查结果
/// </summary>
public class FileHashCheckResult
{
    public bool IsBlacklisted { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string? BlacklistReason { get; set; }
    public DateTime? BlacklistedAt { get; set; }
    public IEnumerable<string> HashAlgorithms { get; set; } = new List<string>();
}

/// <summary>
/// 文件下载安全验证结果
/// </summary>
public class FileDownloadSecurityResult
{
    public bool IsAllowed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public DownloadQuotaInfo QuotaInfo { get; set; } = new DownloadQuotaInfo();
}

/// <summary>
/// 下载配额信息
/// </summary>
public class DownloadQuotaInfo
{
    public long DownloadedToday { get; set; }
    public long DailyLimit { get; set; }
    public long Remaining { get; set; }
    public DateTime ResetTime { get; set; }
}

/// <summary>
/// 文件删除权限验证结果
/// </summary>
public class FileDeleteSecurityResult
{
    public bool IsAllowed { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsOwner { get; set; }
    public bool HasAdminPrivileges { get; set; }
    public bool IsSystemFile { get; set; }
}

/// <summary>
/// 文件访问权限验证结果
/// </summary>
public class FileAccessSecurityResult
{
    public bool IsAllowed { get; set; }
    public string? ErrorMessage { get; set; }
    public FileAccessLevel AccessLevel { get; set; }
    public bool IsOwner { get; set; }
    public bool HasSharedAccess { get; set; }
    public DateTime? AccessExpiry { get; set; }
}

/// <summary>
/// 文件分享安全验证结果
/// </summary>
public class FileShareSecurityResult
{
    public bool IsAllowed { get; set; }
    public string? ErrorMessage { get; set; }
    public string ShareToken { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool HasPasswordProtection { get; set; }
    public ShareAccessLevel AccessLevel { get; set; }
}

/// <summary>
/// 文件安全配置
/// </summary>
public class FileSecurityConfig
{
    public bool EnableVirusScanning { get; set; } = true;
    public bool EnableContentAnalysis { get; set; } = true;
    public bool EnableFileHashCheck { get; set; } = true;
    public bool EnableSizeValidation { get; set; } = true;
    public bool EnableTypeValidation { get; set; } = true;
    public long DefaultMaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public long MaxTotalUploadSize { get; set; } = 100 * 1024 * 1024; // 100MB
    public int MaxFilesPerUpload { get; set; } = 10;
    public IEnumerable<FileTypeConfig> AllowedFileTypes { get; set; } = new List<FileTypeConfig>();
    public IEnumerable<string> BlockedExtensions { get; set; } = new List<string>();
    public IEnumerable<string> SafeMimeTypes { get; set; } = new List<string>();
    public bool EnableLogging { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
}

/// <summary>
/// 文件类型配置
/// </summary>
public class FileTypeConfig
{
    public string Extension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long MaxSize { get; set; }
    public bool IsEnabled { get; set; } = true;
    public IEnumerable<string> AllowedContexts { get; set; } = new List<string>();
}

/// <summary>
/// 文件大小限制
/// </summary>
public class FileSizeLimit
{
    public long MaxFileSize { get; set; }
    public long MaxTotalSize { get; set; }
    public int MaxFiles { get; set; }
    public string Context { get; set; } = string.Empty;
}

/// <summary>
/// 文件上传信息
/// </summary>
public class FileUploadInfo
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

/// <summary>
/// 上传上下文
/// </summary>
public class UploadContext
{
    public string Context { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserRole { get; set; }
    public string? UserIP { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// 上传请求
/// </summary>
public class UploadRequest
{
    public string Context { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public long TotalSize { get; set; }
    public Guid UserId { get; set; }
    public string? UserIP { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// 文件分享请求
/// </summary>
public class FileShareRequest
{
    public Guid UserId { get; set; }
    public IEnumerable<Guid> SharedWithUsers { get; set; } = new List<Guid>();
    public ShareAccessLevel AccessLevel { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Password { get; set; }
    public bool AllowDownload { get; set; } = true;
}

/// <summary>
/// 文件上传事件
/// </summary>
public class FileUploadEvent
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? UserIP { get; set; }
    public string? UserAgent { get; set; }
    public UploadStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 文件下载事件
/// </summary>
public class FileDownloadEvent
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserIP { get; set; }
    public string? UserAgent { get; set; }
    public DownloadStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 文件安全事件
/// </summary>
public class FileSecurityEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserIP { get; set; }
    public string? UserAgent { get; set; }
    public SecurityLevel Severity { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 文件操作日志
/// </summary>
public class FileOperationLog
{
    public Guid LogId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserIP { get; set; }
    public string? UserAgent { get; set; }
    public OperationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 文件操作日志筛选条件
/// </summary>
public class FileOperationLogFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Operation { get; set; }
    public Guid? UserId { get; set; }
    public Guid? FileId { get; set; }
    public OperationStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 文件安全统计
/// </summary>
public class FileSecurityStats
{
    public int TotalUploads { get; set; }
    public int SuccessfulUploads { get; set; }
    public int FailedUploads { get; set; }
    public int TotalDownloads { get; set; }
    public long TotalDataUploaded { get; set; }
    public long TotalDataDownloaded { get; set; }
    public int SecurityEvents { get; set; }
    public int VirusDetections { get; set; }
    public IEnumerable<FileTypeStats> FileTypeStats { get; set; } = new List<FileTypeStats>();
    public IEnumerable<DailyFileStats> DailyStats { get; set; } = new List<DailyFileStats>();
}

/// <summary>
/// 文件类型统计
/// </summary>
public class FileTypeStats
{
    public string FileType { get; set; } = string.Empty;
    public int UploadCount { get; set; }
    public long TotalSize { get; set; }
    public double AverageSize { get; set; }
    public int SecurityEvents { get; set; }
}

/// <summary>
/// 每日文件统计
/// </summary>
public class DailyFileStats
{
    public DateTime Date { get; set; }
    public int Uploads { get; set; }
    public int Downloads { get; set; }
    public long DataTransferred { get; set; }
    public int SecurityEvents { get; set; }
}

/// <summary>
/// 文件安全评分
/// </summary>
public class FileSecurityScore
{
    public int OverallScore { get; set; }
    public int TypeSafetyScore { get; set; }
    public int ContentSafetyScore { get; set; }
    public int SizeComplianceScore { get; set; }
    public int VirusScanScore { get; set; }
}

// 枚举定义
public enum FileOperation
{
    Read,
    Write,
    Delete,
    Share,
    Download
}

public enum FileAccessLevel
{
    None,
    Read,
    Write,
    Full
}

public enum ShareAccessLevel
{
    View,
    Download,
    Edit
}

public enum UploadStatus
{
    Success,
    Failed,
    Pending,
    Cancelled
}

public enum DownloadStatus
{
    Success,
    Failed,
    Pending,
    Cancelled
}

public enum OperationStatus
{
    Success,
    Failed,
    Pending,
    Cancelled
}

public enum MalwareRiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum CodeRiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum SensitivityLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum RequestRiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum VirusScanStatus
{
    Clean,
    Infected,
    Error,
    Timeout
}