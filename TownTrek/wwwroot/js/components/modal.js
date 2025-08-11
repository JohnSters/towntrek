/**
 * @fileoverview ModalComponent - basic show/hide and ESC handling
 * Note: Will be extended to fully replace confirmation-modal.js
 */

class ModalComponent {
  constructor() {
    this.activeModals = new Set();
    this.bindGlobalHandlers();
  }

  bindGlobalHandlers() {
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        this.hideTopMost();
      }
    });
  }

  show(selector) {
    const modal = typeof selector === 'string' ? document.querySelector(selector) : selector;
    if (!modal) return;
    modal.classList.add('open');
    this.activeModals.add(modal);
  }

  hide(selector) {
    const modal = typeof selector === 'string' ? document.querySelector(selector) : selector;
    if (!modal) return;
    modal.classList.remove('open');
    this.activeModals.delete(modal);
  }

  hideTopMost() {
    const modals = Array.from(this.activeModals);
    const top = modals.pop();
    if (top) this.hide(top);
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ModalComponent;
}

// Global
window.ModalComponent = ModalComponent;


