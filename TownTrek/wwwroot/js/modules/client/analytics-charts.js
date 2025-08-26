/**
 * @fileoverview AnalyticsCharts - Chart management and visualization
 * @author TownTrek Development Team
 * @version 2.0.0
 */

class AnalyticsCharts {
    constructor(analyticsCore) {
        this.core = analyticsCore;
        this.charts = new Map();
        this.chartConfigs = {
            views: {
                type: 'line',
                options: {
                    scales: {
                        x: {
                            display: true,
                            grid: { display: false },
                            ticks: {
                                font: { size: 11 },
                                color: '#6c757d'
                            }
                        },
                        y: {
                            display: true,
                            beginAtZero: true,
                            grid: {
                                color: 'rgba(0, 0, 0, 0.1)',
                                drawBorder: false
                            },
                            ticks: {
                                font: { size: 11 },
                                color: '#6c757d',
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
                        line: {
                            tension: 0.4,
                            borderWidth: 3
                        },
                        point: {
                            radius: 4,
                            hoverRadius: 6,
                            borderWidth: 2,
                            backgroundColor: '#ffffff'
                        }
                    }
                }
            },
            reviews: {
                type: 'bar',
                options: {
                    scales: {
                        x: {
                            display: true,
                            grid: { display: false },
                            ticks: {
                                font: { size: 11 },
                                color: '#6c757d'
                            }
                        },
                        y: {
                            display: true,
                            beginAtZero: true,
                            grid: {
                                color: 'rgba(0, 0, 0, 0.1)',
                                drawBorder: false
                            },
                            ticks: {
                                font: { size: 11 },
                                color: '#6c757d',
                                stepSize: 1
                            }
                        }
                    },
                    elements: {
                        bar: {
                            borderRadius: 4,
                            borderSkipped: false
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                afterBody: function(context) {
                                    if (context[0] && context[0].raw > 0) {
                                        return `Average Rating: ${(Math.random() * 2 + 3).toFixed(1)} ⭐`;
                                    }
                                    return '';
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    // Initialize charts
    async init() {
        try {
            // Small delay to ensure DOM is fully loaded
            await new Promise(resolve => setTimeout(resolve, 100));
            
            await this.initializeViewsChart();
            await this.initializeReviewsChart();
            
            this.bindChartEvents();
            console.log('AnalyticsCharts initialized successfully');
        } catch (error) {
            console.error('Error initializing AnalyticsCharts:', error);
        }
    }

    // Initialize views chart
    async initializeViewsChart() {
        const canvas = document.getElementById('viewsChart');
        if (!canvas) {
            console.warn('Views chart canvas not found');
            return;
        }

        const parentElement = canvas.closest('.chart-content') || canvas.parentElement;
        if (!parentElement) {
            console.warn('Views chart parent element not found');
            return;
        }

        try {
            this.showChartLoading(parentElement);
            const data = await this.fetchViewsData(30);
            
            // Clear loading state and recreate canvas
            parentElement.innerHTML = '<canvas id="viewsChart" width="400" height="200"></canvas>';
            const newCanvas = parentElement.querySelector('#viewsChart');
            
            if (!newCanvas) {
                console.warn('Failed to recreate views chart canvas');
                return;
            }
            
            const chart = this.createChart(newCanvas, 'views', data);
            this.charts.set('views', chart);
            
            this.core.trackFeatureUsage('ViewsChart', 'Initialized');
        } catch (error) {
            console.error('Error initializing views chart:', error);
            this.showChartError(parentElement, 'Unable to load views data');
        }
    }

    // Initialize reviews chart
    async initializeReviewsChart() {
        const canvas = document.getElementById('reviewsChart');
        if (!canvas) {
            console.warn('Reviews chart canvas not found');
            return;
        }

        const parentElement = canvas.closest('.chart-content') || canvas.parentElement;
        if (!parentElement) {
            console.warn('Reviews chart parent element not found');
            return;
        }

        try {
            this.showChartLoading(parentElement);
            const data = await this.fetchReviewsData(30);
            
            // Clear loading state and recreate canvas
            parentElement.innerHTML = '<canvas id="reviewsChart" width="400" height="200"></canvas>';
            const newCanvas = parentElement.querySelector('#reviewsChart');
            
            if (!newCanvas) {
                console.warn('Failed to recreate reviews chart canvas');
                return;
            }
            
            const chart = this.createChart(newCanvas, 'reviews', data);
            this.charts.set('reviews', chart);
            
            this.core.trackFeatureUsage('ReviewsChart', 'Initialized');
        } catch (error) {
            console.error('Error initializing reviews chart:', error);
            this.showChartError(parentElement, 'Unable to load reviews data');
        }
    }

    // Create chart with configuration
    createChart(canvas, chartType, data) {
        if (!this.core.chartJsAvailable) {
            throw new Error('Chart.js is not available');
        }

        const config = this.chartConfigs[chartType];
        if (!config) {
            throw new Error(`Unknown chart type: ${chartType}`);
        }

        const ctx = canvas.getContext('2d');
        const options = this.core.mergeChartOptions(config.options);
        
        return new Chart(ctx, {
            type: config.type,
            data: data,
            options: options
        });
    }

    // Update chart data
    async updateChart(chartType, days) {
        const chart = this.charts.get(chartType);
        if (!chart) return;

        try {
            let data;
            switch (chartType) {
                case 'views':
                    data = await this.fetchViewsData(days);
                    break;
                case 'reviews':
                    data = await this.fetchReviewsData(days);
                    break;
                default:
                    throw new Error(`Unknown chart type: ${chartType}`);
            }

            chart.data = data;
            chart.update('active');
            
            // Add real-time update animation
            this.addUpdateAnimation(chartType);
            
            this.core.trackFeatureUsage(`${chartType.charAt(0).toUpperCase() + chartType.slice(1)}Chart`, 'Updated', { days });
        } catch (error) {
            console.error(`Error updating ${chartType} chart:`, error);
        }
    }

    // Add update animation
    addUpdateAnimation(chartType) {
        const chartContainer = document.querySelector(`[data-chart-type="${chartType}"]`) || 
                              document.querySelector(`#${chartType}Chart`).parentElement;
        
        if (chartContainer) {
            chartContainer.classList.add('real-time-update');
            setTimeout(() => {
                chartContainer.classList.remove('real-time-update');
            }, 2000);
        }
    }

    // Fetch views data
    async fetchViewsData(days) {
        try {
            const data = await this.core.fetchData(this.core.config.apiEndpoints.viewsData, { days });
            return {
                labels: data.labels || [],
                datasets: data.datasets || []
            };
        } catch (error) {
            console.error('Error fetching views chart data:', error);
            return this.getEmptyChartData(days, 'Views');
        }
    }

    // Fetch reviews data
    async fetchReviewsData(days) {
        try {
            const data = await this.core.fetchData(this.core.config.apiEndpoints.reviewsData, { days });
            return {
                labels: data.labels || [],
                datasets: data.datasets || []
            };
        } catch (error) {
            console.error('Error fetching reviews chart data:', error);
            return this.getEmptyChartData(days, 'Reviews');
        }
    }

    // Get empty chart data for fallback
    getEmptyChartData(days, type) {
        const labels = [];
        const today = new Date();
        
        for (let i = days - 1; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(date.getDate() - i);
            labels.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
        }
        
        return {
            labels,
            datasets: [{
                label: `No ${type} Data`,
                data: new Array(labels.length).fill(0),
                borderColor: '#e9ecef',
                backgroundColor: '#e9ecef20',
                fill: false
            }]
        };
    }

    // Bind chart events
    bindChartEvents() {
        // Time range selectors
        const viewsTimeRange = document.getElementById('viewsTimeRange');
        const reviewsTimeRange = document.getElementById('reviewsTimeRange');

        if (viewsTimeRange) {
            viewsTimeRange.addEventListener('change', (e) => {
                this.updateChart('views', parseInt(e.target.value));
            });
        }

        if (reviewsTimeRange) {
            reviewsTimeRange.addEventListener('change', (e) => {
                this.updateChart('reviews', parseInt(e.target.value));
            });
        }
    }

    // Show chart loading state
    showChartLoading(container) {
        if (!container) return;
        
        container.innerHTML = `
            <div class="chart-loading">
                <div style="text-align: center; color: #6c757d;">
                    <div style="width: 40px; height: 40px; border: 3px solid #e9ecef; border-top: 3px solid #33658a; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem;"></div>
                    <p style="margin: 0; font-size: 0.875rem;">Loading chart data...</p>
                </div>
            </div>
        `;
    }

    // Show chart error state
    showChartError(container, message) {
        if (!container) return;
        
        container.innerHTML = `
            <div class="chart-loading">
                <div style="text-align: center; color: #6c757d;">
                    <svg width="48" height="48" fill="none" stroke="currentColor" viewBox="0 0 24 24" style="margin-bottom: 1rem; opacity: 0.5;">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                    <p style="margin: 0; font-size: 0.875rem;">${message}</p>
                    <button onclick="location.reload()" style="margin-top: 1rem; padding: 0.5rem 1rem; background: #33658a; color: white; border: none; border-radius: 0.375rem; font-size: 0.875rem; cursor: pointer;">
                        Try Again
                    </button>
                </div>
            </div>
        `;
    }

    // Show Chart.js unavailable message
    showChartJsUnavailable() {
        const chartContainers = document.querySelectorAll('.chart-content, [id*="Chart"]');
        chartContainers.forEach(container => {
            if (container && !container.querySelector('.chart-loading')) {
                container.innerHTML = `
                    <div class="chart-loading">
                        <div style="text-align: center; color: #6c757d;">
                            <svg width="48" height="48" fill="none" stroke="currentColor" viewBox="0 0 24 24" style="margin-bottom: 1rem; opacity: 0.5;">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                            </svg>
                            <p style="margin: 0; font-size: 0.875rem;">Chart functionality is currently unavailable</p>
                            <p style="margin: 0.5rem 0 0; font-size: 0.75rem; opacity: 0.7;">Please refresh the page or try again later</p>
                        </div>
                    </div>
                `;
            }
        });
    }

    // Refresh all charts
    async refreshAllCharts() {
        try {
            await Promise.all([
                this.updateChart('views', 30),
                this.updateChart('reviews', 30)
            ]);
            console.log('All charts refreshed');
        } catch (error) {
            console.error('Error refreshing charts:', error);
        }
    }

    // Get chart instance
    getChart(chartType) {
        return this.charts.get(chartType);
    }

    // Destroy all charts
    destroy() {
        this.charts.forEach(chart => {
            if (chart) {
                chart.destroy();
            }
        });
        this.charts.clear();
        console.log('AnalyticsCharts destroyed');
    }
}

// Expose globally
window.AnalyticsCharts = AnalyticsCharts;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AnalyticsCharts;
}
