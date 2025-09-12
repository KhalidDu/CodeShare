using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 系统设置管理控制器 - 遵循RESTful API设计原则
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;
    private readonly ISettingsHistoryService _historyService;
    private readonly ISettingsImportExportService _importExportService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        ISystemSettingsService settingsService,
        ISettingsHistoryService historyService,
        ISettingsImportExportService importExportService,
        ILogger<SystemSettingsController> logger)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _importExportService = importExportService ?? throw new ArgumentNullException(nameof(importExportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础设置操作

    /// <summary>
    /// 获取所有系统设置
    /// </summary>
    /// <returns>系统设置DTO</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemSettingsDto>> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            if (settings == null)
            {
                return NotFound("系统设置未初始化");
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取系统设置时发生错误");
        }
    }

    /// <summary>
    /// 获取或创建系统设置
    /// </summary>
    /// <returns>系统设置DTO</returns>
    [HttpGet("get-or-create")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemSettingsDto>> GetOrCreateSettings()
    {
        try
        {
            var settings = await _settingsService.GetOrCreateSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取或创建系统设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取或创建系统设置时发生错误");
        }
    }

    /// <summary>
    /// 更新所有系统设置
    /// </summary>
    /// <param name="settings">系统设置DTO</param>
    /// <returns>更新后的系统设置</returns>
    [HttpPut]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemSettingsDto>> UpdateSettings([FromBody] SystemSettingsDto settings)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedSettings = await _settingsService.UpdateSettingsAsync(settings);
            return Ok(updatedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新系统设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新系统设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "更新系统设置时发生错误");
        }
    }

    /// <summary>
    /// 初始化默认系统设置
    /// </summary>
    /// <returns>初始化后的系统设置</returns>
    [HttpPost("initialize")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemSettingsDto>> InitializeDefaultSettings()
    {
        try
        {
            var isInitialized = await _settingsService.IsSettingsInitializedAsync();
            if (isInitialized)
            {
                return BadRequest("系统设置已经初始化");
            }

            var settings = await _settingsService.InitializeDefaultSettingsAsync();
            return CreatedAtAction(nameof(GetSettings), null, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化系统设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "初始化系统设置时发生错误");
        }
    }

    /// <summary>
    /// 检查系统设置是否已初始化
    /// </summary>
    /// <returns>初始化状态</returns>
    [HttpGet("check-initialization")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> CheckInitialization()
    {
        try
        {
            var isInitialized = await _settingsService.IsSettingsInitializedAsync();
            return Ok(isInitialized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查系统设置初始化状态失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "检查系统设置初始化状态时发生错误");
        }
    }

    /// <summary>
    /// 刷新设置缓存
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPost("refresh-cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> RefreshCache()
    {
        try
        {
            await _settingsService.RefreshCacheAsync();
            return Ok(new { message = "设置缓存已刷新" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新设置缓存失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "刷新设置缓存时发生错误");
        }
    }

    #endregion

    #region 站点设置

    /// <summary>
    /// 获取站点设置
    /// </summary>
    /// <returns>站点设置DTO</returns>
    [HttpGet("site")]
    [ProducesResponseType(typeof(SiteSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteSettingsDto>> GetSiteSettings()
    {
        try
        {
            var settings = await _settingsService.GetSiteSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取站点设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取站点设置时发生错误");
        }
    }

    /// <summary>
    /// 更新站点设置
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的站点设置</returns>
    [HttpPut("site")]
    [ProducesResponseType(typeof(SiteSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteSettingsDto>> UpdateSiteSettings([FromBody] UpdateSiteSettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = User.Identity?.Name ?? "Admin";
            var updatedSettings = await _settingsService.UpdateSiteSettingsAsync(request, updatedBy);
            return Ok(updatedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新站点设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新站点设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "更新站点设置时发生错误");
        }
    }

    /// <summary>
    /// 验证站点设置
    /// </summary>
    /// <param name="settings">站点设置DTO</param>
    /// <returns>验证结果</returns>
    [HttpPost("site/validate")]
    [ProducesResponseType(typeof(SettingsValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsValidationResult>> ValidateSiteSettings([FromBody] SiteSettingsDto settings)
    {
        try
        {
            var result = await _settingsService.ValidateSiteSettingsAsync(settings);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证站点设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "验证站点设置时发生错误");
        }
    }

    #endregion

    #region 安全设置

    /// <summary>
    /// 获取安全设置
    /// </summary>
    /// <returns>安全设置DTO</returns>
    [HttpGet("security")]
    [ProducesResponseType(typeof(SecuritySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SecuritySettingsDto>> GetSecuritySettings()
    {
        try
        {
            var settings = await _settingsService.GetSecuritySettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取安全设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取安全设置时发生错误");
        }
    }

    /// <summary>
    /// 更新安全设置
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的安全设置</returns>
    [HttpPut("security")]
    [ProducesResponseType(typeof(SecuritySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SecuritySettingsDto>> UpdateSecuritySettings([FromBody] UpdateSecuritySettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = User.Identity?.Name ?? "Admin";
            var updatedSettings = await _settingsService.UpdateSecuritySettingsAsync(request, updatedBy);
            return Ok(updatedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新安全设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新安全设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "更新安全设置时发生错误");
        }
    }

    /// <summary>
    /// 验证安全设置
    /// </summary>
    /// <param name="settings">安全设置DTO</param>
    /// <returns>验证结果</returns>
    [HttpPost("security/validate")]
    [ProducesResponseType(typeof(SettingsValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsValidationResult>> ValidateSecuritySettings([FromBody] SecuritySettingsDto settings)
    {
        try
        {
            var result = await _settingsService.ValidateSecuritySettingsAsync(settings);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证安全设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "验证安全设置时发生错误");
        }
    }

    #endregion

    #region 功能设置

    /// <summary>
    /// 获取功能设置
    /// </summary>
    /// <returns>功能设置DTO</returns>
    [HttpGet("features")]
    [ProducesResponseType(typeof(FeatureSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeatureSettingsDto>> GetFeatureSettings()
    {
        try
        {
            var settings = await _settingsService.GetFeatureSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取功能设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取功能设置时发生错误");
        }
    }

    /// <summary>
    /// 更新功能设置
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的功能设置</returns>
    [HttpPut("features")]
    [ProducesResponseType(typeof(FeatureSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeatureSettingsDto>> UpdateFeatureSettings([FromBody] UpdateFeatureSettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = User.Identity?.Name ?? "Admin";
            var updatedSettings = await _settingsService.UpdateFeatureSettingsAsync(request, updatedBy);
            return Ok(updatedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新功能设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新功能设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "更新功能设置时发生错误");
        }
    }

    /// <summary>
    /// 验证功能设置
    /// </summary>
    /// <param name="settings">功能设置DTO</param>
    /// <returns>验证结果</returns>
    [HttpPost("features/validate")]
    [ProducesResponseType(typeof(SettingsValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsValidationResult>> ValidateFeatureSettings([FromBody] FeatureSettingsDto settings)
    {
        try
        {
            var result = await _settingsService.ValidateFeatureSettingsAsync(settings);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证功能设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "验证功能设置时发生错误");
        }
    }

    #endregion

    #region 邮件设置

    /// <summary>
    /// 获取邮件设置
    /// </summary>
    /// <returns>邮件设置DTO</returns>
    [HttpGet("email")]
    [ProducesResponseType(typeof(EmailSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmailSettingsDto>> GetEmailSettings()
    {
        try
        {
            var settings = await _settingsService.GetEmailSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取邮件设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取邮件设置时发生错误");
        }
    }

    /// <summary>
    /// 更新邮件设置
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的邮件设置</returns>
    [HttpPut("email")]
    [ProducesResponseType(typeof(EmailSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmailSettingsDto>> UpdateEmailSettings([FromBody] UpdateEmailSettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = User.Identity?.Name ?? "Admin";
            var updatedSettings = await _settingsService.UpdateEmailSettingsAsync(request, updatedBy);
            return Ok(updatedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新邮件设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新邮件设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "更新邮件设置时发生错误");
        }
    }

    /// <summary>
    /// 验证邮件设置
    /// </summary>
    /// <param name="settings">邮件设置DTO</param>
    /// <returns>验证结果</returns>
    [HttpPost("email/validate")]
    [ProducesResponseType(typeof(SettingsValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsValidationResult>> ValidateEmailSettings([FromBody] EmailSettingsDto settings)
    {
        try
        {
            var result = await _settingsService.ValidateEmailSettingsAsync(settings);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证邮件设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "验证邮件设置时发生错误");
        }
    }

    /// <summary>
    /// 发送测试邮件
    /// </summary>
    /// <param name="request">测试邮件请求</param>
    /// <returns>发送结果</returns>
    [HttpPost("email/test")]
    [ProducesResponseType(typeof(TestEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TestEmailResponse>> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _settingsService.SendTestEmailAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送测试邮件失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "发送测试邮件时发生错误");
        }
    }

    #endregion

    #region 设置历史

    /// <summary>
    /// 获取设置变更历史
    /// </summary>
    /// <param name="request">查询请求</param>
    /// <returns>历史记录响应</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(SettingsHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsHistoryResponse>> GetSettingsHistory([FromQuery] SettingsHistoryRequest request)
    {
        try
        {
            var response = await _settingsService.GetSettingsHistoryAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置变更历史失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取设置变更历史时发生错误");
        }
    }

    /// <summary>
    /// 获取设置变更历史统计
    /// </summary>
    /// <returns>统计信息</returns>
    [HttpGet("history/statistics")]
    [ProducesResponseType(typeof(SettingsHistoryStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsHistoryStatistics>> GetSettingsHistoryStatistics()
    {
        try
        {
            var statistics = await _settingsService.GetSettingsHistoryStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置变更历史统计失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "获取设置变更历史统计时发生错误");
        }
    }

    /// <summary>
    /// 删除设置变更历史记录
    /// </summary>
    /// <param name="historyId">历史记录ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("history/{historyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteSettingsHistory(Guid historyId)
    {
        try
        {
            var deleted = await _settingsService.DeleteSettingsHistoryAsync(historyId);
            if (!deleted)
            {
                return NotFound("指定的历史记录不存在");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除设置变更历史失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "删除设置变更历史时发生错误");
        }
    }

    /// <summary>
    /// 批量删除设置变更历史
    /// </summary>
    /// <param name="request">批量删除请求</param>
    /// <returns>删除数量</returns>
    [HttpDelete("history/batch")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> BatchDeleteSettingsHistory([FromBody] BatchDeleteSettingsHistoryRequest request)
    {
        try
        {
            if (request?.HistoryIds == null || !request.HistoryIds.Any())
            {
                return BadRequest("请提供要删除的历史记录ID列表");
            }

            var deletedCount = await _settingsService.BatchDeleteSettingsHistoryAsync(request.HistoryIds);
            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除设置变更历史失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "批量删除设置变更历史时发生错误");
        }
    }

    #endregion

    #region 导入导出

    /// <summary>
    /// 导出系统设置
    /// </summary>
    /// <param name="format">导出格式 (json, csv, excel)</param>
    /// <returns>导出的设置数据</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> ExportSettings([FromQuery] string format = "json")
    {
        try
        {
            var supportedFormats = _importExportService.GetSupportedExportFormats();
            if (!supportedFormats.Contains(format.ToLower()))
            {
                return BadRequest($"不支持的导出格式。支持的格式: {string.Join(", ", supportedFormats)}");
            }

            var exportedData = await _importExportService.ExportSettingsAsync(format);
            var fileName = _importExportService.GetExportFileName(format);

            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return Ok(exportedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出系统设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "导出系统设置时发生错误");
        }
    }

    /// <summary>
    /// 导入系统设置
    /// </summary>
    /// <param name="request">导入请求</param>
    /// <returns>导入后的设置</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemSettingsDto>> ImportSettings([FromBody] ImportSettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = User.Identity?.Name ?? "Admin";
            var importedSettings = await _importExportService.ImportSettingsAsync(request.JsonData, updatedBy);
            return Ok(importedSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "导入系统设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入系统设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "导入系统设置时发生错误");
        }
    }

    /// <summary>
    /// 验证导入数据
    /// </summary>
    /// <param name="request">验证请求</param>
    /// <returns>验证结果</returns>
    [HttpPost("import/validate")]
    [ProducesResponseType(typeof(SettingsValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsValidationResult>> ValidateImportData([FromBody] ValidateImportDataRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _importExportService.ValidateImportDataAsync(request.JsonData);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证导入数据失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "验证导入数据时发生错误");
        }
    }

    /// <summary>
    /// 导出设置变更历史
    /// </summary>
    /// <param name="request">导出请求</param>
    /// <returns>导出响应</returns>
    [HttpGet("history/export")]
    [ProducesResponseType(typeof(SettingsHistoryExportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsHistoryExportResponse>> ExportSettingsHistory([FromQuery] SettingsHistoryExportRequest request)
    {
        try
        {
            var response = await _importExportService.ExportSettingsHistoryAsync(request);
            
            if (!response.Success)
            {
                return BadRequest(response.ErrorMessage);
            }

            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{response.FileName}\"");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出设置变更历史失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "导出设置变更历史时发生错误");
        }
    }

    /// <summary>
    /// 生成设置备份
    /// </summary>
    /// <param name="includeHistory">是否包含历史记录</param>
    /// <returns>备份数据</returns>
    [HttpGet("backup")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> GenerateBackup([FromQuery] bool includeHistory = true)
    {
        try
        {
            var backupData = await _importExportService.GenerateBackupAsync(includeHistory);
            var fileName = $"settings_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return Ok(backupData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成设置备份失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "生成设置备份时发生错误");
        }
    }

    /// <summary>
    /// 从备份恢复设置
    /// </summary>
    /// <param name="request">恢复请求</param>
    /// <returns>恢复后的设置</returns>
    [HttpPost("restore")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemSettingsDto>> RestoreFromBackup([FromBody] RestoreFromBackupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = User.Identity?.Name ?? "Admin";
            var restoredSettings = await _importExportService.RestoreFromBackupAsync(request.BackupData, updatedBy);
            return Ok(restoredSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "从备份恢复设置失败：参数验证失败");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从备份恢复设置失败");
            return StatusCode(StatusCodes.Status500InternalServerError, "从备份恢复设置时发生错误");
        }
    }

    #endregion
}