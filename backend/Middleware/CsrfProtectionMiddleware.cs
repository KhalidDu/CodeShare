using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CodeSnippetManager.Api.Middleware;

/// <summary>
/// CSRF防护中间件 - 防止跨站请求伪造攻击
/// </summary>
public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CsrfProtectionMiddleware> _logger;
    private readonly CsrfProtectionOptions _options;
    
    private const string CsrfTokenHeaderName = "X-CSRF-Token";
    private const string CsrfTokenCookieName = "CSRF-TOKEN";

    public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger,
        CsrfProtectionOptions? options = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new CsrfProtectionOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 为GET请求生成CSRF令牌
        if (context.Request.Method == "GET" && ShouldGenerateToken(context))
        {
            GenerateCsrfToken(context);
        }
        // 验证需要CSRF保护的请求
        else if (RequiresCsrfProtection(context))
        {
            if (!await ValidateCsrfTokenAsync(context))
            {
                await HandleCsrfValidationFailureAsync(context);
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// 判断是否应该生成CSRF令牌
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否应该生成令牌</returns>
    private bool ShouldGenerateToken(HttpContext context)
    {
        // 跳过API文档和健康检查端点
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        if (path.StartsWith("/swagger") || 
            path.StartsWith("/health") || 
            path.StartsWith("/api/auth/token"))
        {
            return false;
        }

        // 为需要后续POST操作的页面生成令牌
        return path.StartsWith("/api/") || path == "/";
    }

    /// <summary>
    /// 判断请求是否需要CSRF保护
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否需要CSRF保护</returns>
    private bool RequiresCsrfProtection(HttpContext context)
    {
        var method = context.Request.Method;
        
        // 只保护状态改变的操作
        if (method != "POST" && method != "PUT" && method != "DELETE" && method != "PATCH")
        {
            return false;
        }

        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // 跳过不需要CSRF保护的端点
        var exemptPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/refresh",
            "/api/share/access",
            "/swagger",
            "/health"
        };

        foreach (var exemptPath in exemptPaths)
        {
            if (path.StartsWith(exemptPath))
            {
                return false;
            }
        }

        // API端点需要CSRF保护
        return path.StartsWith("/api/");
    }

    /// <summary>
    /// 生成CSRF令牌
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    private void GenerateCsrfToken(HttpContext context)
    {
        try
        {
            // 检查是否已有有效的CSRF令牌
            if (context.Request.Cookies.TryGetValue(CsrfTokenCookieName, out var existingToken) &&
                IsValidTokenFormat(existingToken))
            {
                return;
            }

            // 生成新的CSRF令牌
            var token = GenerateSecureToken();
            
            // 设置CSRF令牌Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = false, // 前端需要读取此Cookie
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromHours(_options.TokenLifetimeHours),
                Path = "/"
            };

            context.Response.Cookies.Append(CsrfTokenCookieName, token, cookieOptions);
            
            // 在响应头中也包含令牌（可选）
            context.Response.Headers["X-CSRF-Token"] = token;
            
            _logger.LogDebug("为客户端生成了新的CSRF令牌");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成CSRF令牌时发生错误");
        }
    }

    /// <summary>
    /// 验证CSRF令牌
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>验证是否成功</returns>
    private Task<bool> ValidateCsrfTokenAsync(HttpContext context)
    {
        try
        {
            // 从请求头获取CSRF令牌
            var headerToken = context.Request.Headers[CsrfTokenHeaderName].FirstOrDefault();
            
            // 从Cookie获取CSRF令牌
            var cookieToken = context.Request.Cookies[CsrfTokenCookieName];
            
            // 检查令牌是否存在
            if (string.IsNullOrEmpty(headerToken) || string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogWarning("CSRF令牌缺失 - Header: {HeaderToken}, Cookie: {CookieToken}", 
                    !string.IsNullOrEmpty(headerToken), !string.IsNullOrEmpty(cookieToken));
                return Task.FromResult(false);
            }

            // 验证令牌格式
            if (!IsValidTokenFormat(headerToken) || !IsValidTokenFormat(cookieToken))
            {
                _logger.LogWarning("CSRF令牌格式无效");
                return Task.FromResult(false);
            }

            // 验证令牌是否匹配
            if (!SecureStringCompare(headerToken, cookieToken))
            {
                _logger.LogWarning("CSRF令牌不匹配");
                return Task.FromResult(false);
            }

            // 验证令牌是否过期
            if (IsTokenExpired(headerToken))
            {
                _logger.LogWarning("CSRF令牌已过期");
                return Task.FromResult(false);
            }

            _logger.LogDebug("CSRF令牌验证成功");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证CSRF令牌时发生错误");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 生成安全的随机令牌
    /// </summary>
    /// <returns>安全令牌</returns>
    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        
        // 添加时间戳
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampBytes = BitConverter.GetBytes(timestamp);
        
        // 组合令牌和时间戳
        var combinedBytes = new byte[tokenBytes.Length + timestampBytes.Length];
        Array.Copy(tokenBytes, 0, combinedBytes, 0, tokenBytes.Length);
        Array.Copy(timestampBytes, 0, combinedBytes, tokenBytes.Length, timestampBytes.Length);
        
        return Convert.ToBase64String(combinedBytes);
    }

    /// <summary>
    /// 验证令牌格式
    /// </summary>
    /// <param name="token">令牌</param>
    /// <returns>格式是否有效</returns>
    private bool IsValidTokenFormat(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        try
        {
            var bytes = Convert.FromBase64String(token);
            return bytes.Length >= 36; // 32字节令牌 + 8字节时间戳
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查令牌是否过期
    /// </summary>
    /// <param name="token">令牌</param>
    /// <returns>是否过期</returns>
    private bool IsTokenExpired(string token)
    {
        try
        {
            var bytes = Convert.FromBase64String(token);
            if (bytes.Length < 40)
            {
                return true;
            }

            // 提取时间戳
            var timestampBytes = new byte[8];
            Array.Copy(bytes, bytes.Length - 8, timestampBytes, 0, 8);
            var timestamp = BitConverter.ToInt64(timestampBytes, 0);
            
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var expiryTime = tokenTime.AddHours(_options.TokenLifetimeHours);
            
            return DateTimeOffset.UtcNow > expiryTime;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// 安全字符串比较（防止时序攻击）
    /// </summary>
    /// <param name="a">字符串A</param>
    /// <param name="b">字符串B</param>
    /// <returns>是否相等</returns>
    private bool SecureStringCompare(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    /// <summary>
    /// 处理CSRF验证失败
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    private async Task HandleCsrfValidationFailureAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        
        _logger.LogWarning("CSRF验证失败 - IP: {ClientId}, UserAgent: {UserAgent}, Path: {Path}", 
            clientId, userAgent, context.Request.Path);

        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Forbidden",
            message = "CSRF令牌验证失败",
            code = "CSRF_TOKEN_INVALID"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

/// <summary>
/// CSRF防护配置选项
/// </summary>
public class CsrfProtectionOptions
{
    /// <summary>
    /// 令牌生命周期（小时）
    /// </summary>
    public int TokenLifetimeHours { get; set; } = 24;

    /// <summary>
    /// 是否启用CSRF保护
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否记录CSRF攻击尝试
    /// </summary>
    public bool LogAttempts { get; set; } = true;
}