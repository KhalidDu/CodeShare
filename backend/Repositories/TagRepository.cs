using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 标签仓储实现 - 使用 Dapper ORM
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TagRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tags WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<Tag>(sql, new { Id = id });
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tags WHERE Name = @Name";
        
        return await connection.QuerySingleOrDefaultAsync<Tag>(sql, new { Name = name });
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tags ORDER BY Name";
        
        return await connection.QueryAsync<Tag>(sql);
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Tags (Id, Name, Color, CreatedBy, CreatedAt)
            VALUES (@Id, @Name, @Color, @CreatedBy, @CreatedAt)";
        
        await connection.ExecuteAsync(sql, tag);
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Tags 
            SET Name = @Name, Color = @Color
            WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, tag);
        return tag;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Tags WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Tag>> GetTagsBySnippetIdAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT t.* 
            FROM Tags t
            INNER JOIN SnippetTags st ON t.Id = st.TagId
            WHERE st.SnippetId = @SnippetId
            ORDER BY t.Name";
        
        return await connection.QueryAsync<Tag>(sql, new { SnippetId = snippetId });
    }
}