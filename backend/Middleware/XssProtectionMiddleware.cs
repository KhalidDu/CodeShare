using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeSnippetManager.Api.Middleware;

/// <summary>
/// XSS防护中间件 - 检测和过滤潜在的XSS攻击
/// </summary>
public class XssProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<XssProtectionMiddleware> _logger;
    private readonly XssProtectionOptions _options;

    // XSS攻击模式
    private static readonly Regex[] XssPatterns = new[]
    {
        new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"vbscript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"onload\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"onerror\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"onclick\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"onmouseover\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<iframe[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<object[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<embed[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"eval\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"expression\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    public XssProtectionMiddleware(RequestDelegate next, ILogger<XssProtectionMiddleware> logger,
        XssProtectionOptions? options = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new XssProtectionOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 只检查POST、PUT、PATCH请求的内容
        if (ShouldCheckRequest(context))
        {
            if (await ContainsXssAsync(context))
            {
                await HandleXssDetectedAsync(context);
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// 判断是否需要检查请求
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否需要检查</returns>
    private bool ShouldCheckRequest(HttpContext context)
    {
        var method = context.Request.Method;
        return method == "POST" || method == "PUT" || method == "PATCH";
    }

    /// <summary>
    /// 检查请求是否包含XSS攻击
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否包含XSS</returns>
    private async Task<bool> ContainsXssAsync(HttpContext context)
    {
        try
        {
            // 检查查询参数
            if (CheckQueryParameters(context))
            {
                return true;
            }

            // 检查请求头
            if (CheckHeaders(context))
            {
                return true;
            }

            // 检查请求体
            if (await CheckRequestBodyAsync(context))
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XSS检查时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 检查查询参数
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否包含XSS</returns>
    private bool CheckQueryParameters(HttpContext context)
    {
        foreach (var param in context.Request.Query)
        {
            var value = param.Value.ToString();
            if (ContainsXssPattern(value))
            {
                _logger.LogWarning("在查询参数 {ParamName} 中检测到XSS攻击: {Value}", 
                    param.Key, value);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查请求头
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否包含XSS</returns>
    private bool CheckHeaders(HttpContext context)
    {
        var headersToCheck = new[] { "User-Agent", "Referer", "X-Forwarded-For" };
        
        foreach (var headerName in headersToCheck)
        {
            if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
            {
                var value = headerValue.ToString();
                if (ContainsXssPattern(value))
                {
                    _logger.LogWarning("在请求头 {HeaderName} 中检测到XSS攻击: {Value}", 
                        headerName, value);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检查请求体
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>是否包含XSS</returns>
    private async Task<bool> CheckRequestBodyAsync(HttpContext context)
    {
        if (!context.Request.HasFormContentType && 
            context.Request.ContentType?.Contains("application/json") != true)
        {
            return false;
        }

        // 启用缓冲以便多次读取请求体
        context.Request.EnableBuffering();
        
        var body = context.Request.Body;
        body.Position = 0;
        
        using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        
        // 重置流位置
        body.Position = 0;

        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        // 检查JSON内容
        if (context.Request.ContentType?.Contains("application/json") == true)
        {
            return CheckJsonContent(content);
        }

        // 检查表单内容
        if (context.Request.HasFormContentType)
        {
            return CheckFormContent(content);
        }

        // 检查原始内容
        return ContainsXssPattern(content);
    }

    /// <summary>
    /// 检查JSON内容
    /// </summary>
    /// <param name="content">JSON内容</param>
    /// <returns>是否包含XSS</returns>
    private bool CheckJsonContent(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            return CheckJsonElement(document.RootElement);
        }
        catch (JsonException)
        {
            // 如果JSON解析失败，检查原始内容
            return ContainsXssPattern(content);
        }
    }

    /// <summary>
    /// 递归检查JSON元素
    /// </summary>
    /// <param name="element">JSON元素</param>
    /// <returns>是否包含XSS</returns>
    private bool CheckJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var stringValue = element.GetString();
                if (!string.IsNullOrEmpty(stringValue) && ContainsXssPattern(stringValue))
                {
                    _logger.LogWarning("在JSON字符串中检测到XSS攻击: {Value}", stringValue);
                    return true;
                }
                break;

            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (CheckJsonElement(property.Value))
                    {
                        return true;
                    }
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (CheckJsonElement(item))
                    {
                        return true;
                    }
                }
                break;
        }

        return false;
    }

    /// <summary>
    /// 检查表单内容
    /// </summary>
    /// <param name="content">表单内容</param>
    /// <returns>是否包含XSS</returns>
    private bool CheckFormContent(string content)
    {
        var pairs = content.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                var value = Uri.UnescapeDataString(parts[1]);
                if (ContainsXssPattern(value))
                {
                    _logger.LogWarning("在表单字段中检测到XSS攻击: {Value}", value);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检查字符串是否包含XSS模式
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>是否包含XSS模式</returns>
    private bool ContainsXssPattern(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // URL解码
        var decodedInput = Uri.UnescapeDataString(input);
        
        // HTML解码
        decodedInput = System.Net.WebUtility.HtmlDecode(decodedInput);

        // 检查所有XSS模式
        foreach (var pattern in XssPatterns)
        {
            if (pattern.IsMatch(decodedInput))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 处理检测到XSS攻击的情况
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    private async Task HandleXssDetectedAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        
        _logger.LogWarning("检测到XSS攻击尝试 - IP: {ClientId}, UserAgent: {UserAgent}, Path: {Path}", 
            clientId, userAgent, context.Request.Path);

        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Bad Request",
            message = "请求包含不安全的内容",
            code = "XSS_DETECTED"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

/// <summary>
/// XSS防护配置选项
/// </summary>
public class XssProtectionOptions
{
    /// <summary>
    /// 是否启用XSS防护
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否记录XSS攻击尝试
    /// </summary>
    public bool LogAttempts { get; set; } = true;

    /// <summary>
    /// 是否阻止包含XSS的请求
    /// </summary>
    public bool BlockRequests { get; set; } = true;
}