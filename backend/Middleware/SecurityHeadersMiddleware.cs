using System.Security.Cryptography;
using System.Text;

namespace CodeSnippetManager.Api.Middleware;

/// <summary>
/// 安全头中间件 - 添加各种安全相关的HTTP头
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 添加安全头
        AddSecurityHeaders(context);

        await _next(context);
    }

    /// <summary>
    /// 添加安全相关的HTTP头
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    private void AddSecurityHeaders(HttpContext context)
    {
        var response = context.Response;

        // X-Content-Type-Options: 防止MIME类型嗅探攻击
        if (!response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            response.Headers["X-Content-Type-Options"] = "nosniff";
        }

        // X-Frame-Options: 防止点击劫持攻击
        if (!response.Headers.ContainsKey("X-Frame-Options"))
        {
            response.Headers["X-Frame-Options"] = "DENY";
        }

        // X-XSS-Protection: 启用浏览器XSS过滤器
        if (!response.Headers.ContainsKey("X-XSS-Protection"))
        {
            response.Headers["X-XSS-Protection"] = "1; mode=block";
        }

        // Referrer-Policy: 控制引用信息的发送
        if (!response.Headers.ContainsKey("Referrer-Policy"))
        {
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        }

        // Content-Security-Policy: 防止XSS和数据注入攻击
        if (!response.Headers.ContainsKey("Content-Security-Policy"))
        {
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                     "style-src 'self' 'unsafe-inline'; " +
                     "img-src 'self' data: https:; " +
                     "font-src 'self' data:; " +
                     "connect-src 'self'; " +
                     "frame-ancestors 'none';";
            
            response.Headers["Content-Security-Policy"] = csp;
        }

        // Strict-Transport-Security: 强制使用HTTPS
        if (context.Request.IsHttps && !response.Headers.ContainsKey("Strict-Transport-Security"))
        {
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        // Permissions-Policy: 控制浏览器功能的使用
        if (!response.Headers.ContainsKey("Permissions-Policy"))
        {
            response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
        }

        _logger.LogDebug("安全头已添加到响应中");
    }
}