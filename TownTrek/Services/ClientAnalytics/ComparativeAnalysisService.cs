using TownTrek.Constants;
using TownTrek.Models;
using TownTrek.Models.Exceptions;
using TownTrek.Models.ViewModels;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services.ClientAnalytics
{
    /// <summary>
    /// Service for comparative analysis functionality
    /// </summary>
    public class ComparativeAnalysisService(
        IAnalyticsDataService dataService,
        IAnalyticsValidationService validationService,
        IAnalyticsEventService eventService,
        IAnalyticsErrorHandler errorHandler,
        ILogger<ComparativeAnalysisService> logger) : IComparativeAnalysisService
    {
        private readonly IAnalyticsDataService _dataService = dataService;
        private readonly IAnalyticsValidationService _validationService = validationService;
        private readonly IAnalyticsEventService _eventService = eventService;
        private readonly IAnalyticsErrorHandler _errorHandler = errorHandler;
        private readonly ILogger<ComparativeAnalysisService> _logger = logger;

        public async Task<ComparativeAnalysisResponse> GetComparativeAnalysisAsync(string userId, ComparativeAnalysisRequest request)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                // Validate request
                var validation = _validationService.ValidateComparativeAnalysisRequest(request);
                if (!validation.IsValid)
                {
                    await _errorHandler.HandleValidationExceptionAsync(
                        validation.ErrorMessage ?? "Invalid comparative analysis request",
                        userId,
                        "ComparativeAnalysisRequest",
                        "InvalidRequest",
                        new Dictionary<string, object> { ["Request"] = request }
                    );
                    throw new AnalyticsValidationException(validation.ErrorMessage ?? "Invalid comparative analysis request", "ComparativeAnalysisRequest", "InvalidRequest");
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
                        await _errorHandler.HandleValidationExceptionAsync(
                            $"Unsupported comparison type: {request.ComparisonType}",
                            userId,
                            "ComparisonType",
                            "UnsupportedType",
                            new Dictionary<string, object> { ["ComparisonType"] = request.ComparisonType }
                        );
                        throw new AnalyticsValidationException($"Unsupported comparison type: {request.ComparisonType}", "ComparisonType", "UnsupportedType");
                }
            }, userId, "GetComparativeAnalysis", new Dictionary<string, object> { ["Request"] = request });
        }

        public async Task<ComparativeAnalysisResponse> GetPeriodOverPeriodComparisonAsync(string userId, int? businessId = null, string comparisonType = "MonthOverMonth", string? platform = null)
        {
            try
            {
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
                _logger.LogError(ex, "Error in GetPeriodOverPeriodComparisonAsync for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetYearOverYearComparisonAsync(string userId, int? businessId = null, string? platform = null)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var currentStart = endDate.AddDays(-365);
                var previousStart = currentStart.AddDays(-365);
                var previousEnd = currentStart.AddDays(-1);

                return await GetCustomRangeComparisonAsync(userId, currentStart, endDate, previousStart, previousEnd, businessId, platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetYearOverYearComparisonAsync for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ComparativeAnalysisResponse> GetCustomRangeComparisonAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? businessId = null, string? platform = null)
        {
            try
            {
                var businesses = businessId.HasValue 
                    ? new List<Business> { await _dataService.GetBusinessAsync(businessId.Value, userId) ?? throw new ArgumentException("Business not found") }
                    : await _dataService.GetUserBusinessesAsync(userId);

                if (!businesses.Any()) 
                {
                    return new ComparativeAnalysisResponse
                    {
                        ComparisonType = "CustomRange",
                        Insights = new List<string> { "No businesses found for analysis." }
                    };
                }

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
                                BorderColor = "#33658a",
                                BackgroundColor = "#33658a"
                            },
                            new ComparativeChartDataset
                            {
                                Label = "Reviews",
                                Data = new List<double> { currentPeriod.TotalReviews, previousPeriod.TotalReviews },
                                BorderColor = "#f6ae2d",
                                BackgroundColor = "#f6ae2d"
                            },
                            new ComparativeChartDataset
                            {
                                Label = "Favorites",
                                Data = new List<double> { currentPeriod.TotalFavorites, previousPeriod.TotalFavorites },
                                BorderColor = "#f26419",
                                BackgroundColor = "#f26419"
                            }
                        }
                    },
                    Insights = GenerateComparativeInsights(comparisonMetrics)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCustomRangeComparisonAsync for user {UserId}", userId);
                throw;
            }
        }

        // Private helper methods
        private PeriodData CalculatePeriodData(List<BusinessViewLog> viewLogs, List<BusinessReview> reviews, List<FavoriteBusiness> favorites, DateTime startDate, DateTime endDate)
        {
            try
            {
                var totalViews = viewLogs?.Count ?? 0;
                var totalReviews = reviews?.Count ?? 0;
                var totalFavorites = favorites?.Count ?? 0;
                var averageRating = reviews?.Any() == true ? reviews.Average(r => r.Rating) : 0;
                var engagementScore = CalculateEngagementScore(totalReviews, totalFavorites, totalViews);
                var periodDays = Math.Max(1, (endDate - startDate).Days);
                
                var averageViewsPerDay = totalViews / (double)periodDays;
                var averageReviewsPerDay = totalReviews / (double)periodDays;
                var averageFavoritesPerDay = totalFavorites / (double)periodDays;

                // Calculate peak and low day views safely
                int peakDayViews = 0;
                DateTime? peakDayDate = null;
                int lowDayViews = 0;
                DateTime? lowDayDate = null;

                if (viewLogs?.Any() == true)
                {
                    var dailyViews = viewLogs.GroupBy(v => v.ViewedAt.Date).ToList();
                    if (dailyViews.Any())
                    {
                        peakDayViews = dailyViews.Max(g => g.Count());
                        peakDayDate = dailyViews.OrderByDescending(g => g.Count()).First().Key;
                        lowDayViews = dailyViews.Min(g => g.Count());
                        lowDayDate = dailyViews.OrderBy(g => g.Count()).First().Key;
                    }
                }

                return new PeriodData
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalViews = totalViews,
                    TotalReviews = totalReviews,
                    TotalFavorites = totalFavorites,
                    AverageRating = averageRating,
                    EngagementScore = engagementScore,
                    AverageViewsPerDay = averageViewsPerDay,
                    AverageReviewsPerDay = averageReviewsPerDay,
                    AverageFavoritesPerDay = averageFavoritesPerDay,
                    PeakDayViews = peakDayViews,
                    PeakDayDate = peakDayDate,
                    LowDayViews = lowDayViews,
                    LowDayDate = lowDayDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating period data");
                // Return safe defaults
                return new PeriodData
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalViews = 0,
                    TotalReviews = 0,
                    TotalFavorites = 0,
                    AverageRating = 0,
                    EngagementScore = 0,
                    AverageViewsPerDay = 0,
                    AverageReviewsPerDay = 0,
                    AverageFavoritesPerDay = 0,
                    PeakDayViews = 0,
                    LowDayViews = 0
                };
            }
        }

        private ComparisonMetrics CalculateComparisonMetrics(PeriodData current, PeriodData previous)
        {
            var viewsChangePercent = CalculatePercentageChange(current.TotalViews, previous.TotalViews);
            var reviewsChangePercent = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews);
            var ratingChangePercent = CalculatePercentageChange(current.AverageRating, previous.AverageRating);
            var engagementChangePercent = CalculatePercentageChange(current.EngagementScore, previous.EngagementScore);
            
            return new ComparisonMetrics
            {
                // Legacy properties for backward compatibility
                ViewsChangePercent = viewsChangePercent,
                ReviewsChangePercent = reviewsChangePercent,
                FavoritesChangePercent = CalculatePercentageChange(current.TotalFavorites, previous.TotalFavorites),
                RatingChangePercent = ratingChangePercent,
                EngagementChangePercent = engagementChangePercent,
                AverageViewsPerDayChangePercent = CalculatePercentageChange(current.AverageViewsPerDay, previous.AverageViewsPerDay),
                AverageReviewsPerDayChangePercent = CalculatePercentageChange(current.AverageReviewsPerDay, previous.AverageReviewsPerDay),
                AverageFavoritesPerDayChangePercent = CalculatePercentageChange(current.AverageFavoritesPerDay, previous.AverageFavoritesPerDay),
                
                // Additional properties needed by JavaScript
                ViewsGrowthPercentage = viewsChangePercent,
                ReviewsGrowthPercentage = reviewsChangePercent,
                RatingGrowthPercentage = ratingChangePercent,
                EngagementGrowthPercentage = engagementChangePercent,
                OverallPerformanceChange = GetOverallPerformanceChange(current, previous),
                
                OverallTrend = GetOverallTrend(current, previous),
                PerformanceRating = GetPerformanceRating(current, previous),
                KeyChanges = GenerateKeyChanges(current, previous)
            };
        }

        private double CalculateEngagementScore(int reviews, int favorites, int views)
        {
            try
            {
                if (views == 0) return 0;
                // Use simple calculation without constants that might not exist
                return (reviews + favorites) * 100.0 / views;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating engagement score");
                return 0;
            }
        }

        private double CalculatePercentageChange(double current, double previous)
        {
            try
            {
                if (previous == 0) return current > 0 ? 100.0 : 0;
                return (current - previous) / previous * 100.0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating percentage change");
                return 0;
            }
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

        private string GetOverallPerformanceChange(PeriodData current, PeriodData previous)
        {
            var viewsChange = current.TotalViews - previous.TotalViews;
            var reviewsChange = current.TotalReviews - previous.TotalReviews;
            var ratingChange = current.AverageRating - previous.AverageRating;

            if (viewsChange > 0 && reviewsChange > 0 && ratingChange > 0)
                return "Significantly Improved";
            if (viewsChange > 0 || reviewsChange > 0 || ratingChange > 0)
                return "Improved";
            if (viewsChange < 0 && reviewsChange < 0 && ratingChange < 0)
                return "Declined";
            return "Stable";
        }

        private string GetPerformanceRating(PeriodData current, PeriodData previous)
        {
            try
            {
                var engagementScore = current.EngagementScore;
                var rating = current.AverageRating;
                
                // Use simple thresholds
                if (engagementScore >= 50.0 && rating >= 4.5)
                    return "Excellent";
                if (engagementScore >= 25.0 && rating >= 4.0)
                    return "Good";
                if (rating >= 3.0)
                    return "Fair";
                return "Poor";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating performance rating");
                return "Fair";
            }
        }

        private List<string> GenerateKeyChanges(PeriodData current, PeriodData previous)
        {
            try
            {
                var changes = new List<string>();
                
                var viewsChange = CalculatePercentageChange(current.TotalViews, previous.TotalViews);
                var reviewsChange = CalculatePercentageChange(current.TotalReviews, previous.TotalReviews);
                var ratingChange = CalculatePercentageChange(current.AverageRating, previous.AverageRating);
                
                // Use simple thresholds
                if (Math.Abs(viewsChange) >= 10.0) // 10% change threshold
                {
                    var direction = viewsChange > 0 ? "increased" : "decreased";
                    changes.Add($"Views {direction} by {Math.Abs(viewsChange):F1}%");
                }
                
                if (Math.Abs(reviewsChange) >= 10.0) // 10% change threshold
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating key changes");
                return new List<string> { "Unable to analyze changes." };
            }
        }

        private List<string> GenerateComparativeInsights(ComparisonMetrics metrics)
        {
            try
            {
                var insights = new List<string>();
                
                // Use simple thresholds
                if (Math.Abs(metrics.ViewsChangePercent) >= 10.0)
                {
                    insights.Add($"Views changed by {metrics.ViewsChangePercent:F1}% compared to the previous period");
                }
                
                if (Math.Abs(metrics.ReviewsChangePercent) >= 10.0)
                {
                    insights.Add($"Reviews changed by {metrics.ReviewsChangePercent:F1}% compared to the previous period");
                }
                
                if (insights.Count == 0)
                {
                    insights.Add("Performance remained stable compared to the previous period.");
                }
                
                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comparative insights");
                return new List<string> { "Unable to generate insights." };
            }
        }
    }
}
