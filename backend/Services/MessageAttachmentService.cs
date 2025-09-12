using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 消息附件服务实现 - 遵循单一职责原则
/// 提供完整的消息附件上传、下载、验证和管理功能
/// </summary>
public class MessageAttachmentService
{
    private readonly IMessageAttachmentRepository _messageAttachmentRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICacheService _cacheService;
    private readonly IPermissionService _permissionService;
    private readonly IInputValidationService _inputValidationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IVirusScanService _virusScanService;
    private readonly ILogger<MessageAttachmentService> _logger;

    // 配置常量
    private const long MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB
    private const int MAX_FILES_PER_MESSAGE = 10;
    private const string ATTACHMENT_CACHE_PREFIX = "attachment_";
    private readonly string[] ALLOWED_FILE_TYPES = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", // 图片
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", // 文档
        ".txt", ".md", ".json", ".xml", ".csv", // 文本
        ".zip", ".rar", ".7z", ".tar", ".gz", // 压缩文件
        ".mp3", ".wav", ".flac", ".m4a", // 音频
        ".mp4", ".avi", ".mkv", ".mov", ".wmv", // 视频
        ".cs", ".js", ".ts", ".html", ".css", ".py", ".java", ".cpp", ".php" // 代码文件
    };

    public MessageAttachmentService(
        IMessageAttachmentRepository messageAttachmentRepository,
        IMessageRepository messageRepository,
        ICacheService cacheService,
        IPermissionService permissionService,
        IInputValidationService inputValidationService,
        IFileStorageService fileStorageService,
        IVirusScanService virusScanService,
        ILogger<MessageAttachmentService> logger)
    {
        _messageAttachmentRepository = messageAttachmentRepository ?? throw new ArgumentNullException(nameof(messageAttachmentRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _inputValidationService = inputValidationService ?? throw new ArgumentNullException(nameof(inputValidationService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _virusScanService = virusScanService ?? throw new ArgumentNullException(nameof(virusScanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 附件上传管理

    /// <summary>
    /// 上传消息附件
    /// </summary>
    public async Task<MessageAttachmentDto> UploadAttachmentAsync(Guid messageId, Stream fileStream, string fileName, string contentType, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试为消息 {MessageId} 上传附件: {FileName}", currentUserId, messageId, fileName);

            // 验证消息权限
            var canAttach = await ValidateMessageAttachmentPermissionAsync(messageId, currentUserId);
            if (!canAttach)
            {
                _logger.LogWarning("用户 {UserId} 无权限为消息 {MessageId} 上传附件", currentUserId, messageId);
                throw new UnauthorizedAccessException("您没有权限为此消息上传附件");
            }

            // 验证文件
            await ValidateFileAsync(fileStream, fileName, contentType);

            // 检查消息附件数量限制
            var existingAttachments = await _messageAttachmentRepository.GetMessageAttachmentsAsync(messageId);
            if (existingAttachments.Count() >= MAX_FILES_PER_MESSAGE)
            {
                throw new InvalidOperationException($"每条消息最多只能上传{MAX_FILES_PER_MESSAGE}个附件");
            }

            // 计算文件哈希值
            var fileHash = await CalculateFileHashAsync(fileStream);
            fileStream.Position = 0; // 重置流位置

            // 病毒扫描
            var virusScanResult = await _virusScanService.ScanFileAsync(fileStream);
            if (virusScanResult.IsInfected)
            {
                _logger.LogWarning("检测到病毒文件: {FileName}, 威胁: {Threat}", fileName, virusScanResult.ThreatName);
                throw new InvalidOperationException("检测到病毒文件，上传被拒绝");
            }

            // 生成唯一文件名
            var uniqueFileName = GenerateUniqueFileName(fileName);
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            // 确定附件类型
            var attachmentType = DetermineAttachmentType(fileExtension);

            // 上传文件到存储
            var storagePath = await _fileStorageService.UploadFileAsync(fileStream, uniqueFileName, "message-attachments");

            // 创建附件记录
            var attachment = new MessageAttachment
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                FileName = uniqueFileName,
                OriginalFileName = fileName,
                FileSize = fileStream.Length,
                ContentType = contentType,
                FileExtension = fileExtension,
                FilePath = storagePath,
                FileUrl = await _fileStorageService.GetFileUrlAsync(storagePath),
                AttachmentType = attachmentType,
                AttachmentStatus = AttachmentStatus.Active,
                FileHash = fileHash,
                UploadedAt = DateTime.UtcNow,
                UploadProgress = 100
            };

            // 保存附件记录
            var createdAttachment = await _messageAttachmentRepository.CreateAsync(attachment);

            // 生成缩略图（如果是图片）
            if (attachmentType == AttachmentType.Image)
            {
                try
                {
                    var thumbnailPath = await GenerateThumbnailAsync(fileStream, uniqueFileName);
                    if (!string.IsNullOrEmpty(thumbnailPath))
                    {
                        createdAttachment.ThumbnailPath = thumbnailPath;
                        await _messageAttachmentRepository.UpdateAsync(createdAttachment);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "生成缩略图失败: {FileName}", fileName);
                    // 缩略图生成失败不影响主要功能
                }
            }

            _logger.LogInformation("用户 {UserId} 成功为消息 {MessageId} 上传附件: {FileName} ({FileSize} bytes)", 
                currentUserId, messageId, fileName, fileStream.Length);

            return await MapToAttachmentDtoAsync(createdAttachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传消息附件时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量上传消息附件
    /// </summary>
    public async Task<IEnumerable<MessageAttachmentDto>> UploadMultipleAttachmentsAsync(Guid messageId, IEnumerable<MessageFileUploadDto> files, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试为消息 {MessageId} 批量上传 {Count} 个附件", currentUserId, messageId, files.Count());

            if (!files.Any())
            {
                return new List<MessageAttachmentDto>();
            }

            // 检查消息附件数量限制
            var existingAttachments = await _messageAttachmentRepository.GetMessageAttachmentsAsync(messageId);
            var remainingSlots = MAX_FILES_PER_MESSAGE - existingAttachments.Count();
            
            if (remainingSlots <= 0)
            {
                throw new InvalidOperationException($"此消息已达到附件数量上限（{MAX_FILES_PER_MESSAGE}个）");
            }

            // 限制上传数量
            var filesToUpload = files.Take(remainingSlots).ToList();
            var attachmentDtos = new List<MessageAttachmentDto>();
            var errors = new List<string>();

            foreach (var file in filesToUpload)
            {
                try
                {
                    var attachmentDto = await UploadAttachmentAsync(messageId, file.FileStream, file.FileName, file.ContentType, currentUserId);
                    attachmentDtos.Add(attachmentDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "批量上传附件时单个文件上传失败: {FileName}", file.FileName);
                    errors.Add($"文件 {file.FileName} 上传失败: {ex.Message}");
                }
            }

            _logger.LogInformation("用户 {UserId} 批量上传附件完成: 成功 {SuccessCount}, 失败 {FailedCount}", 
                currentUserId, attachmentDtos.Count, filesToUpload.Count - attachmentDtos.Count);

            return attachmentDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量上传消息附件时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 下载消息附件
    /// </summary>
    public async Task<Stream> DownloadAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试下载附件 {AttachmentId}", currentUserId, attachmentId);

            // 获取附件信息
            var attachment = await _messageAttachmentRepository.GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                _logger.LogWarning("附件 {AttachmentId} 不存在", attachmentId);
                throw new ArgumentException("附件不存在");
            }

            // 验证下载权限
            var canDownload = await ValidateAttachmentDownloadPermissionAsync(attachment, currentUserId);
            if (!canDownload)
            {
                _logger.LogWarning("用户 {UserId} 无权限下载附件 {AttachmentId}", currentUserId, attachmentId);
                throw new UnauthorizedAccessException("您没有权限下载此附件");
            }

            // 检查附件状态
            if (attachment.AttachmentStatus != AttachmentStatus.Active)
            {
                throw new InvalidOperationException("附件不可用");
            }

            // 检查附件是否过期
            if (attachment.Message?.ExpiresAt.HasValue == true && attachment.Message.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("附件已过期");
            }

            // 从存储获取文件流
            var fileStream = await _fileStorageService.DownloadFileAsync(attachment.FilePath);

            // 更新下载次数
            await _messageAttachmentRepository.IncrementDownloadCountAsync(attachmentId);

            _logger.LogInformation("用户 {UserId} 成功下载附件 {AttachmentId}: {FileName}", currentUserId, attachmentId, attachment.OriginalFileName);

            return fileStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载消息附件时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取消息附件列表
    /// </summary>
    public async Task<IEnumerable<MessageAttachmentDto>> GetMessageAttachmentsAsync(Guid messageId, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取消息 {MessageId} 的附件列表", messageId);

            // 验证消息查看权限
            var canView = await _messageRepository.ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.View);
            if (!canView)
            {
                _logger.LogWarning("用户 {UserId} 无权限查看消息 {MessageId} 的附件", currentUserId, messageId);
                throw new UnauthorizedAccessException("您没有权限查看此消息的附件");
            }

            // 获取附件列表
            var attachments = await _messageAttachmentRepository.GetMessageAttachmentsAsync(messageId);

            // 映射为DTO
            var attachmentDtos = new List<MessageAttachmentDto>();
            foreach (var attachment in attachments)
            {
                attachmentDtos.Add(await MapToAttachmentDtoAsync(attachment));
            }

            return attachmentDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息附件列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 删除消息附件
    /// </summary>
    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试删除附件 {AttachmentId}", currentUserId, attachmentId);

            // 获取附件信息
            var attachment = await _messageAttachmentRepository.GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                _logger.LogWarning("附件 {AttachmentId} 不存在", attachmentId);
                return false;
            }

            // 验证删除权限
            var canDelete = await ValidateAttachmentDeletePermissionAsync(attachment, currentUserId);
            if (!canDelete)
            {
                _logger.LogWarning("用户 {UserId} 无权限删除附件 {AttachmentId}", currentUserId, attachmentId);
                throw new UnauthorizedAccessException("您没有权限删除此附件");
            }

            // 删除存储文件
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // 删除缩略图
            if (!string.IsNullOrEmpty(attachment.ThumbnailPath))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(attachment.ThumbnailPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除缩略图失败: {ThumbnailPath}", attachment.ThumbnailPath);
                }
            }

            // 软删除附件记录
            var result = await _messageAttachmentRepository.SoftDeleteAsync(attachmentId);

            if (result)
            {
                _logger.LogInformation("用户 {UserId} 成功删除附件 {AttachmentId}: {FileName}", currentUserId, attachmentId, attachment.OriginalFileName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息附件时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取附件下载URL
    /// </summary>
    public async Task<string> GetAttachmentDownloadUrlAsync(Guid attachmentId, Guid currentUserId, TimeSpan? expiration = null)
    {
        try
        {
            _logger.LogDebug("获取附件 {AttachmentId} 的下载URL", attachmentId);

            // 获取附件信息
            var attachment = await _messageAttachmentRepository.GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                _logger.LogWarning("附件 {AttachmentId} 不存在", attachmentId);
                throw new ArgumentException("附件不存在");
            }

            // 验证下载权限
            var canDownload = await ValidateAttachmentDownloadPermissionAsync(attachment, currentUserId);
            if (!canDownload)
            {
                _logger.LogWarning("用户 {UserId} 无权限下载附件 {AttachmentId}", currentUserId, attachmentId);
                throw new UnauthorizedAccessException("您没有权限下载此附件");
            }

            // 生成下载URL
            var expiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(1));
            var downloadUrl = await _fileStorageService.GenerateDownloadUrlAsync(attachment.FilePath, expiresAt);

            _logger.LogDebug("成功生成附件 {AttachmentId} 的下载URL", attachmentId);

            return downloadUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件下载URL时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 验证附件文件完整性
    /// </summary>
    public async Task<bool> ValidateAttachmentIntegrityAsync(Guid attachmentId, string expectedHash)
    {
        try
        {
            _logger.LogDebug("验证附件 {AttachmentId} 的文件完整性", attachmentId);

            // 获取附件信息
            var attachment = await _messageAttachmentRepository.GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                _logger.LogWarning("附件 {AttachmentId} 不存在", attachmentId);
                return false;
            }

            // 比较哈希值
            if (string.IsNullOrEmpty(attachment.FileHash) || string.IsNullOrEmpty(expectedHash))
            {
                return false;
            }

            return string.Equals(attachment.FileHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证附件文件完整性时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 获取用户附件统计信息
    /// </summary>
    public async Task<MessageAttachmentStatsDto> GetUserAttachmentStatsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的附件统计信息", userId);

            // 尝试从缓存获取
            var cacheKey = $"{ATTACHMENT_CACHE_PREFIX}stats_{userId}";
            var cachedStats = await _cacheService.GetAsync<MessageAttachmentStatsDto>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }

            // 从数据库获取统计信息
            var stats = await _messageAttachmentRepository.GetUserAttachmentStatsAsync(userId);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户附件统计信息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 验证文件
    /// </summary>
    private async Task ValidateFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("文件不能为空");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("文件名不能为空");
        }

        if (fileName.Length > 255)
        {
            throw new ArgumentException("文件名长度不能超过255个字符");
        }

        // 验证文件大小
        if (fileStream.Length > MAX_FILE_SIZE)
        {
            throw new ArgumentException($"文件大小不能超过{MAX_FILE_SIZE / (1024 * 1024)}MB");
        }

        if (fileStream.Length == 0)
        {
            throw new ArgumentException("文件不能为空");
        }

        // 验证文件扩展名
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !ALLOWED_FILE_TYPES.Contains(fileExtension))
        {
            throw new ArgumentException("不支持的文件类型");
        }

        // 验证文件内容类型
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("文件内容类型不能为空");
        }

        // 使用输入验证服务验证文件名
        await _inputValidationService.ValidateFileNameAsync(fileName);

        // 验证文件内容（简单检查文件头）
        await ValidateFileContentAsync(fileStream, fileExtension);
    }

    /// <summary>
    /// 验证文件内容
    /// </summary>
    private async Task ValidateFileContentAsync(Stream fileStream, string fileExtension)
    {
        try
        {
            // 保存当前位置
            var originalPosition = fileStream.Position;
            fileStream.Position = 0;

            // 读取文件头进行验证
            var buffer = new byte[Math.Min(16, fileStream.Length)];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);

            // 根据文件扩展名验证文件头
            var isValidFile = fileExtension switch
            {
                ".jpg" or ".jpeg" => IsValidJpegHeader(buffer),
                ".png" => IsValidPngHeader(buffer),
                ".gif" => IsValidGifHeader(buffer),
                ".pdf" => IsValidPdfHeader(buffer),
                ".zip" => IsValidZipHeader(buffer),
                _ => true // 其他文件类型暂时跳过文件头验证
            };

            if (!isValidFile)
            {
                throw new ArgumentException("文件内容与扩展名不匹配");
            }

            // 恢复位置
            fileStream.Position = originalPosition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证文件内容时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 验证消息附件权限
    /// </summary>
    private async Task<bool> ValidateMessageAttachmentPermissionAsync(Guid messageId, Guid userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
        {
            return false;
        }

        // 只有消息发送者可以上传附件
        return message.SenderId == userId;
    }

    /// <summary>
    /// 验证附件下载权限
    /// </summary>
    private async Task<bool> ValidateAttachmentDownloadPermissionAsync(MessageAttachment attachment, Guid userId)
    {
        if (attachment.Message == null)
        {
            return false;
        }

        // 消息的发送者和接收者都可以下载附件
        return attachment.Message.SenderId == userId || attachment.Message.ReceiverId == userId;
    }

    /// <summary>
    /// 验证附件删除权限
    /// </summary>
    private async Task<bool> ValidateAttachmentDeletePermissionAsync(MessageAttachment attachment, Guid userId)
    {
        if (attachment.Message == null)
        {
            return false;
        }

        // 只有消息发送者可以删除附件
        return attachment.Message.SenderId == userId;
    }

    /// <summary>
    /// 计算文件哈希值
    /// </summary>
    private async Task<string> CalculateFileHashAsync(Stream fileStream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(fileStream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 生成唯一文件名
    /// </summary>
    private string GenerateUniqueFileName(string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..8];
        
        return $"{fileNameWithoutExtension}_{timestamp}_{random}{fileExtension}";
    }

    /// <summary>
    /// 确定附件类型
    /// </summary>
    private AttachmentType DetermineAttachmentType(string fileExtension)
    {
        return fileExtension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" => AttachmentType.Image,
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" => AttachmentType.Document,
            ".mp3" or ".wav" or ".flac" or ".m4a" => AttachmentType.Audio,
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" => AttachmentType.Video,
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => AttachmentType.Archive,
            ".cs" or ".js" or ".ts" or ".html" or ".css" or ".py" or ".java" or ".cpp" or ".php" => AttachmentType.Code,
            _ => AttachmentType.Other
        };
    }

    /// <summary>
    /// 生成缩略图
    /// </summary>
    private async Task<string> GenerateThumbnailAsync(Stream imageStream, string fileName)
    {
        // 这里应该使用图像处理库生成缩略图
        // 简化实现，返回空字符串
        // 在实际项目中，可以使用System.Drawing.Common或SixLabors.ImageSharp等库
        return string.Empty;
    }

    /// <summary>
    /// 映射附件实体到DTO
    /// </summary>
    private async Task<MessageAttachmentDto> MapToAttachmentDtoAsync(MessageAttachment attachment)
    {
        return new MessageAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            FileSize = attachment.FileSize,
            ContentType = attachment.ContentType,
            FileExtension = attachment.FileExtension,
            FileUrl = attachment.FileUrl,
            AttachmentType = attachment.AttachmentType,
            AttachmentStatus = attachment.AttachmentStatus,
            ThumbnailUrl = !string.IsNullOrEmpty(attachment.ThumbnailPath) 
                ? await _fileStorageService.GetFileUrlAsync(attachment.ThumbnailPath) 
                : null,
            UploadProgress = attachment.UploadProgress,
            DownloadCount = attachment.DownloadCount,
            UploadedAt = attachment.UploadedAt
        };
    }

    #region 文件头验证方法

    private bool IsValidJpegHeader(byte[] buffer)
    {
        return buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xD8;
    }

    private bool IsValidPngHeader(byte[] buffer)
    {
        return buffer.Length >= 8 && 
               buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
               buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A;
    }

    private bool IsValidGifHeader(byte[] buffer)
    {
        return buffer.Length >= 3 && buffer[0] == 'G' && buffer[1] == 'I' && buffer[2] == 'F';
    }

    private bool IsValidPdfHeader(byte[] buffer)
    {
        return buffer.Length >= 4 && 
               buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46;
    }

    private bool IsValidZipHeader(byte[] buffer)
    {
        return buffer.Length >= 4 && 
               buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
    }

    #endregion

    #endregion
}