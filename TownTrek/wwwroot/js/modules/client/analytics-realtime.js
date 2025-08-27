/**
 * @fileoverview AnalyticsRealtime - SignalR integration and real-time analytics updates
 * @author TownTrek Development Team
 * @version 2.0.0
 */

class AnalyticsRealtime {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 1000;
        this.refreshInterval = 0;
        this.autoRefreshEnabled = false;
        this.notificationContainer = null;
        this.analyticsCore = null;
        this.analyticsCharts = null;
        this.eventListeners = new Map();
        this.connectionHealthCheck = null;
        this.lastActivity = Date.now();
        
        this.config = {
            hubUrl: '/analyticsHub',
            reconnectDelays: [0, 2000, 10000, 30000],
            maxReconnectAttempts: 5,
            notificationTimeout: 10000,
            defaultRefreshInterval: 0,
            healthCheckInterval: 30000, // 30 seconds
            maxInactivityTime: 300000, // 5 minutes
            connectionTimeout: 10000 // 10 seconds
        };
    }

    // Static method to check if real-time analytics should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-dashboard') !== null;
    }

    init(analyticsCore, analyticsCharts) {
        this.analyticsCore = analyticsCore;
        this.analyticsCharts = analyticsCharts;
        
        this.createNotificationContainer();
        this.initializeSignalR();
        this.bindEvents();
    }

    createNotificationContainer() {
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
            if (typeof signalR === 'undefined') {
                console.warn('SignalR is not available - real-time updates will be disabled');
                return;
            }

            // Check if connection already exists and is in a good state
            if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
                return;
            }

            // Clean up existing connection if it exists
            if (this.connection) {
                await this.cleanupConnection();
            }

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(this.config.hubUrl)
                .withAutomaticReconnect(this.config.reconnectDelays)
                .configureLogging(signalR.LogLevel.Information)
                .build();

            this.setupSignalREventHandlers();
            
            // Set connection timeout
            const connectionPromise = this.connection.start();
            const timeoutPromise = new Promise((_, reject) => {
                setTimeout(() => reject(new Error('Connection timeout')), this.config.connectionTimeout);
            });

            await Promise.race([connectionPromise, timeoutPromise]);
            
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;
            this.lastActivity = Date.now();

            // Start health monitoring
            this.startHealthMonitoring();
        } catch (error) {
            console.error('Error establishing SignalR connection:', error);
            this.handleConnectionError();
        }
    }

    setupSignalREventHandlers() {
        if (!this.connection) return;

        // Connection lifecycle events
        this.connection.onreconnecting((error) => {
            this.isConnected = false;
            this.lastActivity = Date.now();
            this.showConnectionStatus('Reconnecting...', 'warning');
        });

        this.connection.onreconnected((connectionId) => {
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;
            this.lastActivity = Date.now();
            this.showConnectionStatus('Connected', 'success');
        });

        this.connection.onclose((error) => {
            this.isConnected = false;
            this.showConnectionStatus('Disconnected', 'error');
        });

        // Analytics update events with activity tracking
        this.addEventListener('ReceiveClientAnalyticsUpdate', (analytics) => {
            this.lastActivity = Date.now();
            this.handleClientAnalyticsUpdate(analytics);
        });

        this.addEventListener('ReceiveBusinessAnalyticsUpdate', (businessId, analytics) => {
            this.lastActivity = Date.now();
            this.handleBusinessAnalyticsUpdate(businessId, analytics);
        });

        this.addEventListener('ReceiveViewsChartUpdate', (chartData) => {
            this.lastActivity = Date.now();
            this.handleViewsChartUpdate(chartData);
        });

        this.addEventListener('ReceiveReviewsChartUpdate', (chartData) => {
            this.lastActivity = Date.now();
            this.handleReviewsChartUpdate(chartData);
        });

        this.addEventListener('ReceivePerformanceInsightsUpdate', (insights) => {
            this.lastActivity = Date.now();
            this.handlePerformanceInsightsUpdate(insights);
        });

        this.addEventListener('ReceiveCompetitorInsightsUpdate', (insights) => {
            this.lastActivity = Date.now();
            this.handleCompetitorInsightsUpdate(insights);
        });

        this.addEventListener('ReceiveCategoryBenchmarksUpdate', (category, benchmarks) => {
            this.lastActivity = Date.now();
            this.handleCategoryBenchmarksUpdate(category, benchmarks);
        });

        // Notification events
        this.addEventListener('ReceiveAnalyticsNotification', (notification) => {
            this.lastActivity = Date.now();
            this.showNotification(notification);
        });

        this.addEventListener('ReceiveBroadcastUpdate', (message, data) => {
            this.lastActivity = Date.now();
            this.handleBroadcastUpdate(message, data);
        });
    }

    handleConnectionError() {
        this.reconnectAttempts++;
        if (this.reconnectAttempts < this.config.maxReconnectAttempts) {
            console.log(`Reconnection attempt ${this.reconnectAttempts}/${this.config.maxReconnectAttempts}`);
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

    // Analytics update handlers
    handleClientAnalyticsUpdate(analytics) {
        console.log('Received client analytics update:', analytics);
        this.updateOverviewCards(analytics.overview);
        this.updateBusinessCards(analytics.businessAnalytics);
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'client-analytics', { 
                updateType: 'client-analytics' 
            });
        }
    }

    handleBusinessAnalyticsUpdate(businessId, analytics) {
        console.log('Received business analytics update:', businessId, analytics);
        this.updateBusinessCard(businessId, analytics);
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'business-analytics', { 
                businessId: businessId 
            });
        }
    }

    handleViewsChartUpdate(chartData) {
        console.log('Received views chart update:', chartData);
        if (this.analyticsCharts) {
            this.analyticsCharts.updateViewsChart(chartData);
        }
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'views-chart', { 
                dataPoints: chartData.labels?.length || 0 
            });
        }
    }

    handleReviewsChartUpdate(chartData) {
        console.log('Received reviews chart update:', chartData);
        if (this.analyticsCharts) {
            this.analyticsCharts.updateReviewsChart(chartData);
        }
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'reviews-chart', { 
                dataPoints: chartData.labels?.length || 0 
            });
        }
    }

    handlePerformanceInsightsUpdate(insights) {
        console.log('Received performance insights update:', insights);
        this.updatePerformanceInsights(insights);
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'performance-insights', { 
                insightCount: insights.length 
            });
        }
    }

    handleCompetitorInsightsUpdate(insights) {
        console.log('Received competitor insights update:', insights);
        this.updateCompetitorInsights(insights);
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'competitor-insights', { 
                insightCount: insights.length 
            });
        }
    }

    handleCategoryBenchmarksUpdate(category, benchmarks) {
        console.log('Received category benchmarks update:', category, benchmarks);
        this.updateCategoryBenchmarks(category, benchmarks);
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('real-time-update', 'category-benchmarks', { 
                category: category 
            });
        }
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

    // UI update methods
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

        insightsContainer.innerHTML = '';

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

        competitorsContainer.innerHTML = '';

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

        // Auto-remove after timeout
        setTimeout(() => {
            if (notificationElement.parentNode) {
                notificationElement.remove();
            }
        }, this.config.notificationTimeout);

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
        if (this.analyticsCore) {
            // Trigger a manual refresh through the core module
            await this.analyticsCore.trackFeatureUsage('manual-refresh', 'user-action');
        }
    }

    // Health monitoring
    startHealthMonitoring() {
        if (this.connectionHealthCheck) {
            clearInterval(this.connectionHealthCheck);
        }

        this.connectionHealthCheck = setInterval(() => {
            this.checkConnectionHealth();
        }, this.config.healthCheckInterval);
    }

    stopHealthMonitoring() {
        if (this.connectionHealthCheck) {
            clearInterval(this.connectionHealthCheck);
            this.connectionHealthCheck = null;
        }
    }

    checkConnectionHealth() {
        const now = Date.now();
        const timeSinceLastActivity = now - this.lastActivity;

        // Check for inactivity
        if (timeSinceLastActivity > this.config.maxInactivityTime) {
            console.log('Connection inactive for too long, reconnecting...');
            this.reconnect();
            return;
        }

        // Check connection state
        if (this.connection && this.connection.state !== signalR.HubConnectionState.Connected) {
            console.log('Connection state is not connected, attempting reconnect...');
            this.reconnect();
            return;
        }

        // Send ping to keep connection alive
        if (this.connection && this.isConnected) {
            this.connection.invoke('Ping').catch(error => {
                console.warn('Ping failed, connection may be stale:', error);
                this.reconnect();
            });
        }
    }

    async reconnect() {
        console.log('Attempting to reconnect SignalR...');
        this.isConnected = false;
        this.stopHealthMonitoring();
        
        try {
            await this.cleanupConnection();
            await this.initializeSignalR();
        } catch (error) {
            console.error('Reconnection failed:', error);
            this.handleConnectionError();
        }
    }

    async cleanupConnection() {
        if (this.connection) {
            try {
                // Remove all event listeners
                this.removeAllEventListeners();
                
                // Stop the connection
                await this.connection.stop();
            } catch (error) {
                console.warn('Error during connection cleanup:', error);
            } finally {
                this.connection = null;
                this.isConnected = false;
            }
        }

        this.stopHealthMonitoring();
    }

    removeAllEventListeners() {
        // Remove all registered event listeners
        this.eventListeners.forEach((listener, event) => {
            if (this.connection) {
                this.connection.off(event, listener);
            }
        });
        this.eventListeners.clear();
    }

    // Enhanced event listener registration
    addEventListener(event, handler) {
        if (this.connection) {
            this.connection.on(event, handler);
            this.eventListeners.set(event, handler);
        }
    }

    // Cleanup
    disconnect() {
        this.cleanupConnection();
    }
}

// Expose globally
window.AnalyticsRealtime = AnalyticsRealtime;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AnalyticsRealtime;
}

// Global instance
window.analyticsRealtime = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (AnalyticsRealtime.shouldInitialize()) {
        window.analyticsRealtime = new AnalyticsRealtime();
    }
});
