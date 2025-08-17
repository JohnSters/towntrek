/**
 * Advanced Analytics Module
 * Handles predictive analytics, anomaly detection, and custom metrics functionality
 */

class AdvancedAnalyticsManager {
    constructor() {
        this.baseUrl = '/client/advanced-analytics';
        this.currentData = {
            predictive: null,
            anomalies: null,
            metrics: null
        };
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadInitialData();
    }

    bindEvents() {
        // Predictive Analytics
        document.getElementById('forecastPeriod')?.addEventListener('change', (e) => {
            this.loadPredictiveAnalytics(parseInt(e.target.value));
        });
        document.getElementById('refreshPredictive')?.addEventListener('click', () => {
            const period = parseInt(document.getElementById('forecastPeriod').value);
            this.loadPredictiveAnalytics(period);
        });

        // Anomaly Detection
        document.getElementById('anomalyPeriod')?.addEventListener('change', (e) => {
            this.loadAnomalyDetection(parseInt(e.target.value));
        });
        document.getElementById('refreshAnomalies')?.addEventListener('click', () => {
            const period = parseInt(document.getElementById('anomalyPeriod').value);
            this.loadAnomalyDetection(period);
        });

        // Custom Metrics
        document.getElementById('refreshMetrics')?.addEventListener('click', () => {
            this.loadCustomMetrics();
        });
        document.getElementById('createMetric')?.addEventListener('click', () => {
            this.showCreateMetricModal();
        });
        document.getElementById('saveMetric')?.addEventListener('click', () => {
            this.createCustomMetric();
        });

        // Modal events
        const createMetricModal = document.getElementById('createMetricModal');
        if (createMetricModal) {
            createMetricModal.addEventListener('hidden.bs.modal', () => {
                this.resetCreateMetricForm();
            });
        }
    }

    async loadInitialData() {
        try {
            await Promise.all([
                this.loadPredictiveAnalytics(30),
                this.loadAnomalyDetection(30),
                this.loadCustomMetrics()
            ]);
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showError('Failed to load analytics data. Please try again.');
        }
    }

