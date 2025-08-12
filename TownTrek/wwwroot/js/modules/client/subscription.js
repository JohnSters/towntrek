/**
 * @fileoverview ClientSubscriptionManager - handles subscription plan selection
 */

class ClientSubscriptionManager {
  constructor() {
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized) return;
    this.setupPlanSelection();
    this.isInitialized = true;
    console.log('✅ ClientSubscriptionManager initialized');
  }

  setupPlanSelection() {
    const planButtons = document.querySelectorAll('.plan-actions .btn-cta[data-tier-id]');
    planButtons.forEach((button) => {
      const tierId = parseInt(button.getAttribute('data-tier-id') || '0', 10);
      if (!tierId) return;
      button.addEventListener('click', (event) => this.selectPlan(event, tierId));
    });
  }

  selectPlan(event, tierId) {
    const button = event.currentTarget;
    const originalText = button.textContent;
    button.disabled = true;
    button.textContent = 'Processing...';

    // Placeholder for payment integration
    setTimeout(() => {
      alert('Payment integration would be implemented here for tier ID: ' + tierId);
      button.disabled = false;
      button.textContent = originalText;
    }, 1000);
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ClientSubscriptionManager;
}

// Global for auto-init
window.ClientSubscriptionManager = ClientSubscriptionManager;


