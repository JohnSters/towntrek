/**
 * @fileoverview AdminTownManagement - handles Admin/Towns list and interactions
 */

class AdminTownManagement {
  constructor() {
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized) return;
    this.setupTableInteractions();
    this.setupFilterAndExport();
    this.isInitialized = true;
    console.log('✅ AdminTownManagement initialized');
  }

  setupTableInteractions() {
    const tableRows = document.querySelectorAll('.admin-table tbody tr');
    tableRows.forEach((row) => {
      row.addEventListener('mouseenter', function () {
        this.style.backgroundColor = 'var(--light-gray)';
      });
      row.addEventListener('mouseleave', function () {
        this.style.backgroundColor = '';
      });
    });
  }

  setupFilterAndExport() {
    const filterBtn = document.querySelector('.header-btn[title="Filter"]');
    const exportBtn = document.querySelector('.header-btn[title="Export"]');

    if (filterBtn) {
      filterBtn.addEventListener('click', () => {
        if (window.NotificationManager) {
          NotificationManager.info('Filter functionality coming soon');
        }
      });
    }

    if (exportBtn) {
      exportBtn.addEventListener('click', () => {
        if (window.NotificationManager) {
          NotificationManager.info('Export functionality coming soon');
        }
      });
    }
  }

  static confirmDeleteTown(townName, townId) {
    if (typeof showConfirmationModal === 'function') {
      showConfirmationModal({
        title: 'Delete Town',
        message: `Are you sure you want to delete "${townName}"?`,
        details: 'This action cannot be undone. The town will be permanently removed.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        iconType: 'danger',
        confirmButtonType: 'danger',
        formAction: '/AdminTowns/Delete',
        formMethod: 'post',
        formData: { id: townId },
      });
    } else {
      const confirmed = window.confirm(`Delete town "${townName}"?`);
      if (confirmed) {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = '/AdminTowns/Delete';
        const idInput = document.createElement('input');
        idInput.type = 'hidden';
        idInput.name = 'id';
        idInput.value = townId;
        form.appendChild(idInput);
        document.body.appendChild(form);
        form.submit();
      }
    }
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = AdminTownManagement;
}

// Global
window.AdminTownManagement = AdminTownManagement;
// Backwards compatibility for inline onclick in views
window.confirmDeleteTown = AdminTownManagement.confirmDeleteTown;
