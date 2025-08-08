using System;
using System.Threading.Tasks;

namespace CodeSnippetManager.Api.Interfaces
{
    /// <summary>
    /// 缓存服务接口
    /// 遵循依赖倒置原则，提供缓存抽象
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值，如果不存在则返回默认值</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiration">过期时间</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 删除匹配模式的缓存
        /// </summary>
        /// <param name="pattern">匹配模式</param>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 获取或设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂函数</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>缓存值</returns>
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// 刷新缓存过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiration">新的过期时间</param>
        Task RefreshAsync(string key, TimeSpan expiration);
    }
}