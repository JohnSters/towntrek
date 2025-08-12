/**
 * @fileoverview AdminUserManagement - handles Users index interactions
 */

class AdminUserManagement {
  constructor() {
    this.formElement = document.getElementById('userSearchForm');
    this.searchInputElement = document.getElementById('userSearchInput');
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized) return;
    this.bindSearchDebounce();
    this.isInitialized = true;
    console.log('✅ AdminUserManagement initialized');
  }

  bindSearchDebounce() {
    if (!this.formElement || !this.searchInputElement) return;

    const submitWithResetPage = () => {
      const pageField = this.formElement.querySelector('input[name="page"]');
      if (pageField) pageField.value = '1';
      this.formElement.submit();
    };

    const handler = (typeof Utils !== 'undefined' && Utils.debounce)
      ? Utils.debounce(submitWithResetPage, 350)
      : (() => {
          let timerId;
          return () => {
            clearTimeout(timerId);
            timerId = setTimeout(submitWithResetPage, 350);
          };
        })();

    this.searchInputElement.addEventListener('input', handler);
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = AdminUserManagement;
}

// Global
window.AdminUserManagement = AdminUserManagement;