    async loadPredictiveAnalytics(forecastDays = 30) {
        const loadingElement = document.getElementById('predictiveLoading');
        const dataElement = document.getElementById('predictiveData');

        try {
            this.showLoading(loadingElement, dataElement);

            const response = await fetch(`${this.baseUrl}/predictive?forecastDays=${forecastDays}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.currentData.predictive = data;
            this.renderPredictiveAnalytics(data);

        } catch (error) {
            console.error('Error loading predictive analytics:', error);
            this.showError('Failed to load predictive analytics. Please try again.');
        } finally {
            this.hideLoading(loadingElement, dataElement);
        }
    }

    async loadAnomalyDetection(analysisDays = 30) {
        const loadingElement = document.getElementById('anomalyLoading');
        const dataElement = document.getElementById('anomalyData');

        try {
            this.showLoading(loadingElement, dataElement);

            const response = await fetch(`${this.baseUrl}/anomalies?analysisDays=${analysisDays}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.currentData.anomalies = data;
            this.renderAnomalyDetection(data);

        } catch (error) {
            console.error('Error loading anomaly detection:', error);
            this.showError('Failed to load anomaly detection. Please try again.');
        } finally {
            this.hideLoading(loadingElement, dataElement);
        }
    }

    async loadCustomMetrics() {
        const loadingElement = document.getElementById('metricsLoading');
        const dataElement = document.getElementById('metricsData');

        try {
            this.showLoading(loadingElement, dataElement);

            const response = await fetch(`${this.baseUrl}/metrics`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.currentData.metrics = data;
            this.renderCustomMetrics(data);

        } catch (error) {
            console.error('Error loading custom metrics:', error);
            this.showError('Failed to load custom metrics. Please try again.');
        } finally {
            this.hideLoading(loadingElement, dataElement);
        }
    }

    renderPredictiveAnalytics(data) {
        const container = document.getElementById('predictiveData');
        if (!container) return;

        if (!data || !data.forecasts || data.forecasts.length === 0) {
            container.innerHTML = this.getEmptyState('predictive');
            return;
        }

        const html = data.forecasts.map(forecast => `
            <div class="predictive-card">
                <h3>${forecast.metricType} Forecast</h3>
                <div class="predictive-metric">
                    <span class="metric-label">Current Value</span>
                    <span class="metric-value">${this.formatNumber(forecast.currentValue)}</span>
                </div>
                <div class="predictive-metric">
                    <span class="metric-label">Predicted Value</span>
                    <span class="metric-value">${this.formatNumber(forecast.predictedValue)}</span>
                </div>
                <div class="predictive-metric">
                    <span class="metric-label">Growth Rate</span>
                    <span class="metric-change ${this.getChangeClass(forecast.growthRate)}">
                        ${forecast.growthRate > 0 ? '+' : ''}${forecast.growthRate.toFixed(1)}%
                    </span>
                </div>
                <div class="predictive-metric">
                    <span class="metric-label">Confidence</span>
                    <span class="metric-value">${(forecast.confidence * 100).toFixed(0)}%</span>
                </div>
            </div>
        `).join('');

        container.innerHTML = html;
    }

    renderAnomalyDetection(data) {
        const container = document.getElementById('anomalyData');
        if (!container) return;

        if (!data || !data.anomalies || data.anomalies.length === 0) {
            container.innerHTML = this.getEmptyState('anomaly');
            return;
        }

        const html = data.anomalies.map(anomaly => `
            <div class="anomaly-card ${anomaly.severity.toLowerCase()}">
                <div class="anomaly-header">
                    <h4 class="anomaly-title">${anomaly.title}</h4>
                    <span class="anomaly-severity ${anomaly.severity.toLowerCase()}">${anomaly.severity}</span>
                </div>
                <div class="anomaly-details">
                    <div class="anomaly-detail">
                        <span class="detail-label">Metric</span>
                        <span class="detail-value">${anomaly.metricType}</span>
                    </div>
                    <div class="anomaly-detail">
                        <span class="detail-label">Actual Value</span>
                        <span class="detail-value">${this.formatNumber(anomaly.actualValue)}</span>
                    </div>
                    <div class="anomaly-detail">
                        <span class="detail-label">Expected Value</span>
                        <span class="detail-value">${this.formatNumber(anomaly.expectedValue)}</span>
                    </div>
                    <div class="anomaly-detail">
                        <span class="detail-label">Deviation</span>
                        <span class="detail-value">${anomaly.deviationPercentage.toFixed(1)}%</span>
                    </div>
                </div>
                <div class="anomaly-actions">
                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="advancedAnalytics.acknowledgeAnomaly(${anomaly.id})">
                        Acknowledge
                    </button>
                </div>
            </div>
        `).join('');

        container.innerHTML = html;
    }

    renderCustomMetrics(data) {
        const container = document.getElementById('metricsData');
        if (!container) return;

        if (!data || !data.metrics || data.metrics.length === 0) {
            container.innerHTML = this.getEmptyState('metrics');
            return;
        }

        const html = data.metrics.map(metric => `
            <div class="metric-card">
                <div class="metric-header">
                    <div class="metric-info">
                        <h3>${metric.name}</h3>
                        <span class="metric-category">${metric.category}</span>
                    </div>
                    <div class="metric-actions">
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="advancedAnalytics.editMetric(${metric.id})">
                            Edit
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="advancedAnalytics.deleteMetric(${metric.id})">
                            Delete
                        </button>
                    </div>
                </div>
                <div class="metric-value-display">
                    <div class="metric-current-value">${this.formatNumber(metric.currentValue)}</div>
                    <div class="metric-unit">${metric.unit}</div>
                </div>
                <div class="metric-change-display">
                    <span>Previous: ${this.formatNumber(metric.previousValue)}</span>
                    <span class="metric-change ${this.getChangeClass(metric.changePercentage)}">
                        ${metric.changePercentage > 0 ? '+' : ''}${metric.changePercentage.toFixed(1)}%
                    </span>
                </div>
                <div class="metric-formula">${metric.formula}</div>
            </div>
        `).join('');

        container.innerHTML = html;
    }

    getEmptyState(type) {
        const states = {
            predictive: `
                <div class="empty-state">
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
                    </svg>
                    <h3>No Predictive Data Available</h3>
                    <p>We need more historical data to generate predictions. Check back in a few days.</p>
                </div>
            `,
            anomaly: `
                <div class="empty-state">
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                    <h3>No Anomalies Detected</h3>
                    <p>Great! Your business metrics are performing within normal ranges.</p>
                </div>
            `,
            metrics: `
                <div class="empty-state">
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
                    </svg>
                    <h3>No Custom Metrics</h3>
                    <p>Create your first custom metric to track specific business KPIs.</p>
                    <button type="button" class="btn btn-primary" onclick="advancedAnalytics.showCreateMetricModal()">
                        Create Your First Metric
                    </button>
                </div>
            `
        };

        return states[type] || '';
    }

    showCreateMetricModal() {
        const modal = new bootstrap.Modal(document.getElementById('createMetricModal'));
        modal.show();
    }

    async createCustomMetric() {
        const form = document.getElementById('createMetricForm');
        const formData = new FormData(form);
        const data = Object.fromEntries(formData.entries());

        try {
            const response = await fetch(`${this.baseUrl}/metrics`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            // Close modal and refresh metrics
            const modal = bootstrap.Modal.getInstance(document.getElementById('createMetricModal'));
            modal.hide();
            
            this.loadCustomMetrics();
            this.showSuccess('Custom metric created successfully!');

        } catch (error) {
            console.error('Error creating custom metric:', error);
            this.showError('Failed to create custom metric. Please try again.');
        }
    }

    async acknowledgeAnomaly(anomalyId) {
        try {
            const response = await fetch(`${this.baseUrl}/anomalies/${anomalyId}/acknowledge`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            // Refresh anomaly data
            const period = parseInt(document.getElementById('anomalyPeriod').value);
            this.loadAnomalyDetection(period);
            this.showSuccess('Anomaly acknowledged successfully!');

        } catch (error) {
            console.error('Error acknowledging anomaly:', error);
            this.showError('Failed to acknowledge anomaly. Please try again.');
        }
    }

    async deleteMetric(metricId) {
        if (!confirm('Are you sure you want to delete this metric? This action cannot be undone.')) {
            return;
        }

        try {
            const response = await fetch(`${this.baseUrl}/metrics/${metricId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            this.loadCustomMetrics();
            this.showSuccess('Metric deleted successfully!');

        } catch (error) {
            console.error('Error deleting metric:', error);
            this.showError('Failed to delete metric. Please try again.');
        }
    }

    resetCreateMetricForm() {
        const form = document.getElementById('createMetricForm');
        if (form) {
            form.reset();
        }
    }

    showLoading(loadingElement, dataElement) {
        if (loadingElement) loadingElement.style.display = 'flex';
        if (dataElement) dataElement.style.display = 'none';
    }

    hideLoading(loadingElement, dataElement) {
        if (loadingElement) loadingElement.style.display = 'none';
        if (dataElement) dataElement.style.display = 'block';
    }

    formatNumber(value) {
        if (typeof value !== 'number') return value;
        
        if (value >= 1000000) {
            return (value / 1000000).toFixed(1) + 'M';
        } else if (value >= 1000) {
            return (value / 1000).toFixed(1) + 'K';
        } else {
            return value.toLocaleString();
        }
    }

    getChangeClass(change) {
        if (change > 0) return 'positive';
        if (change < 0) return 'negative';
        return 'neutral';
    }

    showSuccess(message) {
        // You can implement your preferred notification system here
        alert(message); // Placeholder - replace with proper notification
    }

    showError(message) {
        // You can implement your preferred notification system here
        alert(message); // Placeholder - replace with proper notification
    }
}

// Initialize the Advanced Analytics Manager when the DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.advancedAnalytics = new AdvancedAnalyticsManager();
});
