using Dapper;
using System.Text;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 消息附件仓储实现 - 使用 Dapper ORM
/// 提供完整的消息附件CRUD操作、查询功能和文件管理
/// </summary>
public class MessageAttachmentRepository : IMessageAttachmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<MessageAttachmentRepository> _logger;

    public MessageAttachmentRepository(IDbConnectionFactory connectionFactory, ILogger<MessageAttachmentRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础 CRUD 操作

    public async Task<MessageAttachment?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM MessageAttachments WHERE Id = @Id AND DeletedAt IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<MessageAttachment>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件失败: {AttachmentId}", id);
            throw;
        }
    }

    public async Task<MessageAttachment?> GetByIdWithMessageAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT a.*, m.*
                FROM MessageAttachments a
                LEFT JOIN Messages m ON a.MessageId = m.Id
                WHERE a.Id = @Id AND a.DeletedAt IS NULL";
            
            var attachmentDict = new Dictionary<Guid, MessageAttachment>();
            
            var attachments = await connection.QueryAsync<MessageAttachment, Message, MessageAttachment>(
                sql,
                (attachment, message) =>
                {
                    if (!attachmentDict.TryGetValue(attachment.Id, out var existingAttachment))
                    {
                        existingAttachment = attachment;
                        existingAttachment.Message = message;
                        attachmentDict.Add(attachment.Id, existingAttachment);
                    }
                    return existingAttachment;
                },
                new { Id = id },
                splitOn: "Id"
            );
            
            return attachments.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件（包含消息信息）失败: {AttachmentId}", id);
            throw;
        }
    }

    public async Task<MessageAttachment> CreateAsync(MessageAttachment attachment)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO MessageAttachments (
                    Id, MessageId, FileName, OriginalFileName, FileSize, ContentType, FileExtension,
                    FilePath, FileUrl, AttachmentType, AttachmentStatus, FileHash, ThumbnailPath,
                    UploadProgress, DownloadCount, UploadedAt, LastDownloadedAt, DeletedAt
                ) VALUES (
                    @Id, @MessageId, @FileName, @OriginalFileName, @FileSize, @ContentType, @FileExtension,
                    @FilePath, @FileUrl, @AttachmentType, @AttachmentStatus, @FileHash, @ThumbnailPath,
                    @UploadProgress, @DownloadCount, @UploadedAt, @LastDownloadedAt, @DeletedAt
                )";
            
            await connection.ExecuteAsync(sql, attachment);
            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建附件失败: {FileName}", attachment.FileName);
            throw;
        }
    }

    public async Task<MessageAttachment> UpdateAsync(MessageAttachment attachment)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE MessageAttachments SET
                    FileName = @FileName,
                    OriginalFileName = @OriginalFileName,
                    FileSize = @FileSize,
                    ContentType = @ContentType,
                    FileExtension = @FileExtension,
                    FilePath = @FilePath,
                    FileUrl = @FileUrl,
                    AttachmentType = @AttachmentType,
                    AttachmentStatus = @AttachmentStatus,
                    FileHash = @FileHash,
                    ThumbnailPath = @ThumbnailPath,
                    UploadProgress = @UploadProgress,
                    DownloadCount = @DownloadCount,
                    LastDownloadedAt = @LastDownloadedAt
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            await connection.ExecuteAsync(sql, attachment);
            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新附件失败: {AttachmentId}", attachment.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE MessageAttachments SET DeletedAt = @Now WHERE Id = @Id AND DeletedAt IS NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Now = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除附件失败: {AttachmentId}", id);
            throw;
        }
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM MessageAttachments WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "硬删除附件失败: {AttachmentId}", id);
            throw;
        }
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE MessageAttachments SET DeletedAt = NULL WHERE Id = @Id AND DeletedAt IS NOT NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复附件失败: {AttachmentId}", id);
            throw;
        }
    }

    #endregion

    #region 分页查询

    public async Task<PaginatedResult<MessageAttachment>> GetPagedAsync(MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            var orderByClause = BuildAttachmentOrderByClause(filter.SortBy);
            
            var countSql = $"SELECT COUNT(*) FROM MessageAttachments a {whereClause}";
            var dataSql = $@"
                SELECT a.* FROM MessageAttachments a 
                {whereClause}
                {orderByClause}
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = BuildAttachmentParameters(filter);
            parameters["Limit"] = filter.PageSize;
            parameters["Offset"] = (filter.Page - 1) * filter.PageSize;
            
            var totalCount = await connection.QuerySingleOrDefaultAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<MessageAttachment>(dataSql, parameters);
            
            // 加载关联数据
            if (filter.IncludeMessage)
            {
                items = await LoadMessageDataAsync(items.ToList(), connection);
            }
            
            return new PaginatedResult<MessageAttachment>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取附件列表失败");
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetByMessageIdAsync(Guid messageId, MessageAttachmentFilterDto filter)
    {
        filter.MessageId = messageId;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<MessageAttachment>> GetByMessageIdsAsync(IEnumerable<Guid> messageIds, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            whereClause.Append(" AND a.MessageId IN @MessageIds");
            
            var orderByClause = BuildAttachmentOrderByClause(filter.SortBy);
            
            var sql = $@"
                SELECT a.* FROM MessageAttachments a 
                {whereClause}
                {orderByClause}";
            
            var parameters = BuildAttachmentParameters(filter);
            parameters.Add("MessageIds", messageIds);
            
            var items = await connection.QueryAsync<MessageAttachment>(sql, parameters);
            
            if (filter.IncludeMessage)
            {
                items = await LoadMessageDataAsync(items.ToList(), connection);
            }
            
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据消息ID列表获取附件失败");
            throw;
        }
    }

    public async Task<PaginatedResult<MessageAttachment>> GetByUserIdAsync(Guid userId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            whereClause.Append(" AND m.SenderId = @UserId");
            
            var orderByClause = BuildAttachmentOrderByClause(filter.SortBy);
            
            var countSql = $@"
                SELECT COUNT(*) FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                {whereClause}";
            
            var dataSql = $@"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                {whereClause}
                {orderByClause}
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = BuildAttachmentParameters(filter);
            parameters.Add("UserId", userId);
            parameters.Add("Limit", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            var totalCount = await connection.QuerySingleOrDefaultAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<MessageAttachment>(dataSql, parameters);
            
            if (filter.IncludeMessage)
            {
                items = await LoadMessageDataAsync(items.ToList(), connection);
            }
            
            return new PaginatedResult<MessageAttachment>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据用户ID获取附件失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 按类型查询

    public async Task<IEnumerable<MessageAttachment>> GetByAttachmentTypeAsync(AttachmentType attachmentType, MessageAttachmentFilterDto filter)
    {
        filter.AttachmentType = attachmentType;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<MessageAttachment>> GetByFileExtensionAsync(string fileExtension, MessageAttachmentFilterDto filter)
    {
        filter.FileExtension = fileExtension;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<MessageAttachment>> GetByContentTypeAsync(string contentType, MessageAttachmentFilterDto filter)
    {
        filter.ContentType = contentType;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<MessageAttachment>> GetByStatusAsync(AttachmentStatus status, MessageAttachmentFilterDto filter)
    {
        filter.AttachmentStatus = status;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    #endregion

    #region 搜索和筛选

    public async Task<IEnumerable<MessageAttachment>> SearchAttachmentsAsync(string searchTerm, MessageAttachmentFilterDto filter)
    {
        filter.Search = searchTerm;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<MessageAttachment>> GetByFileSizeRangeAsync(long minSize, long maxSize, MessageAttachmentFilterDto filter)
    {
        filter.MinFileSize = minSize;
        filter.MaxFileSize = maxSize;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<MessageAttachment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, MessageAttachmentFilterDto filter)
    {
        filter.StartDate = startDate;
        filter.EndDate = endDate;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    #endregion

    #region 下载管理

    public async Task<bool> IncrementDownloadCountAsync(Guid attachmentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string updateSql = @"
                UPDATE MessageAttachments SET
                    DownloadCount = DownloadCount + 1,
                    LastDownloadedAt = @Now
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            const string historySql = @"
                INSERT INTO AttachmentDownloadHistory (
                    Id, AttachmentId, UserId, DownloadedAt, DownloadIp, DownloadUserAgent
                ) VALUES (
                    @Id, @AttachmentId, @UserId, @DownloadedAt, @DownloadIp, @DownloadUserAgent
                )";
            
            await connection.ExecuteAsync(updateSql, new { Id = attachmentId, Now = DateTime.UtcNow });
            
            await connection.ExecuteAsync(historySql, new
            {
                Id = Guid.NewGuid(),
                AttachmentId = attachmentId,
                UserId = userId,
                DownloadedAt = DateTime.UtcNow,
                DownloadIp = "", // 需要从HttpContext获取
                DownloadUserAgent = "" // 需要从HttpContext获取
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "增加附件下载次数失败: {AttachmentId}, {UserId}", attachmentId, userId);
            throw;
        }
    }

    public async Task<int> IncrementMultipleDownloadCountAsync(IEnumerable<Guid> attachmentIds, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string updateSql = @"
                UPDATE MessageAttachments SET
                    DownloadCount = DownloadCount + 1,
                    LastDownloadedAt = @Now
                WHERE Id IN @AttachmentIds AND DeletedAt IS NULL";
            
            var now = DateTime.UtcNow;
            var historyTasks = attachmentIds.Select(async attachmentId =>
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO AttachmentDownloadHistory (
                        Id, AttachmentId, UserId, DownloadedAt, DownloadIp, DownloadUserAgent
                    ) VALUES (
                        @Id, @AttachmentId, @UserId, @DownloadedAt, @DownloadIp, @DownloadUserAgent
                    )", new
                {
                    Id = Guid.NewGuid(),
                    AttachmentId = attachmentId,
                    UserId = userId,
                    DownloadedAt = now,
                    DownloadIp = "",
                    DownloadUserAgent = ""
                });
            });
            
            var updateResult = await connection.ExecuteAsync(updateSql, new { AttachmentIds = attachmentIds, Now = now });
            await Task.WhenAll(historyTasks);
            
            return updateResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量增加附件下载次数失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<AttachmentDownloadHistory>> GetDownloadHistoryAsync(Guid attachmentId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT * FROM AttachmentDownloadHistory
                WHERE AttachmentId = @AttachmentId
                ORDER BY DownloadedAt DESC
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = new DynamicParameters();
            parameters.Add("AttachmentId", attachmentId);
            parameters.Add("Limit", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            return await connection.QueryAsync<AttachmentDownloadHistory>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件下载历史失败: {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetDownloadedByUserAsync(Guid userId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN AttachmentDownloadHistory h ON a.Id = h.AttachmentId
                WHERE h.UserId = @UserId AND a.DeletedAt IS NULL
                ORDER BY h.DownloadedAt DESC
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("Limit", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            return await connection.QueryAsync<MessageAttachment>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户下载的附件失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 上传进度管理

    public async Task<bool> UpdateUploadProgressAsync(Guid attachmentId, int progress)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageAttachments SET
                    UploadProgress = @Progress
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = attachmentId, Progress = progress });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新附件上传进度失败: {AttachmentId}, {Progress}%", attachmentId, progress);
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetUploadingAttachmentsAsync(Guid userId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE m.SenderId = @UserId 
                AND a.AttachmentStatus = @Uploading 
                AND a.DeletedAt IS NULL
                ORDER BY a.UploadedAt ASC
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("Uploading", AttachmentStatus.Uploading);
            parameters.Add("Limit", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            return await connection.QueryAsync<MessageAttachment>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取正在上传的附件失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetUploadFailedAttachmentsAsync(Guid userId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE m.SenderId = @UserId 
                AND a.AttachmentStatus = @UploadFailed 
                AND a.DeletedAt IS NULL
                ORDER BY a.UploadedAt DESC
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("UploadFailed", AttachmentStatus.UploadFailed);
            parameters.Add("Limit", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            return await connection.QueryAsync<MessageAttachment>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取上传失败的附件失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 状态管理

    public async Task<bool> UpdateAttachmentStatusAsync(Guid attachmentId, AttachmentStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageAttachments SET
                    AttachmentStatus = @Status
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = attachmentId, Status = status });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新附件状态失败: {AttachmentId}, {Status}", attachmentId, status);
            throw;
        }
    }

    public async Task<int> UpdateMultipleAttachmentStatusAsync(IEnumerable<Guid> attachmentIds, AttachmentStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageAttachments SET
                    AttachmentStatus = @Status
                WHERE Id IN @AttachmentIds AND DeletedAt IS NULL";
            
            return await connection.ExecuteAsync(sql, new { AttachmentIds = attachmentIds, Status = status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新附件状态失败: {Status}", status);
            throw;
        }
    }

    public async Task<bool> SetAttachmentActiveAsync(Guid attachmentId)
    {
        return await UpdateAttachmentStatusAsync(attachmentId, AttachmentStatus.Active);
    }

    public async Task<bool> MarkAsVirusScanningAsync(Guid attachmentId)
    {
        return await UpdateAttachmentStatusAsync(attachmentId, AttachmentStatus.VirusScanning);
    }

    public async Task<bool> MarkAsVirusDetectedAsync(Guid attachmentId, string virusInfo)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageAttachments SET
                    AttachmentStatus = @Status,
                    FilePath = CONCAT(FilePath, '_VIRUS_', @VirusInfo)
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = attachmentId, 
                Status = AttachmentStatus.VirusDetected,
                VirusInfo = virusInfo
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记附件为检测到病毒失败: {AttachmentId}", attachmentId);
            throw;
        }
    }

    #endregion

    #region 文件验证和完整性

    public async Task<bool> ValidateFileIntegrityAsync(Guid attachmentId, string expectedHash)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT FileHash FROM MessageAttachments WHERE Id = @Id AND DeletedAt IS NULL";
            
            var actualHash = await connection.QuerySingleOrDefaultAsync<string?>(sql, new { Id = attachmentId });
            return actualHash == expectedHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证附件文件完整性失败: {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<bool> UpdateFileHashAsync(Guid attachmentId, string fileHash)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageAttachments SET
                    FileHash = @FileHash
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = attachmentId, FileHash = fileHash });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新附件文件哈希值失败: {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<MessageAttachment?> GetByFileHashAsync(string fileHash)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM MessageAttachments WHERE FileHash = @FileHash AND DeletedAt IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<MessageAttachment>(sql, new { FileHash = fileHash });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据文件哈希值获取附件失败: {FileHash}", fileHash);
            throw;
        }
    }

    #endregion

    #region 统计信息

    public async Task<MessageAttachmentStatsDto> GetAttachmentStatsAsync(MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            
            const string sql = $@"
                SELECT 
                    COUNT(*) as TotalAttachments,
                    COALESCE(SUM(FileSize), 0) as TotalFileSize,
                    COALESCE(AVG(FileSize), 0) as AverageFileSize,
                    COALESCE(MAX(FileSize), 0) as MaxFileSize,
                    COALESCE(MIN(FileSize), 0) as MinFileSize,
                    COALESCE(SUM(DownloadCount), 0) as TotalDownloads,
                    COALESCE(AVG(DownloadCount), 0) as AverageDownloads,
                    COALESCE(MAX(DownloadCount), 0) as MaxDownloads
                FROM MessageAttachments a
                {whereClause}";
            
            var parameters = BuildAttachmentParameters(filter);
            var stats = await connection.QuerySingleAsync<dynamic>(sql, parameters);
            
            var result = new MessageAttachmentStatsDto
            {
                TotalAttachments = stats.TotalAttachments,
                TotalFileSize = stats.TotalFileSize,
                AverageFileSize = stats.AverageFileSize,
                MaxFileSize = stats.MaxFileSize,
                MinFileSize = stats.MinFileSize,
                TotalDownloads = stats.TotalDownloads,
                AverageDownloads = stats.AverageDownloads,
                MaxDownloads = stats.MaxDownloads
            };
            
            // 获取按类型统计
            const string typeStatsSql = $@"
                SELECT AttachmentType, COUNT(*) as Count
                FROM MessageAttachments a
                {whereClause}
                GROUP BY AttachmentType";
            
            var typeStats = await connection.QueryAsync<dynamic>(typeStatsSql, parameters);
            foreach (var stat in typeStats)
            {
                result.AttachmentTypeStats[(AttachmentType)stat.AttachmentType] = (int)stat.Count;
            }
            
            // 获取按状态统计
            const string statusStatsSql = $@"
                SELECT AttachmentStatus, COUNT(*) as Count
                FROM MessageAttachments a
                {whereClause}
                GROUP BY AttachmentStatus";
            
            var statusStats = await connection.QueryAsync<dynamic>(statusStatsSql, parameters);
            foreach (var stat in statusStats)
            {
                result.AttachmentStatusStats[(AttachmentStatus)stat.AttachmentStatus] = (int)stat.Count;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件统计信息失败");
            throw;
        }
    }

    public async Task<MessageAttachmentStatsDto> GetUserAttachmentStatsAsync(Guid userId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            whereClause.Append(" AND m.SenderId = @UserId");
            
            const string sql = $@"
                SELECT 
                    COUNT(*) as TotalAttachments,
                    COALESCE(SUM(a.FileSize), 0) as TotalFileSize,
                    COALESCE(SUM(a.DownloadCount), 0) as TotalDownloads
                FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                {whereClause}";
            
            var parameters = BuildAttachmentParameters(filter);
            parameters.Add("UserId", userId);
            var stats = await connection.QuerySingleAsync<dynamic>(sql, parameters);
            
            return new MessageAttachmentStatsDto
            {
                TotalAttachments = stats.TotalAttachments,
                TotalFileSize = stats.TotalFileSize,
                TotalDownloads = stats.TotalDownloads
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户附件统计信息失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetAttachmentCountAsync(MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            var countSql = $"SELECT COUNT(*) FROM MessageAttachments a {whereClause}";
            
            var parameters = BuildAttachmentParameters(filter);
            return await connection.QuerySingleOrDefaultAsync<int>(countSql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件数量失败");
            throw;
        }
    }

    public async Task<int> GetAttachmentCountByMessageIdAsync(Guid messageId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM MessageAttachments WHERE MessageId = @MessageId AND DeletedAt IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<int>(sql, new { MessageId = messageId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息附件数量失败: {MessageId}", messageId);
            throw;
        }
    }

    public async Task<long> GetUserTotalFileSizeAsync(Guid userId, MessageAttachmentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildAttachmentWhereClause(filter);
            whereClause.Append(" AND m.SenderId = @UserId");
            
            const string sql = $@"
                SELECT COALESCE(SUM(a.FileSize), 0)
                FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                {whereClause}";
            
            var parameters = BuildAttachmentParameters(filter);
            parameters.Add("UserId", userId);
            
            return await connection.QuerySingleOrDefaultAsync<long>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户附件总大小失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 权限和验证

    public async Task<bool> CanUserDownloadAttachmentAsync(Guid attachmentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE a.Id = @AttachmentId 
                AND (m.SenderId = @UserId OR m.ReceiverId = @UserId) 
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL";
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { AttachmentId = attachmentId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户下载附件权限失败: {AttachmentId}, {UserId}", attachmentId, userId);
            throw;
        }
    }

    public async Task<bool> CanUserDeleteAttachmentAsync(Guid attachmentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE a.Id = @AttachmentId 
                AND m.SenderId = @UserId 
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL";
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { AttachmentId = attachmentId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户删除附件权限失败: {AttachmentId}, {UserId}", attachmentId, userId);
            throw;
        }
    }

    public async Task<bool> IsAttachmentExpiredAsync(Guid attachmentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE a.Id = @AttachmentId 
                AND m.ExpiresAt IS NOT NULL 
                AND m.ExpiresAt < NOW()
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL";
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { AttachmentId = attachmentId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查附件是否过期失败: {AttachmentId}", attachmentId);
            throw;
        }
    }

    #endregion

    #region 高级查询

    public async Task<IEnumerable<MessageAttachment>> GetLatestAttachmentsAsync(Guid userId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE m.SenderId = @UserId 
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL
                ORDER BY a.UploadedAt DESC 
                LIMIT @Count";
            
            return await connection.QueryAsync<MessageAttachment>(sql, new { UserId = userId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新附件失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetMostDownloadedAttachmentsAsync(Guid userId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE m.SenderId = @UserId 
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL
                ORDER BY a.DownloadCount DESC, a.UploadedAt DESC 
                LIMIT @Count";
            
            return await connection.QueryAsync<MessageAttachment>(sql, new { UserId = userId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最多下载附件失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetLargestAttachmentsAsync(Guid userId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE m.SenderId = @UserId 
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL
                ORDER BY a.FileSize DESC, a.UploadedAt DESC 
                LIMIT @Count";
            
            return await connection.QueryAsync<MessageAttachment>(sql, new { UserId = userId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最大附件文件失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<MessageAttachment>> GetExpiringAttachmentsAsync(Guid userId, int days = 7)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT a.* FROM MessageAttachments a
                JOIN Messages m ON a.MessageId = m.Id
                WHERE (m.SenderId = @UserId OR m.ReceiverId = @UserId) 
                AND m.ExpiresAt IS NOT NULL 
                AND m.ExpiresAt <= DATE_ADD(NOW(), INTERVAL @Days DAY)
                AND a.DeletedAt IS NULL 
                AND m.DeletedAt IS NULL
                ORDER BY m.ExpiresAt ASC";
            
            return await connection.QueryAsync<MessageAttachment>(sql, new { UserId = userId, Days = days });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取即将过期附件失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 缓存相关

    public async Task<MessageAttachment?> GetFromCacheAsync(Guid id)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接从数据库获取
        return await GetByIdAsync(id);
    }

    public async Task<bool> SetCacheAsync(MessageAttachment attachment, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接返回成功
        return true;
    }

    public async Task<bool> RemoveCacheAsync(Guid id)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接返回成功
        return true;
    }

    public async Task<int> RemoveMultipleCacheAsync(IEnumerable<Guid> attachmentIds)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接返回成功数量
        return attachmentIds.Count();
    }

    #endregion

    #region 批量操作

    public async Task<int> BulkInsertAsync(IEnumerable<MessageAttachment> attachments)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO MessageAttachments (
                    Id, MessageId, FileName, OriginalFileName, FileSize, ContentType, FileExtension,
                    FilePath, FileUrl, AttachmentType, AttachmentStatus, FileHash, ThumbnailPath,
                    UploadProgress, DownloadCount, UploadedAt, LastDownloadedAt, DeletedAt
                ) VALUES (
                    @Id, @MessageId, @FileName, @OriginalFileName, @FileSize, @ContentType, @FileExtension,
                    @FilePath, @FileUrl, @AttachmentType, @AttachmentStatus, @FileHash, @ThumbnailPath,
                    @UploadProgress, @DownloadCount, @UploadedAt, @LastDownloadedAt, @DeletedAt
                )";
            
            return await connection.ExecuteAsync(sql, attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入附件失败");
            throw;
        }
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<MessageAttachment> attachments)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE MessageAttachments SET
                    FileName = @FileName,
                    OriginalFileName = @OriginalFileName,
                    FileSize = @FileSize,
                    ContentType = @ContentType,
                    FileExtension = @FileExtension,
                    FilePath = @FilePath,
                    FileUrl = @FileUrl,
                    AttachmentType = @AttachmentType,
                    AttachmentStatus = @AttachmentStatus,
                    FileHash = @FileHash,
                    ThumbnailPath = @ThumbnailPath,
                    UploadProgress = @UploadProgress,
                    DownloadCount = @DownloadCount,
                    LastDownloadedAt = @LastDownloadedAt
                WHERE Id = @Id";
            
            return await connection.ExecuteAsync(sql, attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新附件失败");
            throw;
        }
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<Guid> attachmentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM MessageAttachments WHERE Id IN @AttachmentIds";
            
            return await connection.ExecuteAsync(sql, new { AttachmentIds = attachmentIds });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除附件失败");
            throw;
        }
    }

    public async Task<int> BulkSoftDeleteAsync(IEnumerable<Guid> attachmentIds, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageAttachments SET
                    DeletedAt = @Now
                WHERE Id IN @AttachmentIds AND DeletedAt IS NULL";
            
            return await connection.ExecuteAsync(sql, new
            {
                AttachmentIds = attachmentIds,
                Now = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量软删除附件失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    private string BuildAttachmentWhereClause(MessageAttachmentFilterDto filter)
    {
        var whereClause = new StringBuilder("WHERE a.DeletedAt IS NULL");
        
        if (filter.MessageId.HasValue)
            whereClause.Append(" AND a.MessageId = @MessageId");
        
        if (filter.AttachmentType.HasValue)
            whereClause.Append(" AND a.AttachmentType = @AttachmentType");
        
        if (filter.AttachmentStatus.HasValue)
            whereClause.Append(" AND a.AttachmentStatus = @AttachmentStatus");
        
        if (!string.IsNullOrEmpty(filter.ContentType))
            whereClause.Append(" AND a.ContentType = @ContentType");
        
        if (!string.IsNullOrEmpty(filter.FileExtension))
            whereClause.Append(" AND a.FileExtension = @FileExtension");
        
        if (filter.MinFileSize.HasValue)
            whereClause.Append(" AND a.FileSize >= @MinFileSize");
        
        if (filter.MaxFileSize.HasValue)
            whereClause.Append(" AND a.FileSize <= @MaxFileSize");
        
        if (filter.StartDate.HasValue)
            whereClause.Append(" AND a.UploadedAt >= @StartDate");
        
        if (filter.EndDate.HasValue)
            whereClause.Append(" AND a.UploadedAt <= @EndDate");
        
        if (!string.IsNullOrEmpty(filter.Search))
            whereClause.Append(" AND (a.FileName LIKE @Search OR a.OriginalFileName LIKE @Search)");
        
        return whereClause.ToString();
    }

    private string BuildAttachmentOrderByClause(AttachmentSort sortBy)
    {
        return sortBy switch
        {
            AttachmentSort.UploadedAtAsc => "ORDER BY a.UploadedAt ASC",
            AttachmentSort.FileSizeDesc => "ORDER BY a.FileSize DESC, a.UploadedAt DESC",
            AttachmentSort.FileSizeAsc => "ORDER BY a.FileSize ASC, a.UploadedAt ASC",
            AttachmentSort.FileName => "ORDER BY a.FileName ASC, a.UploadedAt DESC",
            AttachmentSort.DownloadCountDesc => "ORDER BY a.DownloadCount DESC, a.UploadedAt DESC",
            AttachmentSort.DownloadCountAsc => "ORDER BY a.DownloadCount ASC, a.UploadedAt ASC",
            _ => "ORDER BY a.UploadedAt DESC"
        };
    }

    private DynamicParameters BuildAttachmentParameters(MessageAttachmentFilterDto filter)
    {
        var parameters = new DynamicParameters();
        
        if (filter.MessageId.HasValue)
            parameters.Add("MessageId", filter.MessageId.Value);
        
        if (filter.AttachmentType.HasValue)
            parameters.Add("AttachmentType", filter.AttachmentType.Value);
        
        if (filter.AttachmentStatus.HasValue)
            parameters.Add("AttachmentStatus", filter.AttachmentStatus.Value);
        
        if (!string.IsNullOrEmpty(filter.ContentType))
            parameters.Add("ContentType", filter.ContentType);
        
        if (!string.IsNullOrEmpty(filter.FileExtension))
            parameters.Add("FileExtension", filter.FileExtension);
        
        if (filter.MinFileSize.HasValue)
            parameters.Add("MinFileSize", filter.MinFileSize.Value);
        
        if (filter.MaxFileSize.HasValue)
            parameters.Add("MaxFileSize", filter.MaxFileSize.Value);
        
        if (filter.StartDate.HasValue)
            parameters.Add("StartDate", filter.StartDate.Value);
        
        if (filter.EndDate.HasValue)
            parameters.Add("EndDate", filter.EndDate.Value);
        
        if (!string.IsNullOrEmpty(filter.Search))
            parameters.Add("Search", $"%{filter.Search}%");
        
        return parameters;
    }

    private async Task<IEnumerable<MessageAttachment>> LoadMessageDataAsync(List<MessageAttachment> attachments, IDbConnection connection)
    {
        if (!attachments.Any())
            return attachments;

        var messageIds = attachments.Select(a => a.MessageId).Distinct().ToList();
        
        const string sql = @"
            SELECT m.*, 
                   s.Username as SenderName, s.Email as SenderEmail,
                   r.Username as ReceiverName, r.Email as ReceiverEmail
            FROM Messages m
            LEFT JOIN Users s ON m.SenderId = s.Id
            LEFT JOIN Users r ON m.ReceiverId = r.Id
            WHERE m.Id IN @MessageIds AND m.DeletedAt IS NULL";
        
        var messages = await connection.QueryAsync<dynamic>(sql, new { MessageIds = messageIds });
        
        foreach (var attachment in attachments)
        {
            var message = messages.FirstOrDefault(m => m.Id == attachment.MessageId);
            if (message != null)
            {
                attachment.Message = new Message
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Subject = message.Subject,
                    Content = message.Content,
                    MessageType = message.MessageType,
                    Status = message.Status,
                    Priority = message.Priority,
                    CreatedAt = message.CreatedAt,
                    UpdatedAt = message.UpdatedAt
                };
            }
        }
        
        return attachments;
    }

    #endregion
}