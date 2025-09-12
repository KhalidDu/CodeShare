using System.Text.Json;
using System.Text;
using System.Globalization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 设置导入导出服务实现 - 遵循单一职责原则
/// </summary>
public class SettingsImportExportService : ISettingsImportExportService
{
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly ISettingsHistoryService _historyService;
    private readonly ISettingsValidationService _validationService;
    private readonly ILogger<SettingsImportExportService> _logger;

    // 支持的导出格式
    private static readonly List<string> SupportedFormats = new() { "json", "csv", "excel" };

    public SettingsImportExportService(
        ISystemSettingsRepository settingsRepository,
        ISettingsHistoryService historyService,
        ISettingsValidationService validationService,
        ILogger<SettingsImportExportService> logger)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ExportSettingsAsync(string format = "json")
    {
        try
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            if (settings == null)
            {
                throw new InvalidOperationException("没有找到系统设置");
            }

            return format.ToLower() switch
            {
                "json" => await ExportToJsonAsync(settings),
                "csv" => await ExportToCsvAsync(settings),
                "excel" => await ExportToExcelAsync(settings),
                _ => throw new ArgumentException($"不支持的导出格式: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出设置失败，格式: {Format}", format);
            throw;
        }
    }

    public async Task<SystemSettingsDto> ImportSettingsAsync(string jsonData, string updatedBy)
    {
        try
        {
            // 验证JSON数据
            var validationResult = await ValidateImportDataAsync(jsonData);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"导入数据验证失败: {string.Join(", ", validationResult.Errors.SelectMany(e => e.Value))}");
            }

