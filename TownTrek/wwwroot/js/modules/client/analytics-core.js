/**
 * @fileoverview AnalyticsCore - Core analytics functionality and utilities
 * @author TownTrek Development Team
 * @version 2.0.0
 */

class AnalyticsCore {
    constructor() {
        this.isInitialized = false;
        this.chartJsAvailable = false;
        this.interactionStartTime = Date.now();
        this.eventListeners = new Map();
        this.config = {
            apiEndpoints: {
                getClientAnalytics: '/Client/ClientAnalytics/GetClientAnalytics',
                getBusinessAnalytics: '/Client/ClientAnalytics/GetBusinessAnalytics',
                getViewsOverTime: '/Client/ChartData/ViewsOverTimeData',
                getReviewsOverTime: '/Client/ChartData/ReviewsOverTimeData',
                viewsData: '/Client/ChartData/ViewsOverTimeData',
                reviewsData: '/Client/ChartData/ReviewsOverTimeData',
                getCategoryBenchmarks: '/Client/BusinessAnalytics/Benchmarks',
                getCompetitorInsights: '/Client/BusinessAnalytics/Competitors',
                trackUsage: '/Client/Analytics/TrackUsage',
                exportBusinessPdf: '/Client/Export/ExportBusinessPdf',
                exportOverviewPdf: '/Client/Export/ExportOverviewPdf',
                exportCsv: '/Client/Export/ExportCsv',
                generateShareableLink: '/Client/Export/GenerateShareableLink',
                sendEmailReport: '/Client/Export/SendEmailReport',
                scheduleEmailReport: '/Client/Export/ScheduleEmailReport'
            },
            chartDefaults: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true,
                            padding: 20,
                            font: { size: 12 }
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
                }
            }
        };
    }

    // Static method to check if analytics should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-dashboard') !== null;
    }

    init() {
        if (this.isInitialized) return;
        
        try {
            this.checkChartJsAvailability();
            this.bindCoreEvents();
            this.isInitialized = true;
        } catch (error) {
            console.error('Error initializing AnalyticsCore:', error);
        }
    }

    // Track feature usage for analytics monitoring
    async trackFeatureUsage(featureName, interactionType, metadata = {}) {
        try {
            const duration = Date.now() - this.interactionStartTime;
            const trackingData = {
                featureName: featureName,
                interactionType: interactionType,
                duration: duration,
                metadata: metadata
            };

            await fetch(this.config.apiEndpoints.trackUsage, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(trackingData)
            });
        } catch (error) {
            console.warn('Failed to track feature usage:', error);
        }
    }

    checkChartJsAvailability() {
        if (typeof Chart !== 'undefined' && Chart) {
            this.chartJsAvailable = true;
        } else {
            this.chartJsAvailable = false;
            console.warn('Chart.js is not available - charts will be disabled');
        }
    }

    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    bindCoreEvents() {
        // Track chart interactions
        this.setupChartInteractions();
        
        // Card hover effects
        this.setupCardHoverEffects();
    }

    setupChartInteractions() {
        // Track chart hover interactions
        const chartContainers = document.querySelectorAll('.chart-container');
        chartContainers.forEach(container => {
            container.addEventListener('mouseenter', () => {
                this.trackFeatureUsage('ChartInteraction', 'Hover', { 
                    chartType: container.dataset.chartType || 'unknown' 
                });
            });
        });

        // Track chart click interactions
        const charts = document.querySelectorAll('canvas');
        charts.forEach(canvas => {
            canvas.addEventListener('click', (e) => {
                this.trackFeatureUsage('ChartInteraction', 'Click', { 
                    chartType: canvas.id || 'unknown' 
                });
            });
        });
    }

    setupCardHoverEffects() {
        const cards = document.querySelectorAll('.overview-card, .performance-card, .premium-card');
        
        cards.forEach(card => {
            card.addEventListener('mouseenter', () => {
                card.style.transform = 'translateY(-4px)';
            });
            
            card.addEventListener('mouseleave', () => {
                card.style.transform = 'translateY(0)';
            });
        });
    }

    // Utility methods for chart creation
    createChart(canvas, type, data, options = {}) {
        if (!this.chartJsAvailable) {
            throw new Error('Chart.js is not available');
        }

        const ctx = canvas.getContext('2d');
        const mergedOptions = this.mergeChartOptions(options);
        
        return new Chart(ctx, {
            type: type,
            data: data,
            options: mergedOptions
        });
    }

    mergeChartOptions(customOptions = {}) {
        return {
            ...this.config.chartDefaults,
            ...customOptions,
            plugins: {
                ...this.config.chartDefaults.plugins,
                ...customOptions.plugins
            }
        };
    }

    // Utility methods for data fetching
    async fetchData(endpoint, params = {}) {
        try {
            const queryString = new URLSearchParams(params).toString();
            const url = queryString ? `${endpoint}?${queryString}` : endpoint;
            
            const response = await fetch(url, {
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error(`Error fetching data from ${endpoint}:`, error);
            throw error;
        }
    }

    // Event management
    addEventListener(event, callback) {
        if (!this.eventListeners.has(event)) {
            this.eventListeners.set(event, []);
        }
        this.eventListeners.get(event).push(callback);
    }

    removeEventListener(event, callback) {
        if (this.eventListeners.has(event)) {
            const callbacks = this.eventListeners.get(event);
            const index = callbacks.indexOf(callback);
            if (index > -1) {
                callbacks.splice(index, 1);
            }
        }
    }

    emitEvent(event, data) {
        if (this.eventListeners.has(event)) {
            this.eventListeners.get(event).forEach(callback => {
                try {
                    callback(data);
                } catch (error) {
                    console.error(`Error in event listener for ${event}:`, error);
                }
            });
        }
    }

    // Cleanup
    destroy() {
        this.eventListeners.clear();
        this.isInitialized = false;
    }
}

// Expose globally
window.AnalyticsCore = AnalyticsCore;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AnalyticsCore;
}
