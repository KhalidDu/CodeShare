using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 认证服务实现 - 遵循面向接口编程原则
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IInputValidationService _validationService;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IInputValidationService validationService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // 验证输入
        var usernameValidation = _validationService.ValidateUsername(loginDto.Username);
        if (!usernameValidation.IsValid)
        {
            throw new ArgumentException(usernameValidation.ErrorMessage);
        }

        // 清理输入
        var sanitizedUsername = _validationService.SanitizeUserInput(loginDto.Username);
        
        var user = await _userRepository.GetByUsernameAsync(sanitizedUsername);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is disabled");
        }

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = MapToUserDto(user)
        };
    }

    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implement refresh token validation
        throw new NotImplementedException("Refresh token functionality will be implemented in later tasks");
    }

    public async Task<bool> LogoutAsync(string token)
    {
        // TODO: Implement token blacklisting
        // 当前实现只是简单返回成功，实际应该实现Token黑名单功能
        return await Task.FromResult(true);
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        // 验证输入
        var usernameValidation = _validationService.ValidateUsername(registerDto.Username);
        if (!usernameValidation.IsValid)
        {
            throw new ArgumentException(usernameValidation.ErrorMessage);
        }

        var emailValidation = _validationService.ValidateEmail(registerDto.Email);
        if (!emailValidation.IsValid)
        {
            throw new ArgumentException(emailValidation.ErrorMessage);
        }

        var passwordValidation = _validationService.ValidatePassword(registerDto.Password);
        if (!passwordValidation.IsValid)
        {
            throw new ArgumentException(passwordValidation.ErrorMessage);
        }

        // 清理输入
        var sanitizedUsername = _validationService.SanitizeUserInput(registerDto.Username);
        var sanitizedEmail = _validationService.SanitizeUserInput(registerDto.Email);

        // Check if user already exists
        var existingUser = await _userRepository.GetByUsernameAsync(sanitizedUsername);
        if (existingUser != null)
        {
            throw new ArgumentException("Username already exists");
        }

        existingUser = await _userRepository.GetByEmailAsync(sanitizedEmail);
        if (existingUser != null)
        {
            throw new ArgumentException("Email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = sanitizedUsername,
            Email = sanitizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.Viewer, // Default role
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _userRepository.CreateAsync(user);
        return MapToUserDto(created);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        // 验证新密码
        var passwordValidation = _validationService.ValidatePassword(newPassword);
        if (!passwordValidation.IsValid)
        {
            throw new ArgumentException(passwordValidation.ErrorMessage);
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // 验证当前密码
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // 更新密码
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    private static UserDto MapToUserDto(User user)
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