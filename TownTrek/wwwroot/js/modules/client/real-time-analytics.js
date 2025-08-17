/**
 * @fileoverview RealTimeAnalyticsManager - handles real-time analytics updates via SignalR
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class RealTimeAnalyticsManager {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 1000; // Start with 1 second
        this.refreshInterval = 0; // 0 = disabled
        this.autoRefreshEnabled = false;
        this.notificationContainer = null;
        this.analyticsManager = null; // Reference to main analytics manager
    }

    // Static method to check if real-time analytics should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-dashboard') !== null;
    }

    init(analyticsManager) {
        this.analyticsManager = analyticsManager;
        this.createNotificationContainer();
        this.initializeSignalR();
        this.bindEvents();
        console.log('RealTimeAnalyticsManager initialized');
    }

    createNotificationContainer() {
        // Create notification container if it doesn't exist
        if (!document.getElementById('analytics-notifications')) {
            this.notificationContainer = document.createElement('div');
            this.notificationContainer.id = 'analytics-notifications';
            this.notificationContainer.className = 'analytics-notifications';
            document.body.appendChild(this.notificationContainer);
        } else {
            this.notificationContainer = document.getElementById('analytics-notifications');
        }
    }

    async initializeSignalR() {
        try {
            // Check if SignalR is available
            if (typeof signalR === 'undefined') {
                console.warn('SignalR is not available - real-time updates will be disabled');
                return;
            }

            // Create connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/analyticsHub')
                .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry delays
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up event handlers
            this.setupSignalREventHandlers();

            // Start connection
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;

            console.log('SignalR connection established for analytics');
        } catch (error) {
            console.error('Error establishing SignalR connection:', error);
            this.handleConnectionError();
        }
    }

    setupSignalREventHandlers() {
        if (!this.connection) return;

        // Handle connection events
        this.connection.onreconnecting((error) => {
            console.log('SignalR reconnecting...', error);
            this.isConnected = false;
            this.showConnectionStatus('Reconnecting...', 'warning');
        });

        this.connection.onreconnected((connectionId) => {
            console.log('SignalR reconnected with connection ID:', connectionId);
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;
            this.showConnectionStatus('Connected', 'success');
        });

        this.connection.onclose((error) => {
            console.log('SignalR connection closed:', error);
            this.isConnected = false;
            this.showConnectionStatus('Disconnected', 'error');
        });

        // Handle analytics updates
        this.connection.on('ReceiveClientAnalyticsUpdate', (analytics) => {
            this.handleClientAnalyticsUpdate(analytics);
        });

        this.connection.on('ReceiveBusinessAnalyticsUpdate', (businessId, analytics) => {
            this.handleBusinessAnalyticsUpdate(businessId, analytics);
        });

        this.connection.on('ReceiveViewsChartUpdate', (chartData) => {
            this.handleViewsChartUpdate(chartData);
        });

        this.connection.on('ReceiveReviewsChartUpdate', (chartData) => {
            this.handleReviewsChartUpdate(chartData);
        });

        this.connection.on('ReceivePerformanceInsightsUpdate', (insights) => {
            this.handlePerformanceInsightsUpdate(insights);
        });

        this.connection.on('ReceiveCompetitorInsightsUpdate', (insights) => {
            this.handleCompetitorInsightsUpdate(insights);
        });

        this.connection.on('ReceiveCategoryBenchmarksUpdate', (category, benchmarks) => {
            this.handleCategoryBenchmarksUpdate(category, benchmarks);
        });

        // Handle notifications
        this.connection.on('ReceiveAnalyticsNotification', (notification) => {
            this.showNotification(notification);
        });

        // Handle broadcast updates
        this.connection.on('ReceiveBroadcastUpdate', (message, data) => {
            this.handleBroadcastUpdate(message, data);
        });
    }

    handleConnectionError() {
        this.reconnectAttempts++;
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            console.log(`Reconnection attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}`);
            setTimeout(() => {
                this.initializeSignalR();
            }, this.reconnectDelay);
            this.reconnectDelay *= 2; // Exponential backoff
        } else {
            console.error('Max reconnection attempts reached');
            this.showConnectionStatus('Connection failed', 'error');
        }
    }

    showConnectionStatus(message, type) {
        const statusElement = document.getElementById('analytics-connection-status');
        if (statusElement) {
            statusElement.textContent = message;
            statusElement.className = `connection-status ${type}`;
        }
    }

    // Handle different types of analytics updates
    handleClientAnalyticsUpdate(analytics) {
        console.log('Received client analytics update:', analytics);
        this.updateOverviewCards(analytics.overview);
        this.updateBusinessCards(analytics.businessAnalytics);
    }

    handleBusinessAnalyticsUpdate(businessId, analytics) {
        console.log('Received business analytics update:', businessId, analytics);
        this.updateBusinessCard(businessId, analytics);
    }

    handleViewsChartUpdate(chartData) {
        console.log('Received views chart update:', chartData);
        if (this.analyticsManager && this.analyticsManager.viewsChart) {
            this.analyticsManager.updateViewsChart(chartData);
        }
    }

    handleReviewsChartUpdate(chartData) {
        console.log('Received reviews chart update:', chartData);
        if (this.analyticsManager && this.analyticsManager.reviewsChart) {
            this.analyticsManager.updateReviewsChart(chartData);
        }
    }

    handlePerformanceInsightsUpdate(insights) {
        console.log('Received performance insights update:', insights);
        this.updatePerformanceInsights(insights);
    }

    handleCompetitorInsightsUpdate(insights) {
        console.log('Received competitor insights update:', insights);
        this.updateCompetitorInsights(insights);
    }

    handleCategoryBenchmarksUpdate(category, benchmarks) {
        console.log('Received category benchmarks update:', category, benchmarks);
        this.updateCategoryBenchmarks(category, benchmarks);
    }

    handleBroadcastUpdate(message, data) {
        console.log('Received broadcast update:', message, data);
        this.showNotification({
            title: 'System Update',
            message: message,
            type: 'info',
            timestamp: new Date()
        });
    }

    // Update UI elements
    updateOverviewCards(overview) {
        // Update total views
        const viewsElement = document.querySelector('.overview-card .card-value');
        if (viewsElement && overview.totalViews !== undefined) {
            viewsElement.textContent = overview.totalViews.toLocaleString();
        }

        // Update total reviews
        const reviewsElement = document.querySelector('.overview-card:nth-child(2) .card-value');
        if (reviewsElement && overview.totalReviews !== undefined) {
            reviewsElement.textContent = overview.totalReviews.toString();
        }

        // Update total favorites
        const favoritesElement = document.querySelector('.overview-card:nth-child(3) .card-value');
        if (favoritesElement && overview.totalFavorites !== undefined) {
            favoritesElement.textContent = overview.totalFavorites.toString();
        }

        // Update growth rates
        this.updateGrowthRates(overview);
    }

    updateGrowthRates(overview) {
        // Update views growth rate
        if (overview.viewsGrowthRate !== undefined) {
            const viewsGrowthElement = document.querySelector('.overview-card .card-growth');
            if (viewsGrowthElement) {
                const isPositive = overview.viewsGrowthRate > 0;
                viewsGrowthElement.textContent = `${isPositive ? '+' : ''}${overview.viewsGrowthRate.toFixed(1)}%`;
                viewsGrowthElement.className = `card-growth ${isPositive ? 'positive' : 'negative'}`;
            }
        }

        // Update favorites growth rate
        if (overview.favoritesGrowthRate !== undefined) {
            const favoritesGrowthElement = document.querySelector('.overview-card:nth-child(3) .card-growth');
            if (favoritesGrowthElement) {
                const isPositive = overview.favoritesGrowthRate > 0;
                favoritesGrowthElement.textContent = `${isPositive ? '+' : ''}${overview.favoritesGrowthRate.toFixed(1)}%`;
                favoritesGrowthElement.className = `card-growth ${isPositive ? 'positive' : 'negative'}`;
            }
        }
    }

    updateBusinessCards(businessAnalytics) {
        businessAnalytics.forEach(business => {
            this.updateBusinessCard(business.businessId, business);
        });
    }

    updateBusinessCard(businessId, analytics) {
        const businessCard = document.querySelector(`[data-business-id="${businessId}"]`);
        if (!businessCard) return;

        // Update business metrics
        const viewsElement = businessCard.querySelector('.business-views');
        if (viewsElement && analytics.totalViews !== undefined) {
            viewsElement.textContent = analytics.totalViews.toLocaleString();
        }

        const reviewsElement = businessCard.querySelector('.business-reviews');
        if (reviewsElement && analytics.totalReviews !== undefined) {
            reviewsElement.textContent = analytics.totalReviews.toString();
        }

        const ratingElement = businessCard.querySelector('.business-rating');
        if (ratingElement && analytics.averageRating !== undefined) {
            ratingElement.textContent = analytics.averageRating.toFixed(1);
        }
    }

    updatePerformanceInsights(insights) {
        const insightsContainer = document.querySelector('.performance-insights');
        if (!insightsContainer) return;

        // Clear existing insights
        insightsContainer.innerHTML = '';

        // Add new insights
        insights.forEach(insight => {
            const insightElement = document.createElement('div');
            insightElement.className = 'insight-card';
            insightElement.innerHTML = `
                <div class="insight-header">
                    <h4>${insight.title}</h4>
                    <span class="insight-type ${insight.type}">${insight.type}</span>
                </div>
                <p>${insight.description}</p>
                <div class="insight-metrics">
                    <span class="metric">${insight.metric}</span>
                </div>
            `;
            insightsContainer.appendChild(insightElement);
        });
    }

    updateCompetitorInsights(insights) {
        const competitorsContainer = document.querySelector('.competitor-insights');
        if (!competitorsContainer) return;

        // Clear existing insights
        competitorsContainer.innerHTML = '';

        // Add new insights
        insights.forEach(insight => {
            const insightElement = document.createElement('div');
            insightElement.className = 'competitor-insight';
            insightElement.innerHTML = `
                <div class="insight-header">
                    <h4>${insight.title}</h4>
                </div>
                <p>${insight.description}</p>
                <div class="insight-data">
                    <span class="data-point">${insight.dataPoint}</span>
                </div>
            `;
            competitorsContainer.appendChild(insightElement);
        });
    }

    updateCategoryBenchmarks(category, benchmarks) {
        const benchmarksContainer = document.querySelector('.category-benchmarks');
        if (!benchmarksContainer) return;

        // Update benchmark data
        if (benchmarks.averageRating !== undefined) {
            const avgRatingElement = benchmarksContainer.querySelector('.avg-rating');
            if (avgRatingElement) {
                avgRatingElement.textContent = benchmarks.averageRating.toFixed(1);
            }
        }

        if (benchmarks.averageViews !== undefined) {
            const avgViewsElement = benchmarksContainer.querySelector('.avg-views');
            if (avgViewsElement) {
                avgViewsElement.textContent = benchmarks.averageViews.toLocaleString();
            }
        }
    }

    // Notification system
    showNotification(notification) {
        const notificationElement = document.createElement('div');
        notificationElement.className = `analytics-notification ${notification.type}`;
        notificationElement.innerHTML = `
            <div class="notification-header">
                <h4>${notification.title}</h4>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
            <p>${notification.message}</p>
            <div class="notification-time">${new Date(notification.timestamp).toLocaleTimeString()}</div>
        `;

        this.notificationContainer.appendChild(notificationElement);

        // Auto-remove after 10 seconds
        setTimeout(() => {
            if (notificationElement.parentNode) {
                notificationElement.remove();
            }
        }, 10000);

        // Add entrance animation
        setTimeout(() => {
            notificationElement.classList.add('show');
        }, 100);
    }

    // Auto-refresh functionality
    setRefreshInterval(intervalSeconds) {
        this.refreshInterval = intervalSeconds;
        
        if (this.connection && this.isConnected) {
            this.connection.invoke('UpdateRefreshInterval', intervalSeconds);
        }

        // Update UI to reflect the change
        this.updateRefreshIntervalUI(intervalSeconds);
    }

    updateRefreshIntervalUI(intervalSeconds) {
        const intervalSelect = document.getElementById('refreshInterval');
        if (intervalSelect) {
            intervalSelect.value = intervalSeconds;
        }

        const statusElement = document.getElementById('refresh-status');
        if (statusElement) {
            if (intervalSeconds > 0) {
                statusElement.textContent = `Auto-refresh: ${this.formatInterval(intervalSeconds)}`;
                statusElement.className = 'refresh-status active';
            } else {
                statusElement.textContent = 'Auto-refresh: Disabled';
                statusElement.className = 'refresh-status disabled';
            }
        }
    }

    formatInterval(seconds) {
        if (seconds < 60) {
            return `${seconds}s`;
        } else if (seconds < 3600) {
            return `${Math.floor(seconds / 60)}m`;
        } else {
            return `${Math.floor(seconds / 3600)}h`;
        }
    }

    // Event binding
    bindEvents() {
        // Refresh interval selector
        const intervalSelect = document.getElementById('refreshInterval');
        if (intervalSelect) {
            intervalSelect.addEventListener('change', (e) => {
                const interval = parseInt(e.target.value);
                this.setRefreshInterval(interval);
            });
        }

        // Manual refresh button
        const refreshButton = document.getElementById('manualRefresh');
        if (refreshButton) {
            refreshButton.addEventListener('click', () => {
                this.manualRefresh();
            });
        }

        // Connection status indicator
        this.createConnectionStatusIndicator();
    }

    createConnectionStatusIndicator() {
        const statusContainer = document.querySelector('.analytics-header');
        if (statusContainer && !document.getElementById('analytics-connection-status')) {
            const statusElement = document.createElement('div');
            statusElement.id = 'analytics-connection-status';
            statusElement.className = 'connection-status';
            statusElement.textContent = this.isConnected ? 'Connected' : 'Disconnected';
            statusContainer.appendChild(statusElement);
        }
    }

    async manualRefresh() {
        if (this.analyticsManager) {
            await this.analyticsManager.refreshAllData();
        }
    }

    // Cleanup
    disconnect() {
        if (this.connection) {
            this.connection.stop();
        }
        this.isConnected = false;
    }
}

// Global instance
window.realTimeAnalyticsManager = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (RealTimeAnalyticsManager.shouldInitialize()) {
        window.realTimeAnalyticsManager = new RealTimeAnalyticsManager();
    }
});
