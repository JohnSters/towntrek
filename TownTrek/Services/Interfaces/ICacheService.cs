namespace TownTrek.Services.Interfaces
{
    /// <summary>
    /// Unified cache service interface for analytics data caching
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get a value from cache
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class?;

        /// <summary>
        /// Set a value in cache with default expiration
        /// </summary>
        Task SetAsync<T>(string key, T value) where T : class?;

        /// <summary>
        /// Set a value in cache with custom expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class?;

        /// <summary>
        /// Remove a value from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Remove multiple values from cache by pattern
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Check if a key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Get cache statistics
        /// </summary>
        Task<CacheStatistics> GetStatisticsAsync();

        /// <summary>
        /// Clear all cache
        /// </summary>
        Task ClearAllAsync();

        /// <summary>
        /// Get or set a value (get from cache if exists, otherwise set and return)
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class?;
    }

    /// <summary>
    /// Cache statistics for monitoring
    /// </summary>
    public class CacheStatistics
    {
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public long TotalRequests => HitCount + MissCount;
        public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests * 100 : 0;
        public long TotalKeys { get; set; }
        public long MemoryUsageBytes { get; set; }
        public bool IsRedisAvailable { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
