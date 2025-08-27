/**
 * @fileoverview ClientAnalyticsManager - Coordinates all analytics modules for the client analytics dashboard
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class ClientAnalyticsManager {
    constructor(options = {}) {
        this.mode = options.mode || 'default';
        this.isInitialized = false;
        this.modules = {
            core: null,
            charts: null,
            realtime: null,
            export: null,
            comparative: null
        };
        this.config = {
            autoInitialize: true,
            enableRealTime: true,
            enableExport: true,
            enableCharts: true
        };
    }

    /**
     * Initialize the analytics manager and all sub-modules
     */
    async init() {
        if (this.isInitialized) {
            console.warn('ClientAnalyticsManager already initialized');
            return;
        }

        try {
            console.log('Initializing ClientAnalyticsManager...');

            // Wait for all required modules to be available
            await this.waitForModules();

            // Initialize AnalyticsCore first (required by other modules)
            if (window.AnalyticsCore) {
                this.modules.core = new AnalyticsCore();
                this.modules.core.init();
                console.log('✅ AnalyticsCore initialized');
            } else {
                throw new Error('AnalyticsCore not found');
            }

            // Initialize AnalyticsCharts
            if (this.config.enableCharts && window.AnalyticsCharts) {
                this.modules.charts = new AnalyticsCharts(this.modules.core);
                await this.modules.charts.init();
                console.log('✅ AnalyticsCharts initialized');
                
                // Force refresh charts to ensure they load
                setTimeout(async () => {
                    try {
                        await this.modules.charts.refreshAllCharts();
                        console.log('✅ Charts refreshed after initialization');
                    } catch (error) {
                        console.error('Error refreshing charts:', error);
                    }
                }, 500);
            }

            // Initialize AnalyticsRealtime
            if (this.config.enableRealTime && window.AnalyticsRealtime) {
                this.modules.realtime = new AnalyticsRealtime();
                this.modules.realtime.init(this.modules.core, this.modules.charts);
                console.log('✅ AnalyticsRealtime initialized');
            }

            // Initialize AnalyticsExport
            if (this.config.enableExport && window.AnalyticsExport) {
                this.modules.export = new AnalyticsExport();
                this.modules.export.init(this.modules.core);
                window.analyticsExport = this.modules.export; // Set global instance
                console.log('✅ AnalyticsExport initialized');
            }

            // Initialize AnalyticsComparative (optional - only on comparative analysis pages)
            if (window.AnalyticsComparative && AnalyticsComparative.shouldInitialize()) {
                this.modules.comparative = new AnalyticsComparative(this.modules.core);
                this.modules.comparative.init();
                console.log('✅ AnalyticsComparative initialized');
            } else {
                console.log('ℹ️ AnalyticsComparative not available or not needed on this page');
            }

            this.isInitialized = true;
            console.log('🎉 ClientAnalyticsManager initialized successfully');

            // Track initialization for analytics
            if (this.modules.core) {
                await this.modules.core.trackFeatureUsage('analytics_dashboard', 'initialized', {
                    mode: this.mode,
                    modules: Object.keys(this.modules).filter(key => this.modules[key] !== null)
                });
            }

        } catch (error) {
            console.error('❌ Error initializing ClientAnalyticsManager:', error);
            throw error;
        }
    }

    /**
     * Wait for all required modules to be available
     * @private
     */
    async waitForModules() {
        const requiredModules = ['AnalyticsCore', 'AnalyticsCharts', 'AnalyticsRealtime', 'AnalyticsExport'];
        const maxWaitTime = 5000; // 5 seconds
        const checkInterval = 100; // Check every 100ms
        let elapsedTime = 0;

        while (elapsedTime < maxWaitTime) {
            const missingModules = requiredModules.filter(module => !window[module]);
            
            if (missingModules.length === 0) {
                console.log('All required modules are available');
                return;
            }

            console.log(`Waiting for modules: ${missingModules.join(', ')}`);
            await new Promise(resolve => setTimeout(resolve, checkInterval));
            elapsedTime += checkInterval;
        }

        const stillMissing = requiredModules.filter(module => !window[module]);
        throw new Error(`Timeout waiting for modules: ${stillMissing.join(', ')}`);
    }

    /**
     * Get a specific module instance
     * @param {string} moduleName - Name of the module (core, charts, realtime, export)
     * @returns {Object|null} Module instance
     */
    getModule(moduleName) {
        return this.modules[moduleName] || null;
    }

    /**
     * Refresh all charts and data
     */
    async refresh() {
        if (!this.isInitialized) {
            console.warn('ClientAnalyticsManager not initialized');
            return;
        }

        try {
            console.log('Refreshing analytics data...');

            if (this.modules.charts) {
                await this.modules.charts.refreshAllCharts();
            }

            if (this.modules.core) {
                await this.modules.core.trackFeatureUsage('analytics_dashboard', 'refreshed');
            }

            console.log('✅ Analytics data refreshed successfully');
        } catch (error) {
            console.error('❌ Error refreshing analytics data:', error);
            throw error;
        }
    }

    /**
     * Destroy all modules and cleanup
     */
    destroy() {
        if (!this.isInitialized) return;

        try {
            console.log('Destroying ClientAnalyticsManager...');

            // Destroy modules in reverse order
            if (this.modules.comparative) {
                this.modules.comparative.destroy();
            }
            if (this.modules.export) {
                this.modules.export.destroy();
            }
            if (this.modules.realtime) {
                this.modules.realtime.destroy();
            }
            if (this.modules.charts) {
                this.modules.charts.destroy();
            }
            if (this.modules.core) {
                this.modules.core.destroy();
            }

            // Clear references
            this.modules = {
                core: null,
                charts: null,
                realtime: null,
                export: null,
                comparative: null
            };

            this.isInitialized = false;
            console.log('✅ ClientAnalyticsManager destroyed successfully');
        } catch (error) {
            console.error('❌ Error destroying ClientAnalyticsManager:', error);
        }
    }

    /**
     * Get health status of all modules
     * @returns {Object} Health status
     */
    getHealthStatus() {
        return {
            isInitialized: this.isInitialized,
            mode: this.mode,
            modules: {
                core: this.modules.core ? 'initialized' : 'not_found',
                charts: this.modules.charts ? 'initialized' : 'not_found',
                realtime: this.modules.realtime ? 'initialized' : 'not_found',
                export: this.modules.export ? 'initialized' : 'not_found',
                comparative: this.modules.comparative ? 'initialized' : 'not_found'
            },
            timestamp: new Date().toISOString()
        };
    }
}

// Expose globally
window.ClientAnalyticsManager = ClientAnalyticsManager;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ClientAnalyticsManager;
}
