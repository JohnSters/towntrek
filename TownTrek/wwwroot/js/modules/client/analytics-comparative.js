/**
 * @fileoverview AnalyticsComparative - Comparative analysis functionality
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class AnalyticsComparative {
    constructor(analyticsCore) {
        this.core = analyticsCore;
        this.isInitialized = false;
        this.currentBusinessId = null;
        this.config = {
            apiEndpoints: {
                comparativeAnalysisData: '/Client/BusinessAnalytics/ComparativeAnalysisData',
                yearOverYearComparison: '/Client/BusinessAnalytics/YearOverYearComparison',
                customRangeComparison: '/Client/BusinessAnalytics/CustomRangeComparison'
            }
        };
    }

    // Static method to check if comparative analysis should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-container') !== null && 
               document.querySelector('#comparisonType') !== null;
    }

    init() {
        if (this.isInitialized) return;
        
        try {
            this.bindEvents();
            this.initializeFormDefaults();
            this.isInitialized = true;
            
            console.log('AnalyticsComparative initialized successfully');
        } catch (error) {
            console.error('Error initializing AnalyticsComparative:', error);
        }
    }

    bindEvents() {
        // Comparison type change
        const comparisonTypeSelect = document.getElementById('comparisonType');
        if (comparisonTypeSelect) {
            comparisonTypeSelect.addEventListener('change', (e) => {
                this.handleComparisonTypeChange(e.target.value);
            });
        }

        // Platform filter change
        const platformFilterSelect = document.getElementById('platformFilter');
        if (platformFilterSelect) {
            platformFilterSelect.addEventListener('change', (e) => {
                this.handlePlatformFilterChange(e.target.value);
            });
        }

        // Business filter change
        const businessFilterSelect = document.getElementById('businessFilter');
        if (businessFilterSelect) {
            businessFilterSelect.addEventListener('change', (e) => {
                this.handleBusinessFilterChange(e.target.value);
            });
        }

        // Refresh comparison button
        const refreshButton = document.getElementById('refreshComparison');
        if (refreshButton) {
            refreshButton.addEventListener('click', () => {
                this.refreshComparison();
            });
        }

        // Custom range comparison form
        const runCustomComparisonButton = document.getElementById('runCustomComparison');
        if (runCustomComparisonButton) {
            runCustomComparisonButton.addEventListener('click', () => {
                this.runCustomComparison();
            });
        }

        // Date input validation
        this.setupDateValidation();
    }

    initializeFormDefaults() {
        // Set default dates for custom range comparison
        const today = new Date();
        const thirtyDaysAgo = new Date(today);
        thirtyDaysAgo.setDate(today.getDate() - 30);
        
        const sixtyDaysAgo = new Date(today);
        sixtyDaysAgo.setDate(today.getDate() - 60);
        
        const currentStartInput = document.getElementById('currentStart');
        const currentEndInput = document.getElementById('currentEnd');
        const previousStartInput = document.getElementById('previousStart');
        const previousEndInput = document.getElementById('previousEnd');

        if (currentStartInput) {
            currentStartInput.value = this.formatDateForInput(thirtyDaysAgo);
        }
        if (currentEndInput) {
            currentEndInput.value = this.formatDateForInput(today);
        }
        if (previousStartInput) {
            previousStartInput.value = this.formatDateForInput(sixtyDaysAgo);
        }
        if (previousEndInput) {
            previousEndInput.value = this.formatDateForInput(thirtyDaysAgo);
        }
    }

    setupDateValidation() {
        const currentStartInput = document.getElementById('currentStart');
        const currentEndInput = document.getElementById('currentEnd');
        const previousStartInput = document.getElementById('previousStart');
        const previousEndInput = document.getElementById('previousEnd');

        // Validate current period dates
        if (currentStartInput && currentEndInput) {
            currentEndInput.addEventListener('change', () => {
                if (currentStartInput.value && currentEndInput.value) {
                    const startDate = new Date(currentStartInput.value);
                    const endDate = new Date(currentEndInput.value);
                    
                    if (endDate <= startDate) {
                        this.showError('Current period end date must be after start date');
                        currentEndInput.value = '';
                    }
                }
            });
        }

        // Validate previous period dates
        if (previousStartInput && previousEndInput) {
            previousEndInput.addEventListener('change', () => {
                if (previousStartInput.value && previousEndInput.value) {
                    const startDate = new Date(previousStartInput.value);
                    const endDate = new Date(previousEndInput.value);
                    
                    if (endDate <= startDate) {
                        this.showError('Previous period end date must be after start date');
                        previousEndInput.value = '';
                    }
                }
            });
        }
    }

    async handleComparisonTypeChange(comparisonType) {
        try {
            this.showLoading();
            
            const params = {
                comparisonType: comparisonType,
                businessId: this.currentBusinessId,
                platform: document.getElementById('platformFilter')?.value || 'All'
            };

            const response = await this.core.fetchData(this.config.apiEndpoints.comparativeAnalysisData, params);
            
            if (response.success) {
                this.updateComparisonData(response.data);
                this.hideLoading();
                
                if (this.core) {
                    this.core.trackFeatureUsage('comparative-analysis', 'type-changed', { 
                        comparisonType: comparisonType 
                    });
                }
            } else {
                this.showError(response.message || 'Failed to load comparison data');
            }
        } catch (error) {
            console.error('Error changing comparison type:', error);
            this.showError('Failed to load comparison data');
        }
    }

    async handlePlatformFilterChange(platform) {
        try {
            this.showLoading();
            
            const comparisonType = document.getElementById('comparisonType')?.value || 'MonthOverMonth';
            const params = {
                comparisonType: comparisonType,
                businessId: this.currentBusinessId,
                platform: platform
            };

            const response = await this.core.fetchData(this.config.apiEndpoints.comparativeAnalysisData, params);
            
            if (response.success) {
                this.updateComparisonData(response.data);
                this.hideLoading();
                
                if (this.core) {
                    this.core.trackFeatureUsage('comparative-analysis', 'platform-filter-changed', { 
                        platform: platform 
                    });
                }
            } else {
                this.showError(response.message || 'Failed to load comparison data');
            }
        } catch (error) {
            console.error('Error changing platform filter:', error);
            this.showError('Failed to load comparison data');
        }
    }

    async handleBusinessFilterChange(businessId) {
        this.currentBusinessId = businessId ? parseInt(businessId) : null;
        await this.refreshComparison();
    }

    async refreshComparison() {
        try {
            this.showLoading();
            
            const comparisonType = document.getElementById('comparisonType')?.value || 'MonthOverMonth';
            const platform = document.getElementById('platformFilter')?.value || 'All';
            
            const params = {
                comparisonType: comparisonType,
                businessId: this.currentBusinessId,
                platform: platform
            };

            const response = await this.core.fetchData(this.config.apiEndpoints.comparativeAnalysisData, params);
            
            if (response.success) {
                this.updateComparisonData(response.data);
                this.hideLoading();
                
                if (this.core) {
                    this.core.trackFeatureUsage('comparative-analysis', 'refreshed', { 
                        comparisonType: comparisonType,
                        businessId: this.currentBusinessId,
                        platform: platform
                    });
                }
            } else {
                this.showError(response.message || 'Failed to refresh comparison data');
            }
        } catch (error) {
            console.error('Error refreshing comparison:', error);
            this.showError('Failed to refresh comparison data');
        }
    }

    async runCustomComparison() {
        try {
            // Validate form inputs
            const currentStart = document.getElementById('currentStart')?.value;
            const currentEnd = document.getElementById('currentEnd')?.value;
            const previousStart = document.getElementById('previousStart')?.value;
            const previousEnd = document.getElementById('previousEnd')?.value;

            if (!currentStart || !currentEnd || !previousStart || !previousEnd) {
                this.showError('Please fill in all date fields');
                return;
            }

            this.showLoading();

            const requestData = {
                comparisonType: 'CustomRange',
                businessId: this.currentBusinessId,
                currentPeriodStart: currentStart,
                currentPeriodEnd: currentEnd,
                previousPeriodStart: previousStart,
                previousPeriodEnd: previousEnd,
                platform: document.getElementById('platformFilter')?.value || 'All'
            };

            const response = await fetch(this.config.apiEndpoints.customRangeComparison, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.core.getAntiForgeryToken()
                },
                body: JSON.stringify(requestData)
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.updateComparisonData(data.data);
                    this.hideLoading();
                    
                    if (this.core) {
                        this.core.trackFeatureUsage('comparative-analysis', 'custom-range-comparison', { 
                            businessId: this.currentBusinessId,
                            currentPeriodStart: currentStart,
                            currentPeriodEnd: currentEnd
                        });
                    }
                } else {
                    this.showError(data.message || 'Failed to run custom comparison');
                }
            } else {
                this.showError('Failed to run custom comparison');
            }
        } catch (error) {
            console.error('Error running custom comparison:', error);
            this.showError('Failed to run custom comparison');
        }
    }

    updateComparisonData(data) {
        // Update overview metrics
        this.updateOverviewMetrics(data);
        
        // Update charts
        this.updateCharts(data);
        
        // Update period details
        this.updatePeriodDetails(data);
        
        // Update business comparison
        this.updateBusinessComparison(data);
        
        // Update insights
        this.updateInsights(data);
    }

    updateOverviewMetrics(data) {
        const metricsContainer = document.querySelector('.comparative-overview');
        if (!metricsContainer || !data.comparisonMetrics) return;

        // Update growth percentages
        const viewsGrowth = metricsContainer.querySelector('.views-growth');
        if (viewsGrowth && data.comparisonMetrics.viewsGrowthPercentage !== undefined) {
            const growth = data.comparisonMetrics.viewsGrowthPercentage;
            viewsGrowth.textContent = `${growth > 0 ? '+' : ''}${growth.toFixed(1)}%`;
            viewsGrowth.className = `views-growth ${growth >= 0 ? 'positive' : 'negative'}`;
        }

        const reviewsGrowth = metricsContainer.querySelector('.reviews-growth');
        if (reviewsGrowth && data.comparisonMetrics.reviewsGrowthPercentage !== undefined) {
            const growth = data.comparisonMetrics.reviewsGrowthPercentage;
            reviewsGrowth.textContent = `${growth > 0 ? '+' : ''}${growth.toFixed(1)}%`;
            reviewsGrowth.className = `reviews-growth ${growth >= 0 ? 'positive' : 'negative'}`;
        }

        const ratingGrowth = metricsContainer.querySelector('.rating-growth');
        if (ratingGrowth && data.comparisonMetrics.ratingGrowthPercentage !== undefined) {
            const growth = data.comparisonMetrics.ratingGrowthPercentage;
            ratingGrowth.textContent = `${growth > 0 ? '+' : ''}${growth.toFixed(1)}%`;
            ratingGrowth.className = `rating-growth ${growth >= 0 ? 'positive' : 'negative'}`;
        }
    }

    updateCharts(data) {
        if (!data.chartData || !window.Chart) return;

        // Update comparison chart
        const chartCanvas = document.getElementById('comparisonChart');
        if (chartCanvas) {
            const ctx = chartCanvas.getContext('2d');
            
            // Destroy existing chart if it exists
            if (window.comparisonChart) {
                window.comparisonChart.destroy();
            }

            window.comparisonChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.chartData.labels || [],
                    datasets: data.chartData.datasets || []
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    plugins: {
                        legend: {
                            position: 'top'
                        }
                    }
                }
            });
        }
    }

    updatePeriodDetails(data) {
        const periodDetailsContainer = document.querySelector('.period-details');
        if (!periodDetailsContainer) return;

        // Update current period data
        if (data.currentPeriod) {
            const currentViews = periodDetailsContainer.querySelector('.current-views');
            if (currentViews) {
                currentViews.textContent = data.currentPeriod.totalViews.toLocaleString();
            }

            const currentReviews = periodDetailsContainer.querySelector('.current-reviews');
            if (currentReviews) {
                currentReviews.textContent = data.currentPeriod.totalReviews.toString();
            }

            const currentRating = periodDetailsContainer.querySelector('.current-rating');
            if (currentRating) {
                currentRating.textContent = data.currentPeriod.averageRating.toFixed(1);
            }
        }

        // Update previous period data
        if (data.previousPeriod) {
            const previousViews = periodDetailsContainer.querySelector('.previous-views');
            if (previousViews) {
                previousViews.textContent = data.previousPeriod.totalViews.toLocaleString();
            }

            const previousReviews = periodDetailsContainer.querySelector('.previous-reviews');
            if (previousReviews) {
                previousReviews.textContent = data.previousPeriod.totalReviews.toString();
            }

            const previousRating = periodDetailsContainer.querySelector('.previous-rating');
            if (previousRating) {
                previousRating.textContent = data.previousPeriod.averageRating.toFixed(1);
            }
        }
    }

    updateBusinessComparison(data) {
        const businessComparisonContainer = document.querySelector('.business-comparison');
        if (!businessComparisonContainer || !data.businessData) return;

        // Update business-specific metrics
        const businessViews = businessComparisonContainer.querySelector('.business-views');
        if (businessViews && data.businessData.currentPeriodViews !== undefined) {
            businessViews.textContent = data.businessData.currentPeriodViews.toLocaleString();
        }

        const businessReviews = businessComparisonContainer.querySelector('.business-reviews');
        if (businessReviews && data.businessData.currentPeriodReviews !== undefined) {
            businessReviews.textContent = data.businessData.currentPeriodReviews.toString();
        }
    }

    updateInsights(data) {
        const insightsContainer = document.querySelector('.comparative-insights');
        if (!insightsContainer || !data.insights) return;

        insightsContainer.innerHTML = '';
        
        data.insights.forEach(insight => {
            const insightElement = document.createElement('div');
            insightElement.className = 'insight-item';
            insightElement.innerHTML = `
                <div class="insight-content">
                    <p>${insight}</p>
                </div>
            `;
            insightsContainer.appendChild(insightElement);
        });
    }

    showLoading() {
        const loadingElement = document.getElementById('comparisonLoading');
        if (loadingElement) {
            loadingElement.style.display = 'flex';
        }
    }

    hideLoading() {
        const loadingElement = document.getElementById('comparisonLoading');
        if (loadingElement) {
            loadingElement.style.display = 'none';
        }
    }

    showError(message) {
        this.hideLoading();
        
        const errorElement = document.getElementById('comparisonError');
        if (errorElement) {
            const messageElement = errorElement.querySelector('p');
            if (messageElement) {
                messageElement.textContent = message;
            }
            errorElement.style.display = 'block';
            
            // Auto-hide after 5 seconds
            setTimeout(() => {
                errorElement.style.display = 'none';
            }, 5000);
        } else {
            // Fallback to alert if error element not found
            alert(message);
        }
    }

    formatDateForInput(date) {
        return date.toISOString().split('T')[0];
    }

    destroy() {
        this.isInitialized = false;
        console.log('AnalyticsComparative destroyed');
    }
}

// Expose globally
window.AnalyticsComparative = AnalyticsComparative;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AnalyticsComparative;
}
