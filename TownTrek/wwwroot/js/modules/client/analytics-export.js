/**
 * @fileoverview AnalyticsExport - Export and sharing functionality for analytics
 * @author TownTrek Development Team
 * @version 2.0.0
 */

class AnalyticsExport {
    constructor() {
        this.modalId = 'analytics-export-modal';
        this.currentBusinessId = null;
        this.analyticsCore = null;
        
        this.config = {
            apiEndpoints: {
                exportBusinessPdf: '/Client/Export/ExportBusinessPdf',
                exportOverviewPdf: '/Client/Export/ExportOverviewPdf',
                exportCsv: '/Client/Export/ExportCsv',
                generateShareableLink: '/Client/Export/GenerateShareableLink',
                sendEmailReport: '/Client/Export/SendEmailReport',
                scheduleEmailReport: '/Client/Export/ScheduleEmailReport'
            },
            defaultDateRanges: {
                '7': 'Last 7 days',
                '30': 'Last 30 days',
                '90': 'Last 90 days',
                '365': 'Last year'
            },
            defaultLinkExpirations: {
                '1': '1 day',
                '7': '7 days',
                '30': '30 days',
                '90': '90 days'
            },
            defaultReportFrequencies: {
                'once': 'Send once',
                'daily': 'Daily',
                'weekly': 'Weekly',
                'monthly': 'Monthly'
            }
        };
    }

    // Static method to check if export functionality should be initialized
    static shouldInitialize() {
        return document.querySelector('.analytics-dashboard') !== null;
    }

    init(analyticsCore) {
        this.analyticsCore = analyticsCore;
        this.createModal();
        this.bindEvents();
        
        console.log('AnalyticsExport initialized');
    }

