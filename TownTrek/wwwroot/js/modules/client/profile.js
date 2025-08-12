/**
 * @fileoverview ClientProfileManager - handles profile edit interactions
 */

class ClientProfileManager {
  constructor() {
    this.formElement = document.querySelector('.add-business-form');
    this.submitButton = this.formElement?.querySelector('button[type="submit"]') || null;
    this.profilePictureInput = document.querySelector('input[name="ProfilePictureUrl"]');
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized || !this.formElement) return;
    this.setupFormSubmission();
    this.setupProfilePicturePreview();
    this.isInitialized = true;
    console.log('✅ ClientProfileManager initialized');
  }

  setupFormSubmission() {
    if (!this.submitButton) return;
    this.formElement.addEventListener('submit', () => this.showLoadingState());
  }

  showLoadingState() {
    this.submitButton.disabled = true;
    this.submitButton.innerHTML = `
      <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
      </svg>
      Saving...`;
  }

  setupProfilePicturePreview() {
    if (!this.profilePictureInput) return;
    this.profilePictureInput.addEventListener('blur', (e) => {
      const url = (e.target.value || '').trim();
      if (url && this.isValidUrl(url)) {
        this.showImagePreview(url);
      } else {
        this.hideImagePreview();
      }
    });
  }

  isValidUrl(value) {
    try {
      // eslint-disable-next-line no-new
      new URL(value);
      return true;
    } catch {
      return false;
    }
  }

  showImagePreview(url) {
    this.hideImagePreview();
    const preview = document.createElement('div');
    preview.className = 'image-preview';
    preview.innerHTML = `<img src="${url}" alt="Profile Picture Preview" class="profile-preview-image" onerror="this.parentElement.style.display='none';" />`;
    this.profilePictureInput.parentElement?.appendChild(preview);
  }

  hideImagePreview() {
    const preview = document.querySelector('.image-preview');
    if (preview) preview.remove();
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ClientProfileManager;
}

// Global for auto-init
window.ClientProfileManager = ClientProfileManager;


