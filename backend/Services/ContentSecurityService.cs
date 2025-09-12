using System.Text;
using System.Text.RegularExpressions;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 内容安全服务实现 - 提供完整的内容验证和安全检查功能
/// 实现XSS防护、SQL注入防护、内容分析和安全审计
/// </summary>
public class ContentSecurityService : IContentSecurityService
{
    private readonly ILogger<ContentSecurityService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IInputValidationService _inputValidationService;
    private readonly IPermissionService _permissionService;

    // 正则表达式模式
    private static readonly Regex[] XssPatterns = new[]
    {
        new Regex(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline),
        new Regex(@"javascript:", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"vbscript:", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"on\w+\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"eval\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"expression\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"url\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"import\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"<iframe[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"<object[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"<embed[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"<link[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"<meta[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"<\?php[^>]*\?>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline),
        new Regex(@"<%[^>]*%>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline),
        new Regex(@"<!\[CDATA\[.*?\]\]>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)
    };

    private static readonly Regex[] SqlInjectionPatterns = new[]
    {
        new Regex(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE){0,1}|INSERT( +INTO){0,1}|MERGE|SELECT|UPDATE|UNION( +ALL){0,1})\b)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(WHERE|GROUP BY|HAVING|ORDER BY|LIMIT|OFFSET)\b)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(OR|AND)\s+\d+\s*=\s*\d+\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(OR|AND)\s+\d+\s*=\s*'\w+'\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(OR|AND)\s+'\w+'\s*=\s*'\w+'\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(OR|AND)\s+\d+\s*=\s*\d+\s*--\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(OR|AND)\s+'\w+'\s*=\s*'\w+'\s*--\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(XP_|SP_)\w+\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(WAITFOR\s+DELAY|BULK\s+INSERT|OPENROWSET|OPENDATASOURCE)\b)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\;\s*(DROP|DELETE|UPDATE|INSERT)\s+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"('\s*OR\s*'1'\s*=\s*'1')", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"('\s*OR\s*1\s*=\s*1\s*--)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\*\s*OR\s*1\s*=\s*1\s*--)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
    };

    private static readonly Regex[] CommandInjectionPatterns = new[]
    {
        new Regex(@"(\b(||&|;|`|\$\(|\${)\b)", RegexOptions.Compiled),
        new Regex(@"(\b(cat|ls|pwd|whoami|id|ps|kill|rm|mv|cp|wget|curl|nc|net|nslookup|dig)\b)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(powershell|cmd|bash|sh|zsh|fish)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"(\b(/bin/|/usr/bin/|/usr/local/bin/)\w+\b)", RegexOptions.Compiled),
        new Regex(@"(\${.*})", RegexOptions.Compiled),
        new Regex(@"(\$\w+)", RegexOptions.Compiled),
        new Regex(@"(;\s*\w+\s*)", RegexOptions.Compiled)
    };

    // 危险关键词
    private static readonly string[] DangerousKeywords = new[]
    {
        "<script", "</script>", "javascript:", "vbscript:", "onload=", "onerror=", "onclick=", "onmouseover=",
        "eval(", "expression(", "url(", "import(", "document.cookie", "window.location", "alert(", "confirm(",
        "prompt(", "setTimeout(", "setInterval(", "innerHTML", "outerHTML", "document.write", "document.writeln"
    };

    // 敏感词列表
    private static readonly string[] DefaultSensitiveWords = new[]
    {
        "管理员", "密码", "口令", "token", "secret", "key", "api_key", "private_key", "access_token",
        "refresh_token", "session", "cookie", "auth", "credential", "login", "signin", "signup",
        "银行", "信用卡", "身份证", "护照", "银行卡", "支付", "密码", "账户", "资金", "交易"
    };

    public ContentSecurityService(
        ILogger<ContentSecurityService> logger,
        IMemoryCache cache,
        IInputValidationService inputValidationService,
        IPermissionService permissionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _inputValidationService = inputValidationService ?? throw new ArgumentNullException(nameof(inputValidationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
    }

    // 内容验证
    #region 内容验证

    /// <inheritdoc/>
    public async Task<ContentSecurityResult> ValidateContentAsync(string content, string contentType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new ContentSecurityResult
                {
                    IsValid = false,
                    ErrorMessage = "内容不能为空",
                    SecurityLevel = SecurityLevel.High
                };
            }

            var contentHash = GenerateContentHash(content);
            
            // 检查缓存
            var cacheKey = $"content_security_{contentHash}";
            if (_cache.TryGetValue(cacheKey, out ContentSecurityResult? cachedResult))
            {
                return cachedResult ?? new ContentSecurityResult { IsValid = false, ErrorMessage = "缓存错误" };
            }

            var result = new ContentSecurityResult
            {
                ContentHash = contentHash,
                ValidatedAt = DateTime.UtcNow
            };

            // 基本长度检查
            if (content.Length > 50000) // 50KB
            {
                result.IsValid = false;
                result.ErrorMessage = "内容长度超过限制";
                result.SecurityLevel = SecurityLevel.Medium;
                result.DetectedIssues = new List<string> { "内容过长" };
                return result;
            }

            // XSS检查
            var xssResult = await DetectXssAsync(content);
            if (xssResult.ContainsXss)
            {
                result.IsValid = false;
                result.SecurityLevel = SecurityLevel.Critical;
                result.DetectedIssues = xssResult.DetectedPatterns.ToList();
                result.ErrorMessage = "检测到XSS攻击风险";
                return result;
            }

            // SQL注入检查
            var sqlResult = await DetectSqlInjectionAsync(content);
            if (sqlResult.ContainsSqlInjection)
            {
                result.IsValid = false;
                result.SecurityLevel = SecurityLevel.Critical;
                result.DetectedIssues = sqlResult.DetectedPatterns.ToList();
                result.ErrorMessage = "检测到SQL注入攻击风险";
                return result;
            }

            // 敏感词检查
            var sensitiveResult = await CheckSensitiveWordsAsync(content);
            if (sensitiveResult.ContainsSensitiveWords)
            {
                result.SecurityLevel = SecurityLevel.Medium;
                result.DetectedIssues = sensitiveResult.DetectedWords.Select(w => $"敏感词: {w}").ToList();
                
                // 根据敏感词严重程度决定是否允许通过
                if (sensitiveResult.SeverityScore > 0.8)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "包含高敏感度内容";
                    result.SecurityLevel = SecurityLevel.High;
                    return result;
                }
            }

            // 内容类型特定检查
            switch (contentType.ToLower())
            {
                case "comment":
                    result = await ValidateCommentContent(content, result);
                    break;
                case "message":
                    result = await ValidateMessageContent(content, result);
                    break;
                case "notification":
                    result = await ValidateNotificationContent(content, result);
                    break;
                case "code":
                    result = await ValidateCodeContent(content, "text", result);
                    break;
            }

            // 获取安全评分
            result.SecurityLevel = await CalculateSecurityLevel(result);

            // 缓存结果
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "内容安全验证失败: {Content}", content);
            return new ContentSecurityResult
            {
                IsValid = false,
                ErrorMessage = "安全验证系统错误",
                SecurityLevel = SecurityLevel.High
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ContentSecurityResult> ValidateMessageContentAsync(string content, string messageType)
    {
        var result = await ValidateContentAsync(content, "message");
        
        // 消息特定验证
        if (result.IsValid)
        {
            // 检查是否包含链接
            var urlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled);
            var urls = urlRegex.Matches(content);
            if (urls.Count > 5)
            {
                result.IsValid = false;
                result.ErrorMessage = "消息中包含过多链接";
                result.SecurityLevel = SecurityLevel.Medium;
            }

            // 检查消息长度
            if (content.Length > 1000)
            {
                result.IsValid = false;
                result.ErrorMessage = "消息长度超过限制";
                result.SecurityLevel = SecurityLevel.Low;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ContentSecurityResult> ValidateNotificationContentAsync(string title, string content)
    {
        var titleResult = await ValidateContentAsync(title, "notification");
        var contentResult = await ValidateContentAsync(content, "notification");

        if (!titleResult.IsValid || !contentResult.IsValid)
        {
            return new ContentSecurityResult
            {
                IsValid = false,
                ErrorMessage = titleResult.ErrorMessage ?? contentResult.ErrorMessage,
                SecurityLevel = SecurityLevel.High
            };
        }

        var result = new ContentSecurityResult
        {
            IsValid = true,
            SecurityLevel = SecurityLevel.Low,
            ValidatedAt = DateTime.UtcNow,
            ContentHash = GenerateContentHash(title + content)
        };

        // 通知特定验证
        if (title.Length > 200)
        {
            result.IsValid = false;
            result.ErrorMessage = "通知标题过长";
            result.SecurityLevel = SecurityLevel.Low;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<CodeSecurityResult> ValidateCodeContentAsync(string code, string language)
    {
        var result = new CodeSecurityResult
        {
            Language = language,
            ValidatedAt = DateTime.UtcNow,
            ContentHash = GenerateContentHash(code)
        };

        // 基本验证
        if (string.IsNullOrWhiteSpace(code))
        {
            result.IsValid = false;
            result.ErrorMessage = "代码内容不能为空";
            result.SecurityLevel = SecurityLevel.High;
            return result;
        }

        // 代码长度检查
        if (code.Length > 100000) // 100KB
        {
            result.IsValid = false;
            result.ErrorMessage = "代码长度超过限制";
            result.SecurityLevel = SecurityLevel.Medium;
            return result;
        }

        // 代码行数统计
        result.CodeLines = code.Split('\n').Length;

        // 根据语言进行特定检查
        switch (language.ToLower())
        {
            case "sql":
                result = await ValidateSqlCode(code, result);
                break;
            case "javascript":
            case "js":
                result = await ValidateJavaScriptCode(code, result);
                break;
            case "php":
                result = await ValidatePhpCode(code, result);
                break;
            case "python":
                result = await ValidatePythonCode(code, result);
                break;
            case "csharp":
            case "c#":
                result = await ValidateCSharpCode(code, result);
                break;
            default:
                result = await ValidateGenericCode(code, result);
                break;
        }

        // 安全评分
        result.SecurityLevel = await CalculateSecurityLevel(result);

        return result;
    }

    /// <inheritdoc/>
    public async Task<BatchContentSecurityResult> ValidateContentBatchAsync(IEnumerable<ContentValidationRequest> contents)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var results = new List<ContentSecurityResult>();
        var validCount = 0;
        var invalidCount = 0;

        foreach (var content in contents)
        {
            var result = await ValidateContentAsync(content.Content, content.ContentType);
            results.Add(result);
            
            if (result.IsValid)
                validCount++;
            else
                invalidCount++;
        }

        stopwatch.Stop();

        return new BatchContentSecurityResult
        {
            TotalItems = results.Count,
            ValidItems = validCount,
            InvalidItems = invalidCount,
            Results = results,
            ProcessedAt = DateTime.UtcNow,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <inheritdoc/>
    public async Task<SensitiveWordResult> CheckSensitiveWordsAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new SensitiveWordResult();
        }

        var lowerContent = content.ToLower();
        var detectedWords = new List<string>();
        var wordCategories = new HashSet<string>();

        // 获取敏感词列表
        var sensitiveWords = await GetSensitiveWordsAsync();

        foreach (var word in sensitiveWords)
        {
            if (lowerContent.Contains(word.ToLower()))
            {
                detectedWords.Add(word);
                wordCategories.Add(GetWordCategory(word));
            }
        }

        // 计算严重程度
        double severityScore = 0;
        if (detectedWords.Count > 0)
        {
            severityScore = Math.Min(detectedWords.Count * 0.2, 1.0);
            
            // 高敏感词加重
            if (detectedWords.Any(w => IsHighlySensitiveWord(w)))
            {
                severityScore = Math.Min(severityScore + 0.3, 1.0);
            }
        }

        return new SensitiveWordResult
        {
            ContainsSensitiveWords = detectedWords.Count > 0,
            DetectedWords = detectedWords,
            WordCategories = wordCategories,
            TotalCount = detectedWords.Count,
            SeverityScore = severityScore
        };
    }

    /// <inheritdoc/>
    public async Task<int> GetContentSafetyScoreAsync(string content)
    {
        var result = await ValidateContentAsync(content, "general");
        
        // 基础分数
        int score = 100;
        
        // 根据安全级别扣分
        switch (result.SecurityLevel)
        {
            case SecurityLevel.Critical:
                score -= 80;
                break;
            case SecurityLevel.High:
                score -= 60;
                break;
            case SecurityLevel.Medium:
                score -= 40;
                break;
            case SecurityLevel.Low:
                score -= 20;
                break;
        }
        
        // 根据检测到的问题扣分
        score -= result.DetectedIssues.Count() * 10;
        
        return Math.Max(0, score);
    }

    #endregion

    // XSS防护
    #region XSS防护

    /// <inheritdoc/>
    public async Task<string> SanitizeHtmlAsync(string html, IEnumerable<string>? allowedTags = null)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        try
        {
            // 默认允许的安全标签
            var safeTags = allowedTags ?? new[] { "p", "br", "strong", "em", "u", "ol", "ul", "li", "blockquote", "code", "pre" };
            
            // 移除危险的标签和属性
            var sanitized = html;
            
            // 移除脚本标签
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // 移除事件处理器
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            
            // 移除危险的样式
            sanitized = Regex.Replace(sanitized, @"style\s*=\s*[""'][^""']*expression\([^""']*[""']", "", RegexOptions.IgnoreCase);
            
            // 移除危险的协议
            sanitized = Regex.Replace(sanitized, @"(javascript|vbscript|data):[^""'\s]*", "", RegexOptions.IgnoreCase);
            
            // 只保留允许的标签
            var allowedTagsPattern = string.Join("|", safeTags);
            sanitized = Regex.Replace(sanitized, $@"<(?!\/?({allowedTagsPattern})\s*\/?>)[^>]+>", "", RegexOptions.IgnoreCase);
            
            // HTML解码
            sanitized = System.Net.WebUtility.HtmlDecode(sanitized);
            
            return sanitized.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTML清理失败: {Html}", html);
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<string> SanitizeUserInputAsync(string input, string inputType)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        try
        {
            var sanitized = input;
            
            // 根据输入类型进行不同的清理
            switch (inputType.ToLower())
            {
                case "html":
                case "richtext":
                    sanitized = await SanitizeHtmlAsync(sanitized);
                    break;
                case "url":
                    sanitized = await SanitizeUrlAsync(sanitized);
                    break;
                case "text":
                default:
                    sanitized = await SanitizeTextAsync(sanitized);
                    break;
            }
            
            return sanitized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户输入清理失败: {Input}", input);
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<string> EncodeHtmlAsync(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        return System.Net.WebUtility.HtmlEncode(content);
    }

    /// <inheritdoc/>
    public async Task<XssDetectionResult> DetectXssAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new XssDetectionResult { ContainsXss = false, RiskLevel = XssRiskLevel.None };
        }

        var detectedPatterns = new List<string>();
        var riskLevel = XssRiskLevel.None;

        foreach (var pattern in XssPatterns)
        {
            var matches = pattern.Matches(input);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    detectedPatterns.Add(match.Value);
                }
            }
        }

        // 检查危险关键词
        foreach (var keyword in DangerousKeywords)
        {
            if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                detectedPatterns.Add(keyword);
            }
        }

        // 评估风险等级
        if (detectedPatterns.Count > 0)
        {
            if (detectedPatterns.Any(p => p.Contains("script", StringComparison.OrdinalIgnoreCase)))
            {
                riskLevel = XssRiskLevel.Critical;
            }
            else if (detectedPatterns.Any(p => p.Contains("javascript:", StringComparison.OrdinalIgnoreCase)))
            {
                riskLevel = XssRiskLevel.High;
            }
            else if (detectedPatterns.Count > 3)
            {
                riskLevel = XssRiskLevel.Medium;
            }
            else
            {
                riskLevel = XssRiskLevel.Low;
            }
        }

        return new XssDetectionResult
        {
            ContainsXss = detectedPatterns.Count > 0,
            DetectedPatterns = detectedPatterns,
            RiskLevel = riskLevel
        };
    }

    /// <inheritdoc/>
    public async Task<UrlSecurityResult> ValidateUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new UrlSecurityResult { IsValid = false, ErrorMessage = "URL不能为空" };
        }

        try
        {
            var uri = new Uri(url);
            var result = new UrlSecurityResult
            {
                IsValid = true,
                ParsedUri = uri
            };

            // 检查协议
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                result.IsValid = false;
                result.ErrorMessage = "仅支持HTTP和HTTPS协议";
                result.HasDangerousProtocol = true;
                return result;
            }

            // 检查是否为内网URL
            if (IsInternalUrl(uri))
            {
                result.IsInternalUrl = true;
            }

            // 检查是否包含凭证
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                result.ContainsCredentials = true;
            }

            // 检查端口
            if (uri.Port != 80 && uri.Port != 443 && uri.Port > 1024)
            {
                _logger.LogWarning("URL使用非标准端口: {Url}", url);
            }

            return result;
        }
        catch (UriFormatException ex)
        {
            _logger.LogWarning(ex, "URL格式无效: {Url}", url);
            return new UrlSecurityResult { IsValid = false, ErrorMessage = "URL格式无效" };
        }
    }

