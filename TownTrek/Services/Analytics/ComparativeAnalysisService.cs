using TownTrek.Constants;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.Analytics
{
    /// <summary>
    /// Service for comparative analysis functionality
    /// </summary>
    public class ComparativeAnalysisService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        ILogger<ComparativeAnalysisService> logger) : IComparativeAnalysisService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly ILogger<ComparativeAnalysisService> _logger = logger;

        public async Task<ComparativeAnalysisResponse> GetComparativeAnalysisAsync(string userId, ComparativeAnalysisRequest request)
        {
            try
            {
                // Validate request
                var validation = _validationService.ValidateComparativeAnalysisRequest(request);
                if (!validation.IsValid)
                {
                    throw new ArgumentException(validation.ErrorMessage);
                }

                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "ComparativeAnalysis", request);

                switch (request.ComparisonType)
                {
                    case "WeekOverWeek":
                    case "MonthOverMonth":
                    case "QuarterOverQuarter":
                        return await GetPeriodOverPeriodComparisonAsync(userId, request.BusinessId, request.ComparisonType, request.Platform);
                    case "YearOverYear":
                        return await GetYearOverYearComparisonAsync(userId, request.BusinessId, request.Platform);
                    case "CustomRange":
                        return await GetCustomRangeComparisonAsync(userId, request.CurrentPeriodStart, request.CurrentPeriodEnd, 
                            request.PreviousPeriodStart ?? DateTime.MinValue, request.PreviousPeriodEnd ?? DateTime.MinValue, 
                            request.BusinessId, request.Platform);
                    default:
                        throw new ArgumentException($"Unsupported comparison type: {request.ComparisonType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparative analysis for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetComparativeAnalysis", ex.Message, new { UserId = userId, Request = request });
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetPeriodOverPeriodComparisonAsync(string userId, int? businessId = null, string comparisonType = "MonthOverMonth", string? platform = null)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "PeriodOverPeriodComparison", new { BusinessId = businessId, ComparisonType = comparisonType, Platform = platform });

                var endDate = DateTime.UtcNow;
                var periodDays = comparisonType switch
                {
                    "WeekOverWeek" => 7,
                    "MonthOverMonth" => 30,
                    "QuarterOverQuarter" => 90,
                    _ => 30
                };

                var currentStart = endDate.AddDays(-periodDays);
                var previousStart = currentStart.AddDays(-periodDays);
                var previousEnd = currentStart.AddDays(-1);

                return await GetCustomRangeComparisonAsync(userId, currentStart, endDate, previousStart, previousEnd, businessId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting period over period comparison for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetPeriodOverPeriodComparison", ex.Message, new { UserId = userId, BusinessId = businessId, ComparisonType = comparisonType, Platform = platform });
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetYearOverYearComparisonAsync(string userId, int? businessId = null, string? platform = null)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "YearOverYearComparison", new { BusinessId = businessId, Platform = platform });

                var endDate = DateTime.UtcNow;
                var currentStart = endDate.AddDays(-365);
                var previousStart = currentStart.AddDays(-365);
                var previousEnd = currentStart.AddDays(-1);

                return await GetCustomRangeComparisonAsync(userId, currentStart, endDate, previousStart, previousEnd, businessId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting year over year comparison for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetYearOverYearComparison", ex.Message, new { UserId = userId, BusinessId = businessId, Platform = platform });
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetCustomRangeComparisonAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? businessId = null, string? platform = null)
        {
            try
            {
                // Record analytics access event
                await _eventService.RecordAnalyticsAccessEventAsync(userId, "CustomRangeComparison", new { BusinessId = businessId, Platform = platform, CurrentStart = currentStart, CurrentEnd = currentEnd, PreviousStart = previousStart, PreviousEnd = previousEnd });

                var businesses = businessId.HasValue 
                    ? new List<Business> { await _dataService.GetBusinessAsync(businessId.Value, userId) ?? throw new ArgumentException("Business not found") }
                    : await _dataService.GetUserBusinessesAsync(userId);

                if (!businesses.Any()) return new ComparativeAnalysisResponse();

                var businessIds = businesses.Select(b => b.Id).ToList();

                // Get data for both periods
                var currentViewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, currentStart, currentEnd, platform);
                var previousViewLogs = await _dataService.GetBusinessViewLogsAsync(businessIds, previousStart, previousEnd, platform);
                var currentReviews = await _dataService.GetBusinessReviewsAsync(businessIds, currentStart, currentEnd);
                var previousReviews = await _dataService.GetBusinessReviewsAsync(businessIds, previousStart, previousEnd);
                var currentFavorites = await _dataService.GetBusinessFavoritesAsync(businessIds, currentStart, currentEnd);
                var previousFavorites = await _dataService.GetBusinessFavoritesAsync(businessIds, previousStart, previousEnd);

                // Calculate period data
                var currentPeriod = CalculatePeriodData(currentViewLogs, currentReviews, currentFavorites, currentStart, currentEnd);
                var previousPeriod = CalculatePeriodData(previousViewLogs, previousReviews, previousFavorites, previousStart, previousEnd);

                // Calculate comparison metrics
                var comparisonMetrics = CalculateComparisonMetrics(currentPeriod, previousPeriod);

                return new ComparativeAnalysisResponse
                {
                    ComparisonType = "CustomRange",
                    CurrentPeriod = currentPeriod,
                    PreviousPeriod = previousPeriod,
                    ComparisonMetrics = comparisonMetrics,
                    ChartData = new ComparativeChartData
                    {
                        Labels = new List<string> { "Current Period", "Previous Period" },
                        Datasets = new List<ComparativeChartDataset>
                        {
                            new ComparativeChartDataset
                            {
                                Label = "Views",
                                Data = new List<double> { currentPeriod.TotalViews, previousPeriod.TotalViews },
                                BorderColor = AnalyticsConstants.ChartColors.LapisLazuli,
                                BackgroundColor = AnalyticsConstants.ChartColors.LapisLazuli + AnalyticsConstants.ChartOpacity.Light
                            },
                            new ComparativeChartDataset
                            {
                                Label = "Reviews",
                                Data = new List<double> { currentPeriod.TotalReviews, previousPeriod.TotalReviews },
                                BorderColor = AnalyticsConstants.ChartColors.HunyadiYellow,
                                BackgroundColor = AnalyticsConstants.ChartColors.HunyadiYellow + AnalyticsConstants.ChartOpacity.Light
                            },
                            new ComparativeChartDataset
                            {
                                Label = "Favorites",
                                Data = new List<double> { currentPeriod.TotalFavorites, previousPeriod.TotalFavorites },
                                BorderColor = AnalyticsConstants.ChartColors.OrangePantone,
                                BackgroundColor = AnalyticsConstants.ChartColors.OrangePantone + AnalyticsConstants.ChartOpacity.Light
                            }
                        }
                    },
                    Insights = GenerateComparativeInsights(comparisonMetrics)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom range comparison for UserId {UserId}", userId);
                await _eventService.RecordAnalyticsErrorEventAsync(userId, "GetCustomRangeComparison", ex.Message, new { UserId = userId, BusinessId = businessId, Platform = platform, CurrentStart = currentStart, CurrentEnd = currentEnd, PreviousStart = previousStart, PreviousEnd = previousEnd });
                throw;
            }
        }

        // Private helper methods
        private PeriodData CalculatePeriodData(List<BusinessViewLog> viewLogs, List<BusinessReview> reviews, List<FavoriteBusiness> favorites, DateTime startDate, DateTime endDate)
        {
            return new PeriodData
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalViews = viewLogs.Count,
                TotalReviews = reviews.Count,
                TotalFavorites = favorites.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                EngagementScore = CalculateEngagementScore(reviews.Count, favorites.Count, viewLogs.Count),
                AverageViewsPerDay = viewLogs.Count / Math.Max(1, (endDate - startDate).Days),
                AverageReviewsPerDay = reviews.Count / Math.Max(1, (endDate - startDate).Days),
                AverageFavoritesPerDay = favorites.Count / Math.Max(1, (endDate - startDate).Days),
                PeakDayViews = viewLogs.GroupBy(v => v.ViewedAt.Date).Max(g => g.Count()),
                PeakDayDate = viewLogs.GroupBy(v => v.ViewedAt.Date).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key,
                LowDayViews = viewLogs.GroupBy(v => v.ViewedAt.Date).Min(g => g.Count()),
                LowDayDate = viewLogs.GroupBy(v => v.ViewedAt.Date).OrderBy(g => g.Count()).FirstOrDefault()?.Key
            };
        }

        private ComparisonMetrics CalculateComparisonMetrics(PeriodData current, PeriodData previous)
        {
            return new ComparisonMetrics
            {
                ViewsChangePercent = CalculatePercentageChange(current.TotalViews, previous.TotalViews),
                ReviewsChangePercent = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews),
                FavoritesChangePercent = CalculatePercentageChange(current.TotalFavorites, previous.TotalFavorites),
                RatingChangePercent = CalculatePercentageChange(current.AverageRating, previous.AverageRating),
                EngagementChangePercent = CalculatePercentageChange(current.EngagementScore, previous.EngagementScore),
                AverageViewsPerDayChangePercent = CalculatePercentageChange(current.AverageViewsPerDay, previous.AverageViewsPerDay),
                AverageReviewsPerDayChangePercent = CalculatePercentageChange(current.AverageReviewsPerDay, previous.AverageReviewsPerDay),
                AverageFavoritesPerDayChangePercent = CalculatePercentageChange(current.AverageFavoritesPerDay, previous.AverageFavoritesPerDay),
                OverallTrend = GetOverallTrend(current, previous),
                PerformanceRating = GetPerformanceRating(current, previous),
                KeyChanges = GenerateKeyChanges(current, previous)
            };
        }

        private double CalculateEngagementScore(int reviews, int favorites, int views)
        {
            if (views == 0) return 0;
            return (reviews + favorites) * AnalyticsConstants.EngagementScoreMultiplier / views * AnalyticsConstants.PercentageMultiplier;
        }

        private double CalculatePercentageChange(double current, double previous)
        {
            if (previous == 0) return current > 0 ? AnalyticsConstants.PercentageMultiplier : 0;
            return (current - previous) / previous * AnalyticsConstants.PercentageMultiplier;
        }

        private string GetOverallTrend(PeriodData current, PeriodData previous)
        {
            var improvements = 0;
            var declines = 0;
            
            if (current.TotalViews > previous.TotalViews) improvements++;
            else if (current.TotalViews < previous.TotalViews) declines++;
            
            if (current.TotalReviews > previous.TotalReviews) improvements++;
            else if (current.TotalReviews < previous.TotalReviews) declines++;
            
            if (current.AverageRating > previous.AverageRating) improvements++;
            else if (current.AverageRating < previous.AverageRating) declines++;
            
            if (improvements > declines) return "Improving";
            if (declines > improvements) return "Declining";
            return "Stable";
        }

        private string GetPerformanceRating(PeriodData current, PeriodData previous)
        {
            var engagementScore = current.EngagementScore;
            var rating = current.AverageRating;
            
            if (engagementScore >= AnalyticsConstants.StrongEngagementThreshold && rating >= AnalyticsConstants.ExcellentRatingThreshold)
                return "Excellent";
            if (engagementScore >= AnalyticsConstants.MinFavoritesThreshold && rating >= AnalyticsConstants.GoodRatingThreshold)
                return "Good";
            if (rating >= AnalyticsConstants.PoorRatingThreshold)
                return "Fair";
            return "Poor";
        }

        private List<string> GenerateKeyChanges(PeriodData current, PeriodData previous)
        {
            var changes = new List<string>();
            
            var viewsChange = CalculatePercentageChange(current.TotalViews, previous.TotalViews);
            var reviewsChange = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews);
            var ratingChange = CalculatePercentageChange(current.AverageRating, previous.AverageRating);
            
            if (Math.Abs(viewsChange) >= AnalyticsConstants.SignificantViewsChangeThreshold)
            {
                var direction = viewsChange > 0 ? "increased" : "decreased";
                changes.Add($"Views {direction} by {Math.Abs(viewsChange):F1}%");
            }
            
            if (Math.Abs(reviewsChange) >= AnalyticsConstants.SignificantReviewsChangeThreshold)
            {
                var direction = reviewsChange > 0 ? "increased" : "decreased";
                changes.Add($"Reviews {direction} by {Math.Abs(reviewsChange):F1}%");
            }
            
            if (Math.Abs(ratingChange) >= 5.0) // 5% rating change threshold
            {
                var direction = ratingChange > 0 ? "improved" : "declined";
                changes.Add($"Average rating {direction} by {Math.Abs(ratingChange):F1}%");
            }
            
            return changes;
        }

        private List<string> GenerateComparativeInsights(ComparisonMetrics metrics)
        {
            var insights = new List<string>();
            
            if (Math.Abs(metrics.ViewsChangePercent) >= AnalyticsConstants.SignificantViewsChangeThreshold)
            {
                insights.Add($"Views changed by {metrics.ViewsChangePercent:F1}% compared to the previous period");
            }
            
            if (Math.Abs(metrics.ReviewsChangePercent) >= AnalyticsConstants.SignificantReviewsChangeThreshold)
            {
                insights.Add($"Reviews changed by {metrics.ReviewsChangePercent:F1}% compared to the previous period");
            }
            
            return insights;
        }
    }
}
