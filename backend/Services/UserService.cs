using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 用户业务服务实现 - 遵循单一职责原则
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _repository.GetByUsernameAsync(username);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto user)
    {
        var entity = new User
        {
            Id = Guid.NewGuid(),
            Username = user.Username,
            Email = user.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
            Role = user.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto user)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new ArgumentException("User not found");

        if (!string.IsNullOrEmpty(user.Username))
            existing.Username = user.Username;
        if (!string.IsNullOrEmpty(user.Email))
            existing.Email = user.Email;
        if (user.Role.HasValue)
            existing.Role = user.Role.Value;
        if (user.IsActive.HasValue)
            existing.IsActive = user.IsActive.Value;

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<bool> ResetPasswordAsync(Guid id, string newPassword)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(user);
        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}