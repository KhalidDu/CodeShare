using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 用户业务服务接口 - 遵循接口隔离原则
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetUserAsync(Guid id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto> CreateUserAsync(CreateUserDto user);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto user);
    Task<bool> DeleteUserAsync(Guid id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> ResetPasswordAsync(Guid id, string newPassword);
}