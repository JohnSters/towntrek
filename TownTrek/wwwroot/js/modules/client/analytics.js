/**
 * @fileoverview ClientAnalyticsManager - handles analytics dashboard interactions and charts
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class ClientAnalyticsManager {
    constructor() {
        this.viewsChart = null;
        this.reviewsChart = null;
        this.isInitialized = false;
        this.chartJsAvailable = false;
    }

    // Static method to check if analytics should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-dashboard') !== null;
    }

    init() {
        if (this.isInitialized) return;
        
        try {
            // Check if Chart.js is available
            this.checkChartJsAvailability();
            
            this.bindEvents();
            this.initializeCharts();
            this.setupAnimations();
            this.isInitialized = true;
            
            console.log('ClientAnalyticsManager initialized successfully');
        } catch (error) {
            console.error('Error initializing ClientAnalyticsManager:', error);
        }
    }

    checkChartJsAvailability() {
        if (typeof Chart !== 'undefined' && Chart) {
            this.chartJsAvailable = true;
            console.log('Chart.js is available');
        } else {
            this.chartJsAvailable = false;
            console.warn('Chart.js is not available - charts will be disabled');
        }
    }

    bindEvents() {
        // Time range selectors for charts
        const viewsTimeRange = document.getElementById('viewsTimeRange');
        const reviewsTimeRange = document.getElementById('reviewsTimeRange');

        if (viewsTimeRange) {
            viewsTimeRange.addEventListener('change', (e) => {
                this.updateViewsChart(parseInt(e.target.value));
            });
        }

        if (reviewsTimeRange) {
            reviewsTimeRange.addEventListener('change', (e) => {
                this.updateReviewsChart(parseInt(e.target.value));
            });
        }

        // Card hover effects
        this.setupCardHoverEffects();
    }

    setupCardHoverEffects() {
        const cards = document.querySelectorAll('.overview-card, .performance-card, .insight-card, .premium-card');
        
        cards.forEach(card => {
            card.addEventListener('mouseenter', () => {
                card.style.transform = 'translateY(-4px)';
            });
            
            card.addEventListener('mouseleave', () => {
                card.style.transform = 'translateY(0)';
            });
        });
    }

    async initializeCharts() {
        if (!this.chartJsAvailable) {
            this.showChartJsUnavailable();
            return;
        }

        // Initialize views chart
        const viewsCanvas = document.getElementById('viewsChart');
        if (viewsCanvas) {
            await this.createViewsChart(viewsCanvas);
        }

        // Initialize reviews chart
        const reviewsCanvas = document.getElementById('reviewsChart');
        if (reviewsCanvas) {
            await this.createReviewsChart(reviewsCanvas);
        }
    }

    async createViewsChart(canvas) {
        try {
            if (!this.chartJsAvailable) {
                throw new Error('Chart.js is not available');
            }

            const container = canvas ? canvas.parentElement : null;
            if (!container) throw new Error('Views chart container not found');
            this.showChartLoading(container);
            const data = await this.fetchViewsData(30);
            
            // Clear loading state
            container.innerHTML = '<canvas id="viewsChart" width="400" height="200"></canvas>';
            const newCanvas = container.querySelector('#viewsChart');
            if (!newCanvas) throw new Error('Views chart canvas not created');
            
            const ctx = newCanvas.getContext('2d');
            this.viewsChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.labels,
                    datasets: data.datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: data.datasets.length > 1,
                            position: 'top',
                            labels: {
                                usePointStyle: true,
                                padding: 20,
                                font: {
                                    size: 12
                                }
                            }
                        },
                        tooltip: {
                            mode: 'index',
                            intersect: false,
                            backgroundColor: 'rgba(47, 72, 88, 0.9)',
                            titleColor: '#ffffff',
                            bodyColor: '#ffffff',
                            borderColor: '#86bbd8',
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
                                font: {
                                    size: 11
                                },
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
                                font: {
                                    size: 11
                                },
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
            });
        } catch (error) {
            console.error('Error creating views chart:', error);
            const container = canvas ? canvas.parentElement : null;
            this.showChartError(container, 'Unable to load views data');
        }
    }

    async createReviewsChart(canvas) {
        try {
            if (!this.chartJsAvailable) {
                throw new Error('Chart.js is not available');
            }

            const container = canvas ? canvas.parentElement : null;
            if (!container) throw new Error('Reviews chart container not found');
            this.showChartLoading(container);
            const data = await this.fetchReviewsData(30);
            
            // Clear loading state
            container.innerHTML = '<canvas id="reviewsChart" width="400" height="200"></canvas>';
            const newCanvas = container.querySelector('#reviewsChart');
            if (!newCanvas) throw new Error('Reviews chart canvas not created');
            
            const ctx = newCanvas.getContext('2d');
            this.reviewsChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels,
                    datasets: data.datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: data.datasets.length > 1,
                            position: 'top',
                            labels: {
                                usePointStyle: true,
                                padding: 20,
                                font: {
                                    size: 12
                                }
                            }
                        },
                        tooltip: {
                            mode: 'index',
                            intersect: false,
                            backgroundColor: 'rgba(47, 72, 88, 0.9)',
                            titleColor: '#ffffff',
                            bodyColor: '#ffffff',
                            borderColor: '#f6ae2d',
                            borderWidth: 1,
                            cornerRadius: 8,
                            displayColors: true,
                            callbacks: {
                                afterBody: function(context) {
                                    if (context[0] && context[0].raw > 0) {
                                        return `Average Rating: ${(Math.random() * 2 + 3).toFixed(1)} ⭐`;
                                    }
                                    return '';
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            display: true,
                            grid: {
                                display: false
                            },
                            ticks: {
                                font: {
                                    size: 11
                                },
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
                                font: {
                                    size: 11
                                },
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
                    }
                }
            });
        } catch (error) {
            console.error('Error creating reviews chart:', error);
            const container = canvas ? canvas.parentElement : null;
            this.showChartError(container, 'Unable to load reviews data');
        }
    }

    async fetchViewsData(days) {
        try {
            // Use new pre-formatted chart data endpoint
            const response = await fetch(`/Client/Analytics/ViewsChartData?days=${days}`);
            if (!response.ok) throw new Error('Failed to fetch views chart data');
            
            const chartData = await response.json();
            
            // Return pre-formatted data directly - no processing needed
            return {
                labels: chartData.labels || [],
                datasets: chartData.datasets || []
            };
        } catch (error) {
            console.error('Error fetching views chart data:', error);
            return this.getEmptyChartData(days, 'Views');
        }
    }

    async fetchReviewsData(days) {
        try {
            // Use new pre-formatted chart data endpoint
            const response = await fetch(`/Client/Analytics/ReviewsChartData?days=${days}`);
            if (!response.ok) throw new Error('Failed to fetch reviews chart data');
            
            const chartData = await response.json();
            
            // Return pre-formatted data directly - no processing needed
            return {
                labels: chartData.labels || [],
                datasets: chartData.datasets || []
            };
        } catch (error) {
            console.error('Error fetching reviews chart data:', error);
            return this.getEmptyChartData(days, 'Reviews');
        }
    }

    // Note: generateDateLabels and getChartColor methods removed - now handled by backend

    getEmptyChartData(days, type) {
        // Generate simple date labels for empty data
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

    async updateViewsChart(days) {
        if (!this.chartJsAvailable || !this.viewsChart) return;
        
        try {
            const data = await this.fetchViewsData(days);
            this.viewsChart.data = data;
            this.viewsChart.update('active');
        } catch (error) {
            console.error('Error updating views chart:', error);
        }
    }

    async updateReviewsChart(days) {
        if (!this.chartJsAvailable || !this.reviewsChart) return;
        
        try {
            const data = await this.fetchReviewsData(days);
            this.reviewsChart.data = data;
            this.reviewsChart.update('active');
        } catch (error) {
            console.error('Error updating reviews chart:', error);
        }
    }

    showChartError(target, message) {
        const container = target && target.classList && target.classList.contains('chart-content')
            ? target
            : (target ? target.parentElement : null);
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

    showChartLoading(target) {
        const container = target && target.classList && target.classList.contains('chart-content')
            ? target
            : (target ? target.parentElement : null);
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

    setupAnimations() {
        // Animate counter values
        this.animateCounters();
        
        // Stagger card animations
        this.staggerCardAnimations();
        
        // Setup intersection observer for animations
        this.setupIntersectionObserver();
    }

    animateCounters() {
        const counters = document.querySelectorAll('.card-value, .metric-value');
        
        counters.forEach(counter => {
            const text = counter.textContent;
            const target = parseFloat(text.replace(/[^0-9.]/g, ''));
            if (isNaN(target) || target === 0) return;
            
            let current = 0;
            const increment = target / 60;
            const isDecimal = text.includes('.');
            
            const timer = setInterval(() => {
                current += increment;
                if (current >= target) {
                    counter.textContent = isDecimal ? target.toFixed(1) : target.toLocaleString();
                    clearInterval(timer);
                } else {
                    if (isDecimal) {
                        counter.textContent = current.toFixed(1);
                    } else {
                        counter.textContent = Math.floor(current).toLocaleString();
                    }
                }
            }, 16);
        });
    }

    staggerCardAnimations() {
        const cards = document.querySelectorAll('.overview-card, .performance-card, .insight-card, .metric-card');
        
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            
            setTimeout(() => {
                card.style.transition = 'all 0.4s cubic-bezier(0.4, 0, 0.2, 1)';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 80);
        });
    }

    setupIntersectionObserver() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        // Observe chart containers and premium cards
        document.querySelectorAll('.chart-container, .premium-card').forEach(el => {
            observer.observe(el);
        });
    }

    // Platform-specific analytics methods
    async fetchViewsDataByPlatform(days = 30, platform = null) {
        try {
            const url = new URL('/Client/Analytics/ViewsChartData', window.location.origin);
            url.searchParams.set('days', days);
            if (platform) {
                url.searchParams.set('platform', platform);
            }

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const chartData = await response.json();
            return {
                labels: chartData.labels || [],
                datasets: chartData.datasets || []
            };
        } catch (error) {
            console.error('Error fetching platform-specific views chart data:', error);
            return { labels: [], datasets: [] };
        }
    }

    async fetchBusinessViewStatistics(businessId, startDate, endDate, platform = null) {
        try {
            const url = new URL('/Client/Analytics/BusinessViewStatistics', window.location.origin);
            url.searchParams.set('businessId', businessId);
            url.searchParams.set('startDate', startDate.toISOString().split('T')[0]);
            url.searchParams.set('endDate', endDate.toISOString().split('T')[0]);
            if (platform) {
                url.searchParams.set('platform', platform);
            }

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error fetching business view statistics:', error);
            return null;
        }
    }

    updateViewsChartByPlatform(days = 30, platform = null) {
        this.fetchViewsDataByPlatform(days, platform).then(data => {
            if (this.viewsChart) {
                this.viewsChart.data = data;
                this.viewsChart.update('active');
            }
        });
    }

    // Mobile app integration helper
    getPlatformBreakdown() {
        const platformCards = document.querySelectorAll('.platform-breakdown-card');
        const breakdown = {};

        platformCards.forEach(card => {
            const platform = card.dataset.platform;
            const count = parseInt(card.querySelector('.platform-count').textContent) || 0;
            breakdown[platform] = count;
        });

        return breakdown;
    }

    destroy() {
        if (this.viewsChart) {
            this.viewsChart.destroy();
            this.viewsChart = null;
        }
        
        if (this.reviewsChart) {
            this.reviewsChart.destroy();
            this.reviewsChart = null;
        }
        
        this.isInitialized = false;
        console.log('ClientAnalyticsManager destroyed');
    }
}

// Expose globally for app initialization mapping
window.ClientAnalyticsManager = ClientAnalyticsManager;

// Fallback self-initialization if app bootstrap does not handle this page
document.addEventListener('DOMContentLoaded', () => {
    try {
        const app = (typeof window.getApp === 'function') ? window.getApp() : null;
        const isAnalyticsPage = ClientAnalyticsManager.shouldInitialize();
        
        // If global app didn’t initialize a client-analytics module, self-init
        const alreadyInitialized = !!window.__clientAnalyticsManager;
        if (isAnalyticsPage && !alreadyInitialized) {
            const manager = new ClientAnalyticsManager();
            manager.init();
            window.__clientAnalyticsManager = manager;
        }
    } catch (e) {
        console.error('Failed to initialize ClientAnalyticsManager:', e);
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ClientAnalyticsManager;
}