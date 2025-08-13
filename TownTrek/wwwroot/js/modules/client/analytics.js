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
    }

    // Static method to check if analytics should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-dashboard') !== null;
    }

    init() {
        if (this.isInitialized) return;
        
        try {
            this.bindEvents();
            this.initializeCharts();
            this.setupAnimations();
            this.isInitialized = true;
            
            console.log('ClientAnalyticsManager initialized successfully');
        } catch (error) {
            console.error('Error initializing ClientAnalyticsManager:', error);
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
            this.showChartLoading(canvas);
            const data = await this.fetchViewsData(30);
            
            // Clear loading state
            canvas.parentElement.innerHTML = '<canvas id="viewsChart" width="400" height="200"></canvas>';
            const newCanvas = canvas.parentElement.querySelector('#viewsChart');
            
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
            this.showChartError(canvas, 'Unable to load views data');
        }
    }

    async createReviewsChart(canvas) {
        try {
            this.showChartLoading(canvas);
            const data = await this.fetchReviewsData(30);
            
            // Clear loading state
            canvas.parentElement.innerHTML = '<canvas id="reviewsChart" width="400" height="200"></canvas>';
            const newCanvas = canvas.parentElement.querySelector('#reviewsChart');
            
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
            this.showChartError(canvas, 'Unable to load reviews data');
        }
    }

    async fetchViewsData(days) {
        try {
            const response = await fetch(`/Client/Analytics/ViewsOverTimeData?days=${days}`);
            if (!response.ok) throw new Error('Failed to fetch views data');
            
            const rawData = await response.json();
            
            // Process data for Chart.js
            const businessGroups = {};
            rawData.forEach(item => {
                if (!businessGroups[item.businessName]) {
                    businessGroups[item.businessName] = [];
                }
                businessGroups[item.businessName].push({
                    date: new Date(item.date),
                    views: item.views
                });
            });

            // Generate labels (dates)
            const labels = this.generateDateLabels(days);
            
            // Generate datasets for each business
            const datasets = Object.keys(businessGroups).map((businessName, index) => {
                const color = this.getChartColor(index);
                const businessData = businessGroups[businessName];
                
                // Fill in missing dates with 0 views
                const dataPoints = labels.map(label => {
                    const dataPoint = businessData.find(d => 
                        d.date.toDateString() === new Date(label).toDateString()
                    );
                    return dataPoint ? dataPoint.views : 0;
                });

                return {
                    label: businessName,
                    data: dataPoints,
                    borderColor: color,
                    backgroundColor: color + '20',
                    fill: false,
                    tension: 0.4
                };
            });

            return { labels, datasets };
        } catch (error) {
            console.error('Error fetching views data:', error);
            return this.getEmptyChartData(days, 'Views');
        }
    }

    async fetchReviewsData(days) {
        try {
            const response = await fetch(`/Client/Analytics/ReviewsOverTimeData?days=${days}`);
            if (!response.ok) throw new Error('Failed to fetch reviews data');
            
            const rawData = await response.json();
            
            // Process data for Chart.js
            const businessGroups = {};
            rawData.forEach(item => {
                if (!businessGroups[item.businessName]) {
                    businessGroups[item.businessName] = [];
                }
                businessGroups[item.businessName].push({
                    date: new Date(item.date),
                    reviewCount: item.reviewCount,
                    averageRating: item.averageRating
                });
            });

            // Generate labels (dates)
            const labels = this.generateDateLabels(days);
            
            // Generate datasets for each business
            const datasets = Object.keys(businessGroups).map((businessName, index) => {
                const color = this.getChartColor(index);
                const businessData = businessGroups[businessName];
                
                // Fill in missing dates with 0 reviews
                const dataPoints = labels.map(label => {
                    const dataPoint = businessData.find(d => 
                        d.date.toDateString() === new Date(label).toDateString()
                    );
                    return dataPoint ? dataPoint.reviewCount : 0;
                });

                return {
                    label: businessName,
                    data: dataPoints,
                    backgroundColor: color + '80',
                    borderColor: color,
                    borderWidth: 1
                };
            });

            return { labels, datasets };
        } catch (error) {
            console.error('Error fetching reviews data:', error);
            return this.getEmptyChartData(days, 'Reviews');
        }
    }

    generateDateLabels(days) {
        const labels = [];
        const today = new Date();
        
        for (let i = days - 1; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(date.getDate() - i);
            
            if (days <= 7) {
                labels.push(date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' }));
            } else if (days <= 30) {
                labels.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
            } else {
                labels.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
            }
        }
        
        return labels;
    }

    getChartColor(index) {
        const colors = [
            '#33658a', // lapis-lazuli
            '#86bbd8', // carolina-blue
            '#f6ae2d', // hunyadi-yellow
            '#f26419', // orange-pantone
            '#2f4858', // charcoal
            '#6c757d', // dark-gray
        ];
        return colors[index % colors.length];
    }

    getEmptyChartData(days, type) {
        const labels = this.generateDateLabels(days);
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
        if (!this.viewsChart) return;
        
        try {
            const data = await this.fetchViewsData(days);
            this.viewsChart.data = data;
            this.viewsChart.update('active');
        } catch (error) {
            console.error('Error updating views chart:', error);
        }
    }

    async updateReviewsChart(days) {
        if (!this.reviewsChart) return;
        
        try {
            const data = await this.fetchReviewsData(days);
            this.reviewsChart.data = data;
            this.reviewsChart.update('active');
        } catch (error) {
            console.error('Error updating reviews chart:', error);
        }
    }

    showChartError(canvas, message) {
        const container = canvas.parentElement;
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

    showChartLoading(canvas) {
        const container = canvas.parentElement;
        container.innerHTML = `
            <div class="chart-loading">
                <div style="text-align: center; color: #6c757d;">
                    <div style="width: 40px; height: 40px; border: 3px solid #e9ecef; border-top: 3px solid #33658a; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem;"></div>
                    <p style="margin: 0; font-size: 0.875rem;">Loading chart data...</p>
                </div>
            </div>
        `;
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

// Register with the global app
if (typeof window.TownTrekApp !== 'undefined') {
    window.TownTrekApp.registerModule('ClientAnalyticsManager', ClientAnalyticsManager);
} else {
    console.warn('TownTrekApp not found. ClientAnalyticsManager not registered.');
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ClientAnalyticsManager;
}