    /// <inheritdoc/>
    public async Task<string> SanitizeMarkdownAsync(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        try
        {
            var sanitized = markdown;
            
            // 移除危险的HTML标签
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // 清理链接
            var linkRegex = new Regex(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled);
            var matches = linkRegex.Matches(sanitized);
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 2)
                {
                    var url = match.Groups[2].Value;
                    var urlResult = await ValidateUrlAsync(url);
                    if (!urlResult.IsValid)
                    {
                        // 移除危险的链接
                        sanitized = sanitized.Replace(match.Value, $"[{match.Groups[1].Value}](#)");
                    }
                }
            }
            
            return sanitized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Markdown清理失败: {Markdown}", markdown);
            return string.Empty;
        }
    }

    #endregion

    // SQL注入防护
    #region SQL注入防护

    /// <inheritdoc/>
    public async Task<SqlInjectionResult> DetectSqlInjectionAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new SqlInjectionResult { ContainsSqlInjection = false, RiskLevel = InjectionRiskLevel.None };
        }

        var detectedPatterns = new List<string>();
        var riskLevel = InjectionRiskLevel.None;

        foreach (var pattern in SqlInjectionPatterns)
        {
            var matches = pattern.Matches(input);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    detectedPatterns.Add(match.Value);
                }
            }
        }

        // 评估风险等级
        if (detectedPatterns.Count > 0)
        {
            if (detectedPatterns.Any(p => p.Contains("DROP", StringComparison.OrdinalIgnoreCase) ||
                                         p.Contains("DELETE", StringComparison.OrdinalIgnoreCase) ||
                                         p.Contains("EXEC", StringComparison.OrdinalIgnoreCase)))
            {
                riskLevel = InjectionRiskLevel.Critical;
            }
            else if (detectedPatterns.Any(p => p.Contains("UNION", StringComparison.OrdinalIgnoreCase)))
            {
                riskLevel = InjectionRiskLevel.High;
            }
            else if (detectedPatterns.Count > 2)
            {
                riskLevel = InjectionRiskLevel.Medium;
            }
            else
            {
                riskLevel = InjectionRiskLevel.Low;
            }
        }

        return new SqlInjectionResult
        {
            ContainsSqlInjection = detectedPatterns.Count > 0,
            DetectedPatterns = detectedPatterns,
            RiskLevel = riskLevel
        };
    }

    /// <inheritdoc/>
    public async Task<string> SanitizeSqlParameterAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        try
        {
            var sanitized = input;
            
            // 移除SQL关键字
            foreach (var keyword in new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "EXEC", "ALTER" })
            {
                var pattern = $@"\b{keyword}\b";
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }
            
            // 移除危险的字符组合
            sanitized = sanitized.Replace("--", "");
            sanitized = sanitized.Replace("/*", "");
            sanitized = sanitized.Replace("*/", "");
            sanitized = sanitized.Replace(";", "");
            
            return sanitized.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL参数清理失败: {Input}", input);
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<SqlParameterValidationResult> ValidateSqlParametersAsync(Dictionary<string, object> parameters)
    {
        var parameterResults = new List<ParameterValidationResult>();
        var invalidCount = 0;

        foreach (var parameter in parameters)
        {
            var result = new ParameterValidationResult
            {
                ParameterName = parameter.Key,
                ParameterValue = parameter.Value,
                IsValid = true
            };

            if (parameter.Value is string stringValue)
            {
                var sqlResult = await DetectSqlInjectionAsync(stringValue);
                if (sqlResult.ContainsSqlInjection)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "检测到SQL注入风险";
                    invalidCount++;
                }
            }

            parameterResults.Add(result);
        }

        return new SqlParameterValidationResult
        {
            AllParametersValid = invalidCount == 0,
            ParameterResults = parameterResults,
            InvalidParametersCount = invalidCount
        };
    }

    /// <inheritdoc/>
    public async Task<CommandInjectionResult> DetectCommandInjectionAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new CommandInjectionResult { ContainsCommandInjection = false, RiskLevel = InjectionRiskLevel.None };
        }

        var detectedCommands = new List<string>();
        var riskLevel = InjectionRiskLevel.None;

        foreach (var pattern in CommandInjectionPatterns)
        {
            var matches = pattern.Matches(input);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    detectedCommands.Add(match.Value);
                }
            }
        }

        // 评估风险等级
        if (detectedCommands.Count > 0)
        {
            if (detectedCommands.Any(c => c.Contains("rm", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("del", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("format", StringComparison.OrdinalIgnoreCase)))
            {
                riskLevel = InjectionRiskLevel.Critical;
            }
            else if (detectedCommands.Count > 2)
            {
                riskLevel = InjectionRiskLevel.Medium;
            }
            else
            {
                riskLevel = InjectionRiskLevel.Low;
            }
        }

        return new CommandInjectionResult
        {
            ContainsCommandInjection = detectedCommands.Count > 0,
            DetectedCommands = detectedCommands,
            RiskLevel = riskLevel
        };
    }

    #endregion

    // 内容分析和报告
    #region 内容分析和报告

    /// <inheritdoc/>
    public async Task<ContentRiskAnalysisResult> AnalyzeContentRiskAsync(string content)
    {
        var validation = await ValidateContentAsync(content, "general");
        var sensitiveResult = await CheckSensitiveWordsAsync(content);
        var xssResult = await DetectXssAsync(content);
        var sqlResult = await DetectSqlInjectionAsync(content);

        var riskFactors = new List<string>();
        var recommendations = new List<string>();

        // 分析风险因素
        if (!validation.IsValid)
        {
            riskFactors.Add("内容验证失败");
        }

        if (xssResult.ContainsXss)
        {
            riskFactors.Add("XSS攻击风险");
            recommendations.Add("避免使用JavaScript代码和HTML标签");
        }

        if (sqlResult.ContainsSqlInjection)
        {
            riskFactors.Add("SQL注入风险");
            recommendations.Add("避免使用SQL关键字和特殊字符");
        }

        if (sensitiveResult.ContainsSensitiveWords)
        {
            riskFactors.Add("包含敏感词");
            recommendations.Add("避免使用敏感词汇");
        }

        if (content.Length > 1000)
        {
            riskFactors.Add("内容过长");
            recommendations.Add("精简内容长度");
        }

        // 计算风险评分
        double riskScore = 0;
        if (xssResult.ContainsXss) riskScore += 0.4;
        if (sqlResult.ContainsSqlInjection) riskScore += 0.4;
        if (sensitiveResult.ContainsSensitiveWords) riskScore += 0.2;
        if (content.Length > 1000) riskScore += 0.1;

        var riskLevel = riskScore switch
        {
            >= 0.8 => RiskLevel.Critical,
            >= 0.6 => RiskLevel.High,
            >= 0.4 => RiskLevel.Medium,
            >= 0.2 => RiskLevel.Low,
            _ => RiskLevel.None
        };

        return new ContentRiskAnalysisResult
        {
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            RiskFactors = riskFactors,
            Recommendations = recommendations,
            ContentCategory = ContentCategory.Text,
            ConfidenceScore = 0.95
        };
    }

    /// <inheritdoc/>
    public async Task<ContentSecurityReport> GenerateSecurityReportAsync(string content, IEnumerable<ContentSecurityResult> analysisResults)
    {
        var report = new ContentSecurityReport
        {
            Content = content,
            GeneratedAt = DateTime.UtcNow
        };

        var securityIssues = new List<SecurityIssue>();
        var recommendations = new List<SecurityRecommendation>();

        foreach (var result in analysisResults)
        {
            if (!result.IsValid)
            {
                securityIssues.Add(new SecurityIssue
                {
                    IssueType = "ValidationFailed",
                    Description = result.ErrorMessage ?? "内容验证失败",
                    Severity = result.SecurityLevel,
                    Recommendations = result.DetectedIssues.ToList()
                });
            }

            if (result.DetectedIssues.Any())
            {
                foreach (var issue in result.DetectedIssues)
                {
                    securityIssues.Add(new SecurityIssue
                    {
                        IssueType = "SecurityIssue",
                        Description = issue,
                        Severity = result.SecurityLevel
                    });
                }
            }
        }

        // 生成建议
        if (securityIssues.Any())
        {
            recommendations.Add(new SecurityRecommendation
            {
                Title = "内容安全建议",
                Description = "建议对内容进行安全处理后再提交",
                Priority = RecommendationPriority.Medium,
                Action = "Review and sanitize content"
            });
        }

        report.SecurityIssues = securityIssues;
        report.Recommendations = recommendations;
        report.OverallStatus = securityIssues.Any() ? OverallSecurityStatus.MediumRisk : OverallSecurityStatus.Safe;

        // 计算指标
        report.Metrics = new SecurityMetrics
        {
            TotalChecks = analysisResults.Count(),
            IssuesFound = securityIssues.Count,
            AverageRiskScore = securityIssues.Any() ? securityIssues.Average(i => (int)i.Severity) : 0,
            HighRiskIssues = securityIssues.Count(i => i.Severity == SecurityLevel.High || i.Severity == SecurityLevel.Critical),
            MediumRiskIssues = securityIssues.Count(i => i.Severity == SecurityLevel.Medium),
            LowRiskIssues = securityIssues.Count(i => i.Severity == SecurityLevel.Low)
        };

        return report;
    }

    /// <inheritdoc/>
    public async Task<ContentSecurityStats> GetSecurityStatsAsync(DateTime startDate, DateTime endDate)
    {
        // 这里应该从数据库或缓存中获取统计数据
        // 暂时返回模拟数据
        return new ContentSecurityStats
        {
            TotalContentChecks = 1000,
            ValidContentCount = 850,
            InvalidContentCount = 150,
            SecurityEventsCount = 25,
            AverageRiskScore = 0.3,
            EventCounts = new List<SecurityEventCount>
            {
                new SecurityEventCount { EventType = "XSS", Count = 10, Severity = SecurityLevel.High },
                new SecurityEventCount { EventType = "SQLInjection", Count = 5, Severity = SecurityLevel.Critical },
                new SecurityEventCount { EventType = "SensitiveWords", Count = 10, Severity = SecurityLevel.Medium }
            },
            DailyStats = Enumerable.Range(0, 7)
                .Select(i => new DailySecurityStats
                {
                    Date = DateTime.UtcNow.AddDays(-i),
                    TotalChecks = 100 + i * 10,
                    IssuesFound = 10 + i,
                    AverageRiskScore = 0.2 + i * 0.05,
                    SecurityEvents = 2 + i / 2
                })
                .Reverse()
                .ToList()
        };
    }

    /// <inheritdoc/>
    public async Task<bool> LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        try
        {
            _logger.LogWarning("安全事件: {EventType} - {Description}", 
                securityEvent.EventType, securityEvent.EventDescription);
            
            // 这里应该将事件保存到数据库
            // 暂时只记录日志
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录安全事件失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<SecurityEvent>> GetSecurityEventsAsync(SecurityEventFilter filter)
    {
        // 这里应该从数据库中获取安全事件
        // 暂时返回空结果
        return new PaginatedResult<SecurityEvent>
        {
            Items = new List<SecurityEvent>(),
            TotalCount = 0,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    #endregion

    // 缓存和性能优化
    #region 缓存和性能优化

    /// <inheritdoc/>
    public async Task<bool> ClearSecurityCacheAsync(string contentHash)
    {
        try
        {
            var cacheKey = $"content_security_{contentHash}";
            _cache.Remove(cacheKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理安全缓存失败: {ContentHash}", contentHash);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> WarmupSecurityCacheAsync(IEnumerable<string> contents)
    {
        try
        {
            foreach (var content in contents)
            {
                var cacheKey = $"content_security_{GenerateContentHash(content)}";
                if (!_cache.TryGetValue(cacheKey, out _))
                {
                    var result = await ValidateContentAsync(content, "general");
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };
                    _cache.Set(cacheKey, result, cacheOptions);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预热安全缓存失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<SecurityCacheStats> GetCacheStatsAsync()
    {
        // 这里应该返回真实的缓存统计信息
        // 暂时返回模拟数据
        return new SecurityCacheStats
        {
            TotalCacheEntries = 100,
            CacheHits = 80,
            CacheMisses = 20,
            HitRate = 0.8,
            MemoryUsage = 1024 * 1024, // 1MB
            LastCleanup = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    #endregion

    // 配置管理
    #region 配置管理

    /// <inheritdoc/>
    public async Task<SecurityPolicyConfig> GetSecurityPolicyAsync()
    {
        // 这里应该从配置或数据库中获取安全策略
        // 暂时返回默认配置
        return new SecurityPolicyConfig
        {
            EnableXssProtection = true,
            EnableSqlInjectionProtection = true,
            EnableSensitiveWordFilter = true,
            EnableContentAnalysis = true,
            MinimumSecurityLevel = SecurityLevel.Medium,
            AllowedHtmlTags = new[] { "p", "br", "strong", "em", "u", "ol", "ul", "li", "blockquote", "code", "pre" },
            AllowedUrlProtocols = new[] { "http:", "https:" },
            MaxContentLength = 50000,
            MaxBatchSize = 100,
            EnableCaching = true,
            CacheExpirationMinutes = 30,
            EnableLogging = true,
            EnableNotifications = true
        };
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateSecurityPolicyAsync(SecurityPolicyConfig config)
    {
        try
        {
            // 这里应该将配置保存到数据库
            _logger.LogInformation("安全策略配置已更新");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新安全策略配置失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ResetSecurityPolicyAsync()
    {
        try
        {
            var defaultConfig = new SecurityPolicyConfig();
            return await UpdateSecurityPolicyAsync(defaultConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置安全策略配置失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetSensitiveWordsAsync()
    {
        // 这里应该从数据库中获取敏感词列表
        // 暂时返回默认敏感词
        return DefaultSensitiveWords;
    }

    /// <inheritdoc/>
    public async Task<bool> AddSensitiveWordAsync(string word, string category)
    {
        try
        {
            // 这里应该将敏感词保存到数据库
            _logger.LogInformation("添加敏感词: {Word}, 分类: {Category}", word, category);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加敏感词失败: {Word}", word);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveSensitiveWordAsync(string word)
    {
        try
        {
            // 这里应该从数据库中删除敏感词
            _logger.LogInformation("移除敏感词: {Word}", word);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除敏感词失败: {Word}", word);
            return false;
        }
    }

    #endregion

    // 私有辅助方法
    #region 私有辅助方法

    private string GenerateContentHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<ContentSecurityResult> ValidateCommentContent(string content, ContentSecurityResult result)
    {
        // 评论特定验证
        if (content.Length > 1000)
        {
            result.IsValid = false;
            result.ErrorMessage = "评论长度超过限制";
            result.SecurityLevel = SecurityLevel.Low;
            return result;
        }

        // 检查是否包含过多链接
        var urlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled);
        var urls = urlRegex.Matches(content);
        if (urls.Count > 3)
        {
            result.IsValid = false;
            result.ErrorMessage = "评论中包含过多链接";
            result.SecurityLevel = SecurityLevel.Medium;
            return result;
        }

        return result;
    }

    private async Task<ContentSecurityResult> ValidateMessageContent(string content, ContentSecurityResult result)
    {
        // 消息特定验证
        if (content.Length > 2000)
        {
            result.IsValid = false;
            result.ErrorMessage = "消息长度超过限制";
            result.SecurityLevel = SecurityLevel.Low;
            return result;
        }

        return result;
    }

    private async Task<ContentSecurityResult> ValidateNotificationContent(string content, ContentSecurityResult result)
    {
        // 通知特定验证
        if (content.Length > 500)
        {
            result.IsValid = false;
            result.ErrorMessage = "通知内容长度超过限制";
            result.SecurityLevel = SecurityLevel.Low;
            return result;
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidateCodeContent(string code, string language, CodeSecurityResult result)
    {
        // 代码通用验证
        if (string.IsNullOrWhiteSpace(code))
        {
            result.IsValid = false;
            result.ErrorMessage = "代码内容不能为空";
            result.SecurityLevel = SecurityLevel.High;
            return result;
        }

        // 检查代码长度
        if (code.Length > 100000)
        {
            result.IsValid = false;
            result.ErrorMessage = "代码长度超过限制";
            result.SecurityLevel = SecurityLevel.Medium;
            return result;
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidateSqlCode(string code, CodeSecurityResult result)
    {
        result.ContainsDatabaseOperations = true;

        // 检查危险的SQL操作
        var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "GRANT", "REVOKE" };
        foreach (var keyword in dangerousKeywords)
        {
            if (code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险操作: {keyword}");
                result.SecurityLevel = SecurityLevel.High;
            }
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidateJavaScriptCode(string code, CodeSecurityResult result)
    {
        // 检查危险的JavaScript操作
        var dangerousPatterns = new[] { "eval(", "document.write", "innerHTML", "outerHTML", "setTimeout(", "setInterval(" };
        foreach (var pattern in dangerousPatterns)
        {
            if (code.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
            }
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidatePhpCode(string code, CodeSecurityResult result)
    {
        // 检查危险的PHP操作
        var dangerousPatterns = new[] { "eval(", "exec(", "system(", "shell_exec(", "passthru(", "proc_open(" };
        foreach (var pattern in dangerousPatterns)
        {
            if (code.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
                result.SecurityLevel = SecurityLevel.High;
            }
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidatePythonCode(string code, CodeSecurityResult result)
    {
        // 检查危险的Python操作
        var dangerousPatterns = new[] { "eval(", "exec(", "subprocess", "os.system", "commands.getstatusoutput" };
        foreach (var pattern in dangerousPatterns)
        {
            if (code.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
            }
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidateCSharpCode(string code, CodeSecurityResult result)
    {
        // 检查危险的C#操作
        var dangerousPatterns = new[] { "Process.Start", "Reflection", "DllImport", "unsafe" };
        foreach (var pattern in dangerousPatterns)
        {
            if (code.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
            }
        }

        return result;
    }

    private async Task<CodeSecurityResult> ValidateGenericCode(string code, CodeSecurityResult result)
    {
        // 通用代码验证
        var dangerousPatterns = new[] { "password", "secret", "key", "token", "credential" };
        foreach (var pattern in dangerousPatterns)
        {
            if (code.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"可能包含敏感信息: {pattern}");
            }
        }

        return result;
    }

    private async Task<SecurityLevel> CalculateSecurityLevel(ContentSecurityResult result)
    {
        if (!result.IsValid)
        {
            return SecurityLevel.High;
        }

        var score = 0;
        if (result.DetectedIssues.Any()) score += 20;
        
        // 可以根据更多因素计算安全级别
        return score switch
        {
            >= 40 => SecurityLevel.Critical,
            >= 30 => SecurityLevel.High,
            >= 20 => SecurityLevel.Medium,
            >= 10 => SecurityLevel.Low,
            _ => SecurityLevel.Low
        };
    }

    private string GetWordCategory(string word)
    {
        if (word.Contains("密码") || word.Contains("口令") || word.Contains("token") || word.Contains("key"))
            return "安全凭证";
        if (word.Contains("银行") || word.Contains("信用卡") || word.Contains("身份证") || word.Contains("护照"))
            return "个人信息";
        if (word.Contains("管理员") || word.Contains("权限"))
            return "系统管理";
        return "其他";
    }

    private bool IsHighlySensitiveWord(string word)
    {
        var highlySensitiveWords = new[] { "密码", "口令", "token", "secret", "key", "api_key", "private_key" };
        return highlySensitiveWords.Contains(word.ToLower());
    }

    private bool IsInternalUrl(Uri uri)
    {
        return uri.HostNameType == UriHostNameType.IPv4 || 
               uri.HostNameType == UriHostNameType.IPv6 ||
               uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> SanitizeUrlAsync(string url)
    {
        var result = await ValidateUrlAsync(url);
        return result.IsValid ? url : "#";
    }

    private async Task<string> SanitizeTextAsync(string text)
    {
        var sanitized = text;
        
        // 移除控制字符
        sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        
        // 标准化空白字符
        sanitized = Regex.Replace(sanitized, @"\s+", " ");
        
        // 移除前后空白
        sanitized = sanitized.Trim();
        
        return sanitized;
    }

    #endregion
}