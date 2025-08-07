using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 标签仓储接口 - 遵循里氏替换原则
/// </summary>
public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id);
    Task<Tag?> GetByNameAsync(string name);
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Tag>> GetTagsBySnippetIdAsync(Guid snippetId);
    
    /// <summary>
    /// 根据名称前缀搜索标签，用于自动补全功能
    /// </summary>
    /// <param name="prefix">标签名称前缀</param>
    /// <param name="limit">返回结果数量限制</param>
    /// <returns>匹配的标签列表</returns>
    Task<IEnumerable<Tag>> SearchTagsByPrefixAsync(string prefix, int limit = 10);
    
    /// <summary>
    /// 获取标签使用统计信息
    /// </summary>
    /// <returns>标签及其使用次数</returns>
    Task<IEnumerable<TagUsageStatistic>> GetTagUsageStatisticsAsync();
    
    /// <summary>
    /// 获取最常用的标签
    /// </summary>
    /// <param name="limit">返回结果数量限制</param>
    /// <returns>按使用次数排序的标签列表</returns>
    Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int limit = 20);
    
    /// <summary>
    /// 检查标签是否被使用
    /// </summary>
    /// <param name="tagId">标签ID</param>
    /// <returns>是否被使用</returns>
    Task<bool> IsTagInUseAsync(Guid tagId);
}