    createModal() {
        const modalHtml = `
            <div id="${this.modalId}" class="confirmation-modal" style="--modal-width: 600px">
                <div class="confirmation-modal-overlay" data-close></div>
                <div class="confirmation-modal-content">
                    <div class="confirmation-modal-header">
                        <div class="confirmation-modal-icon info">
                            <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
                            </svg>
                        </div>
                        <h3 class="confirmation-modal-title">Export & Share Analytics</h3>
                    </div>
                    <div class="confirmation-modal-body">
                        <div class="export-section">
                            <h4>Export Data</h4>
                            <div class="form-group">
                                <label class="form-label">Date Range</label>
                                <select id="export-date-range" class="form-control">
                                    <option value="7">Last 7 days</option>
                                    <option value="30" selected>Last 30 days</option>
                                    <option value="90">Last 90 days</option>
                                    <option value="365">Last year</option>
                                </select>
                            </div>
                            <div class="export-buttons">
                                <button type="button" class="btn btn-primary" onclick="analyticsExport.exportPdf()">
                                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
                                    </svg>
                                    Export PDF
                                </button>
                                <button type="button" class="btn btn-secondary" onclick="analyticsExport.exportCsv()">
                                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
                                    </svg>
                                    Export CSV
                                </button>
                            </div>
                        </div>

                        <div class="share-section">
                            <h4>Share Dashboard</h4>
                            <div class="form-group">
                                <label class="form-label">Link Expiration</label>
                                <select id="link-expiration" class="form-control">
                                    <option value="1">1 day</option>
                                    <option value="7" selected>7 days</option>
                                    <option value="30">30 days</option>
                                    <option value="90">90 days</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Description (optional)</label>
                                <input type="text" id="link-description" class="form-control" placeholder="Brief description of this dashboard">
                            </div>
                            <button type="button" class="btn btn-success" onclick="analyticsExport.generateShareableLink()">
                                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.367 2.684 3 3 0 00-5.367-2.684z"></path>
                                </svg>
                                Generate Shareable Link
                            </button>
                            <div id="shareable-link-section" class="shareable-link-section" style="display: none;">
                                <div class="input-group">
                                    <input type="text" id="shareable-link" class="form-control" readonly>
                                    <button type="button" class="btn btn-outline" onclick="analyticsExport.copyShareableLink()">
                                        <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path>
                                        </svg>
                                        Copy
                                    </button>
                                </div>
                            </div>
                        </div>

                        <div class="email-section">
                            <h4>Email Reports</h4>
                            <div class="form-group">
                                <label class="form-label">Email Address</label>
                                <input type="email" id="email-address" class="form-control" placeholder="Enter email address">
                            </div>
                            <div class="form-group">
                                <label class="form-label">Report Frequency</label>
                                <select id="report-frequency" class="form-control">
                                    <option value="once">Send once</option>
                                    <option value="daily">Daily</option>
                                    <option value="weekly" selected>Weekly</option>
                                    <option value="monthly">Monthly</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Custom Message (optional)</label>
                                <textarea id="custom-message" class="form-control" rows="3" placeholder="Add a personal message to your report"></textarea>
                            </div>
                            <div class="email-buttons">
                                <button type="button" class="btn btn-info" onclick="analyticsExport.sendEmailReport()">
                                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
                                    </svg>
                                    Send Now
                                </button>
                                <button type="button" class="btn btn-warning" onclick="analyticsExport.scheduleEmailReport()">
                                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                                    </svg>
                                    Schedule
                                </button>
                            </div>
                        </div>
                    </div>
                    <div class="confirmation-modal-actions">
                        <button type="button" class="confirmation-modal-btn confirmation-modal-btn-secondary" data-close>Close</button>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if present
        const existing = document.getElementById(this.modalId);
        if (existing) existing.remove();

        document.body.insertAdjacentHTML('beforeend', modalHtml);
    }

    bindEvents() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;

        // Close modal events
        modal.addEventListener('click', (e) => {
            if (e.target.dataset.close !== undefined) {
                this.hide();
            }
        });

        // Escape key
        modal.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.hide();
            }
        });
    }

    show(businessId = null) {
        this.currentBusinessId = businessId;
        const modal = document.getElementById(this.modalId);
        if (modal) {
            modal.classList.add('open');
        }
        
        if (this.analyticsCore) {
            this.analyticsCore.trackFeatureUsage('export-modal', 'opened', { 
                businessId: businessId 
            });
        }
    }

    hide() {
        const modal = document.getElementById(this.modalId);
        if (modal) {
            modal.classList.remove('open');
        }
    }

    async exportPdf() {
        try {
            const dateRange = document.getElementById('export-date-range').value;
            const url = this.currentBusinessId 
                ? `${this.config.apiEndpoints.exportBusinessPdf}?businessId=${this.currentBusinessId}&dateRange=${dateRange}`
                : `${this.config.apiEndpoints.exportOverviewPdf}?dateRange=${dateRange}`;

            window.open(url, '_blank');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('export-pdf', 'success', { 
                    businessId: this.currentBusinessId,
                    dateRange: dateRange 
                });
            }
        } catch (error) {
            console.error('PDF export error:', error);
            this.showError('Failed to export PDF. Please try again.');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('export-pdf', 'error', { 
                    error: error.message 
                });
            }
        }
    }

    async exportCsv() {
        try {
            const dateRange = document.getElementById('export-date-range').value;
            const url = this.currentBusinessId 
                ? `${this.config.apiEndpoints.exportCsv}?businessId=${this.currentBusinessId}&dateRange=${dateRange}`
                : `${this.config.apiEndpoints.exportCsv}?dateRange=${dateRange}`;

            window.open(url, '_blank');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('export-csv', 'success', { 
                    businessId: this.currentBusinessId,
                    dateRange: dateRange 
                });
            }
        } catch (error) {
            console.error('CSV export error:', error);
            this.showError('Failed to export CSV. Please try again.');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('export-csv', 'error', { 
                    error: error.message 
                });
            }
        }
    }

    async generateShareableLink() {
        try {
            const expiration = document.getElementById('link-expiration').value;
            const description = document.getElementById('link-description').value;
            const dashboardType = this.currentBusinessId ? 'business' : 'overview';

            const response = await fetch(this.config.apiEndpoints.generateShareableLink, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    businessId: this.currentBusinessId,
                    dashboardType: dashboardType,
                    expirationDays: parseInt(expiration),
                    description: description
                })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    document.getElementById('shareable-link').value = data.shareableUrl;
                    document.getElementById('shareable-link-section').style.display = 'block';
                    this.showSuccess('Shareable link generated successfully!');
                    
                    if (this.analyticsCore) {
                        this.analyticsCore.trackFeatureUsage('generate-shareable-link', 'success', { 
                            businessId: this.currentBusinessId,
                            dashboardType: dashboardType,
                            expirationDays: expiration 
                        });
                    }
                } else {
                    this.showError(data.message || 'Failed to generate shareable link.');
                    
                    if (this.analyticsCore) {
                        this.analyticsCore.trackFeatureUsage('generate-shareable-link', 'error', { 
                            error: data.message 
                        });
                    }
                }
            } else {
                this.showError('Failed to generate shareable link. Please try again.');
            }
        } catch (error) {
            console.error('Generate shareable link error:', error);
            this.showError('Failed to generate shareable link. Please try again.');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('generate-shareable-link', 'error', { 
                    error: error.message 
                });
            }
        }
    }

    async copyShareableLink() {
        try {
            const linkInput = document.getElementById('shareable-link');
            await navigator.clipboard.writeText(linkInput.value);
            this.showSuccess('Link copied to clipboard!');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('copy-shareable-link', 'success');
            }
        } catch (error) {
            console.error('Copy link error:', error);
            this.showError('Failed to copy link. Please copy manually.');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('copy-shareable-link', 'error', { 
                    error: error.message 
                });
            }
        }
    }

    async sendEmailReport() {
        try {
            const email = document.getElementById('email-address').value;
            const customMessage = document.getElementById('custom-message').value;
            const dateRange = document.getElementById('export-date-range').value;

            if (!email) {
                this.showError('Please enter an email address.');
                return;
            }

            const response = await fetch(this.config.apiEndpoints.sendEmailReport, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    businessId: this.currentBusinessId,
                    email: email,
                    customMessage: customMessage,
                    dateRange: parseInt(dateRange)
                })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showSuccess('Email report sent successfully!');
                    
                    if (this.analyticsCore) {
                        this.analyticsCore.trackFeatureUsage('send-email-report', 'success', { 
                            businessId: this.currentBusinessId,
                            dateRange: dateRange 
                        });
                    }
                } else {
                    this.showError(data.message || 'Failed to send email report.');
                    
                    if (this.analyticsCore) {
                        this.analyticsCore.trackFeatureUsage('send-email-report', 'error', { 
                            error: data.message 
                        });
                    }
                }
            } else {
                this.showError('Failed to send email report. Please try again.');
            }
        } catch (error) {
            console.error('Send email report error:', error);
            this.showError('Failed to send email report. Please try again.');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('send-email-report', 'error', { 
                    error: error.message 
                });
            }
        }
    }

    async scheduleEmailReport() {
        try {
            const email = document.getElementById('email-address').value;
            const frequency = document.getElementById('report-frequency').value;
            const customMessage = document.getElementById('custom-message').value;
            const dateRange = document.getElementById('export-date-range').value;

            if (!email) {
                this.showError('Please enter an email address.');
                return;
            }

            const response = await fetch(this.config.apiEndpoints.scheduleEmailReport, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    businessId: this.currentBusinessId,
                    email: email,
                    frequency: frequency,
                    customMessage: customMessage,
                    dateRange: parseInt(dateRange)
                })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showSuccess('Email report scheduled successfully!');
                    
                    if (this.analyticsCore) {
                        this.analyticsCore.trackFeatureUsage('schedule-email-report', 'success', { 
                            businessId: this.currentBusinessId,
                            frequency: frequency,
                            dateRange: dateRange 
                        });
                    }
                } else {
                    this.showError(data.message || 'Failed to schedule email report.');
                    
                    if (this.analyticsCore) {
                        this.analyticsCore.trackFeatureUsage('schedule-email-report', 'error', { 
                            error: data.message 
                        });
                    }
                }
            } else {
                this.showError('Failed to schedule email report. Please try again.');
            }
        } catch (error) {
            console.error('Schedule email report error:', error);
            this.showError('Failed to schedule email report. Please try again.');
            
            if (this.analyticsCore) {
                this.analyticsCore.trackFeatureUsage('schedule-email-report', 'error', { 
                    error: error.message 
                });
            }
        }
    }

    getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    showSuccess(message) {
        if (window.showFlashMessage) {
            window.showFlashMessage(message, 'success');
        } else {
            alert(message);
        }
    }

    showError(message) {
        if (window.showFlashMessage) {
            window.showFlashMessage(message, 'error');
        } else {
            alert(message);
        }
    }
}

// Global instance
window.analyticsExport = null;

// Global function for opening the modal (for backward compatibility)
window.openAnalyticsExportModal = function(businessId = null) {
    if (window.analyticsExport) {
        window.analyticsExport.show(businessId);
    }
};

// Global function for opening the share modal (for backward compatibility)
window.openAnalyticsShareModal = function(businessId = null) {
    if (window.analyticsExport) {
        window.analyticsExport.show(businessId);
    }
};
