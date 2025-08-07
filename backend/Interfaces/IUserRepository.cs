using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 用户仓储接口 - 遵循单一职责原则
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
}