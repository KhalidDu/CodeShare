using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Middleware;

/// <summary>
/// JWT中间件 - 处理JWT Token验证和用户上下文设置
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        var token = ExtractTokenFromHeader(context);

        if (!string.IsNullOrEmpty(token))
        {
            await AttachUserToContextAsync(context, token, userRepository);
        }

        await _next(context);
    }

    /// <summary>
    /// 从请求头中提取JWT Token
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>JWT Token</returns>
    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }

    /// <summary>
    /// 验证JWT Token并将用户信息附加到上下文
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="token">JWT Token</param>
    /// <param name="userRepository">用户仓储</param>
    private async Task AttachUserToContextAsync(HttpContext context, string token, IUserRepository userRepository)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
            var key = Encoding.ASCII.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // 获取用户ID
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("JWT Token中的用户ID无效: {UserIdClaim}", userIdClaim);
                return;
            }

            // 验证用户是否存在且处于活跃状态
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("用户不存在或已被禁用: {UserId}", userId);
                return;
            }

            // 将用户信息添加到HTTP上下文
            context.Items["User"] = user;
            context.Items["UserId"] = userId;

            _logger.LogDebug("用户 {UserId} 的JWT Token验证成功", userId);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("JWT Token已过期");
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("JWT Token验证失败: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT Token验证时发生未知错误");
        }
    }
}