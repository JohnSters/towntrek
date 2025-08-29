/**
 * @fileoverview Modal and simple modal utilities for TownTrek
 * Provides ModalComponent, ConfirmationModal, and FormModal.
 * Designed for legacy-compatibility global registration and used by core/app.js auto-init.
 * @author TownTrek Development Team
 * @version 2.0.0
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

/**
 * Confirmation modal utility to replace legacy confirmation-modal.js
 */
class ConfirmationModal {
  static show(config = {}) {
    const {
      title = 'Confirm Action',
      message = 'Are you sure you want to proceed?',
      details = '',
      confirmText = 'Confirm',
      cancelText = 'Cancel',
      iconType = 'info',
      confirmButtonType = 'primary',
      formAction,
      formMethod = 'post',
      formData = null,
      onConfirm,
    } = config;

    const escapeHtml = (str) => {
      const div = document.createElement('div');
      div.textContent = String(str ?? '');
      return div.innerHTML;
    };

    const modalId = 'tt-confirmation-modal';
    const existing = document.getElementById(modalId);
    if (existing) existing.remove();

    const iconMap = {
      success: '<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>',
      warning: '<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>',
      danger: '<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>',
      info: '<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>',
    };

    const html = `
      <div id="${modalId}" class="confirmation-modal">
        <div class="confirmation-modal-overlay" data-close></div>
        <div class="confirmation-modal-content">
          <div class="confirmation-modal-header">
            <div class="confirmation-modal-icon ${iconType}">${iconMap[iconType] || iconMap.info}</div>
            <h3 class="confirmation-modal-title">${escapeHtml(title)}</h3>
          </div>
          <div class="confirmation-modal-body">
            <p class="confirmation-modal-message">${escapeHtml(message)}</p>
            ${details ? `<div class="confirmation-modal-details"><p class="confirmation-modal-details-text">${escapeHtml(details)}</p></div>` : ''}
          </div>
          <div class="confirmation-modal-actions">
            <button type="button" class="confirmation-modal-btn confirmation-modal-btn-secondary" data-cancel>${escapeHtml(cancelText)}</button>
            <button type="button" class="confirmation-modal-btn confirmation-modal-btn-${confirmButtonType}" data-confirm>${escapeHtml(confirmText)}</button>
          </div>
        </div>
      </div>`;

    document.body.insertAdjacentHTML('beforeend', html);
    const modal = document.getElementById(modalId);
    
    // Add the 'open' class to make the modal visible
    setTimeout(() => modal?.classList.add('open'), 10);
    
    const close = () => {
      modal?.classList.remove('open');
      setTimeout(() => modal?.remove(), 300);
    };
    modal.querySelector('[data-close]')?.addEventListener('click', close);
    modal.querySelector('[data-cancel]')?.addEventListener('click', close);
    modal.addEventListener('keydown', (e) => { if (e.key === 'Escape') close(); });

    const confirmHandler = () => {
      if (formAction) {
        const form = document.createElement('form');
        form.method = formMethod;
        form.action = formAction;
        form.style.display = 'none';
        // Anti-forgery token
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
          const hidden = document.createElement('input');
          hidden.type = 'hidden';
          hidden.name = '__RequestVerificationToken';
          hidden.value = tokenInput.value;
          form.appendChild(hidden);
        }
        if (formData) {
          Object.entries(formData).forEach(([key, value]) => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = value;
            form.appendChild(input);
          });
        }
        document.body.appendChild(form);
        form.submit();
      } else if (typeof onConfirm === 'function') {
        onConfirm();
      }
      close();
    };

    modal.querySelector('[data-confirm]')?.addEventListener('click', confirmHandler);
  }
}

// Legacy compatibility helpers
window.showConfirmationModal = (config) => ConfirmationModal.show(config);
window.confirmApproveBusiness = (name, id) => ConfirmationModal.show({
    title: 'Approve Business',
    message: `Are you sure you want to approve "${name}"?`,
    details: 'This business will become live and visible to users.',
    confirmText: 'Approve',
    iconType: 'success',
    confirmButtonType: 'success',
    formAction: '/AdminBusinesses/Approve',
    formMethod: 'post',
    formData: { id }
  });
