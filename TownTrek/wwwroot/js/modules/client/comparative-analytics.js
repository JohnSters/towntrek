/**
 * Comparative Analytics Module
 * Handles comparative analysis functionality including charts, data fetching, and user interactions
 */

class ComparativeAnalytics {
    constructor() {
        this.chart = null;
        this.currentData = null;
        this.businessId = this.getBusinessIdFromUrl();
        this.initializeEventListeners();
        this.initializeChart();
        this.loadBusinesses();
    }

    /**
     * Initialize event listeners for user interactions
     */
    initializeEventListeners() {
        // Comparison type change
        document.getElementById('comparisonType')?.addEventListener('change', (e) => {
            this.refreshComparison();
        });

        // Platform filter change
        document.getElementById('platformFilter')?.addEventListener('change', (e) => {
            this.refreshComparison();
        });

        // Business filter change
        document.getElementById('businessFilter')?.addEventListener('change', (e) => {
            this.refreshComparison();
        });

        // Refresh button
        document.getElementById('refreshComparison')?.addEventListener('click', (e) => {
            e.preventDefault();
            this.refreshComparison();
        });

        // Custom comparison form
        document.getElementById('runCustomComparison')?.addEventListener('click', (e) => {
            e.preventDefault();
            this.runCustomComparison();
        });

        // Date input validation
        this.initializeDateValidation();
    }

    /**
     * Initialize date input validation for custom range comparison
     */
    initializeDateValidation() {
        const currentStart = document.getElementById('currentStart');
        const currentEnd = document.getElementById('currentEnd');
        const previousStart = document.getElementById('previousStart');
        const previousEnd = document.getElementById('previousEnd');

        if (currentStart && currentEnd) {
            currentStart.addEventListener('change', () => {
                if (currentEnd.value && currentStart.value > currentEnd.value) {
                    currentEnd.value = currentStart.value;
                }
            });

            currentEnd.addEventListener('change', () => {
                if (currentStart.value && currentEnd.value < currentStart.value) {
                    currentStart.value = currentEnd.value;
                }
            });
        }

        if (previousStart && previousEnd) {
            previousStart.addEventListener('change', () => {
                if (previousEnd.value && previousStart.value > previousEnd.value) {
                    previousEnd.value = previousStart.value;
                }
            });

            previousEnd.addEventListener('change', () => {
                if (previousStart.value && previousEnd.value < previousStart.value) {
                    previousStart.value = previousEnd.value;
                }
            });
        }
    }

    /**
     * Get business ID from URL parameters
     */
    getBusinessIdFromUrl() {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get('businessId') ? parseInt(urlParams.get('businessId')) : null;
    }

    /**
     * Load user businesses for filter dropdown
     */
    async loadBusinesses() {
        try {
            const response = await fetch('/Client/Analytics/GetUserBusinesses');
            if (response.ok) {
                const businesses = await response.json();
                this.populateBusinessFilter(businesses);
            }
        } catch (error) {
            console.error('Error loading businesses:', error);
        }
    }

    /**
     * Populate business filter dropdown
     */
    populateBusinessFilter(businesses) {
        const businessFilter = document.getElementById('businessFilter');
        if (!businessFilter || !businesses || businesses.length === 0) return;

        // Clear existing options except "All Businesses"
        while (businessFilter.children.length > 1) {
            businessFilter.removeChild(businessFilter.lastChild);
        }

        // Add business options
        businesses.forEach(business => {
            const option = document.createElement('option');
            option.value = business.id;
            option.textContent = business.name;
            businessFilter.appendChild(option);
        });
    }

