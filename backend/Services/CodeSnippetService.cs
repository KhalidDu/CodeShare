using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 代码片段业务服务实现 - 遵循依赖倒置原则
/// </summary>
public class CodeSnippetService : ICodeSnippetService
{
    private readonly ICodeSnippetRepository _repository;
    private readonly IPermissionService _permissionService;
    private readonly ITagRepository _tagRepository;
    private readonly IVersionManagementService _versionManagementService;

    public CodeSnippetService(
        ICodeSnippetRepository repository,
        IPermissionService permissionService,
        ITagRepository tagRepository,
        IVersionManagementService versionManagementService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _versionManagementService = versionManagementService ?? throw new ArgumentNullException(nameof(versionManagementService));
    }

    /// <summary>
    /// 获取代码片段列表 - 根据用户权限过滤结果
    /// </summary>
    public async Task<PaginatedResult<CodeSnippetDto>> GetSnippetsAsync(SnippetFilterDto filter, Guid? currentUserId = null)
    {
        var domainFilter = new SnippetFilter
        {
            Search = filter.Search,
            Language = filter.Language,
            Tag = filter.Tag,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        var result = await _repository.GetPagedAsync(domainFilter);
        
        // 过滤用户有权限访问的代码片段
        var filteredItems = new List<CodeSnippetDto>();
        foreach (var snippet in result.Items)
        {
            if (currentUserId.HasValue)
            {
                var canAccess = await _permissionService.CanAccessSnippetAsync(
                    currentUserId.Value, snippet.Id, PermissionOperation.Read);
                if (canAccess)
                {
                    filteredItems.Add(MapToDto(snippet));
                }
            }
            else if (snippet.IsPublic)
            {
                // 未登录用户只能看到公开的代码片段
                filteredItems.Add(MapToDto(snippet));
            }
        }
        
        return new PaginatedResult<CodeSnippetDto>
        {
            Items = filteredItems,
            TotalCount = filteredItems.Count, // 注意：这里应该是过滤后的总数
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    /// <summary>
    /// 获取单个代码片段详情 - 包含权限验证
    /// </summary>
    public async Task<CodeSnippetDto?> GetSnippetAsync(Guid id, Guid? currentUserId = null)
    {
        var snippet = await _repository.GetByIdWithTagsAsync(id);
        if (snippet == null)
        {
            return null;
        }

        // 检查访问权限
        if (currentUserId.HasValue)
        {
            var canAccess = await _permissionService.CanAccessSnippetAsync(
                currentUserId.Value, id, PermissionOperation.Read);
            if (!canAccess)
            {
                return null;
            }
        }
        else if (!snippet.IsPublic)
        {
            // 未登录用户只能访问公开的代码片段
            return null;
        }

        return MapToDto(snippet);
    }

    /// <summary>
    /// 创建代码片段 - 包含权限验证和标签处理
    /// </summary>
    public async Task<CodeSnippetDto> CreateSnippetAsync(CreateSnippetDto snippet, Guid currentUserId)
    {
        // 检查创建权限
        var canCreate = await _permissionService.CanCreateSnippetAsync(currentUserId);
        if (!canCreate)
        {
            throw new UnauthorizedAccessException("用户没有创建代码片段的权限");
        }

        var entity = new CodeSnippet
        {
            Id = Guid.NewGuid(),
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublic = snippet.IsPublic,
            ViewCount = 0,
            CopyCount = 0
        };

        var created = await _repository.CreateAsync(entity);

        // 创建初始版本
        await _versionManagementService.CreateInitialVersionAsync(created.Id);

        // 处理标签
        if (snippet.Tags.Any())
        {
            var tagIds = await ProcessTagsAsync(snippet.Tags, currentUserId);
            await _repository.UpdateSnippetTagsAsync(created.Id, tagIds);
            
            // 重新获取包含标签的代码片段
            created = await _repository.GetByIdWithTagsAsync(created.Id);
        }

        return MapToDto(created!);
    }

    /// <summary>
    /// 更新代码片段 - 包含权限验证和标签处理
    /// </summary>
    public async Task<CodeSnippetDto> UpdateSnippetAsync(Guid id, UpdateSnippetDto snippet, Guid currentUserId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new ArgumentException("代码片段不存在");
        }

        // 检查编辑权限
        var canEdit = await _permissionService.CanAccessSnippetAsync(
            currentUserId, id, PermissionOperation.Edit);
        if (!canEdit)
        {
            throw new UnauthorizedAccessException("用户没有编辑此代码片段的权限");
        }

        // 检查是否有实质性变更，如果有则创建版本
        bool hasContentChanges = false;
        var changeDescriptions = new List<string>();

        if (!string.IsNullOrEmpty(snippet.Title) && existing.Title != snippet.Title)
        {
            changeDescriptions.Add($"标题从 '{existing.Title}' 更改为 '{snippet.Title}'");
            existing.Title = snippet.Title;
            hasContentChanges = true;
        }

        if (!string.IsNullOrEmpty(snippet.Description) && existing.Description != snippet.Description)
        {
            changeDescriptions.Add("描述已更新");
            existing.Description = snippet.Description;
            hasContentChanges = true;
        }

        if (!string.IsNullOrEmpty(snippet.Code) && existing.Code != snippet.Code)
        {
            changeDescriptions.Add("代码内容已更新");
            existing.Code = snippet.Code;
            hasContentChanges = true;
        }

        if (!string.IsNullOrEmpty(snippet.Language) && existing.Language != snippet.Language)
        {
            changeDescriptions.Add($"语言从 '{existing.Language}' 更改为 '{snippet.Language}'");
            existing.Language = snippet.Language;
            hasContentChanges = true;
        }

        if (snippet.IsPublic.HasValue && existing.IsPublic != snippet.IsPublic.Value)
        {
            changeDescriptions.Add($"可见性更改为 {(snippet.IsPublic.Value ? "公开" : "私有")}");
            existing.IsPublic = snippet.IsPublic.Value;
            hasContentChanges = true;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        // 如果有内容变更，先创建版本再更新
        if (hasContentChanges)
        {
            var changeDescription = string.Join("; ", changeDescriptions);
            await _versionManagementService.CreateVersionAsync(id, changeDescription);
        }

        var updated = await _repository.UpdateAsync(existing);

        // 处理标签更新
        if (snippet.Tags != null)
        {
            var tagIds = await ProcessTagsAsync(snippet.Tags, currentUserId);
            await _repository.UpdateSnippetTagsAsync(id, tagIds);
        }

        // 重新获取包含标签的代码片段
        var result = await _repository.GetByIdWithTagsAsync(id);
        return MapToDto(result!);
    }

    /// <summary>
    /// 删除代码片段 - 包含权限验证
    /// </summary>
    public async Task<bool> DeleteSnippetAsync(Guid id, Guid currentUserId)
    {
        // 检查删除权限
        var canDelete = await _permissionService.CanDeleteSnippetAsync(currentUserId, id);
        if (!canDelete)
        {
            throw new UnauthorizedAccessException("用户没有删除此代码片段的权限");
        }

        return await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// 获取用户的代码片段列表
    /// </summary>
    public async Task<IEnumerable<CodeSnippetDto>> GetUserSnippetsAsync(Guid userId, Guid? currentUserId = null)
    {
        var snippets = await _repository.GetByUserIdAsync(userId);
        
        // 如果不是查看自己的代码片段，需要过滤权限
        if (currentUserId != userId)
        {
            var filteredSnippets = new List<CodeSnippet>();
            foreach (var snippet in snippets)
            {
                if (currentUserId.HasValue)
                {
                    var canAccess = await _permissionService.CanAccessSnippetAsync(
                        currentUserId.Value, snippet.Id, PermissionOperation.Read);
                    if (canAccess)
                    {
                        filteredSnippets.Add(snippet);
                    }
                }
                else if (snippet.IsPublic)
                {
                    filteredSnippets.Add(snippet);
                }
            }
            snippets = filteredSnippets;
        }

        return snippets.Select(MapToDto);
    }

    /// <summary>
    /// 增加查看次数 - 包含权限验证
    /// </summary>
    public async Task<bool> IncrementViewCountAsync(Guid id, Guid? currentUserId = null)
    {
        // 检查访问权限
        if (currentUserId.HasValue)
        {
            var canAccess = await _permissionService.CanAccessSnippetAsync(
                currentUserId.Value, id, PermissionOperation.Read);
            if (!canAccess)
            {
                return false;
            }
        }
        else
        {
            // 未登录用户只能访问公开的代码片段
            var snippet = await _repository.GetByIdAsync(id);
            if (snippet == null || !snippet.IsPublic)
            {
                return false;
            }
        }

        return await _repository.IncrementViewCountAsync(id);
    }

    /// <summary>
    /// 增加复制次数 - 包含权限验证
    /// </summary>
    public async Task<bool> IncrementCopyCountAsync(Guid id, Guid? currentUserId = null)
    {
        // 检查复制权限
        if (currentUserId.HasValue)
        {
            var canCopy = await _permissionService.CanAccessSnippetAsync(
                currentUserId.Value, id, PermissionOperation.Copy);
            if (!canCopy)
            {
                return false;
            }
        }
        else
        {
            // 未登录用户只能复制公开的代码片段
            var snippet = await _repository.GetByIdAsync(id);
            if (snippet == null || !snippet.IsPublic)
            {
                return false;
            }
        }

        return await _repository.IncrementCopyCountAsync(id);
    }

    /// <summary>
    /// 处理标签 - 创建不存在的标签并返回标签ID列表
    /// </summary>
    private async Task<IEnumerable<Guid>> ProcessTagsAsync(IEnumerable<string> tagNames, Guid currentUserId)
    {
        var tagIds = new List<Guid>();
        
        foreach (var tagName in tagNames.Distinct())
        {
            if (string.IsNullOrWhiteSpace(tagName))
                continue;

            // 查找现有标签
            var existingTag = await _tagRepository.GetByNameAsync(tagName.Trim());
            if (existingTag != null)
            {
                tagIds.Add(existingTag.Id);
            }
            else
            {
                // 创建新标签
                var newTag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = tagName.Trim(),
                    Color = "#007bff", // 默认颜色
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };
                
                var created = await _tagRepository.CreateAsync(newTag);
                tagIds.Add(created.Id);
            }
        }

        return tagIds;
    }

    /// <summary>
    /// 将实体映射为 DTO
    /// </summary>
    private static CodeSnippetDto MapToDto(CodeSnippet snippet)
    {
        return new CodeSnippetDto
        {
            Id = snippet.Id,
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = snippet.CreatedBy,
            CreatorName = snippet.CreatorName,
            CreatedAt = snippet.CreatedAt,
            UpdatedAt = snippet.UpdatedAt,
            IsPublic = snippet.IsPublic,
            ViewCount = snippet.ViewCount,
            CopyCount = snippet.CopyCount,
            Tags = snippet.Tags?.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt
            }).ToList() ?? new List<TagDto>()
        };
    }
}