window.confirmRejectBusiness = (name, id) => ConfirmationModal.show({
  title: 'Reject Business',
  message: `Are you sure you want to reject "${name}"?`,
  details: 'This business will be marked as inactive and hidden from users.',
  confirmText: 'Reject',
  iconType: 'warning',
  confirmButtonType: 'warning',
  formAction: '/AdminBusinesses/Reject',
  formMethod: 'post',
  formData: { id }
});
window.confirmSuspendBusiness = (name, id) => ConfirmationModal.show({
  title: 'Suspend Business',
  message: `Are you sure you want to suspend "${name}"?`,
  details: 'This business will be temporarily hidden from users until reactivated.',
  confirmText: 'Suspend',
  iconType: 'warning',
  confirmButtonType: 'warning',
  formAction: '/AdminBusinesses/Suspend',
  formMethod: 'post',
  formData: { id }
});
window.confirmDeleteBusiness = (name, id) => ConfirmationModal.show({
  title: 'Delete Business',
  message: `Are you sure you want to delete "${name}"?`,
  details: 'This action cannot be undone. The business will be permanently removed.',
  confirmText: 'Delete',
  iconType: 'danger',
  confirmButtonType: 'danger',
  formAction: '/AdminBusinesses/Delete',
  formMethod: 'post',
  formData: { id }
});
window.confirmDeleteClientBusiness = (name, id) => ConfirmationModal.show({
  title: 'Delete Business',
  message: `Are you sure you want to delete "${name}"?`,
  details: 'This action cannot be undone. The business will be permanently removed.',
  confirmText: 'Delete',
  iconType: 'danger',
  confirmButtonType: 'danger',
  formAction: '/Client/Business/DeleteBusiness',
  formMethod: 'post',
  formData: { id }
});

/**
 * Form modal utility for simple form dialogs
 * Usage:
 * FormModal.show({
 *   title: 'Edit',
 *   fields: [
 *     { name: 'altText', label: 'Alt Text', type: 'text', value: '' },
 *     { name: 'displayOrder', label: 'Display Order', type: 'number', value: '' }
 *   ],
 *   submitText: 'Save',
 *   cancelText: 'Cancel',
 *   onSubmit: async (values, close) => { ...; close(); }
 * })
 */
class FormModal {
  static show(config = {}) {
    const {
      title = 'Form',
      fields = [],
      submitText = 'Submit',
      cancelText = 'Cancel',
      onSubmit,
      width = '520px',
    } = config;

    const modalId = 'tt-form-modal';
    const existing = document.getElementById(modalId);
    if (existing) existing.remove();

    const fieldsHtml = fields.map((f) => {
      const safeVal = (FormModal.escapeHtml(String(f.value ?? '')));
      const input = f.type === 'number'
        ? `<input type="number" class="form-input" name="${f.name}" value="${safeVal}" ${f.min != null ? `min="${f.min}"` : ''} ${f.max != null ? `max="${f.max}"` : ''} />`
        : `<input type="text" class="form-input" name="${f.name}" value="${safeVal}" placeholder="${FormModal.escapeHtml(f.placeholder || '')}" />`;
      return `<div class="form-group${f.fullWidth ? ' form-group-full' : ''}">
        <label class="form-label">${FormModal.escapeHtml(f.label || f.name)}</label>
        ${input}
      </div>`;
    }).join('');

    const html = `
      <div id="${modalId}" class="confirmation-modal" style="--modal-width: ${width}">
        <div class="confirmation-modal-overlay" data-close></div>
        <div class="confirmation-modal-content">
          <div class="confirmation-modal-header">
            <div class="confirmation-modal-icon info">
              <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
            </div>
            <h3 class="confirmation-modal-title">${FormModal.escapeHtml(title)}</h3>
          </div>
          <div class="confirmation-modal-body">
            <div class="form-grid">${fieldsHtml}</div>
          </div>
          <div class="confirmation-modal-actions">
            <button type="button" class="confirmation-modal-btn confirmation-modal-btn-secondary" data-close>${FormModal.escapeHtml(cancelText)}</button>
            <button type="button" class="confirmation-modal-btn confirmation-modal-btn-primary" data-submit>${FormModal.escapeHtml(submitText)}</button>
          </div>
        </div>
      </div>`;

    document.body.insertAdjacentHTML('beforeend', html);
    const modal = document.getElementById(modalId);
    const close = () => modal?.remove();
    modal.addEventListener('click', (e) => { if (e.target.dataset.close !== undefined) close(); });
    const submitBtn = modal.querySelector('[data-submit]');
    submitBtn.addEventListener('click', async () => {
      const values = {};
      fields.forEach((f) => {
        const el = modal.querySelector(`[name="${f.name}"]`);
        values[f.name] = el ? el.value : '';
      });
      try {
        if (typeof onSubmit === 'function') {
          await onSubmit(values, close);
        } else {
          close();
        }
      } catch (error) {
        console.error('FormModal submit error:', error);
      }
    });
  }

  static escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { ModalComponent, ConfirmationModal, FormModal };
}

// Global helpers
window.FormModal = FormModal;
window.showFormModal = (config) => FormModal.show(config);

// Global
window.ModalComponent = ModalComponent;
window.ConfirmationModal = ConfirmationModal;

