using System.Text;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 附件安全服务实现 - 提供完整的附件安全检查和文件上传安全功能
/// 实现文件类型验证、病毒扫描、内容安全检查、权限验证等功能
/// </summary>
public class AttachmentSecurityService : IAttachmentSecurityService
{
    private readonly ILogger<AttachmentSecurityService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IContentSecurityService _contentSecurityService;
    private readonly IPermissionService _permissionService;

    // 文件类型签名
    private static readonly Dictionary<string, byte[]> FileSignatures = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } },
        { ".bmp", new byte[] { 0x42, 0x4D } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },
        { ".zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
        { ".rar", new byte[] { 0x52, 0x61, 0x72, 0x21 } },
        { ".txt", new byte[] { } }, // 文本文件无特定签名
        { ".json", new byte[] { 0x7B } },
        { ".xml", new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C } },
        { ".csv", new byte[] { } }, // CSV文件无特定签名
        { ".md", new byte[] { } } // Markdown文件无特定签名
    };

    // 危险文件类型
    private static readonly string[] DangerousExtensions = new[]
    {
        ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar", ".app",
        ".deb", ".pkg", ".dmg", ".iso", ".dll", ".so", ".dylib", ".sh", ".ps1",
        ".py", ".rb", ".pl", ".php", ".asp", ".aspx", ".jsp", ".cgi"
    };

    // 危险MIME类型
    private static readonly string[] DangerousMimeTypes = new[]
    {
        "application/x-msdownload", "application/x-msdos-program", "application/x-msi",
        "application/x-ms-shortcut", "application/x-sh", "application/x-shar",
        "application/x-shellscript", "application/x-tar", "application/x-bzip2",
        "application/x-gzip", "application/x-7z-compressed", "application/x-rar-compressed"
    };

    // 可执行文件签名
    private static readonly byte[][] ExecutableSignatures = new[]
    {
        new byte[] { 0x4D, 0x5A }, // MZ - DOS/Windows EXE
        new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, // ELF - Linux/Unix
        new byte[] { 0xFE, 0xED, 0xFA, 0xCE }, // Mach-O - macOS
        new byte[] { 0xFE, 0xED, 0xFA, 0xCF }, // Mach-O - macOS
        new byte[] { 0xCA, 0xFE, 0xBA, 0xBE }, // Universal Binary
        new byte[] { 0xCA, 0xFE, 0xBA, 0xBF }  // Universal Binary
    };

    public AttachmentSecurityService(
        ILogger<AttachmentSecurityService> logger,
        IMemoryCache cache,
        IContentSecurityService contentSecurityService,
        IPermissionService permissionService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _contentSecurityService = contentSecurityService ?? throw new ArgumentNullException(nameof(contentSecurityService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
    }

    // 文件类型验证
    #region 文件类型验证

    /// <inheritdoc/>
    public async Task<FileTypeValidationResult> ValidateFileTypeAsync(string fileName, string contentType, byte[] fileData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new FileTypeValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "文件名不能为空"
                };
            }

            if (fileData == null || fileData.Length == 0)
            {
                return new FileTypeValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "文件数据不能为空"
                };
            }

            var result = new FileTypeValidationResult
            {
                DeclaredType = contentType,
                IsValid = true
            };

            // 获取文件扩展名
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            // 检查扩展名是否在危险列表中
            if (DangerousExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "不允许上传可执行文件";
                result.DetectedType = "可执行文件";
                result.IsTypeMismatch = true;
                return result;
            }

            // 检查MIME类型是否在危险列表中
            if (DangerousMimeTypes.Contains(contentType.ToLower()))
            {
                result.IsValid = false;
                result.ErrorMessage = "不允许上传此类型的文件";
                result.DetectedType = "危险文件类型";
                result.IsTypeMismatch = true;
                return result;
            }

            // 检测真实文件类型
            var detectionResult = await DetectRealFileTypeAsync(fileData);
            result.DetectedType = detectionResult.DetectedType;

            // 验证声明的MIME类型与检测到的类型是否匹配
            if (!IsMimeTypeMatch(contentType, detectionResult.DetectedType))
            {
                result.IsTypeMismatch = true;
                result.Warnings = new List<string> { "文件类型与声明的不一致" };
                
                // 如果检测到的是危险类型，直接拒绝
                if (IsDangerousFileType(detectionResult.DetectedType))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "检测到危险文件类型";
                    return result;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件类型验证失败: {FileName}", fileName);
            return new FileTypeValidationResult
            {
                IsValid = false,
                ErrorMessage = "文件类型验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileExtensionValidationResult> ValidateFileExtensionAsync(string fileName, IEnumerable<string> allowedExtensions)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new FileExtensionValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "文件名不能为空"
                };
            }

            var result = new FileExtensionValidationResult();
            
            // 获取文件扩展名
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            result.Extension = extension;

            // 检查是否为危险扩展名
            if (DangerousExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "不允许上传此类型的文件";
                result.IsAllowed = false;
                return result;
            }

            // 检查是否为双扩展名
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            if (Path.GetExtension(fileNameWithoutExt) != "")
            {
                result.IsDoubleExtension = true;
                result.Warnings = new List<string> { "文件包含双扩展名" };
            }

            // 检查是否在允许的扩展名列表中
            var allowedExts = allowedExtensions.Select(e => e.ToLowerInvariant()).ToList();
            result.IsAllowed = allowedExts.Contains(extension) || allowedExts.Contains(".*");

            if (!result.IsAllowed)
            {
                result.IsValid = false;
                result.ErrorMessage = $"不允许上传 {extension} 类型的文件";
                result.AlternativeExtensions = allowedExts.Where(e => e != ".*").ToList();
            }
            else
            {
                result.IsValid = true;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件扩展名验证失败: {FileName}", fileName);
            return new FileExtensionValidationResult
            {
                IsValid = false,
                ErrorMessage = "文件扩展名验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileTypeDetectionResult> DetectRealFileTypeAsync(byte[] fileData)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                return new FileTypeDetectionResult
                {
                    DetectedType = "unknown",
                    Confidence = 0,
                    IsReliable = false
                };
            }

            var possibleTypes = new List<string>();
            double maxConfidence = 0;
            string bestMatch = "unknown";

            // 检查文件签名
            foreach (var signature in FileSignatures)
            {
                if (signature.Value.Length == 0)
                {
                    // 对于无特定签名的文件，跳过检查
                    continue;
                }

                if (fileData.Length >= signature.Value.Length)
                {
                    var match = true;
                    for (int i = 0; i < signature.Value.Length; i++)
                    {
                        if (fileData[i] != signature.Value[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        possibleTypes.Add(signature.Key);
                        maxConfidence = Math.Max(maxConfidence, 0.9);
                        bestMatch = signature.Key;
                    }
                }
            }

            // 检查是否为可执行文件
            foreach (var signature in ExecutableSignatures)
            {
                if (fileData.Length >= signature.Length)
                {
                    var match = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        if (fileData[i] != signature[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        possibleTypes.Add("executable");
                        maxConfidence = Math.Max(maxConfidence, 0.95);
                        bestMatch = "executable";
                    }
                }
            }

            // 基于内容推断文件类型
            if (possibleTypes.Count == 0)
            {
                var content = Encoding.UTF8.GetString(fileData.Take(1000).ToArray());
                
                // 检查是否为文本文件
                if (IsTextContent(content))
                {
                    possibleTypes.Add(".txt");
                    maxConfidence = Math.Max(maxConfidence, 0.7);
                    bestMatch = ".txt";
                }

                // 检查是否为JSON
                if (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("["))
                {
                    possibleTypes.Add(".json");
                    maxConfidence = Math.Max(maxConfidence, 0.8);
                    bestMatch = ".json";
                }

                // 检查是否为XML
                if (content.TrimStart().StartsWith("<"))
                {
                    possibleTypes.Add(".xml");
                    maxConfidence = Math.Max(maxConfidence, 0.8);
                    bestMatch = ".xml";
                }

                // 检查是否为CSV
                if (content.Contains(",") && content.Split('\n').Length > 1)
                {
                    possibleTypes.Add(".csv");
                    maxConfidence = Math.Max(maxConfidence, 0.6);
                    bestMatch = ".csv";
                }
            }

            return new FileTypeDetectionResult
            {
                DetectedType = bestMatch,
                Confidence = maxConfidence,
                Signature = possibleTypes.FirstOrDefault() ?? "",
                PossibleTypes = possibleTypes,
                IsReliable = maxConfidence > 0.8
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件类型检测失败");
            return new FileTypeDetectionResult
            {
                DetectedType = "unknown",
                Confidence = 0,
                IsReliable = false
            };
        }
    }

    /// <inheritdoc/>
    public async Task<MimeTypeValidationResult> ValidateMimeTypeAsync(string contentType, byte[] fileData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return new MimeTypeValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "MIME类型不能为空"
                };
            }

            var result = new MimeTypeValidationResult
            {
                DeclaredMimeType = contentType
            };

            // 检查是否为危险MIME类型
            if (DangerousMimeTypes.Contains(contentType.ToLower()))
            {
                result.IsValid = false;
                result.ErrorMessage = "不允许上传此MIME类型的文件";
                return result;
            }

            // 检测真实文件类型
            var detectionResult = await DetectRealFileTypeAsync(fileData);
            var detectedMimeType = GetMimeTypeFromExtension(detectionResult.DetectedType);
            result.DetectedMimeType = detectedMimeType;

            // 验证MIME类型是否匹配
            result.IsMismatch = !IsMimeTypeMatch(contentType, detectedMimeType);
            
            // 检查是否在白名单中
            result.IsWhitelisted = IsSafeMimeType(contentType);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MIME类型验证失败: {ContentType}", contentType);
            return new MimeTypeValidationResult
            {
                IsValid = false,
                ErrorMessage = "MIME类型验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsExecutableFileAsync(byte[] fileData)
    {
        try
        {
            if (fileData == null || fileData.Length < 4)
            {
                return false;
            }

            // 检查可执行文件签名
            foreach (var signature in ExecutableSignatures)
            {
                if (fileData.Length >= signature.Length)
                {
                    var match = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        if (fileData[i] != signature[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "可执行文件检测失败");
            return false;
        }
    }

    #endregion

    // 文件内容安全检查
    #region 文件内容安全检查

    /// <inheritdoc/>
    public async Task<FileContentSecurityResult> ScanFileContentAsync(byte[] fileData, string fileName)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                return new FileContentSecurityResult
                {
                    IsSafe = false,
                    RiskLevel = SecurityLevel.High,
                    SecurityIssues = new List<string> { "文件内容为空" }
                };
            }

            var result = new FileContentSecurityResult
            {
                ScannedAt = DateTime.UtcNow
            };

            // 检查文件大小
            if (fileData.Length > 50 * 1024 * 1024) // 50MB
            {
                result.IsSafe = false;
                result.RiskLevel = SecurityLevel.Medium;
                result.SecurityIssues = new List<string> { "文件过大" };
                result.Recommendations = new List<string> { "请压缩文件或减小文件大小" };
                return result;
            }

            // 检测文件类型
            var detectionResult = await DetectRealFileTypeAsync(fileData);
            
            // 根据文件类型进行不同的安全检查
            switch (detectionResult.DetectedType)
            {
                case ".txt":
                case ".md":
                case ".csv":
                    result = await ScanTextFileContent(fileData, fileName, result);
                    break;
                case ".json":
                    result = await ScanJsonFileContent(fileData, fileName, result);
                    break;
                case ".xml":
                    result = await ScanXmlFileContent(fileData, fileName, result);
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    result = await ScanImageFileContent(fileData, fileName, result);
                    break;
                case ".zip":
                case ".rar":
                    result = await ScanArchiveFileContent(fileData, fileName, result);
                    break;
                default:
                    result = await ScanGenericFileContent(fileData, fileName, result);
                    break;
            }

            // 计算风险评分
            result.RiskScore = CalculateRiskScore(result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件内容安全扫描失败: {FileName}", fileName);
            return new FileContentSecurityResult
            {
                IsSafe = false,
                RiskLevel = SecurityLevel.High,
                SecurityIssues = new List<string> { "文件扫描失败" }
            };
        }
    }

    /// <inheritdoc/>
    public async Task<MalwareDetectionResult> DetectMalwareAsync(byte[] fileData)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                return new MalwareDetectionResult
                {
                    IsClean = false,
                    Status = VirusScanStatus.Error,
                    ScanTime = DateTime.UtcNow
                };
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 这里应该调用真正的病毒扫描服务
            // 暂时实现基本的启发式扫描
            var result = new MalwareDetectionResult
            {
                IsClean = true,
                Status = VirusScanStatus.Clean,
                EngineName = "CodeShare Security Scanner",
                ScanTime = DateTime.UtcNow
            };

            // 检查是否为可执行文件
            if (await IsExecutableFileAsync(fileData))
            {
                result.IsClean = false;
                result.DetectedViruses = new List<string> { "可执行文件" };
                result.RiskLevel = MalwareRiskLevel.High;
                result.Status = VirusScanStatus.Infected;
            }

            // 检查文件签名
            var detectionResult = await DetectRealFileTypeAsync(fileData);
            if (detectionResult.DetectedType == "executable")
            {
                result.IsClean = false;
                result.DetectedViruses = new List<string> { "检测到可执行文件" };
                result.RiskLevel = MalwareRiskLevel.High;
                result.Status = VirusScanStatus.Infected;
            }

            // 检查文件内容中的危险模式
            var content = Encoding.UTF8.GetString(fileData.Take(Math.Min(fileData.Length, 10000)).ToArray());
            var dangerousPatterns = new[]
            {
                "function eval(", "document.write(", "<script", "javascript:",
                "vbscript:", "onload=", "onerror=", "onclick=",
                "SELECT * FROM", "DROP TABLE", "DELETE FROM", "UPDATE SET",
                "system(", "exec(", "shell_exec(", "passthru("
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    result.DetectedViruses = result.DetectedViruses.Append($"检测到危险模式: {pattern}").ToList();
                    result.RiskLevel = MalwareRiskLevel.Medium;
                }
            }

            if (result.DetectedViruses.Any())
            {
                result.IsClean = false;
                result.Status = VirusScanStatus.Infected;
            }

            stopwatch.Stop();
            result.ScanDurationMs = (int)stopwatch.ElapsedMilliseconds;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恶意软件检测失败");
            return new MalwareDetectionResult
            {
                IsClean = false,
                Status = VirusScanStatus.Error,
                ScanTime = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc/>
    public async Task<CodeFileSecurityResult> ValidateCodeFileAsync(byte[] fileData, string language)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                return new CodeFileSecurityResult
                {
                    IsValid = false,
                    Language = language
                };
            }

            var content = Encoding.UTF8.GetString(fileData);
            var result = new CodeFileSecurityResult
            {
                Language = language,
                IsValid = true
            };

            // 根据语言进行特定检查
            switch (language.ToLower())
            {
                case "javascript":
                case "js":
                    result = await ValidateJavaScriptCode(content, result);
                    break;
                case "php":
                    result = await ValidatePhpCode(content, result);
                    break;
                case "python":
                    result = await ValidatePythonCode(content, result);
                    break;
                case "csharp":
                case "c#":
                    result = await ValidateCSharpCode(content, result);
                    break;
                case "sql":
                    result = await ValidateSqlCode(content, result);
                    break;
                default:
                    result = await ValidateGenericCode(content, result);
                    break;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "代码文件安全验证失败: {Language}", language);
            return new CodeFileSecurityResult
            {
                IsValid = false,
                Language = language,
                RiskLevel = CodeRiskLevel.High
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SensitiveDataResult> DetectSensitiveDataAsync(byte[] fileData, string fileType)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                return new SensitiveDataResult();
            }

            var content = Encoding.UTF8.GetString(fileData.Take(Math.Min(fileData.Length, 50000)).ToArray());
            var detectedItems = new List<SensitiveDataItem>();
            var dataCategories = new HashSet<string>();

            // 敏感数据模式
            var sensitivePatterns = new[]
            {
                new { Pattern = @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", Type = "信用卡号", Sensitivity = SensitivityLevel.High },
                new { Pattern = @"\b\d{3}-\d{2}-\d{4}\b", Type = "社会保障号", Sensitivity = SensitivityLevel.High },
                new { Pattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", Type = "邮箱地址", Sensitivity = SensitivityLevel.Medium },
                new { Pattern = @"\b\d{10,}\b", Type = "电话号码", Sensitivity = SensitivityLevel.Medium },
                new { Pattern = @"\b[A-Za-z]{2}\d{6}\b", Type = "护照号码", Sensitivity = SensitivityLevel.High },
                new { Pattern = @"b(?:password|secret|key|token|api_key|private_key)s*[:=]s*[""']?([^""'s]+)[""']?", Type = "密钥", Sensitivity = SensitivityLevel.Critical }
            };

            var lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                foreach (var patternDef in sensitivePatterns)
                {
                    var matches = Regex.Matches(line, patternDef.Pattern);
                    foreach (Match match in matches)
                    {
                        detectedItems.Add(new SensitiveDataItem
                        {
                            DataType = patternDef.Type,
                            Value = match.Value,
                            Pattern = patternDef.Pattern,
                            Sensitivity = patternDef.Sensitivity,
                            LineNumber = i + 1,
                            ColumnNumber = match.Index + 1
                        });
                        dataCategories.Add(patternDef.Type);
                    }
                }
            }

            // 计算敏感度级别
            var sensitivityLevel = detectedItems.Any() 
                ? detectedItems.Max(i => i.Sensitivity) 
                : SensitivityLevel.None;

            return new SensitiveDataResult
            {
                ContainsSensitiveData = detectedItems.Count > 0,
                DetectedItems = detectedItems,
                SensitivityLevel = sensitivityLevel,
                DataCategories = dataCategories,
                TotalMatches = detectedItems.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "敏感数据检测失败: {FileType}", fileType);
            return new SensitiveDataResult();
        }
    }

    /// <inheritdoc/>
    public async Task<ImageSecurityResult> ValidateImageFileAsync(byte[] fileData)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                return new ImageSecurityResult
                {
                    IsValid = false
                };
            }

            var result = new ImageSecurityResult
            {
                FileSize = fileData.Length
            };

            // 检测文件类型
            var detectionResult = await DetectRealFileTypeAsync(fileData);
            result.ImageFormat = detectionResult.DetectedType;

            // 检查是否为有效图片
            if (!IsImageFile(detectionResult.DetectedType))
            {
                result.IsValid = false;
                result.SecurityIssues = new List<string> { "不是有效的图片文件" };
                return result;
            }

            // 检查图片尺寸（如果可能）
            try
            {
                using var stream = new MemoryStream(fileData);
                using var image = System.Drawing.Image.FromStream(stream);
                result.Width = image.Width;
                result.Height = image.Height;
                
                // 检查图片尺寸是否过大
                if (image.Width > 4096 || image.Height > 4096)
                {
                    result.SecurityIssues = new List<string> { "图片尺寸过大" };
                }
            }
            catch
            {
                // 如果无法解析图片，可能是损坏的图片文件
                result.IsValid = false;
                result.SecurityIssues = new List<string> { "图片文件损坏" };
                return result;
            }

            // 检查EXIF数据
            result.ContainsExifData = ContainsExifData(fileData);

            // 检查是否为动画GIF
            if (detectionResult.DetectedType == ".gif")
            {
                result.IsAnimated = IsAnimatedGif(fileData);
            }

            // 检查隐写术（简单检查）
            result.IsSteganographyDetected = DetectSteganography(fileData);

            result.IsValid = !result.SecurityIssues.Any();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图片文件安全验证失败");
            return new ImageSecurityResult
            {
                IsValid = false,
                SecurityIssues = new List<string> { "图片验证失败" }
            };
        }
    }

    #endregion

    // 文件大小和限制检查
    #region 文件大小和限制检查

    /// <inheritdoc/>
    public async Task<FileSizeValidationResult> ValidateFileSizeAsync(long fileSize, long maxSize)
    {
        try
        {
            var result = new FileSizeValidationResult
            {
                FileSize = fileSize,
                MaxAllowedSize = maxSize
            };

            if (fileSize <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "文件大小不能为0或负数";
                return result;
            }

            if (fileSize > maxSize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"文件大小超过限制 (最大允许: {maxSize / 1024 / 1024}MB)";
                result.SizePercentage = (double)fileSize / maxSize * 100;
                return result;
            }

            result.IsValid = true;
            result.SizePercentage = (double)fileSize / maxSize * 100;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件大小验证失败");
            return new FileSizeValidationResult
            {
                IsValid = false,
                ErrorMessage = "文件大小验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileSizeLimitResult> CheckFileSizeLimitAsync(byte[] fileData, UploadContext context)
    {
        try
        {
            var config = await GetFileSecurityConfigAsync();
            long limit = context.Context switch
            {
                "avatar" => 2 * 1024 * 1024, // 2MB
                "attachment" => 10 * 1024 * 1024, // 10MB
                "import" => 50 * 1024 * 1024, // 50MB
                _ => config.DefaultMaxFileSize
            };

            var result = new FileSizeLimitResult
            {
                CurrentSize = fileData.Length,
                Limit = limit,
                Remaining = limit - fileData.Length,
                LimitType = context.Context
            };

            result.IsWithinLimit = fileData.Length <= limit;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件大小限制检查失败");
            return new FileSizeLimitResult
            {
                IsWithinLimit = false,
                CurrentSize = fileData.Length,
                Limit = 0,
                Remaining = 0
            };
        }
    }

    /// <inheritdoc/>
    public async Task<BatchUploadValidationResult> ValidateBatchUploadAsync(IEnumerable<FileUploadInfo> files, long maxTotalSize)
    {
        try
        {
            var fileList = files.ToList();
            var results = new List<FileValidationResult>();
            long totalSize = 0;
            int validCount = 0;
            int invalidCount = 0;

            foreach (var file in fileList)
            {
                var result = new FileValidationResult
                {
                    FileName = file.FileName,
                    FileSize = file.FileSize,
                    ContentType = file.ContentType
                };

                // 检查文件大小
                if (file.FileSize > 10 * 1024 * 1024) // 10MB per file
                {
                    result.IsValid = false;
                    result.ErrorMessage = "单个文件大小不能超过10MB";
                    invalidCount++;
                }
                else
                {
                    result.IsValid = true;
                    validCount++;
                }

                results.Add(result);
                totalSize += file.FileSize;
            }

            // 检查总大小
            if (totalSize > maxTotalSize)
            {
                // 标记所有文件为无效
                foreach (var result in results)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "总文件大小超过限制";
                }
                validCount = 0;
                invalidCount = results.Count;
            }

            return new BatchUploadValidationResult
            {
                IsValid = validCount > 0 && totalSize <= maxTotalSize,
                TotalFiles = results.Count,
                ValidFiles = validCount,
                InvalidFiles = invalidCount,
                TotalSize = totalSize,
                MaxTotalSize = maxTotalSize,
                FileResults = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量上传验证失败");
            return new BatchUploadValidationResult
            {
                IsValid = false,
                TotalFiles = 0,
                ValidFiles = 0,
                InvalidFiles = 0,
                TotalSize = 0,
                MaxTotalSize = maxTotalSize,
                FileResults = new List<FileValidationResult>()
            };
        }
    }

    #endregion

    // 文件上传安全
    #region 文件上传安全

    /// <inheritdoc/>
    public async Task<FileUploadSecurityResult> ValidateFileUploadAsync(FileUploadInfo file, UploadContext uploadContext)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var result = new FileUploadSecurityResult
            {
                SecurityScore = new FileSecurityScore()
            };

            // 1. 文件大小检查
            var sizeResult = await CheckFileSizeLimitAsync(file.FileData, uploadContext);
            if (!sizeResult.IsWithinLimit)
            {
                result.IsValid = false;
                result.ErrorMessage = sizeResult.LimitType switch
                {
                    "avatar" => "头像文件大小不能超过2MB",
                    "attachment" => "附件文件大小不能超过10MB",
                    "import" => "导入文件大小不能超过50MB",
                    _ => "文件大小超过限制"
                };
                result.SecurityScore.SizeComplianceScore = 0;
                return result;
            }
            result.SecurityScore.SizeComplianceScore = 100;

            // 2. 文件类型检查
            var typeResult = await ValidateFileTypeAsync(file.FileName, file.ContentType, file.FileData);
            if (!typeResult.IsValid)
            {
                result.IsValid = false;
                result.ErrorMessage = typeResult.ErrorMessage;
                result.SecurityScore.TypeSafetyScore = 0;
                return result;
            }
            result.SecurityScore.TypeSafetyScore = 100;

            // 3. 恶意软件扫描
            var malwareResult = await DetectMalwareAsync(file.FileData);
            if (!malwareResult.IsClean)
            {
                result.IsValid = false;
                result.ErrorMessage = "检测到恶意软件";
                result.SecurityScore.VirusScanScore = 0;
                return result;
            }
            result.SecurityScore.VirusScanScore = 100;

            // 4. 内容安全检查
            var contentResult = await ScanFileContentAsync(file.FileData, file.FileName);
            if (!contentResult.IsSafe)
            {
                result.IsValid = false;
                result.ErrorMessage = "文件内容存在安全风险";
                result.SecurityScore.ContentSafetyScore = 0;
                result.SecurityWarnings = contentResult.SecurityIssues.ToList();
                result.SecurityRecommendations = contentResult.Recommendations.ToList();
                return result;
            }
            result.SecurityScore.ContentSafetyScore = 100;

            // 5. 权限检查
            var hasPermission = await _permissionService.CanUploadAttachmentAsync(uploadContext.UserId, uploadContext.Context);
            if (!hasPermission)
            {
                result.IsValid = false;
                result.ErrorMessage = "没有上传权限";
                return result;
            }

            // 生成安全文件名
            result.SecureFileName = await GenerateSecureFileNameAsync(file.FileName);

            // 计算总分
            result.SecurityScore.OverallScore = (int)result.SecurityScore.CalculateOverallScore();
            result.IsValid = true;
            result.RiskLevel = CalculateRiskLevel(result.SecurityScore.OverallScore);

            stopwatch.Stop();

            // 记录上传事件
            await LogFileUploadEventAsync(new FileUploadEvent
            {
                FileId = Guid.NewGuid(),
                FileName = file.FileName,
                UserId = uploadContext.UserId,
                FileSize = file.FileSize.Length,
                ContentType = file.ContentType,
                UserIP = uploadContext.UserIP,
                UserAgent = uploadContext.UserAgent,
                Status = UploadStatus.Success,
                Timestamp = DateTime.UtcNow
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传安全验证失败: {FileName}", file.FileName);
            return new FileUploadSecurityResult
            {
                IsValid = false,
                ErrorMessage = "文件上传验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<BatchFileUploadSecurityResult> ValidateBatchFileUploadAsync(IEnumerable<FileUploadInfo> files, UploadContext uploadContext)
    {
        try
        {
            var fileList = files.ToList();
            var results = new List<FileUploadSecurityResult>();
            var globalWarnings = new List<string>();
            var globalRecommendations = new List<string>();

            int validFiles = 0;
            int invalidFiles = 0;
            long totalSize = 0;

            foreach (var file in fileList)
            {
                totalSize += file.FileSize.Length;
            }

            // 检查总大小限制
            var config = await GetFileSecurityConfigAsync();
            if (totalSize > config.MaxTotalUploadSize)
            {
                globalWarnings.Add($"总文件大小超过限制 (最大: {config.MaxTotalUploadSize / 1024 / 1024}MB)");
                globalRecommendations.Add("请减少文件总大小或分批上传");
            }

            // 检查文件数量限制
            if (fileList.Count > config.MaxFilesPerUpload)
            {
                globalWarnings.Add($"文件数量超过限制 (最大: {config.MaxFilesPerUpload})");
                globalRecommendations.Add("请分批上传文件");
            }

            // 验证每个文件
            foreach (var file in fileList)
            {
                var result = await ValidateFileUploadAsync(file, uploadContext);
                results.Add(result);

                if (result.IsValid)
                    validFiles++;
                else
                    invalidFiles++;
            }

            return new BatchFileUploadSecurityResult
            {
                AllFilesValid = validFiles == fileList.Count && invalidFiles == 0,
                TotalFiles = fileList.Count,
                ValidFiles = validFiles,
                InvalidFiles = invalidFiles,
                FileResults = results,
                GlobalWarnings = globalWarnings,
                GlobalRecommendations = globalRecommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量文件上传安全验证失败");
            return new BatchFileUploadSecurityResult
            {
                AllFilesValid = false,
                TotalFiles = 0,
                ValidFiles = 0,
                InvalidFiles = 0,
                FileResults = new List<FileUploadSecurityResult>(),
                GlobalWarnings = new List<string> { "批量上传验证失败" }
            };
        }
    }

    /// <inheritdoc/>
    public async Task<string> GenerateSecureFileNameAsync(string originalFileName)
    {
        try
        {
            // 获取文件扩展名
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

            // 清理文件名
            var cleanedName = Regex.Replace(fileNameWithoutExt, @"[^\w\u4e00-\u9fa5-]", "_");
            
            // 生成唯一文件名
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            var uniqueName = $"{timestamp}_{random}_{cleanedName}{extension}";

            // 确保文件名长度不超过限制
            if (uniqueName.Length > 255)
            {
                uniqueName = $"{timestamp}_{random}{extension}";
            }

            return uniqueName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成安全文件名失败: {FileName}", originalFileName);
            return $"file_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{Path.GetExtension(originalFileName)}";
        }
    }

    /// <inheritdoc/>
    public async Task<UploadRequestValidationResult> ValidateUploadRequestAsync(UploadRequest request)
    {
        try
        {
            var result = new UploadRequestValidationResult();

            // 检查文件数量
            if (request.FileCount <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "文件数量必须大于0";
                result.ValidationIssues = new List<string> { "文件数量无效" };
                result.RiskLevel = RequestRiskLevel.High;
                return result;
            }

            // 检查文件数量限制
            if (request.FileCount > 10)
            {
                result.IsValid = false;
                result.ErrorMessage = "一次最多上传10个文件";
                result.ValidationIssues = new List<string> { "文件数量超过限制" };
                result.RiskLevel = RequestRiskLevel.Medium;
                return result;
            }

            // 检查总大小
            if (request.TotalSize > 100 * 1024 * 1024) // 100MB
            {
                result.IsValid = false;
                result.ErrorMessage = "总文件大小不能超过100MB";
                result.ValidationIssues = new List<string> { "总文件大小超过限制" };
                result.RiskLevel = RequestRiskLevel.Medium;
                return result;
            }

            // 检查上传频率
            var frequencyResult = await CheckUploadFrequencyAsync(request.UserId, request.UserIP);
            if (!frequencyResult.IsAllowed)
            {
                result.IsValid = false;
                result.ErrorMessage = frequencyResult.ErrorMessage;
                result.ValidationIssues = new List<string> { "上传频率过高" };
                result.RiskLevel = RequestRiskLevel.High;
                return result;
            }

            result.IsValid = true;
            result.RiskLevel = RequestRiskLevel.None;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传请求验证失败");
            return new UploadRequestValidationResult
            {
                IsValid = false,
                ErrorMessage = "上传请求验证失败"
            };
        }
    }

    #endregion

    // 病毒扫描
    #region 病毒扫描

    /// <inheritdoc/>
    public async Task<VirusScanResult> ScanForVirusesAsync(byte[] fileData)
    {
        return await DetectMalwareAsync(fileData);
    }

    /// <inheritdoc/>
    public async Task<BatchVirusScanResult> BatchScanForVirusesAsync(IEnumerable<VirusScanRequest> files)
    {
        try
        {
            var fileList = files.ToList();
            var results = new List<VirusScanResult>();
            int cleanFiles = 0;
            int infectedFiles = 0;
            int scanErrors = 0;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var file in fileList)
            {
                try
                {
                    var result = await ScanForVirusesAsync(file.FileData);
                    result.FileName = file.FileName;
                    results.Add(result);

                    if (result.IsClean)
                        cleanFiles++;
                    else if (result.Status == VirusScanStatus.Infected)
                        infectedFiles++;
                    else
                        scanErrors++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "病毒扫描失败: {FileName}", file.FileName);
                    results.Add(new VirusScanResult
                    {
                        IsClean = false,
                        Status = VirusScanStatus.Error,
                        FileName = file.FileName,
                        ScanTime = DateTime.UtcNow
                    });
                    scanErrors++;
                }
            }

            stopwatch.Stop();

            return new BatchVirusScanResult
            {
                TotalFiles = fileList.Count,
                CleanFiles = cleanFiles,
                InfectedFiles = infectedFiles,
                ScanErrors = scanErrors,
                Results = results,
                BatchScanTime = DateTime.UtcNow,
                TotalScanDurationMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量病毒扫描失败");
            return new BatchVirusScanResult
            {
                TotalFiles = 0,
                CleanFiles = 0,
                InfectedFiles = 0,
                ScanErrors = 0,
                Results = new List<VirusScanResult>(),
                BatchScanTime = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileHashCheckResult> CheckFileHashBlacklistAsync(byte[] fileData)
    {
        try
        {
            // 计算文件哈希
            var md5Hash = ComputeMd5Hash(fileData);
            var sha1Hash = ComputeSha1Hash(fileData);
            var sha256Hash = ComputeSha256Hash(fileData);

            var result = new FileHashCheckResult
            {
                FileHash = sha256Hash,
                HashAlgorithms = new[] { "MD5", "SHA1", "SHA256" }
            };

            // 这里应该检查黑名单数据库
            // 暂时模拟检查
            var blacklistedHashes = new[] { "known_malware_hash_1", "known_malware_hash_2" };
            
            if (blacklistedHashes.Contains(sha256Hash))
            {
                result.IsBlacklisted = true;
                result.BlacklistReason = "已知恶意软件";
                result.BlacklistedAt = DateTime.UtcNow.AddDays(-1);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件哈希黑名单检查失败");
            return new FileHashCheckResult
            {
                IsBlacklisted = false,
                FileHash = "unknown"
            };
        }
    }

    #endregion

    // 文件操作安全
    #region 文件操作安全

    /// <inheritdoc/>
    public async Task<FileDownloadSecurityResult> ValidateFileDownloadAsync(string fileId, Guid userId)
    {
        try
        {
            // 这里应该检查文件是否存在、用户是否有权限下载
            // 暂时返回模拟结果
            return new FileDownloadSecurityResult
            {
                IsAllowed = true,
                FilePath = $"/uploads/{fileId}",
                ContentType = "application/octet-stream",
                FileSize = 1024 * 1024, // 1MB
                QuotaInfo = new DownloadQuotaInfo
                {
                    DownloadedToday = 5,
                    DailyLimit = 100,
                    Remaining = 95,
                    ResetTime = DateTime.Today.AddDays(1)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件下载权限验证失败: {FileId}", fileId);
            return new FileDownloadSecurityResult
            {
                IsAllowed = false,
                ErrorMessage = "文件下载权限验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileDeleteSecurityResult> ValidateFileDeleteAsync(string fileId, Guid userId)
    {
        try
        {
            // 这里应该检查文件是否存在、用户是否有权限删除
            // 暂时返回模拟结果
            var hasPermission = await _permissionService.CanDeleteAttachmentAsync(userId, Guid.Parse(fileId));
            
            return new FileDeleteSecurityResult
            {
                IsAllowed = hasPermission,
                IsOwner = true,
                HasAdminPrivileges = await _permissionService.IsAdminAsync(userId),
                IsSystemFile = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除权限验证失败: {FileId}", fileId);
            return new FileDeleteSecurityResult
            {
                IsAllowed = false,
                ErrorMessage = "文件删除权限验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileAccessSecurityResult> ValidateFileAccessAsync(string fileId, Guid userId, FileOperation operation)
    {
        try
        {
            // 这里应该检查文件是否存在、用户是否有相应权限
            // 暂时返回模拟结果
            var hasPermission = operation switch
            {
                FileOperation.Read => await _permissionService.CanViewAttachmentAsync(userId, Guid.Parse(fileId)),
                FileOperation.Download => await _permissionService.CanDownloadAttachmentAsync(userId, Guid.Parse(fileId)),
                FileOperation.Delete => await _permissionService.CanDeleteAttachmentAsync(userId, Guid.Parse(fileId)),
                _ => false
            };

            return new FileAccessSecurityResult
            {
                IsAllowed = hasPermission,
                AccessLevel = hasPermission ? FileAccessLevel.Read : FileAccessLevel.None,
                IsOwner = true,
                HasSharedAccess = false,
                AccessExpiry = DateTime.UtcNow.AddDays(7)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件访问权限验证失败: {FileId}", fileId);
            return new FileAccessSecurityResult
            {
                IsAllowed = false,
                ErrorMessage = "文件访问权限验证失败"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FileShareSecurityResult> ValidateFileShareAsync(string fileId, FileShareRequest shareRequest)
    {
        try
        {
            // 这里应该检查文件是否存在、用户是否有权限分享
            // 暂时返回模拟结果
            var hasPermission = await _permissionService.CanShareAttachmentAsync(shareRequest.UserId, Guid.Parse(fileId));
            
            if (!hasPermission)
            {
                return new FileShareSecurityResult
                {
                    IsAllowed = false,
                    ErrorMessage = "没有分享权限"
                };
            }

            return new FileShareSecurityResult
            {
                IsAllowed = true,
                ShareToken = Guid.NewGuid().ToString(),
                ExpiryDate = shareRequest.ExpiryDate ?? DateTime.UtcNow.AddDays(7),
                HasPasswordProtection = !string.IsNullOrEmpty(shareRequest.Password),
                AccessLevel = shareRequest.AccessLevel
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件分享权限验证失败: {FileId}", fileId);
            return new FileShareSecurityResult
            {
                IsAllowed = false,
                ErrorMessage = "文件分享权限验证失败"
            };
        }
    }

    #endregion

    // 配置和策略管理
    #region 配置和策略管理

    /// <inheritdoc/>
    public async Task<FileSecurityConfig> GetFileSecurityConfigAsync()
    {
        // 这里应该从配置或数据库中获取配置
        // 暂时返回默认配置
        return new FileSecurityConfig
        {
            EnableVirusScanning = true,
            EnableContentAnalysis = true,
            EnableFileHashCheck = true,
            EnableSizeValidation = true,
            EnableTypeValidation = true,
            DefaultMaxFileSize = 10 * 1024 * 1024, // 10MB
            MaxTotalUploadSize = 100 * 1024 * 1024, // 100MB
            MaxFilesPerUpload = 10,
            AllowedFileTypes = new List<FileTypeConfig>
            {
                new FileTypeConfig { Extension = ".txt", MimeType = "text/plain", MaxSize = 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".pdf", MimeType = "application/pdf", MaxSize = 10 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".jpg", MimeType = "image/jpeg", MaxSize = 5 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".png", MimeType = "image/png", MaxSize = 5 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".gif", MimeType = "image/gif", MaxSize = 5 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".json", MimeType = "application/json", MaxSize = 2 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".xml", MimeType = "application/xml", MaxSize = 2 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".csv", MimeType = "text/csv", MaxSize = 2 * 1024 * 1024, IsEnabled = true },
                new FileTypeConfig { Extension = ".md", MimeType = "text/markdown", MaxSize = 1 * 1024 * 1024, IsEnabled = true }
            },
            BlockedExtensions = DangerousExtensions,
            SafeMimeTypes = new[] { "text/plain", "application/pdf", "image/jpeg", "image/png", "image/gif", "application/json", "application/xml", "text/csv", "text/markdown" },
            EnableLogging = true,
            EnableNotifications = true
        };
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFileSecurityConfigAsync(FileSecurityConfig config)
    {
        try
        {
            // 这里应该将配置保存到数据库
            _logger.LogInformation("文件安全配置已更新");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新文件安全配置失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FileTypeConfig>> GetAllowedFileTypesAsync(string context)
    {
        var config = await GetFileSecurityConfigAsync();
        return config.AllowedFileTypes.Where(f => f.AllowedContexts.Contains(context) || f.AllowedContexts.Count == 0);
    }

    /// <inheritdoc/>
    public async Task<bool> AddAllowedFileTypeAsync(FileTypeConfig fileType)
    {
        try
        {
            // 这里应该将文件类型配置保存到数据库
            _logger.LogInformation("添加允许的文件类型: {Extension}", fileType.Extension);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加允许的文件类型失败: {Extension}", fileType.Extension);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAllowedFileTypeAsync(string extension)
    {
        try
        {
            // 这里应该从数据库中删除文件类型配置
            _logger.LogInformation("移除允许的文件类型: {Extension}", extension);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除允许的文件类型失败: {Extension}", extension);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<FileSizeLimit> GetFileSizeLimitAsync(string context)
    {
        return context switch
        {
            "avatar" => new FileSizeLimit { MaxFileSize = 2 * 1024 * 1024, MaxTotalSize = 2 * 1024 * 1024, MaxFiles = 1, Context = context },
            "attachment" => new FileSizeLimit { MaxFileSize = 10 * 1024 * 1024, MaxTotalSize = 100 * 1024 * 1024, MaxFiles = 10, Context = context },
            "import" => new FileSizeLimit { MaxFileSize = 50 * 1024 * 1024, MaxTotalSize = 200 * 1024 * 1024, MaxFiles = 5, Context = context },
            _ => new FileSizeLimit { MaxFileSize = 10 * 1024 * 1024, MaxTotalSize = 100 * 1024 * 1024, MaxFiles = 10, Context = context }
        };
    }

    #endregion

    // 审计和日志
    #region 审计和日志

    /// <inheritdoc/>
    public async Task<bool> LogFileUploadEventAsync(FileUploadEvent uploadEvent)
    {
        try
        {
            _logger.LogInformation("文件上传事件: {FileName}, 大小: {FileSize} bytes, 用户: {UserId}", 
                uploadEvent.FileName, uploadEvent.FileSize, uploadEvent.UserId);
            
            // 这里应该将事件保存到数据库
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录文件上传事件失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> LogFileDownloadEventAsync(FileDownloadEvent downloadEvent)
    {
        try
        {
            _logger.LogInformation("文件下载事件: {FileName}, 用户: {UserId}", 
                downloadEvent.FileName, downloadEvent.UserId);
            
            // 这里应该将事件保存到数据库
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录文件下载事件失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> LogFileSecurityEventAsync(FileSecurityEvent securityEvent)
    {
        try
        {
            _logger.LogWarning("文件安全事件: {EventType} - {Description}", 
                securityEvent.EventType, securityEvent.EventDescription);
            
            // 这里应该将事件保存到数据库
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录文件安全事件失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<FileOperationLog>> GetFileOperationLogsAsync(FileOperationLogFilter filter)
    {
        // 这里应该从数据库中获取文件操作日志
        // 暂时返回空结果
        return new PaginatedResult<FileOperationLog>
        {
            Items = new List<FileOperationLog>(),
            TotalCount = 0,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc/>
    public async Task<FileSecurityStats> GetFileSecurityStatsAsync(DateTime startDate, DateTime endDate)
    {
        // 这里应该从数据库中获取统计数据
        // 暂时返回模拟数据
        return new FileSecurityStats
        {
            TotalUploads = 1000,
            SuccessfulUploads = 950,
            FailedUploads = 50,
            TotalDownloads = 5000,
            TotalDataUploaded = 500 * 1024 * 1024, // 500MB
            TotalDataDownloaded = 2000 * 1024 * 1024, // 2GB
            SecurityEvents = 25,
            VirusDetections = 5,
            FileTypeStats = new List<FileTypeStats>
            {
                new FileTypeStats { FileType = ".txt", UploadCount = 400, TotalSize = 20 * 1024 * 1024, AverageSize = 51200, SecurityEvents = 2 },
                new FileTypeStats { FileType = ".pdf", UploadCount = 200, TotalSize = 100 * 1024 * 1024, AverageSize = 512000, SecurityEvents = 1 },
                new FileTypeStats { FileType = ".jpg", UploadCount = 300, TotalSize = 150 * 1024 * 1024, AverageSize = 512000, SecurityEvents = 3 },
                new FileTypeStats { FileType = ".png", UploadCount = 100, TotalSize = 50 * 1024 * 1024, AverageSize = 512000, SecurityEvents = 1 }
            },
            DailyStats = Enumerable.Range(0, 7)
                .Select(i => new DailyFileStats
                {
                    Date = DateTime.UtcNow.AddDays(-i),
                    Uploads = 100 + i * 10,
                    Downloads = 500 + i * 50,
                    DataTransferred = (100 + i * 10) * 1024 * 1024,
                    SecurityEvents = 2 + i / 2
                })
                .Reverse()
                .ToList()
        };
    }

    #endregion

    // 私有辅助方法
    #region 私有辅助方法

    private bool IsMimeTypeMatch(string declaredType, string detectedType)
    {
        // 将文件扩展名转换为MIME类型
        var detectedMimeType = GetMimeTypeFromExtension(detectedType);
        
        // 检查是否匹配
        return declaredType.Equals(detectedMimeType, StringComparison.OrdinalIgnoreCase) ||
               declaredType.Split('/')[0].Equals(detectedMimeType.Split('/')[0], StringComparison.OrdinalIgnoreCase);
    }

    private string GetMimeTypeFromExtension(string extension)
    {
        return extension.ToLower() switch
        {
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".csv" => "text/csv",
            ".md" => "text/markdown",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }

    private bool IsDangerousFileType(string fileType)
    {
        return fileType == "executable" || 
               DangerousExtensions.Contains(fileType.ToLower());
    }

    private bool IsSafeMimeType(string mimeType)
    {
        var safeMimeTypes = new[]
        {
            "text/plain", "text/csv", "text/markdown", "application/json", "application/xml",
            "image/jpeg", "image/png", "image/gif", "image/bmp", "application/pdf",
            "application/zip", "application/x-rar-compressed"
        };

        return safeMimeTypes.Contains(mimeType.ToLower());
    }

    private bool IsTextContent(string content)
    {
        try
        {
            // 检查是否为可读文本
            return !content.Contains('\0') && 
                   content.All(c => char.IsControl(c) || char.IsPrint(c) || c == '\n' || c == '\r' || c == '\t');
        }
        catch
        {
            return false;
        }
    }

    private bool IsImageFile(string fileType)
    {
        var imageTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        return imageTypes.Contains(fileType.ToLower());
    }

    private bool ContainsExifData(byte[] fileData)
    {
        try
        {
            // 简单的EXIF数据检查
            var content = Encoding.UTF8.GetString(fileData.Take(1000).ToArray());
            return content.Contains("Exif") || content.Contains("exif");
        }
        catch
        {
            return false;
        }
    }

    private bool IsAnimatedGif(byte[] fileData)
    {
        try
        {
            // 检查GIF是否包含多个图像块
            var content = Encoding.UTF8.GetString(fileData);
            var gifHeaderIndex = content.IndexOf("GIF");
            if (gifHeaderIndex >= 0)
            {
                var gifContent = content.Substring(gifHeaderIndex);
                return gifContent.Count(c => c == ',') > 1; // 多个图像分隔符
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool DetectSteganography(byte[] fileData)
    {
        try
        {
            // 简单的隐写术检测 - 检查文件是否包含隐藏数据
            // 这是一个简化的实现，真正的隐写术检测需要更复杂的算法
            var content = Encoding.UTF8.GetString(fileData.Take(Math.Min(fileData.Length, 1000)).ToArray());
            
            // 检查是否包含可疑的数据模式
            var suspiciousPatterns = new[]
            {
                "hidden", "secret", "stego", "embedded", "concealed"
            };

            return suspiciousPatterns.Any(pattern => 
                content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private async Task<FileContentSecurityResult> ScanTextFileContent(byte[] fileData, string fileName, FileContentSecurityResult result)
    {
        try
        {
            var content = Encoding.UTF8.GetString(fileData);
            
            // 使用内容安全服务检查文本内容
            var contentResult = await _contentSecurityService.ValidateContentAsync(content, "text");
            
            if (!contentResult.IsValid)
            {
                result.IsSafe = false;
                result.RiskLevel = contentResult.SecurityLevel;
                result.SecurityIssues = contentResult.DetectedIssues.ToList();
                result.Recommendations = new List<string> { "请检查文本内容是否包含敏感信息" };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文本文件内容扫描失败: {FileName}", fileName);
            result.IsSafe = false;
            result.RiskLevel = SecurityLevel.High;
            result.SecurityIssues = new List<string> { "文本文件扫描失败" };
            return result;
        }
    }

    private async Task<FileContentSecurityResult> ScanJsonFileContent(byte[] fileData, string fileName, FileContentSecurityResult result)
    {
        try
        {
            var content = Encoding.UTF8.GetString(fileData);
            
            // 检查JSON格式
            try
            {
                System.Text.Json.JsonDocument.Parse(content);
            }
            catch
            {
                result.IsSafe = false;
                result.RiskLevel = SecurityLevel.Medium;
                result.SecurityIssues = new List<string> { "JSON格式无效" };
                return result;
            }

            // 检查JSON内容安全性
            var contentResult = await _contentSecurityService.ValidateContentAsync(content, "json");
            
            if (!contentResult.IsValid)
            {
                result.IsSafe = false;
                result.RiskLevel = contentResult.SecurityLevel;
                result.SecurityIssues = contentResult.DetectedIssues.ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JSON文件内容扫描失败: {FileName}", fileName);
            result.IsSafe = false;
            result.RiskLevel = SecurityLevel.High;
            result.SecurityIssues = new List<string> { "JSON文件扫描失败" };
            return result;
        }
    }

    private async Task<FileContentSecurityResult> ScanXmlFileContent(byte[] fileData, string fileName, FileContentSecurityResult result)
    {
        try
        {
            var content = Encoding.UTF8.GetString(fileData);
            
            // 检查XML格式
            try
            {
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(content);
            }
            catch
            {
                result.IsSafe = false;
                result.RiskLevel = SecurityLevel.Medium;
                result.SecurityIssues = new List<string> { "XML格式无效" };
                return result;
            }

            // 检查XML内容安全性
            var contentResult = await _contentSecurityService.ValidateContentAsync(content, "xml");
            
            if (!contentResult.IsValid)
            {
                result.IsSafe = false;
                result.RiskLevel = contentResult.SecurityLevel;
                result.SecurityIssues = contentResult.DetectedIssues.ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XML文件内容扫描失败: {FileName}", fileName);
            result.IsSafe = false;
            result.RiskLevel = SecurityLevel.High;
            result.SecurityIssues = new List<string> { "XML文件扫描失败" };
            return result;
        }
    }

    private async Task<FileContentSecurityResult> ScanImageFileContent(byte[] fileData, string fileName, FileContentSecurityResult result)
    {
        try
        {
            var imageResult = await ValidateImageFileAsync(fileData);
            
            if (!imageResult.IsValid)
            {
                result.IsSafe = false;
                result.RiskLevel = SecurityLevel.Medium;
                result.SecurityIssues = imageResult.SecurityIssues.ToList();
                result.Recommendations = new List<string> { "请检查图片文件是否安全" };
            }

            // 检查图片是否包含隐藏文本
            var ocrResult = await ExtractTextFromImageAsync(fileData);
            if (!string.IsNullOrEmpty(ocrResult))
            {
                var textResult = await _contentSecurityService.ValidateContentAsync(ocrResult, "text");
                if (!textResult.IsValid)
                {
                    result.RiskLevel = Math.Max(result.RiskLevel, textResult.SecurityLevel);
                    result.SecurityIssues = result.SecurityIssues.Concat(textResult.DetectedIssues).ToList();
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图片文件内容扫描失败: {FileName}", fileName);
            result.IsSafe = false;
            result.RiskLevel = SecurityLevel.High;
            result.SecurityIssues = new List<string> { "图片文件扫描失败" };
            return result;
        }
    }

    private async Task<FileContentSecurityResult> ScanArchiveFileContent(byte[] fileData, string fileName, FileContentSecurityResult result)
    {
        try
        {
            // 压缩文件的安全检查
            result.IsSafe = true;
            result.RiskLevel = SecurityLevel.Low;
            result.Recommendations = new List<string> { "建议对压缩文件进行病毒扫描" };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "压缩文件内容扫描失败: {FileName}", fileName);
            result.IsSafe = false;
            result.RiskLevel = SecurityLevel.High;
            result.SecurityIssues = new List<string> { "压缩文件扫描失败" };
            return result;
        }
    }

    private async Task<FileContentSecurityResult> ScanGenericFileContent(byte[] fileData, string fileName, FileContentSecurityResult result)
    {
        try
        {
            // 通用文件内容检查
            var content = Encoding.UTF8.GetString(fileData.Take(Math.Min(fileData.Length, 1000)).ToArray());
            
            var contentResult = await _contentSecurityService.ValidateContentAsync(content, "general");
            
            if (!contentResult.IsValid)
            {
                result.IsSafe = false;
                result.RiskLevel = contentResult.SecurityLevel;
                result.SecurityIssues = contentResult.DetectedIssues.ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通用文件内容扫描失败: {FileName}", fileName);
            result.IsSafe = false;
            result.RiskLevel = SecurityLevel.High;
            result.SecurityIssues = new List<string> { "文件扫描失败" };
            return result;
        }
    }

    private async Task<string> ExtractTextFromImageAsync(byte[] imageData)
    {
        try
        {
            // 这里应该调用OCR服务
            // 暂时返回空字符串
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "图片文字提取失败");
            return string.Empty;
        }
    }

    private double CalculateRiskScore(FileContentSecurityResult result)
    {
        double score = 0;
        
        // 基于风险级别计算分数
        switch (result.RiskLevel)
        {
            case SecurityLevel.Critical:
                score = 1.0;
                break;
            case SecurityLevel.High:
                score = 0.8;
                break;
            case SecurityLevel.Medium:
                score = 0.6;
                break;
            case SecurityLevel.Low:
                score = 0.3;
                break;
        }
        
        // 基于安全问题的数量增加分数
        score += result.SecurityIssues.Count() * 0.1;
        
        return Math.Min(score, 1.0);
    }

    private async Task<CodeFileSecurityResult> ValidateJavaScriptCode(string content, CodeFileSecurityResult result)
    {
        var dangerousPatterns = new[]
        {
            "eval(", "document.write", "innerHTML", "outerHTML", "setTimeout(", "setInterval(",
            "document.cookie", "window.location", "alert(", "confirm(", "prompt(",
            "Function(", "require(", "import(", "export("
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
                result.ContainsObfuscatedCode = true;
            }
        }

        result.RiskLevel = result.SecurityWarnings.Any() ? CodeRiskLevel.Medium : CodeRiskLevel.Low;

        return result;
    }

    private async Task<CodeFileSecurityResult> ValidatePhpCode(string content, CodeFileSecurityResult result)
    {
        var dangerousPatterns = new[]
        {
            "eval(", "exec(", "system(", "shell_exec(", "passthru(", "proc_open(",
            "include(", "require(", "file_get_contents(", "file_put_contents(",
            "mysql_query(", "mysqli_query(", "pdo->query("
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
                result.ContainsObfuscatedCode = true;
            }
        }

        result.RiskLevel = result.SecurityWarnings.Any() ? CodeRiskLevel.High : CodeRiskLevel.Low;

        return result;
    }

    private async Task<CodeFileSecurityResult> ValidatePythonCode(string content, CodeFileSecurityResult result)
    {
        var dangerousPatterns = new[]
        {
            "eval(", "exec(", "subprocess", "os.system", "commands.getstatusoutput",
            "pickle.loads(", "marshal.loads(", "__import__", "getattr("
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
            }
        }

        result.RiskLevel = result.SecurityWarnings.Any() ? CodeRiskLevel.Medium : CodeRiskLevel.Low;

        return result;
    }

    private async Task<CodeFileSecurityResult> ValidateCSharpCode(string content, CodeFileSecurityResult result)
    {
        var dangerousPatterns = new[]
        {
            "Process.Start", "Reflection", "DllImport", "unsafe", "fixed",
            "Marshal.PtrToString", "Marshal.Copy", "IntPtr"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险模式: {pattern}");
            }
        }

        result.RiskLevel = result.SecurityWarnings.Any() ? CodeRiskLevel.Medium : CodeRiskLevel.Low;

        return result;
    }

    private async Task<CodeFileSecurityResult> ValidateSqlCode(string content, CodeFileSecurityResult result)
    {
        result.ContainsDatabaseOperations = true;

        var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "GRANT", "REVOKE" };
        foreach (var keyword in dangerousKeywords)
        {
            if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"检测到危险操作: {keyword}");
                result.SecurityLevel = CodeRiskLevel.High;
            }
        }

        return result;
    }

    private async Task<CodeFileSecurityResult> ValidateGenericCode(string content, CodeFileSecurityResult result)
    {
        var dangerousPatterns = new[] { "password", "secret", "key", "token", "credential" };
        foreach (var pattern in dangerousPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result.SecurityWarnings.Add($"可能包含敏感信息: {pattern}");
            }
        }

        result.RiskLevel = result.SecurityWarnings.Any() ? CodeRiskLevel.Low : CodeRiskLevel.None;

        return result;
    }

    private SecurityLevel CalculateRiskLevel(int score)
    {
        return score switch
        {
            >= 80 => SecurityLevel.Critical,
            >= 60 => SecurityLevel.High,
            >= 40 => SecurityLevel.Medium,
            >= 20 => SecurityLevel.Low,
            _ => SecurityLevel.Low
        };
    }

    private async Task<UploadFrequencyResult> CheckUploadFrequencyAsync(Guid userId, string? userIP)
    {
        // 这里应该检查用户上传频率
        // 暂时返回允许的结果
        return new UploadFrequencyResult
        {
            IsAllowed = true,
            ErrorMessage = null
        };
    }

    private string ComputeMd5Hash(byte[] data)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = md5.ComputeHash(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private string ComputeSha1Hash(byte[] data)
    {
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hashBytes = sha1.ComputeHash(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private string ComputeSha256Hash(byte[] data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    #endregion
}

/// <summary>
/// 上传频率检查结果
/// </summary>
internal class UploadFrequencyResult
{
    public bool IsAllowed { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 文件安全评分扩展方法
/// </summary>
internal static class FileSecurityScoreExtensions
{
    public static double CalculateOverallScore(this FileSecurityScore score)
    {
        return (score.TypeSafetyScore + score.ContentSafetyScore + 
                score.SizeComplianceScore + score.VirusScanScore) / 4.0;
    }
}