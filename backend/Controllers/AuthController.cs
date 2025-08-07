using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using System.Security.Claims;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 认证控制器 - 遵循单一职责原则，只负责认证相关的HTTP请求处理
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="loginDto">登录信息</param>
    /// <returns>认证响应</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(loginDto);
            _logger.LogInformation("用户 {Username} 登录成功", loginDto.Username);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {Username} 登录失败: {Message}", loginDto.Username, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户 {Username} 登录时发生错误", loginDto.Username);
            return StatusCode(500, new { message = "登录时发生内部错误" });
        }
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="registerDto">注册信息</param>
    /// <returns>用户信息</returns>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authService.RegisterAsync(registerDto);
            _logger.LogInformation("用户 {Username} 注册成功", registerDto.Username);
            
            return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("用户 {Username} 注册失败: {Message}", registerDto.Username, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户 {Username} 注册时发生错误", registerDto.Username);
            return StatusCode(500, new { message = "注册时发生内部错误" });
        }
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <returns>登出结果</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token不能为空" });
            }

            var result = await _authService.LogoutAsync(token);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("用户 {UserId} 登出成功", userId);
            return Ok(new { message = "登出成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户登出时发生错误");
            return StatusCode(500, new { message = "登出时发生内部错误" });
        }
    }

    /// <summary>
    /// 刷新Token
    /// </summary>
    /// <param name="refreshToken">刷新Token</param>
    /// <returns>新的认证响应</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "刷新Token不能为空" });
            }

            var response = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("刷新Token失败: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新Token时发生错误");
            return StatusCode(500, new { message = "刷新Token时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>当前用户信息</returns>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "无效的用户信息" });
            }

            return Ok(new
            {
                id = userId,
                username = username,
                email = email,
                role = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取当前用户信息时发生错误");
            return StatusCode(500, new { message = "获取用户信息时发生内部错误" });
        }
    }
}