using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 代码片段仓储实现 - 使用 Dapper ORM
/// </summary>
public class CodeSnippetRepository : ICodeSnippetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CodeSnippetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<CodeSnippet?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.*, u.Username as CreatorName 
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            WHERE cs.Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<CodeSnippet>(sql, new { Id = id });
    }

    /// <summary>
    /// 获取代码片段详情，包含标签信息 - 使用 Dapper Multi-mapping
    /// </summary>
    public async Task<CodeSnippet?> GetByIdWithTagsAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
            LEFT JOIN Tags t ON st.TagId = t.Id
            WHERE cs.Id = @Id";

        var snippetDict = new Dictionary<Guid, CodeSnippet>();
        
        var result = await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
            sql,
            (snippet, tag) =>
            {
                if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                {
                    existingSnippet = snippet;
                    existingSnippet.Tags = new List<Tag>();
                    snippetDict.Add(snippet.Id, existingSnippet);
                }

                if (tag != null)
                {
                    existingSnippet.Tags.Add(tag);
                }

                return existingSnippet;
            },
            new { Id = id },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<PaginatedResult<CodeSnippet>> GetPagedAsync(SnippetFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(filter.Search))
        {
            whereClause += " AND (cs.Title LIKE @Search OR cs.Description LIKE @Search OR cs.Code LIKE @Search)";
            parameters.Add("Search", $"%{filter.Search}%");
        }
        
        if (!string.IsNullOrEmpty(filter.Language))
        {
            whereClause += " AND cs.Language = @Language";
            parameters.Add("Language", filter.Language);
        }
        
        if (!string.IsNullOrEmpty(filter.Tag))
        {
            whereClause += " AND EXISTS (SELECT 1 FROM SnippetTags st INNER JOIN Tags t ON st.TagId = t.Id WHERE st.SnippetId = cs.Id AND t.Name = @Tag)";
            parameters.Add("Tag", filter.Tag);
        }
        
        if (filter.CreatedBy.HasValue)
        {
            whereClause += " AND cs.CreatedBy = @CreatedBy";
            parameters.Add("CreatedBy", filter.CreatedBy.Value);
        }

        // 获取总数
        var countSql = $"SELECT COUNT(DISTINCT cs.Id) FROM CodeSnippets cs {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // 获取分页数据 - 使用 Multi-mapping 包含标签信息
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var dataSql = $@"
            SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
            LEFT JOIN Tags t ON st.TagId = t.Id
            {whereClause}
            ORDER BY cs.CreatedAt DESC
            LIMIT @PageSize OFFSET @Offset";

        var snippetDict = new Dictionary<Guid, CodeSnippet>();
        
        await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
            dataSql,
            (snippet, tag) =>
            {
                if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                {
                    existingSnippet = snippet;
                    existingSnippet.Tags = new List<Tag>();
                    snippetDict.Add(snippet.Id, existingSnippet);
                }

                if (tag != null && !existingSnippet.Tags.Any(t => t.Id == tag.Id))
                {
                    existingSnippet.Tags.Add(tag);
                }

                return existingSnippet;
            },
            parameters,
            splitOn: "Id"
        );

        return new PaginatedResult<CodeSnippet>
        {
            Items = snippetDict.Values,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<CodeSnippet> CreateAsync(CodeSnippet snippet)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO CodeSnippets (Id, Title, Description, Code, Language, CreatedBy, CreatedAt, UpdatedAt, IsPublic, ViewCount, CopyCount)
            VALUES (@Id, @Title, @Description, @Code, @Language, @CreatedBy, @CreatedAt, @UpdatedAt, @IsPublic, @ViewCount, @CopyCount)";
        
        await connection.ExecuteAsync(sql, snippet);
        return snippet;
    }

    public async Task<CodeSnippet> UpdateAsync(CodeSnippet snippet)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CodeSnippets 
            SET Title = @Title, Description = @Description, Code = @Code, 
                Language = @Language, UpdatedAt = @UpdatedAt, IsPublic = @IsPublic
            WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, snippet);
        return snippet;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM CodeSnippets WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 根据用户ID获取代码片段，包含标签信息 - 使用 Dapper Multi-mapping
    /// </summary>
    public async Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
            LEFT JOIN Tags t ON st.TagId = t.Id
            WHERE cs.CreatedBy = @UserId
            ORDER BY cs.CreatedAt DESC";

        var snippetDict = new Dictionary<Guid, CodeSnippet>();
        
        await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
            sql,
            (snippet, tag) =>
            {
                if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                {
                    existingSnippet = snippet;
                    existingSnippet.Tags = new List<Tag>();
                    snippetDict.Add(snippet.Id, existingSnippet);
                }

                if (tag != null && !existingSnippet.Tags.Any(t => t.Id == tag.Id))
                {
                    existingSnippet.Tags.Add(tag);
                }

                return existingSnippet;
            },
            new { UserId = userId },
            splitOn: "Id"
        );
        
        return snippetDict.Values;
    }

    /// <summary>
    /// 根据标签获取代码片段，包含标签信息 - 使用 Dapper Multi-mapping
    /// </summary>
    public async Task<IEnumerable<CodeSnippet>> GetByTagAsync(string tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
            LEFT JOIN Tags t ON st.TagId = t.Id
            WHERE EXISTS (
                SELECT 1 FROM SnippetTags st2 
                INNER JOIN Tags t2 ON st2.TagId = t2.Id 
                WHERE st2.SnippetId = cs.Id AND t2.Name = @Tag
            )
            ORDER BY cs.CreatedAt DESC";

        var snippetDict = new Dictionary<Guid, CodeSnippet>();
        
        await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
            sql,
            (snippet, tag) =>
            {
                if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                {
                    existingSnippet = snippet;
                    existingSnippet.Tags = new List<Tag>();
                    snippetDict.Add(snippet.Id, existingSnippet);
                }

                if (tag != null && !existingSnippet.Tags.Any(t => t.Id == tag.Id))
                {
                    existingSnippet.Tags.Add(tag);
                }

                return existingSnippet;
            },
            new { Tag = tag },
            splitOn: "Id"
        );
        
        return snippetDict.Values;
    }

    /// <summary>
    /// 为代码片段添加标签
    /// </summary>
    public async Task<bool> AddTagToSnippetAsync(Guid snippetId, Guid tagId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT IGNORE INTO SnippetTags (SnippetId, TagId)
            VALUES (@SnippetId, @TagId)";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { SnippetId = snippetId, TagId = tagId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 从代码片段移除标签
    /// </summary>
    public async Task<bool> RemoveTagFromSnippetAsync(Guid snippetId, Guid tagId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SnippetTags WHERE SnippetId = @SnippetId AND TagId = @TagId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { SnippetId = snippetId, TagId = tagId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 更新代码片段的所有标签 - 使用事务确保数据一致性
    /// </summary>
    public async Task<bool> UpdateSnippetTagsAsync(Guid snippetId, IEnumerable<Guid> tagIds)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // 删除现有标签关联
            const string deleteSql = "DELETE FROM SnippetTags WHERE SnippetId = @SnippetId";
            await connection.ExecuteAsync(deleteSql, new { SnippetId = snippetId }, transaction);

            // 添加新的标签关联
            if (tagIds.Any())
            {
                const string insertSql = "INSERT INTO SnippetTags (SnippetId, TagId) VALUES (@SnippetId, @TagId)";
                var parameters = tagIds.Select(tagId => new { SnippetId = snippetId, TagId = tagId });
                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    /// <summary>
    /// 增加代码片段查看次数
    /// </summary>
    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE CodeSnippets SET ViewCount = ViewCount + 1 WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 增加代码片段复制次数
    /// </summary>
    public async Task<bool> IncrementCopyCountAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE CodeSnippets SET CopyCount = CopyCount + 1 WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}