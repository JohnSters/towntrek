/**
 * @fileoverview TrialCountdownManager - handles trial period countdown display and updates
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class TrialCountdownManager {
    constructor() {
        this.countdownElements = [];
        this.updateInterval = null;
        this.isDestroyed = false;
    }

    init() {
        this.findCountdownElements();
        this.startCountdown();
        this.bindEvents();
    }

    destroy() {
        this.isDestroyed = true;
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }
        this.countdownElements = [];
    }

    findCountdownElements() {
        // Find all elements with trial countdown data
        const alertElements = document.querySelectorAll('[data-trial-end]');
        const cardElements = document.querySelectorAll('.trial-countdown-card[data-trial-end]');
        
        this.countdownElements = [...alertElements, ...cardElements];
    }

    startCountdown() {
        if (this.countdownElements.length === 0) return;

        // Update immediately
        this.updateCountdowns();

        // Update every minute
        this.updateInterval = setInterval(() => {
            if (this.isDestroyed) return;
            this.updateCountdowns();
        }, 60000); // Update every minute
    }

    updateCountdowns() {
        this.countdownElements.forEach(element => {
            const trialEndStr = element.getAttribute('data-trial-end');
            if (!trialEndStr) return;

            try {
                const trialEndDate = new Date(trialEndStr);
                const now = new Date();
                const timeRemaining = trialEndDate.getTime() - now.getTime();

                if (timeRemaining <= 0) {
                    this.handleTrialExpired(element);
                } else {
                    this.updateCountdownDisplay(element, timeRemaining);
                }
            } catch (error) {
                console.warn('Invalid trial end date:', trialEndStr, error);
            }
        });
    }

    updateCountdownDisplay(element, timeRemaining) {
        const days = Math.floor(timeRemaining / (1000 * 60 * 60 * 24));
        const hours = Math.floor((timeRemaining % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((timeRemaining % (1000 * 60 * 60)) / (1000 * 60));

        // Update days display
        const daysElements = element.querySelectorAll('.trial-countdown-days');
        daysElements.forEach(el => {
            el.textContent = days.toString();
        });

        // Update alert message if it exists
        const alertMessage = element.querySelector('p');
        if (alertMessage && alertMessage.textContent.includes('expires in')) {
            let timeText = '';
            if (days > 0) {
                timeText = `${days} day${days !== 1 ? 's' : ''}`;
                if (hours > 0) {
                    timeText += ` and ${hours} hour${hours !== 1 ? 's' : ''}`;
                }
            } else if (hours > 0) {
                timeText = `${hours} hour${hours !== 1 ? 's' : ''}`;
                if (minutes > 0) {
                    timeText += ` and ${minutes} minute${minutes !== 1 ? 's' : ''}`;
                }
            } else {
                timeText = `${minutes} minute${minutes !== 1 ? 's' : ''}`;
            }

            alertMessage.innerHTML = alertMessage.innerHTML.replace(
                /expires in <span class="trial-countdown-days">\d+<\/span> day[s]?/,
                `expires in <span class="trial-countdown-days">${days}</span> ${timeText}`
            );
        }

        // Update card styling based on time remaining
        if (element.classList.contains('trial-countdown-card')) {
            this.updateCardStyling(element, days);
        }
    }

    updateCardStyling(cardElement, days) {
        const iconElement = cardElement.querySelector('.stat-icon');
        if (!iconElement) return;

        // Remove existing status classes
        iconElement.classList.remove('info', 'warning', 'danger');

        // Add appropriate class based on days remaining
        if (days <= 1) {
            iconElement.classList.add('danger');
        } else if (days <= 3) {
            iconElement.classList.add('warning');
        } else {
            iconElement.classList.add('info');
        }
    }

    handleTrialExpired(element) {
        // Show expired state
        if (element.classList.contains('trial-countdown-alert')) {
            this.showExpiredAlert(element);
        } else if (element.classList.contains('trial-countdown-card')) {
            this.showExpiredCard(element);
        }

        // Optionally redirect to subscription page after a delay
        setTimeout(() => {
            if (window.location.pathname !== '/Client/Subscription') {
                window.location.href = '/Client/Subscription?expired=true';
            }
        }, 5000); // 5 second delay
    }

    showExpiredAlert(alertElement) {
        alertElement.classList.remove('alert-info');
        alertElement.classList.add('alert-danger');
        
        const content = alertElement.querySelector('.alert-content div');
        if (content) {
            content.innerHTML = `
                <strong>Trial Period Expired</strong>
                <p>Your trial period has ended. Please upgrade to a paid plan to continue using TownTrek.</p>
                <a href="/Client/Subscription" class="alert-action">Choose Plan</a>
            `;
        }
    }

    showExpiredCard(cardElement) {
        const valueElement = cardElement.querySelector('.stat-value');
        const labelElement = cardElement.querySelector('.stat-label');
        const changeElement = cardElement.querySelector('.stat-change');
        const iconElement = cardElement.querySelector('.stat-icon');

        if (valueElement) valueElement.textContent = '0';
        if (labelElement) labelElement.textContent = 'Trial Expired';
        if (changeElement) changeElement.textContent = 'Upgrade Required';
        if (iconElement) {
            iconElement.classList.remove('info', 'warning');
            iconElement.classList.add('danger');
        }
    }

    bindEvents() {
        // Handle page visibility changes to update countdown when page becomes visible
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden && !this.isDestroyed) {
                this.updateCountdowns();
            }
        });

        // Handle window focus to update countdown
        window.addEventListener('focus', () => {
            if (!this.isDestroyed) {
                this.updateCountdowns();
            }
        });
    }

    // Static method to check if trial countdown should be initialized
    static shouldInitialize() {
        return document.querySelector('[data-trial-end]') !== null;
    }
}

// Export for module system
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TrialCountdownManager;
}

// Global registration for legacy compatibility
if (typeof window !== 'undefined') {
    window.TrialCountdownManager = TrialCountdownManager;
}