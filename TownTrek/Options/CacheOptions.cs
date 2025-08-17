namespace TownTrek.Options
{
    /// <summary>
    /// Configuration options for caching settings
    /// </summary>
    public class CacheOptions
    {
        public const string SectionName = "Cache";

        /// <summary>
        /// Redis connection string
        /// </summary>
        public string RedisConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Whether to use Redis caching (false = use in-memory only)
        /// </summary>
        public bool UseRedis { get; set; } = false;

        /// <summary>
        /// Default cache expiration time in minutes
        /// </summary>
        public int DefaultExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Analytics data cache expiration time in minutes
        /// </summary>
        public int AnalyticsExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Chart data cache expiration time in minutes
        /// </summary>
        public int ChartDataExpirationMinutes { get; set; } = 10;

        /// <summary>
        /// Business analytics cache expiration time in minutes
        /// </summary>
        public int BusinessAnalyticsExpirationMinutes { get; set; } = 20;

        /// <summary>
        /// User dashboard cache expiration time in minutes
        /// </summary>
        public int DashboardExpirationMinutes { get; set; } = 5;

        /// <summary>
        /// Cache warming enabled
        /// </summary>
        public bool EnableCacheWarming { get; set; } = true;

        /// <summary>
        /// Cache monitoring enabled
        /// </summary>
        public bool EnableCacheMonitoring { get; set; } = true;
    }
}
