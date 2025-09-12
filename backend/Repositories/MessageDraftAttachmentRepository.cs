using CodeSnippetManager.Api.Data;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using Dapper;
using System.Data;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 消息草稿附件仓储实现 - 使用Dapper ORM
/// </summary>
public class MessageDraftAttachmentRepository : IMessageDraftAttachmentRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<MessageDraftAttachmentRepository> _logger;

    public MessageDraftAttachmentRepository(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<MessageDraftAttachmentRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<MessageDraftAttachment> CreateAsync(MessageDraftAttachment draftAttachment)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO MessageDraftAttachments (
                    Id, DraftId, FileName, FilePath, OriginalFileName, FileSize,
                    FileType, ContentType, IsUploaded, StorageType, FileHash,
                    UploadedAt, CreatedAt, UpdatedAt, IsDeleted
                ) VALUES (
                    @Id, @DraftId, @FileName, @FilePath, @OriginalFileName, @FileSize,
                    @FileType, @ContentType, @IsUploaded, @StorageType, @FileHash,
                    @UploadedAt, @CreatedAt, @UpdatedAt, @IsDeleted
                )";

            await connection.ExecuteAsync(sql, draftAttachment);
            _logger.LogInformation($"创建草稿附件成功: {draftAttachment.Id}");
            return draftAttachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"创建草稿附件失败: {draftAttachment.Id}");
            throw;
        }
    }

    public async Task<IEnumerable<MessageDraftAttachment>> CreateBatchAsync(IEnumerable<MessageDraftAttachment> draftAttachments)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sql = @"
                    INSERT INTO MessageDraftAttachments (
                        Id, DraftId, FileName, FilePath, OriginalFileName, FileSize,
                        FileType, ContentType, IsUploaded, StorageType, FileHash,
                        UploadedAt, CreatedAt, UpdatedAt, IsDeleted
                    ) VALUES (
                        @Id, @DraftId, @FileName, @FilePath, @OriginalFileName, @FileSize,
                        @FileType, @ContentType, @IsUploaded, @StorageType, @FileHash,
                        @UploadedAt, @CreatedAt, @UpdatedAt, @IsDeleted
                    )";

                await connection.ExecuteAsync(sql, draftAttachments, transaction);
                transaction.Commit();
                
                _logger.LogInformation($"批量创建草稿附件成功: {draftAttachments.Count()} 个");
                return draftAttachments;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量创建草稿附件失败");
            throw;
        }
    }

    public async Task<MessageDraftAttachment?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM MessageDraftAttachments 
                WHERE Id = @Id AND IsDeleted = 0";

            var draftAttachment = await connection.QueryFirstOrDefaultAsync<MessageDraftAttachment>(sql, new { Id = id });
            return draftAttachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取草稿附件失败: {id}");
            throw;
        }
    }

    public async Task<IEnumerable<MessageDraftAttachment>> GetByDraftIdAsync(Guid draftId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM MessageDraftAttachments 
                WHERE DraftId = @DraftId AND IsDeleted = 0
                ORDER BY CreatedAt ASC";

            var draftAttachments = await connection.QueryAsync<MessageDraftAttachment>(sql, new { DraftId = draftId });
            return draftAttachments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取草稿附件列表失败: {draftId}");
            throw;
        }
    }

    public async Task<MessageDraftAttachment?> GetByDraftIdAndFileNameAsync(Guid draftId, string fileName)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM MessageDraftAttachments 
                WHERE DraftId = @DraftId AND FileName = @FileName AND IsDeleted = 0";

            var draftAttachment = await connection.QueryFirstOrDefaultAsync<MessageDraftAttachment>(sql, 
                new { DraftId = draftId, FileName = fileName });
            return draftAttachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取草稿附件失败: 草稿 {draftId}, 文件 {fileName}");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(MessageDraftAttachment draftAttachment)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageDraftAttachments SET
                    FileName = @FileName,
                    FilePath = @FilePath,
                    OriginalFileName = @OriginalFileName,
                    FileSize = @FileSize,
                    FileType = @FileType,
                    ContentType = @ContentType,
                    IsUploaded = @IsUploaded,
                    StorageType = @StorageType,
                    FileHash = @FileHash,
                    UploadedAt = @UploadedAt,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";

            var affectedRows = await connection.ExecuteAsync(sql, draftAttachment);
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新草稿附件失败: {draftAttachment.Id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageDraftAttachments 
                SET IsDeleted = 1, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除草稿附件失败: {id}");
            throw;
        }
    }

    public async Task<bool> DeleteByDraftIdAsync(Guid draftId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageDraftAttachments 
                SET IsDeleted = 1, UpdatedAt = @UpdatedAt
                WHERE DraftId = @DraftId";

            var affectedRows = await connection.ExecuteAsync(sql, new { DraftId = draftId, UpdatedAt = DateTime.UtcNow });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除草稿附件失败: 草稿 {draftId}");
            throw;
        }
    }

    public async Task<bool> DeleteBatchAsync(IEnumerable<Guid> ids)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageDraftAttachments 
                SET IsDeleted = 1, UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids";

            var affectedRows = await connection.ExecuteAsync(sql, new { Ids = ids, UpdatedAt = DateTime.UtcNow });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量删除草稿附件失败");
            throw;
        }
    }

    public async Task<long> GetTotalSizeByDraftIdAsync(Guid draftId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT COALESCE(SUM(FileSize), 0) FROM MessageDraftAttachments 
                WHERE DraftId = @DraftId AND IsDeleted = 0";

            var totalSize = await connection.ExecuteScalarAsync<long>(sql, new { DraftId = draftId });
            return totalSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取草稿附件总大小失败: {draftId}");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid draftId, string fileName)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(1) FROM MessageDraftAttachments 
                WHERE DraftId = @DraftId AND FileName = @FileName AND IsDeleted = 0";

            var exists = await connection.ExecuteScalarAsync<int>(sql, new { DraftId = draftId, FileName = fileName }) > 0;
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"检查草稿附件存在失败: 草稿 {draftId}, 文件 {fileName}");
            throw;
        }
    }

    public async Task<int> GetCountByDraftIdAsync(Guid draftId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM MessageDraftAttachments 
                WHERE DraftId = @DraftId AND IsDeleted = 0";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { DraftId = draftId });
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取草稿附件数量失败: {draftId}");
            throw;
        }
    }
}