            // 反序列化设置
            var settingsDto = JsonSerializer.Deserialize<SystemSettingsDto>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settingsDto == null)
            {
                throw new ArgumentException("无法解析导入的设置数据");
            }

            // 设置更新信息
            settingsDto.UpdatedBy = updatedBy;
            settingsDto.UpdatedAt = DateTime.UtcNow;

            // 转换为实体并保存
            var entity = MapToEntity(settingsDto);
            var savedEntity = await _settingsRepository.SaveSettingsAsync(entity);

            _logger.LogInformation("系统设置已导入，操作人: {UpdatedBy}", updatedBy);
            return MapToDto(savedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入设置失败，操作人: {UpdatedBy}", updatedBy);
            throw;
        }
    }

    public async Task<SettingsValidationResult> ValidateImportDataAsync(string jsonData)
    {
        var errors = new Dictionary<string, string[]>();
        var warnings = new Dictionary<string, string[]>();

        try
        {
            // 验证JSON格式
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                errors.Add("Data", new[] { "导入数据不能为空" });
                return new SettingsValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
            }

            // 验证JSON语法
            JsonDocument.Parse(jsonData);

            // 验证数据结构
            var settingsDto = JsonSerializer.Deserialize<SystemSettingsDto>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settingsDto == null)
            {
                errors.Add("Data", new[] { "无法解析设置数据" });
                return new SettingsValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
            }

            // 验证各个设置组
            var siteValidation = await _validationService.ValidateSiteSettingsAsync(settingsDto.SiteSettings);
            var securityValidation = await _validationService.ValidateSecuritySettingsAsync(settingsDto.SecuritySettings);
            var featureValidation = await _validationService.ValidateFeatureSettingsAsync(settingsDto.FeatureSettings);
            var emailValidation = await _validationService.ValidateEmailSettingsAsync(settingsDto.EmailSettings);

            // 合并验证结果
            MergeValidationResult(errors, siteValidation.Errors);
            MergeValidationResult(errors, securityValidation.Errors);
            MergeValidationResult(errors, featureValidation.Errors);
            MergeValidationResult(errors, emailValidation.Errors);

            MergeValidationResult(warnings, siteValidation.Warnings);
            MergeValidationResult(warnings, securityValidation.Warnings);
            MergeValidationResult(warnings, featureValidation.Warnings);
            MergeValidationResult(warnings, emailValidation.Warnings);

            return new SettingsValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (JsonException ex)
        {
            errors.Add("Format", new[] { $"JSON格式错误: {ex.Message}" });
            return new SettingsValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
        }
        catch (Exception ex)
        {
            errors.Add("General", new[] { $"验证过程中发生错误: {ex.Message}" });
            return new SettingsValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
        }
    }

    public async Task<SettingsHistoryExportResponse> ExportSettingsHistoryAsync(SettingsHistoryExportRequest request)
    {
        try
        {
            var historyRequest = new SettingsHistoryRequest
            {
                PageNumber = 1,
                PageSize = int.MaxValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                SettingType = request.SettingType,
                ChangedBy = request.ChangedBy,
                IsImportant = request.IsImportant
            };

            var historyResponse = await _historyService.GetChangeHistoryAsync(historyRequest);

            return request.Format.ToLower() switch
            {
                "json" => new SettingsHistoryExportResponse
                {
                    Success = true,
                    Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(historyResponse)),
                    FileName = GetHistoryExportFileName("json"),
                    ContentType = "application/json",
                    FileSize = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(historyResponse)).Length
                },
                "csv" => new SettingsHistoryExportResponse
                {
                    Success = true,
                    Data = Encoding.UTF8.GetBytes(ConvertHistoryToCsv(historyResponse.Items)),
                    FileName = GetHistoryExportFileName("csv"),
                    ContentType = "text/csv",
                    FileSize = Encoding.UTF8.GetBytes(ConvertHistoryToCsv(historyResponse.Items)).Length
                },
                _ => throw new ArgumentException($"不支持的导出格式: {request.Format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出设置历史失败");
            return new SettingsHistoryExportResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> GenerateBackupAsync(bool includeHistory = true)
    {
        try
        {
            var backup = new Dictionary<string, object>();

            // 备份当前设置
            var settings = await _settingsRepository.GetSettingsAsync();
            if (settings != null)
            {
                backup["Settings"] = MapToDto(settings);
            }

            // 备份历史记录
            if (includeHistory)
            {
                var historyRequest = new SettingsHistoryRequest
                {
                    PageNumber = 1,
                    PageSize = int.MaxValue
                };
                var historyResponse = await _historyService.GetChangeHistoryAsync(historyRequest);
                backup["History"] = historyResponse.Items;
            }

            // 添加备份元数据
            backup["BackupMetadata"] = new
            {
                CreatedAt = DateTime.UtcNow,
                Version = "1.0",
                IncludeHistory = includeHistory
            };

            return JsonSerializer.Serialize(backup, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成设置备份失败");
            throw;
        }
    }

    public async Task<SystemSettingsDto> RestoreFromBackupAsync(string backupData, string updatedBy)
    {
        try
        {
            var backup = JsonSerializer.Deserialize<Dictionary<string, object>>(backupData);
            if (backup == null || !backup.ContainsKey("Settings"))
            {
                throw new ArgumentException("备份数据格式不正确");
            }

            var settingsJson = JsonSerializer.Serialize(backup["Settings"]);
            return await ImportSettingsAsync(settingsJson, updatedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从备份恢复设置失败，操作人: {UpdatedBy}", updatedBy);
            throw;
        }
    }

    public List<string> GetSupportedExportFormats()
    {
        return SupportedFormats;
    }

    public string GetExportFileName(string format, bool includeTimestamp = true)
    {
        var timestamp = includeTimestamp ? $"_{DateTime.UtcNow:yyyyMMdd_HHmmss}" : "";
        return $"system_settings{timestamp}.{format.ToLower()}";
    }

    // 私有辅助方法
    private async Task<string> ExportToJsonAsync(SystemSettings settings)
    {
        var dto = MapToDto(settings);
        return JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> ExportToCsvAsync(SystemSettings settings)
    {
        var dto = MapToDto(settings);
        var csv = new StringBuilder();

        // CSV头部
        csv.AppendLine("Category,Key,Value");

        // 站点设置
        csv.AppendLine($"Site,SiteName,\"{dto.SiteSettings.SiteName}\"");
        csv.AppendLine($"Site,SiteDescription,\"{dto.SiteSettings.SiteDescription}\"");
        csv.AppendLine($"Site,LogoUrl,\"{dto.SiteSettings.LogoUrl}\"");
        csv.AppendLine($"Site,Theme,\"{dto.SiteSettings.Theme}\"");
        csv.AppendLine($"Site,Language,\"{dto.SiteSettings.Language}\"");
        csv.AppendLine($"Site,PageSize,\"{dto.SiteSettings.PageSize}\"");
        csv.AppendLine($"Site,AllowRegistration,\"{dto.SiteSettings.AllowRegistration}\"");

        // 安全设置
        csv.AppendLine($"Security,MinPasswordLength,\"{dto.SecuritySettings.MinPasswordLength}\"");
        csv.AppendLine($"Security,RequireUppercase,\"{dto.SecuritySettings.RequireUppercase}\"");
        csv.AppendLine($"Security,RequireLowercase,\"{dto.SecuritySettings.RequireLowercase}\"");
        csv.AppendLine($"Security,RequireNumbers,\"{dto.SecuritySettings.RequireNumbers}\"");
        csv.AppendLine($"Security,RequireSpecialChars,\"{dto.SecuritySettings.RequireSpecialChars}\"");

        // 功能设置
        csv.AppendLine($"Feature,EnableCodeSnippets,\"{dto.FeatureSettings.EnableCodeSnippets}\"");
        csv.AppendLine($"Feature,EnableSharing,\"{dto.FeatureSettings.EnableSharing}\"");
        csv.AppendLine($"Feature,EnableTags,\"{dto.FeatureSettings.EnableTags}\"");
        csv.AppendLine($"Feature,EnableComments,\"{dto.FeatureSettings.EnableComments}\"");

        // 邮件设置
        csv.AppendLine($"Email,SmtpHost,\"{dto.EmailSettings.SmtpHost}\"");
        csv.AppendLine($"Email,SmtpPort,\"{dto.EmailSettings.SmtpPort}\"");
        csv.AppendLine($"Email,FromEmail,\"{dto.EmailSettings.FromEmail}\"");
        csv.AppendLine($"Email,FromName,\"{dto.EmailSettings.FromName}\"");
        csv.AppendLine($"Email,EnableSsl,\"{dto.EmailSettings.EnableSsl}\"");

        return csv.ToString();
    }

    private async Task<string> ExportToExcelAsync(SystemSettings settings)
    {
        // 简化的Excel导出（CSV格式，可以扩展为真正的Excel格式）
        return await ExportToCsvAsync(settings);
    }

    private string ConvertHistoryToCsv(List<SettingsHistoryDto> items)
    {
        var csv = new StringBuilder();

        // CSV头部
        csv.AppendLine("ID,CreatedAt,SettingType,SettingKey,OldValue,NewValue,ChangedBy,ChangeCategory,IsImportant,Status");

        // 数据行
        foreach (var item in items)
        {
            csv.AppendLine($"\"{item.Id}\",\"{item.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{item.SettingType}\",\"{item.SettingKey}\",\"{item.OldValue}\",\"{item.NewValue}\",\"{item.ChangedBy}\",\"{item.ChangeCategory}\",\"{item.IsImportant}\",\"{item.Status}\"");
        }

        return csv.ToString();
    }

    private string GetHistoryExportFileName(string format)
    {
        return $"settings_history_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format.ToLower()}";
    }

    private void MergeValidationResult(Dictionary<string, string[]> target, Dictionary<string, string[]> source)
    {
        foreach (var kvp in source)
        {
            if (target.ContainsKey(kvp.Key))
            {
                target[kvp.Key] = target[kvp.Key].Concat(kvp.Value).ToArray();
            }
            else
            {
                target[kvp.Key] = kvp.Value;
            }
        }
    }

    private static SystemSettingsDto MapToDto(SystemSettings entity)
    {
        return new SystemSettingsDto
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            SiteSettings = MapSiteSettingsToDto(entity.GetSiteSettings()),
            SecuritySettings = MapSecuritySettingsToDto(entity.GetSecuritySettings()),
            FeatureSettings = MapFeatureSettingsToDto(entity.GetFeatureSettings()),
            EmailSettings = MapEmailSettingsToDto(entity.GetEmailSettings())
        };
    }

    private static SystemSettings MapToEntity(SystemSettingsDto dto)
    {
        var entity = new SystemSettings
        {
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            UpdatedBy = dto.UpdatedBy
        };

        entity.SetSiteSettings(MapSiteSettingsToEntity(dto.SiteSettings));
        entity.SetSecuritySettings(MapSecuritySettingsToEntity(dto.SecuritySettings));
        entity.SetFeatureSettings(MapFeatureSettingsToEntity(dto.FeatureSettings));
        entity.SetEmailSettings(MapEmailSettingsToEntity(dto.EmailSettings));

        return entity;
    }

    private static SiteSettingsDto MapSiteSettingsToDto(SiteSettings entity)
    {
        return new SiteSettingsDto
        {
            SiteName = entity.SiteName,
            SiteDescription = entity.SiteDescription,
            LogoUrl = entity.LogoUrl,
            Theme = entity.Theme,
            Language = entity.Language,
            PageSize = entity.PageSize,
            AllowRegistration = entity.AllowRegistration,
            Announcement = entity.Announcement,
            CustomCss = entity.CustomCss,
            CustomJs = entity.CustomJs
        };
    }

    private static SiteSettings MapSiteSettingsToEntity(SiteSettingsDto dto)
    {
        return new SiteSettings
        {
            SiteName = dto.SiteName,
            SiteDescription = dto.SiteDescription,
            LogoUrl = dto.LogoUrl,
            Theme = dto.Theme,
            Language = dto.Language,
            PageSize = dto.PageSize,
            AllowRegistration = dto.AllowRegistration,
            Announcement = dto.Announcement,
            CustomCss = dto.CustomCss,
            CustomJs = dto.CustomJs
        };
    }

    private static SecuritySettingsDto MapSecuritySettingsToDto(SecuritySettings entity)
    {
        return new SecuritySettingsDto
        {
            MinPasswordLength = entity.MinPasswordLength,
            RequireUppercase = entity.RequireUppercase,
            RequireLowercase = entity.RequireLowercase,
            RequireNumbers = entity.RequireNumbers,
            RequireSpecialChars = entity.RequireSpecialChars,
            MaxLoginAttempts = entity.MaxLoginAttempts,
            AccountLockoutDuration = entity.AccountLockoutDuration,
            SessionTimeout = entity.SessionTimeout,
            EnableTwoFactorAuth = entity.EnableTwoFactorAuth,
            EnableCors = entity.EnableCors,
            AllowedCorsOrigins = entity.AllowedCorsOrigins,
            EnableHttpsRedirection = entity.EnableHttpsRedirection,
            ApiRateLimit = entity.ApiRateLimit,
            EnableLoginLogging = entity.EnableLoginLogging,
            EnableActionLogging = entity.EnableActionLogging
        };
    }

    private static SecuritySettings MapSecuritySettingsToEntity(SecuritySettingsDto dto)
    {
        return new SecuritySettings
        {
            MinPasswordLength = dto.MinPasswordLength,
            RequireUppercase = dto.RequireUppercase,
            RequireLowercase = dto.RequireLowercase,
            RequireNumbers = dto.RequireNumbers,
            RequireSpecialChars = dto.RequireSpecialChars,
            MaxLoginAttempts = dto.MaxLoginAttempts,
            AccountLockoutDuration = dto.AccountLockoutDuration,
            SessionTimeout = dto.SessionTimeout,
            EnableTwoFactorAuth = dto.EnableTwoFactorAuth,
            EnableCors = dto.EnableCors,
            AllowedCorsOrigins = dto.AllowedCorsOrigins,
            EnableHttpsRedirection = dto.EnableHttpsRedirection,
            ApiRateLimit = dto.ApiRateLimit,
            EnableLoginLogging = dto.EnableLoginLogging,
            EnableActionLogging = dto.EnableActionLogging
        };
    }

    private static FeatureSettingsDto MapFeatureSettingsToDto(FeatureSettings entity)
    {
        return new FeatureSettingsDto
        {
            EnableCodeSnippets = entity.EnableCodeSnippets,
            EnableSharing = entity.EnableSharing,
            EnableTags = entity.EnableTags,
            EnableComments = entity.EnableComments,
            EnableFavorites = entity.EnableFavorites,
            EnableSearch = entity.EnableSearch,
            EnableExport = entity.EnableExport,
            EnableImport = entity.EnableImport,
            EnableApi = entity.EnableApi,
            EnableWebHooks = entity.EnableWebHooks,
            EnableFileUpload = entity.EnableFileUpload,
            MaxFileSize = entity.MaxFileSize,
            AllowedFileTypes = entity.AllowedFileTypes,
            EnableRealTimeNotifications = entity.EnableRealTimeNotifications,
            EnableAnalytics = entity.EnableAnalytics
        };
    }

    private static FeatureSettings MapFeatureSettingsToEntity(FeatureSettingsDto dto)
    {
        return new FeatureSettings
        {
            EnableCodeSnippets = dto.EnableCodeSnippets,
            EnableSharing = dto.EnableSharing,
            EnableTags = dto.EnableTags,
            EnableComments = dto.EnableComments,
            EnableFavorites = dto.EnableFavorites,
            EnableSearch = dto.EnableSearch,
            EnableExport = dto.EnableExport,
            EnableImport = dto.EnableImport,
            EnableApi = dto.EnableApi,
            EnableWebHooks = dto.EnableWebHooks,
            EnableFileUpload = dto.EnableFileUpload,
            MaxFileSize = dto.MaxFileSize,
            AllowedFileTypes = dto.AllowedFileTypes,
            EnableRealTimeNotifications = dto.EnableRealTimeNotifications,
            EnableAnalytics = dto.EnableAnalytics
        };
    }

    private static EmailSettingsDto MapEmailSettingsToDto(EmailSettings entity)
    {
        return new EmailSettingsDto
        {
            SmtpHost = entity.SmtpHost,
            SmtpPort = entity.SmtpPort,
            SmtpUsername = entity.SmtpUsername,
            SmtpPassword = entity.SmtpPassword,
            FromEmail = entity.FromEmail,
            FromName = entity.FromName,
            EnableSsl = entity.EnableSsl,
            EnableTls = entity.EnableTls,
            TemplatePath = entity.TemplatePath,
            EnableEmailQueue = entity.EnableEmailQueue,
            MaxRetryAttempts = entity.MaxRetryAttempts,
            EmailTimeout = entity.EmailTimeout,
            EnableEmailLogging = entity.EnableEmailLogging,
            TestEmailRecipient = entity.TestEmailRecipient
        };
    }

    private static EmailSettings MapEmailSettingsToEntity(EmailSettingsDto dto)
    {
        return new EmailSettings
        {
            SmtpHost = dto.SmtpHost,
            SmtpPort = dto.SmtpPort,
            SmtpUsername = dto.SmtpUsername,
            SmtpPassword = dto.SmtpPassword,
            FromEmail = dto.FromEmail,
            FromName = dto.FromName,
            EnableSsl = dto.EnableSsl,
            EnableTls = dto.EnableTls,
            TemplatePath = dto.TemplatePath,
            EnableEmailQueue = dto.EnableEmailQueue,
            MaxRetryAttempts = dto.MaxRetryAttempts,
            EmailTimeout = dto.EmailTimeout,
            EnableEmailLogging = dto.EnableEmailLogging,
            TestEmailRecipient = dto.TestEmailRecipient
        };
    }
}