    /**
     * Initialize the comparison chart
     */
    initializeChart() {
        const ctx = document.getElementById('comparisonChart');
        if (!ctx) return;

        // Check if Chart.js is available
        if (typeof Chart === 'undefined') {
            this.showChartJsUnavailable();
            return;
        }

        this.chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: []
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false // We have our own legend
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#33658a',
                        borderWidth: 1,
                        cornerRadius: 8,
                        displayColors: true
                    }
                },
                scales: {
                    x: {
                        display: true,
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#6c757d',
                            font: {
                                size: 12
                            }
                        }
                    },
                    y: {
                        display: true,
                        grid: {
                            color: '#e9ecef',
                            drawBorder: false
                        },
                        ticks: {
                            color: '#6c757d',
                            font: {
                                size: 12
                            },
                            callback: function(value) {
                                return value.toLocaleString();
                            }
                        }
                    }
                },
                interaction: {
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false
                },
                elements: {
                    point: {
                        radius: 4,
                        hoverRadius: 6
                    },
                    line: {
                        tension: 0.4
                    }
                }
            }
        });
    }

    /**
     * Show message when Chart.js is unavailable
     */
    showChartJsUnavailable() {
        const chartContainer = document.querySelector('.chart-container');
        if (chartContainer) {
            chartContainer.innerHTML = `
                <div style="text-align: center; padding: 2rem; color: #6c757d;">
                    <i class="fas fa-chart-line" style="font-size: 3rem; margin-bottom: 1rem;"></i>
                    <p>Chart visualization is currently unavailable.</p>
                    <p>Please refresh the page to try again.</p>
                </div>
            `;
        }
    }

    /**
     * Refresh comparison data
     */
    async refreshComparison() {
        this.showLoading();

        try {
            const comparisonType = document.getElementById('comparisonType')?.value || 'MonthOverMonth';
            const platform = document.getElementById('platformFilter')?.value || 'All';
            const businessId = document.getElementById('businessFilter')?.value || this.businessId;

            const url = new URL('/Client/Analytics/ComparativeAnalysisData', window.location.origin);
            url.searchParams.set('comparisonType', comparisonType);
            if (businessId) url.searchParams.set('businessId', businessId);
            if (platform !== 'All') url.searchParams.set('platform', platform);

            const response = await fetch(url);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.updateDashboard(data);
            this.hideLoading();
        } catch (error) {
            console.error('Error refreshing comparison:', error);
            this.showError();
            this.hideLoading();
        }
    }

    /**
     * Run custom range comparison
     */
    async runCustomComparison() {
        const currentStart = document.getElementById('currentStart')?.value;
        const currentEnd = document.getElementById('currentEnd')?.value;
        const previousStart = document.getElementById('previousStart')?.value;
        const previousEnd = document.getElementById('previousEnd')?.value;

        if (!currentStart || !currentEnd || !previousStart || !previousEnd) {
            this.showCustomRangeError('Please fill in all date fields.');
            return;
        }

        this.showLoading();

        try {
            const businessId = document.getElementById('businessFilter')?.value || this.businessId;
            const platform = document.getElementById('platformFilter')?.value || 'All';

            const request = {
                comparisonType: 'CustomRange',
                businessId: businessId ? parseInt(businessId) : null,
                currentPeriodStart: currentStart,
                currentPeriodEnd: currentEnd,
                previousPeriodStart: previousStart,
                previousPeriodEnd: previousEnd,
                platform: platform === 'All' ? null : platform
            };

            const response = await fetch('/Client/Analytics/CustomRangeComparison', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(request)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.updateDashboard(data);
            this.hideLoading();
        } catch (error) {
            console.error('Error running custom comparison:', error);
            this.showError();
            this.hideLoading();
        }
    }

    /**
     * Update the dashboard with new data
     */
    updateDashboard(data) {
        this.currentData = data;
        this.updateOverviewCards(data);
        this.updateChart(data.chartData);
        this.updatePeriodDetails(data);
        this.updateBusinessComparison(data.businessData);
        this.updateInsights(data.insights);
    }

    /**
     * Update overview cards with comparison data
     */
    updateOverviewCards(data) {
        // Update trend indicator
        const trendIndicator = document.querySelector('.trend-indicator');
        if (trendIndicator) {
            trendIndicator.textContent = data.comparisonMetrics.overallTrend;
            trendIndicator.className = `trend-indicator ${data.comparisonMetrics.overallTrend.toLowerCase()}`;
        }

        // Update performance rating
        const performanceRating = document.querySelector('.metric .value');
        if (performanceRating) {
            performanceRating.textContent = data.comparisonMetrics.performanceRating;
            performanceRating.className = `value ${data.comparisonMetrics.performanceRating.toLowerCase()}`;
        }

        // Update key changes
        const keyChangesContainer = document.querySelector('.key-changes');
        if (keyChangesContainer) {
            keyChangesContainer.innerHTML = '';
            data.comparisonMetrics.keyChanges.slice(0, 3).forEach(change => {
                const badge = document.createElement('span');
                badge.className = 'change-badge';
                badge.textContent = change;
                keyChangesContainer.appendChild(badge);
            });
        }

        // Update views comparison
        this.updateComparisonCard('Views', data.comparisonMetrics.viewsChangePercent, data.currentPeriod.totalViews, data.previousPeriod.totalViews, data.currentPeriod.averageViewsPerDay);

        // Update reviews comparison
        this.updateComparisonCard('Reviews', data.comparisonMetrics.reviewsChangePercent, data.currentPeriod.totalReviews, data.previousPeriod.totalReviews, data.currentPeriod.averageReviewsPerDay);

        // Update rating comparison
        this.updateComparisonCard('Average Rating', data.comparisonMetrics.ratingChangePercent, data.currentPeriod.averageRating, data.previousPeriod.averageRating, data.currentPeriod.engagementScore, true);
    }

    /**
     * Update a specific comparison card
     */
    updateComparisonCard(title, changePercent, currentValue, previousValue, averageValue, isRating = false) {
        const card = Array.from(document.querySelectorAll('.overview-card')).find(card => 
            card.querySelector('h3')?.textContent === title
        );

        if (!card) return;

        // Update change percentage
        const changeElement = card.querySelector('.change-percent');
        if (changeElement) {
            changeElement.textContent = `${changePercent >= 0 ? '+' : ''}${changePercent.toFixed(1)}%`;
            changeElement.className = `change-percent ${changePercent >= 0 ? 'positive' : 'negative'}`;
        }

        // Update values
        const stats = card.querySelectorAll('.stat .value');
        if (stats.length >= 3) {
            stats[0].textContent = isRating ? currentValue.toFixed(1) : currentValue.toLocaleString();
            stats[1].textContent = isRating ? previousValue.toFixed(1) : previousValue.toLocaleString();
            stats[2].textContent = isRating ? averageValue.toFixed(1) : averageValue.toFixed(1);
        }
    }

    /**
     * Update the comparison chart
     */
    updateChart(chartData) {
        if (!this.chart || !chartData) return;

        this.chart.data.labels = chartData.labels;
        this.chart.data.datasets = chartData.datasets;
        this.chart.update('none'); // Update without animation for better performance
    }

    /**
     * Update period details section
     */
    updatePeriodDetails(data) {
        // Update current period
        this.updatePeriodCard('.current-period', data.currentPeriod);
        
        // Update previous period
        this.updatePeriodCard('.previous-period', data.previousPeriod);
    }

    /**
     * Update a specific period card
     */
    updatePeriodCard(selector, periodData) {
        const card = document.querySelector(selector);
        if (!card) return;

        // Update dates
        const datesElement = card.querySelector('.period-dates');
        if (datesElement) {
            const startDate = new Date(periodData.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
            const endDate = new Date(periodData.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            datesElement.textContent = `${startDate} - ${endDate}`;
        }

        // Update stats
        const stats = card.querySelectorAll('.stat-row');
        if (stats.length >= 5) {
            stats[0].querySelector('.stat-value').textContent = periodData.totalViews.toLocaleString();
            stats[1].querySelector('.stat-value').textContent = periodData.totalReviews.toLocaleString();
            stats[2].querySelector('.stat-value').textContent = periodData.totalFavorites.toLocaleString();
            stats[3].querySelector('.stat-value').textContent = periodData.averageRating.toFixed(1);
            stats[4].querySelector('.stat-value').textContent = periodData.engagementScore.toFixed(1);

            // Update peak day if available
            if (stats.length >= 6 && periodData.peakDayDate) {
                const peakDate = new Date(periodData.peakDayDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
                stats[5].querySelector('.stat-value').textContent = `${peakDate} (${periodData.peakDayViews} views)`;
            }
        }
    }

    /**
     * Update business comparison section
     */
    updateBusinessComparison(businessData) {
        if (!businessData) {
            const businessComparison = document.querySelector('.business-comparison');
            if (businessComparison) {
                businessComparison.style.display = 'none';
            }
            return;
        }

        const businessComparison = document.querySelector('.business-comparison');
        if (businessComparison) {
            businessComparison.style.display = 'block';
        }

        // Update business name
        const businessName = document.querySelector('.business-comparison h2');
        if (businessName) {
            businessName.textContent = `Business Comparison: ${businessData.businessName}`;
        }

        // Update category comparison
        this.updateBenchmarkCard('.comparison-card:first-child', businessData.categoryComparison);

        // Update portfolio comparison
        this.updateBenchmarkCard('.comparison-card:last-child', businessData.portfolioComparison, true);
    }

    /**
     * Update a benchmark comparison card
     */
    updateBenchmarkCard(selector, benchmarkData, isPortfolio = false) {
        const card = document.querySelector(selector);
        if (!card) return;

        if (isPortfolio) {
            // Update portfolio rank
            const rankElement = card.querySelector('.portfolio-rank');
            if (rankElement) {
                rankElement.textContent = `Rank #${benchmarkData.portfolioRank} of ${benchmarkData.totalPortfolioBusinesses}`;
            }
        } else {
            // Update category performance indicator
            const indicator = card.querySelector('.performance-indicator');
            if (indicator) {
                indicator.textContent = benchmarkData.categoryPerformance;
                indicator.className = `performance-indicator ${benchmarkData.categoryPerformance.toLowerCase().replace(' ', '-')}`;
            }
        }

        // Update benchmark stats
        const benchmarkItems = card.querySelectorAll('.benchmark-item');
        if (benchmarkItems.length >= 3) {
            const viewsData = isPortfolio ? benchmarkData.viewsVsPortfolio : benchmarkData.viewsVsCategory;
            const reviewsData = isPortfolio ? benchmarkData.reviewsVsPortfolio : benchmarkData.reviewsVsCategory;
            const ratingData = isPortfolio ? benchmarkData.ratingVsPortfolio : benchmarkData.ratingVsCategory;

            this.updateBenchmarkItem(benchmarkItems[0], 'Views', viewsData);
            this.updateBenchmarkItem(benchmarkItems[1], 'Reviews', reviewsData);
            this.updateBenchmarkItem(benchmarkItems[2], 'Rating', ratingData);
        }
    }

    /**
     * Update a benchmark item
     */
    updateBenchmarkItem(item, label, value) {
        const labelElement = item.querySelector('.label');
        const valueElement = item.querySelector('.value');

        if (labelElement) {
            labelElement.textContent = `${label} vs ${label === 'Views' ? 'Portfolio' : 'Category'}:`;
        }

        if (valueElement) {
            valueElement.textContent = `${value >= 0 ? '+' : ''}${value.toFixed(1)}%`;
            valueElement.className = `value ${value >= 0 ? 'positive' : 'negative'}`;
        }
    }

    /**
     * Update insights section
     */
    updateInsights(insights) {
        const insightsSection = document.querySelector('.insights-section');
        if (!insightsSection) return;

        if (!insights || insights.length === 0) {
            insightsSection.style.display = 'none';
            return;
        }

        insightsSection.style.display = 'block';
        const insightsGrid = insightsSection.querySelector('.insights-grid');
        if (!insightsGrid) return;

        insightsGrid.innerHTML = '';
        insights.forEach(insight => {
            const insightCard = document.createElement('div');
            insightCard.className = 'insight-card';
            insightCard.innerHTML = `
                <i class="fas fa-lightbulb"></i>
                <p>${insight}</p>
            `;
            insightsGrid.appendChild(insightCard);
        });
    }

    /**
     * Show loading state
     */
    showLoading() {
        const loading = document.getElementById('comparisonLoading');
        if (loading) {
            loading.style.display = 'flex';
        }
    }

    /**
     * Hide loading state
     */
    hideLoading() {
        const loading = document.getElementById('comparisonLoading');
        if (loading) {
            loading.style.display = 'none';
        }
    }

    /**
     * Show error state
     */
    showError() {
        const error = document.getElementById('comparisonError');
        if (error) {
            error.style.display = 'block';
        }
    }

    /**
     * Show custom range error
     */
    showCustomRangeError(message) {
        // Create or update error message
        let errorDiv = document.querySelector('.custom-range-error');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'custom-range-error';
            errorDiv.style.cssText = 'background-color: #f8d7da; border: 2px solid #dc3545; border-radius: 8px; padding: 1rem; margin-bottom: 1rem; color: #721c24; text-align: center;';
            const form = document.querySelector('.custom-range-form');
            if (form) {
                form.insertBefore(errorDiv, form.firstChild);
            }
        }
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';

        // Hide error after 5 seconds
        setTimeout(() => {
            errorDiv.style.display = 'none';
        }, 5000);
    }

    /**
     * Get anti-forgery token for POST requests
     */
    getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    new ComparativeAnalytics();
});
