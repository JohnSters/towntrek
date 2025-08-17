using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TownTrek.Options;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
    /// <summary>
    /// Unified cache service implementation with Redis and in-memory fallback
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;
        private readonly ILogger<CacheService> _logger;
        private readonly CacheStatistics _statistics;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public CacheService(
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            IOptions<CacheOptions> cacheOptions,
            ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value;
            _logger = logger;
            _statistics = new CacheStatistics();
        }

        public async Task<T?> GetAsync<T>(string key) where T : class?
        {
            try
            {
                var cacheKey = GetCacheKey<T>(key);
                
                // Try Redis first if enabled
                if (_cacheOptions.UseRedis)
                {
                    var redisValue = await _distributedCache.GetStringAsync(cacheKey);
                    if (!string.IsNullOrEmpty(redisValue))
                    {
                        _statistics.HitCount++;
                        return JsonSerializer.Deserialize<T>(redisValue);
                    }
                }

                // Fallback to memory cache
                if (_memoryCache.TryGetValue(cacheKey, out T? memoryValue))
                {
                    _statistics.HitCount++;
                    return memoryValue;
                }

                _statistics.MissCount++;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                _statistics.MissCount++;
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value) where T : class?
        {
            await SetAsync(key, value, TimeSpan.FromMinutes(_cacheOptions.DefaultExpirationMinutes));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class?
        {
            try
            {
                var cacheKey = GetCacheKey<T>(key);
                var serializedValue = JsonSerializer.Serialize(value);

                // Set in Redis if enabled
                if (_cacheOptions.UseRedis)
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration
                    };
                    await _distributedCache.SetStringAsync(cacheKey, serializedValue, options);
                }

                // Always set in memory cache as fallback
                var memoryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                _memoryCache.Set(cacheKey, value, memoryOptions);

                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                // Remove from Redis if enabled
                if (_cacheOptions.UseRedis)
                {
                    await _distributedCache.RemoveAsync(key);
                }

                // Remove from memory cache
                _memoryCache.Remove(key);

                _logger.LogDebug("Removed cache entry for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
            }
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                // Note: Redis supports pattern removal, but IDistributedCache doesn't expose this
                // For now, we'll just log this operation
                _logger.LogInformation("Pattern removal requested for pattern: {Pattern}", pattern);
                
                // In a real implementation, you might want to use StackExchange.Redis directly
                // for pattern-based removal, or maintain a list of keys to remove
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
            }
            
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                // Check Redis first if enabled
                if (_cacheOptions.UseRedis)
                {
                    var value = await _distributedCache.GetStringAsync(key);
                    if (!string.IsNullOrEmpty(value))
                        return true;
                }

                // Check memory cache
                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<CacheStatistics> GetStatisticsAsync()
        {
            try
            {
                // Update Redis availability status
                if (_cacheOptions.UseRedis)
                {
                    try
                    {
                        await _distributedCache.GetStringAsync("health_check");
                        _statistics.IsRedisAvailable = true;
                    }
                    catch
                    {
                        _statistics.IsRedisAvailable = false;
                    }
                }

                // Update memory usage (approximate)
                if (_memoryCache is MemoryCache memoryCache)
                {
                    var field = typeof(MemoryCache).GetField("_coherentState", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field?.GetValue(memoryCache) is object coherentState)
                    {
                        var entriesCollection = coherentState.GetType().GetField("_entries", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (entriesCollection?.GetValue(coherentState) is IDictionary<object, object> entries)
                        {
                            _statistics.TotalKeys = entries.Count;
                        }
                    }
                }

                _statistics.LastUpdated = DateTime.UtcNow;
                return _statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return _statistics;
            }
        }

        public Task ClearAllAsync()
        {
            try
            {
                // Clear memory cache
                if (_memoryCache is MemoryCache memoryCache)
                {
                    memoryCache.Compact(1.0);
                }

                _logger.LogInformation("Cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
            
            return Task.CompletedTask;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class?
        {
            var cacheKey = GetCacheKey<T>(key);
            var cacheExpiration = expiration ?? TimeSpan.FromMinutes(_cacheOptions.DefaultExpirationMinutes);

            // Try to get from cache first
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Use semaphore to prevent multiple simultaneous factory calls for the same key
            await _semaphore.WaitAsync();
            try
            {
                // Double-check pattern: check cache again after acquiring lock
                cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                {
                    return cachedValue;
                }

                // Execute factory and cache result
                var value = await factory();
                await SetAsync(key, value, cacheExpiration);
                return value;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static string GetCacheKey<T>(string key) where T : class?
        {
            return $"TownTrek:{typeof(T).Name}:{key}";
        }
    }
}
