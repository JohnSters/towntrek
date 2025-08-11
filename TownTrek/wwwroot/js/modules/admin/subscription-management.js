/**
 * @fileoverview AdminSubscriptionManagement - handles subscription index interactions
 */

class AdminSubscriptionManagement {
  constructor() {
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized) return;
    this.setupDeactivationConfirmations();
    this.setupTableInteractions();
    this.injectSpinStyles();
    this.isInitialized = true;
    console.log('✅ AdminSubscriptionManagement initialized');
  }

  setupDeactivationConfirmations() {
    const deactivateButtons = document.querySelectorAll('.btn-action.btn-danger[type="submit"]');

    deactivateButtons.forEach((button) => {
      button.removeAttribute('onclick');
      button.addEventListener('click', function (e) {
        e.preventDefault();
        const form = this.closest('form');
        const tierName = this.closest('tr').querySelector('.tier-name')?.textContent?.trim() || 'this tier';
        AdminSubscriptionManagement.showDeactivationConfirmation(tierName, form);
      });
    });
  }

  static showDeactivationConfirmation(tierName, form) {
    const proceed = window.confirm(
      `Are you sure you want to deactivate the "${tierName}" tier?\n\nThis action will prevent new subscriptions to this tier, but existing subscriptions will remain active.`
    );

    if (proceed) {
      const submitButton = form.querySelector('button[type="submit"]');
      if (submitButton) {
        submitButton.disabled = true;
        submitButton.innerHTML = `
          <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
          </svg>`;
      }
      form.submit();
    }
  }

  setupTableInteractions() {
    const tableRows = document.querySelectorAll('.admin-table tbody tr');
    tableRows.forEach((row) => {
      row.addEventListener('click', function (e) {
        if (e.target.closest('.action-buttons')) return;
        const detailsLink = this.querySelector('a[href*="Details"]');
        if (detailsLink) window.location.href = detailsLink.href;
      });
      row.style.cursor = 'pointer';
      row.addEventListener('mouseenter', function () {
        this.style.backgroundColor = 'var(--light-gray)';
      });
      row.addEventListener('mouseleave', function () {
        this.style.backgroundColor = '';
      });
    });
  }

  injectSpinStyles() {
    if (document.getElementById('admin-spin-style')) return;
    const style = document.createElement('style');
    style.id = 'admin-spin-style';
    style.textContent = `
      .animate-spin { animation: spin 1s linear infinite; }
      @keyframes spin { from { transform: rotate(0deg);} to { transform: rotate(360deg);} }
    `;
    document.head.appendChild(style);
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = AdminSubscriptionManagement;
}

// Global
window.AdminSubscriptionManagement = AdminSubscriptionManagement;
