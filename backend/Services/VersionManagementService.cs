using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 版本管理服务实现 - 遵循开闭原则
/// </summary>
public class VersionManagementService : IVersionManagementService
{
    private readonly ISnippetVersionRepository _versionRepository;
    private readonly ICodeSnippetRepository _snippetRepository;

    public VersionManagementService(
        ISnippetVersionRepository versionRepository,
        ICodeSnippetRepository snippetRepository)
    {
        _versionRepository = versionRepository ?? throw new ArgumentNullException(nameof(versionRepository));
        _snippetRepository = snippetRepository ?? throw new ArgumentNullException(nameof(snippetRepository));
    }

    /// <summary>
    /// 创建代码片段的初始版本
    /// </summary>
    public async Task<SnippetVersionDto> CreateInitialVersionAsync(Guid snippetId)
    {
        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        if (snippet == null)
        {
            throw new ArgumentException($"代码片段 {snippetId} 不存在", nameof(snippetId));
        }

        var version = new SnippetVersion
        {
            Id = Guid.NewGuid(),
            SnippetId = snippetId,
            VersionNumber = 1,
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = snippet.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            ChangeDescription = "初始版本"
        };

        var createdVersion = await _versionRepository.CreateAsync(version);
        return MapToDto(createdVersion);
    }

    /// <summary>
    /// 创建新版本
    /// </summary>
    public async Task<SnippetVersionDto> CreateVersionAsync(Guid snippetId, string changeDescription)
    {
        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        if (snippet == null)
        {
            throw new ArgumentException($"代码片段 {snippetId} 不存在", nameof(snippetId));
        }

        var nextVersionNumber = await _versionRepository.GetNextVersionNumberAsync(snippetId);

        var version = new SnippetVersion
        {
            Id = Guid.NewGuid(),
            SnippetId = snippetId,
            VersionNumber = nextVersionNumber,
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = snippet.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            ChangeDescription = changeDescription ?? "版本更新"
        };

        var createdVersion = await _versionRepository.CreateAsync(version);
        return MapToDto(createdVersion);
    }

    /// <summary>
    /// 获取代码片段的版本历史
    /// </summary>
    public async Task<IEnumerable<SnippetVersionDto>> GetVersionHistoryAsync(Guid snippetId)
    {
        var versions = await _versionRepository.GetBySnippetIdAsync(snippetId);
        return versions.Select(MapToDto).OrderByDescending(v => v.VersionNumber);
    }

    /// <summary>
    /// 获取特定版本详情
    /// </summary>
    public async Task<SnippetVersionDto?> GetVersionAsync(Guid versionId)
    {
        var version = await _versionRepository.GetByIdAsync(versionId);
        return version != null ? MapToDto(version) : null;
    }

    /// <summary>
    /// 恢复到指定版本
    /// </summary>
    public async Task<bool> RestoreVersionAsync(Guid snippetId, Guid versionId)
    {
        var version = await _versionRepository.GetByIdAsync(versionId);
        if (version == null || version.SnippetId != snippetId)
        {
            return false;
        }

        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        if (snippet == null)
        {
            return false;
        }

        // 在恢复之前创建当前版本的备份
        await CreateVersionAsync(snippetId, $"恢复到版本 {version.VersionNumber} 前的备份");

        // 恢复代码片段内容
        snippet.Title = version.Title;
        snippet.Description = version.Description;
        snippet.Code = version.Code;
        snippet.Language = version.Language;
        snippet.UpdatedAt = DateTime.UtcNow;

        await _snippetRepository.UpdateAsync(snippet);

        // 创建恢复版本记录
        await CreateVersionAsync(snippetId, $"恢复到版本 {version.VersionNumber}");

        return true;
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    private static SnippetVersionDto MapToDto(SnippetVersion version)
    {
        return new SnippetVersionDto
        {
            Id = version.Id,
            SnippetId = version.SnippetId,
            VersionNumber = version.VersionNumber,
            Title = version.Title,
            Description = version.Description,
            Code = version.Code,
            Language = version.Language,
            CreatedBy = version.CreatedBy,
            CreatedAt = version.CreatedAt,
            ChangeDescription = version.ChangeDescription
        };
    }
}