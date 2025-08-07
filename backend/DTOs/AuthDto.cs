using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 用户登录请求DTO
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 用户注册请求DTO
/// </summary>
public class RegisterDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "确认密码不能为空")]
    [Compare("Password", ErrorMessage = "密码和确认密码不匹配")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// 认证响应DTO
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}