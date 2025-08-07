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

    public async Task<PaginatedResult<CodeSnippet>> GetPagedAsync(SnippetFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(filter.Search))
        {
            whereClause += " AND (cs.Title LIKE @Search OR cs.Description LIKE @Search)";
            parameters.Add("Search", $"%{filter.Search}%");
        }
        
        if (!string.IsNullOrEmpty(filter.Language))
        {
            whereClause += " AND cs.Language = @Language";
            parameters.Add("Language", filter.Language);
        }
        
        if (filter.CreatedBy.HasValue)
        {
            whereClause += " AND cs.CreatedBy = @CreatedBy";
            parameters.Add("CreatedBy", filter.CreatedBy.Value);
        }

        // 获取总数
        var countSql = $"SELECT COUNT(*) FROM CodeSnippets cs {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // 获取分页数据
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var dataSql = $@"
            SELECT cs.*, u.Username as CreatorName 
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            {whereClause}
            ORDER BY cs.CreatedAt DESC
            LIMIT @PageSize OFFSET @Offset";

        var items = await connection.QueryAsync<CodeSnippet>(dataSql, parameters);

        return new PaginatedResult<CodeSnippet>
        {
            Items = items,
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

    public async Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.*, u.Username as CreatorName 
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            WHERE cs.CreatedBy = @UserId
            ORDER BY cs.CreatedAt DESC";
        
        return await connection.QueryAsync<CodeSnippet>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<CodeSnippet>> GetByTagAsync(string tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.*, u.Username as CreatorName 
            FROM CodeSnippets cs 
            LEFT JOIN Users u ON cs.CreatedBy = u.Id 
            INNER JOIN SnippetTags st ON cs.Id = st.SnippetId
            INNER JOIN Tags t ON st.TagId = t.Id
            WHERE t.Name = @Tag
            ORDER BY cs.CreatedAt DESC";
        
        return await connection.QueryAsync<CodeSnippet>(sql, new { Tag = tag });
    }
}