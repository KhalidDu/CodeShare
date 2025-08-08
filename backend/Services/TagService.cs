using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 标签业务服务实现 - 遵循迪米特法则和单一职责原则
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _repository;
    private readonly IInputValidationService _validationService;

    public TagService(ITagRepository repository, IInputValidationService validationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <summary>
    /// 获取所有标签
    /// </summary>
    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _repository.GetAllAsync();
        return tags.Select(MapToDto);
    }

    /// <summary>
    /// 根据ID获取标签
    /// </summary>
    public async Task<TagDto?> GetTagAsync(Guid id)
    {
        var tag = await _repository.GetByIdAsync(id);
        return tag != null ? MapToDto(tag) : null;
    }

    /// <summary>
    /// 创建新标签，包含业务规则验证
    /// </summary>
    public async Task<TagDto> CreateTagAsync(CreateTagDto tag)
    {
        // 验证输入
        var nameValidation = _validationService.ValidateTagName(tag.Name);
        if (!nameValidation.IsValid)
        {
            throw new ArgumentException(nameValidation.ErrorMessage);
        }

        // 清理输入
        var sanitizedName = _validationService.SanitizeUserInput(tag.Name);

        // 业务规则：检查标签名称是否已存在
        var existingTag = await _repository.GetByNameAsync(sanitizedName);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"标签 '{sanitizedName}' 已存在");
        }

        // 业务规则：验证颜色格式
        if (!IsValidHexColor(tag.Color))
        {
            throw new ArgumentException("颜色格式无效，请使用十六进制格式（如：#007bff）");
        }

        var entity = new Tag
        {
            Id = Guid.NewGuid(),
            Name = sanitizedName,
            Color = tag.Color,
            CreatedBy = Guid.NewGuid(), // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    /// <summary>
    /// 更新标签信息，包含业务规则验证
    /// </summary>
    public async Task<TagDto> UpdateTagAsync(Guid id, UpdateTagDto tag)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new ArgumentException("标签不存在");

        // 业务规则：如果更新名称，检查是否与其他标签重复
        if (!string.IsNullOrEmpty(tag.Name) && tag.Name != existing.Name)
        {
            // 验证输入
            var nameValidation = _validationService.ValidateTagName(tag.Name);
            if (!nameValidation.IsValid)
            {
                throw new ArgumentException(nameValidation.ErrorMessage);
            }

            // 清理输入
            var sanitizedName = _validationService.SanitizeUserInput(tag.Name);

            var duplicateTag = await _repository.GetByNameAsync(sanitizedName);
            if (duplicateTag != null)
            {
                throw new InvalidOperationException($"标签 '{sanitizedName}' 已存在");
            }
            
            existing.Name = sanitizedName;
        }

        // 业务规则：验证颜色格式
        if (!string.IsNullOrEmpty(tag.Color))
        {
            if (!IsValidHexColor(tag.Color))
            {
                throw new ArgumentException("颜色格式无效，请使用十六进制格式（如：#007bff）");
            }
            existing.Color = tag.Color;
        }

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    /// <summary>
    /// 删除标签，包含业务规则验证
    /// </summary>
    public async Task<bool> DeleteTagAsync(Guid id)
    {
        // 业务规则：检查标签是否正在被使用
        var isInUse = await _repository.IsTagInUseAsync(id);
        if (isInUse)
        {
            throw new InvalidOperationException("无法删除正在使用的标签，请先移除相关代码片段的标签关联");
        }

        return await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// 获取代码片段关联的标签
    /// </summary>
    public async Task<IEnumerable<TagDto>> GetTagsBySnippetIdAsync(Guid snippetId)
    {
        var tags = await _repository.GetTagsBySnippetIdAsync(snippetId);
        return tags.Select(MapToDto);
    }

    /// <summary>
    /// 搜索标签用于自动补全功能
    /// 确保子类可替换父类，遵循里氏替换原则
    /// </summary>
    public async Task<IEnumerable<TagDto>> SearchTagsAsync(string prefix, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Enumerable.Empty<TagDto>();
        }

        // 业务规则：限制搜索结果数量，防止性能问题
        if (limit > 50)
        {
            limit = 50;
        }

        var tags = await _repository.SearchTagsByPrefixAsync(prefix.Trim(), limit);
        return tags.Select(MapToDto);
    }

    /// <summary>
    /// 获取标签使用统计
    /// </summary>
    public async Task<IEnumerable<TagUsageDto>> GetTagUsageStatisticsAsync()
    {
        var statistics = await _repository.GetTagUsageStatisticsAsync();
        return statistics.Select(MapToUsageDto);
    }

    /// <summary>
    /// 获取最常用的标签
    /// </summary>
    public async Task<IEnumerable<TagDto>> GetMostUsedTagsAsync(int limit = 20)
    {
        // 业务规则：限制返回结果数量
        if (limit > 100)
        {
            limit = 100;
        }

        var tags = await _repository.GetMostUsedTagsAsync(limit);
        return tags.Select(MapToDto);
    }

    /// <summary>
    /// 验证标签是否可以删除
    /// </summary>
    public async Task<bool> CanDeleteTagAsync(Guid tagId)
    {
        return !await _repository.IsTagInUseAsync(tagId);
    }

    /// <summary>
    /// 验证十六进制颜色格式
    /// </summary>
    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
    }

    /// <summary>
    /// 将标签实体映射为DTO
    /// </summary>
    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt
        };
    }

    /// <summary>
    /// 将标签使用统计映射为DTO
    /// </summary>
    private static TagUsageDto MapToUsageDto(TagUsageStatistic statistic)
    {
        return new TagUsageDto
        {
            TagId = statistic.TagId,
            TagName = statistic.TagName,
            TagColor = statistic.TagColor,
            UsageCount = statistic.UsageCount,
            CreatedAt = statistic.CreatedAt
        };
    }
}