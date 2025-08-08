using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Services
{
    /// <summary>
    /// 内存缓存服务实现
    /// 遵循单一职责原则，专门负责内存缓存操作
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, bool> _cacheKeys;
        private readonly TimeSpan _defaultExpiration;

        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheKeys = new ConcurrentDictionary<string, bool>();
            _defaultExpiration = TimeSpan.FromMinutes(30); // 默认30分钟过期
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("缓存键不能为空");
                    return Task.FromResult<T?>(null);
                }

                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    _logger.LogDebug("缓存命中: {Key}", key);
                    
                    if (cachedValue is string jsonString)
                    {
                        return Task.FromResult(JsonSerializer.Deserialize<T>(jsonString));
                    }
                    
                    return Task.FromResult(cachedValue as T);
                }

                _logger.LogDebug("缓存未命中: {Key}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存失败: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

        /// <summary>
        /// 设置缓存值
        /// </summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("缓存键不能为空");
                    return Task.CompletedTask;
                }

                if (value == null)
                {
                    _logger.LogWarning("缓存值不能为空: {Key}", key);
                    return Task.CompletedTask;
                }

                var cacheExpiration = expiration ?? _defaultExpiration;
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration,
                    Priority = CacheItemPriority.Normal,
                    Size = 1
                };

                // 添加过期回调
                options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
                    _logger.LogDebug("缓存已过期: {Key}, 原因: {Reason}", evictedKey, reason);
                });

                // 序列化复杂对象
                var cacheValue = value is string stringValue ? stringValue : JsonSerializer.Serialize(value);
                
                _memoryCache.Set(key, cacheValue, options);
                _cacheKeys.TryAdd(key, true);

                _logger.LogDebug("缓存已设置: {Key}, 过期时间: {Expiration}", key, cacheExpiration);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置缓存失败: {Key}", key);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public Task RemoveAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("缓存键不能为空");
                    return Task.CompletedTask;
                }

                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);

                _logger.LogDebug("缓存已删除: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除缓存失败: {Key}", key);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 删除匹配模式的缓存
        /// </summary>
        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    _logger.LogWarning("缓存模式不能为空");
                    return Task.CompletedTask;
                }

                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var keysToRemove = new List<string>();

                foreach (var key in _cacheKeys.Keys)
                {
                    if (regex.IsMatch(key))
                    {
                        keysToRemove.Add(key);
                    }
                }

                var tasks = keysToRemove.Select(key => RemoveAsync(key));
                Task.WaitAll(tasks.ToArray());

                _logger.LogDebug("已删除匹配模式的缓存: {Pattern}, 数量: {Count}", pattern, keysToRemove.Count);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除匹配模式缓存失败: {Pattern}", pattern);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return Task.FromResult(false);
                }

                return Task.FromResult(_memoryCache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查缓存存在性失败: {Key}", key);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 获取或设置缓存
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            try
            {
                // 先尝试获取缓存
                var cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                {
                    return cachedValue;
                }

                // 缓存不存在，执行工厂函数
                _logger.LogDebug("缓存不存在，执行工厂函数: {Key}", key);
                var value = await factory();

                if (value != null)
                {
                    await SetAsync(key, value, expiration);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取或设置缓存失败: {Key}", key);
                
                // 发生异常时，尝试直接执行工厂函数
                try
                {
                    return await factory();
                }
                catch (Exception factoryEx)
                {
                    _logger.LogError(factoryEx, "工厂函数执行失败: {Key}", key);
                    throw;
                }
            }
        }

        /// <summary>
        /// 刷新缓存过期时间
        /// </summary>
        public Task RefreshAsync(string key, TimeSpan expiration)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("缓存键不能为空");
                    return Task.CompletedTask;
                }

                if (_memoryCache.TryGetValue(key, out var value))
                {
                    // 重新设置缓存以更新过期时间
                    var options = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration,
                        Priority = CacheItemPriority.Normal,
                        Size = 1
                    };

                    _memoryCache.Set(key, value, options);
                    _logger.LogDebug("缓存过期时间已刷新: {Key}, 新过期时间: {Expiration}", key, expiration);
                }
                else
                {
                    _logger.LogWarning("尝试刷新不存在的缓存: {Key}", key);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新缓存过期时间失败: {Key}", key);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                TotalKeys = _cacheKeys.Count,
                // 注意：MemoryCache 不提供直接的统计信息
                // 这里只能提供键的数量
            };
        }
    }

    /// <summary>
    /// 缓存统计信息
    /// </summary>
    public class CacheStatistics
    {
        public int TotalKeys { get; set; }
        public long TotalMemoryUsage { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public double HitRatio => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
    }
}