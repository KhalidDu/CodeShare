using System.Collections.Concurrent;
using System.Net;

namespace CodeSnippetManager.Api.Middleware;

/// <summary>
/// 请求频率限制中间件 - 防止API滥用和DDoS攻击
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    
    // 存储客户端请求记录
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
    
    // 清理任务的取消令牌
    private static readonly Timer _cleanupTimer = new(CleanupExpiredEntries, null, 
        TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, 
        RateLimitOptions? options = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new RateLimitOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (await IsRateLimitExceededAsync(clientId, context))
        {
            await HandleRateLimitExceededAsync(context, clientId);
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// 获取客户端标识符
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>客户端标识符</returns>
    private string GetClientIdentifier(HttpContext context)
    {
        // 优先使用用户ID（如果已认证）
        var userId = context.Items["UserId"]?.ToString();
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // 使用IP地址作为标识符
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // 检查是否通过代理
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ipAddress = forwardedFor.Split(',')[0].Trim();
        }

        return $"ip:{ipAddress}";
    }

    /// <summary>
    /// 检查是否超过频率限制
    /// </summary>
    /// <param name="clientId">客户端标识符</param>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否超过限制</returns>
    private Task<bool> IsRateLimitExceededAsync(string clientId, HttpContext context)
    {
        var now = DateTime.UtcNow;
        var endpoint = GetEndpointKey(context);
        
        var clientInfo = _clients.GetOrAdd(clientId, _ => new ClientRequestInfo());
        
        lock (clientInfo)
        {
            // 清理过期的请求记录
            clientInfo.Requests.RemoveAll(r => now - r.Timestamp > _options.TimeWindow);
            
            // 获取当前端点的限制配置
            var limit = GetEndpointLimit(endpoint);
            
            // 检查是否超过限制
            var requestsInWindow = clientInfo.Requests.Count(r => r.Endpoint == endpoint);
            
            if (requestsInWindow >= limit)
            {
                _logger.LogWarning("客户端 {ClientId} 在端点 {Endpoint} 超过频率限制: {Count}/{Limit}", 
                    clientId, endpoint, requestsInWindow, limit);
                return Task.FromResult(true);
            }
            
            // 记录当前请求
            clientInfo.Requests.Add(new RequestRecord
            {
                Timestamp = now,
                Endpoint = endpoint
            });
            
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 获取端点键
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>端点键</returns>
    private string GetEndpointKey(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";
        
        // 标准化路径（移除参数）
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && segments[0] == "api")
        {
            return $"{method}:/api/{segments[1]}";
        }
        
        return $"{method}:{path}";
    }

    /// <summary>
    /// 获取端点的频率限制
    /// </summary>
    /// <param name="endpoint">端点键</param>
    /// <returns>频率限制</returns>
    private int GetEndpointLimit(string endpoint)
    {
        // 认证相关端点更严格的限制
        if (endpoint.Contains("/auth/") || endpoint.Contains("/login") || endpoint.Contains("/register"))
        {
            return _options.AuthEndpointLimit;
        }
        
        // POST/PUT/DELETE 操作更严格的限制
        if (endpoint.StartsWith("POST:") || endpoint.StartsWith("PUT:") || endpoint.StartsWith("DELETE:"))
        {
            return _options.WriteOperationLimit;
        }
        
        // 默认读取操作限制
        return _options.DefaultLimit;
    }

    /// <summary>
    /// 处理频率限制超出的情况
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="clientId">客户端标识符</param>
    private async Task HandleRateLimitExceededAsync(HttpContext context, string clientId)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";
        
        var retryAfter = (int)_options.TimeWindow.TotalSeconds;
        context.Response.Headers["Retry-After"] = retryAfter.ToString();
        context.Response.Headers["X-RateLimit-Limit"] = _options.DefaultLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = "0";
        context.Response.Headers["X-RateLimit-Reset"] = 
            DateTimeOffset.UtcNow.Add(_options.TimeWindow).ToUnixTimeSeconds().ToString();
        
        var response = new
        {
            error = "Rate limit exceeded",
            message = "请求过于频繁，请稍后再试",
            retryAfter = retryAfter
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        
        _logger.LogWarning("客户端 {ClientId} 触发频率限制，返回429状态码", clientId);
    }

    /// <summary>
    /// 清理过期的客户端记录
    /// </summary>
    /// <param name="state">状态对象</param>
    private static void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredClients = new List<string>();
        
        foreach (var kvp in _clients)
        {
            var clientInfo = kvp.Value;
            lock (clientInfo)
            {
                // 清理过期请求
                clientInfo.Requests.RemoveAll(r => now - r.Timestamp > TimeSpan.FromHours(1));
                
                // 如果客户端没有最近的请求，标记为过期
                if (!clientInfo.Requests.Any())
                {
                    expiredClients.Add(kvp.Key);
                }
            }
        }
        
        // 移除过期的客户端记录
        foreach (var clientId in expiredClients)
        {
            _clients.TryRemove(clientId, out _);
        }
    }
}

/// <summary>
/// 频率限制配置选项
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// 时间窗口（默认1分钟）
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// 默认请求限制（每分钟）
    /// </summary>
    public int DefaultLimit { get; set; } = 100;
    
    /// <summary>
    /// 认证端点请求限制（每分钟）
    /// </summary>
    public int AuthEndpointLimit { get; set; } = 10;
    
    /// <summary>
    /// 写操作请求限制（每分钟）
    /// </summary>
    public int WriteOperationLimit { get; set; } = 30;
}

/// <summary>
/// 客户端请求信息
/// </summary>
internal class ClientRequestInfo
{
    public List<RequestRecord> Requests { get; } = new();
}

/// <summary>
/// 请求记录
/// </summary>
internal class RequestRecord
{
    public DateTime Timestamp { get; set; }
    public string Endpoint { get; set; } = string.Empty;
}