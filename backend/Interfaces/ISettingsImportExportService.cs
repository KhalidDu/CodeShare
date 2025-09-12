using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 设置导入导出服务接口 - 遵循接口隔离原则
/// </summary>
public interface ISettingsImportExportService
{
    /// <summary>
    /// 导出系统设置为指定格式
    /// </summary>
    /// <param name="format">导出格式 (json, csv, excel)</param>
    /// <returns>导出的数据字符串</returns>
    Task<string> ExportSettingsAsync(string format = "json");

    /// <summary>
    /// 导入系统设置
    /// </summary>
    /// <param name="jsonData">JSON格式的设置数据</param>
    /// <param name="updatedBy">操作人</param>
    /// <returns>导入后的设置</returns>
    Task<SystemSettingsDto> ImportSettingsAsync(string jsonData, string updatedBy);

    /// <summary>
    /// 验证导入数据
    /// </summary>
    /// <param name="jsonData">JSON格式的设置数据</param>
    /// <returns>验证结果</returns>
    Task<SettingsValidationResult> ValidateImportDataAsync(string jsonData);

    /// <summary>
    /// 导出设置变更历史
    /// </summary>
    /// <param name="request">导出请求</param>
    /// <returns>导出响应</returns>
    Task<SettingsHistoryExportResponse> ExportSettingsHistoryAsync(SettingsHistoryExportRequest request);

    /// <summary>
    /// 生成设置备份
    /// </summary>
    /// <param name="includeHistory">是否包含历史记录</param>
    /// <returns>备份数据</returns>
    Task<string> GenerateBackupAsync(bool includeHistory = true);

    /// <summary>
    /// 从备份恢复设置
    /// </summary>
    /// <param name="backupData">备份数据</param>
    /// <param name="updatedBy">操作人</param>
    /// <returns>恢复后的设置</returns>
    Task<SystemSettingsDto> RestoreFromBackupAsync(string backupData, string updatedBy);

    /// <summary>
    /// 获取可用的导出格式
    /// </summary>
    /// <returns>支持的导出格式列表</returns>
    List<string> GetSupportedExportFormats();

    /// <summary>
    /// 获取导出文件名
    /// </summary>
    /// <param name="format">导出格式</param>
    /// <param name="includeTimestamp">是否包含时间戳</param>
    /// <returns>文件名</returns>
    string GetExportFileName(string format, bool includeTimestamp = true);
}