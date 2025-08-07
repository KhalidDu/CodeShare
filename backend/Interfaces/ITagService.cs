using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 标签业务服务接口 - 遵循迪米特法则
/// </summary>
public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagAsync(Guid id);
    Task<TagDto> CreateTagAsync(CreateTagDto tag);
    Task<TagDto> UpdateTagAsync(Guid id, UpdateTagDto tag);
    Task<bool> DeleteTagAsync(Guid id);
    Task<IEnumerable<TagDto>> GetTagsBySnippetIdAsync(Guid snippetId);
    
    /// <summary>
    /// 搜索标签用于自动补全
    /// </summary>
    /// <param name="prefix">搜索前缀</param>
    /// <param name="limit">结果数量限制</param>
    /// <returns>匹配的标签列表</returns>
    Task<IEnumerable<TagDto>> SearchTagsAsync(string prefix, int limit = 10);
    
    /// <summary>
    /// 获取标签使用统计
    /// </summary>
    /// <returns>标签使用统计列表</returns>
    Task<IEnumerable<TagUsageDto>> GetTagUsageStatisticsAsync();
    
    /// <summary>
    /// 获取最常用的标签
    /// </summary>
    /// <param name="limit">结果数量限制</param>
    /// <returns>最常用标签列表</returns>
    Task<IEnumerable<TagDto>> GetMostUsedTagsAsync(int limit = 20);
    
    /// <summary>
    /// 验证标签是否可以删除
    /// </summary>
    /// <param name="tagId">标签ID</param>
    /// <returns>是否可以删除</returns>
    Task<bool> CanDeleteTagAsync(Guid tagId);
}