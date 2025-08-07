using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 用户仓储实现 - 使用 Dapper ORM
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Username = @Username";
        
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Email = @Email";
        
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt, UpdatedAt, IsActive)
            VALUES (@Id, @Username, @Email, @PasswordHash, @Role, @CreatedAt, @UpdatedAt, @IsActive)";
        
        await connection.ExecuteAsync(sql, user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Users 
            SET Username = @Username, Email = @Email, Role = @Role, 
                UpdatedAt = @UpdatedAt, IsActive = @IsActive
            WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, user);
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Users ORDER BY CreatedAt DESC";
        
        return await connection.QueryAsync<User>(sql);
    }
}