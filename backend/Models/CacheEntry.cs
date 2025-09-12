using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 缓存条目基类
/// </summary>
/// <typeparam name="T">缓存数据类型</typeparam>
public class CacheEntry<T>
{
    /// <summary>
    /// 缓存的数据
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// 消息缓存条目
/// </summary>
public class MessageCacheEntry : CacheEntry<PaginatedResult<MessageDto>>
{
}

/// <summary>
/// 评论缓存条目
/// </summary>
public class CommentCacheEntry : CacheEntry<PaginatedResult<CommentDto>>
{
}