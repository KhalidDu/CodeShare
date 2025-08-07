using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using System.Data;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 版本管理服务实现 - 遵循开闭原则
/// </summary>
public class VersionManagementService : IVersionManagementService
{
    private readonly ISnippetVersionRepository _versionRepository;
    private readonly ICodeSnippetRepository _snippetRepository;
    private readonly IDbConnectionFactory _connectionFactory;

    public VersionManagementService(
        ISnippetVersionRepository versionRepository,
        ICodeSnippetRepository snippetRepository,
        IDbConnectionFactory connectionFactory)
    {
        _versionRepository = versionRepository ?? throw new ArgumentNullException(nameof(versionRepository));
        _snippetRepository = snippetRepository ?? throw new ArgumentNullException(nameof(snippetRepository));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
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
    /// 恢复到指定版本 - 使用事务确保数据一致性
    /// </summary>
    public async Task<bool> RestoreVersionAsync(Guid snippetId, Guid versionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
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
            await CreateVersionWithTransactionAsync(snippetId, $"恢复到版本 {version.VersionNumber} 前的备份", transaction);

            // 恢复代码片段内容
            snippet.Title = version.Title;
            snippet.Description = version.Description;
            snippet.Code = version.Code;
            snippet.Language = version.Language;
            snippet.UpdatedAt = DateTime.UtcNow;

            await _snippetRepository.UpdateAsync(snippet);

            // 创建恢复版本记录
            await CreateVersionWithTransactionAsync(snippetId, $"恢复到版本 {version.VersionNumber}", transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 比较两个版本的差异
    /// </summary>
    public async Task<VersionComparisonDto?> CompareVersionsAsync(Guid fromVersionId, Guid toVersionId)
    {
        var fromVersion = await _versionRepository.GetByIdAsync(fromVersionId);
        var toVersion = await _versionRepository.GetByIdAsync(toVersionId);

        if (fromVersion == null || toVersion == null)
        {
            return null;
        }

        // 确保两个版本属于同一个代码片段
        if (fromVersion.SnippetId != toVersion.SnippetId)
        {
            throw new ArgumentException("比较的版本必须属于同一个代码片段");
        }

        var comparison = new VersionComparisonDto
        {
            FromVersion = MapToDto(fromVersion),
            ToVersion = MapToDto(toVersion),
            TitleChanged = fromVersion.Title != toVersion.Title,
            DescriptionChanged = fromVersion.Description != toVersion.Description,
            CodeChanged = fromVersion.Code != toVersion.Code,
            LanguageChanged = fromVersion.Language != toVersion.Language,
            CodeDifferences = GenerateCodeDifferences(fromVersion.Code, toVersion.Code)
        };

        return comparison;
    }

    /// <summary>
    /// 在事务中创建版本 - 确保数据一致性
    /// </summary>
    public async Task<SnippetVersionDto> CreateVersionWithTransactionAsync(Guid snippetId, string changeDescription, IDbTransaction? transaction = null)
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

        SnippetVersion createdVersion;
        if (transaction != null)
        {
            createdVersion = await _versionRepository.CreateWithTransactionAsync(version, transaction);
        }
        else
        {
            createdVersion = await _versionRepository.CreateAsync(version);
        }

        return MapToDto(createdVersion);
    }

    /// <summary>
    /// 生成代码差异 - 简单的行级比较
    /// </summary>
    private static List<CodeDiffLine> GenerateCodeDifferences(string fromCode, string toCode)
    {
        var differences = new List<CodeDiffLine>();
        var fromLines = fromCode.Split('\n');
        var toLines = toCode.Split('\n');

        var maxLines = Math.Max(fromLines.Length, toLines.Length);

        for (int i = 0; i < maxLines; i++)
        {
            var fromLine = i < fromLines.Length ? fromLines[i] : null;
            var toLine = i < toLines.Length ? toLines[i] : null;

            if (fromLine == null && toLine != null)
            {
                // 新增行
                differences.Add(new CodeDiffLine
                {
                    LineNumber = i + 1,
                    DiffType = "Added",
                    FromContent = null,
                    ToContent = toLine
                });
            }
            else if (fromLine != null && toLine == null)
            {
                // 删除行
                differences.Add(new CodeDiffLine
                {
                    LineNumber = i + 1,
                    DiffType = "Removed",
                    FromContent = fromLine,
                    ToContent = null
                });
            }
            else if (fromLine != null && toLine != null)
            {
                if (fromLine != toLine)
                {
                    // 修改行
                    differences.Add(new CodeDiffLine
                    {
                        LineNumber = i + 1,
                        DiffType = "Modified",
                        FromContent = fromLine,
                        ToContent = toLine
                    });
                }
                else
                {
                    // 未变化的行 (可选择性包含)
                    differences.Add(new CodeDiffLine
                    {
                        LineNumber = i + 1,
                        DiffType = "Unchanged",
                        FromContent = fromLine,
                        ToContent = toLine
                    });
                }
            }
        }

        return differences;
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