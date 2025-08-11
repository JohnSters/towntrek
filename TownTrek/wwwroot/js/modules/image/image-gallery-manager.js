/**
 * @fileoverview ImageGalleryManager - manages business logo/gallery uploads and actions
 */

class ImageGalleryManager {
  constructor() {
    this.init();
  }

  init() {
    const pageHeader = document.querySelector('.page-header');
    this.businessId = pageHeader?.dataset.businessId;
    this.maxFileSize = parseInt(pageHeader?.dataset.maxFileSize || '0', 10);
    try {
      const raw = pageHeader?.dataset.allowedTypes ? JSON.parse(pageHeader.dataset.allowedTypes) : [];
      // Normalize allowed types to support both MIME types (image/jpeg) and extensions (.jpg, jpg)
      const norm = Array.isArray(raw) ? raw : [];
      this.allowedTypes = norm.map((t) => String(t).trim().toLowerCase());
      this.allowedMimeTypes = this.allowedTypes.filter((t) => t.includes('/'));
      this.allowedExtensions = this.allowedTypes
        .filter((t) => !t.includes('/'))
        .map((t) => (t.startsWith('.') ? t : `.${t}`));
    } catch {
      this.allowedTypes = [];
      this.allowedMimeTypes = [];
      this.allowedExtensions = [];
    }

    this.logoUploadArea = document.getElementById('logoUploadArea');
    this.logoUploadInput = document.getElementById('logoUpload');
    this.galleryUploadArea = document.getElementById('galleryUploadArea');
    this.galleryUploadInput = document.getElementById('galleryUpload');

    this.bindUploadHandlers();
    this.bindDndHandlers(['logoUploadArea', 'galleryUploadArea']);
    this.bindActionHandlers();
  }

  bindUploadHandlers() {
    if (this.logoUploadArea && this.logoUploadInput) {
      this.logoUploadArea.addEventListener('click', () => this.logoUploadInput.click());
      this.logoUploadInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
          this.uploadImage(e.target.files[0], 'Logo');
        }
      });
    }

    if (this.galleryUploadArea && this.galleryUploadInput) {
      this.galleryUploadArea.addEventListener('click', () => this.galleryUploadInput.click());
      this.galleryUploadInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
          this.uploadMultipleImages(Array.from(e.target.files), 'Gallery');
        }
      });
    }
  }

  bindDndHandlers(areaIds) {
    areaIds.forEach((areaId) => {
      const area = document.getElementById(areaId);
      if (!area) return;
      area.addEventListener('dragover', (e) => {
        e.preventDefault();
        area.classList.add('dragover');
      });
      area.addEventListener('dragleave', () => area.classList.remove('dragover'));
      area.addEventListener('drop', (e) => {
        e.preventDefault();
        area.classList.remove('dragover');
        const files = Array.from(e.dataTransfer.files);
        const imageType = areaId === 'logoUploadArea' ? 'Logo' : 'Gallery';
        if (imageType === 'Logo' && files.length > 0) {
          this.uploadImage(files[0], 'Logo');
        } else if (imageType === 'Gallery') {
          this.uploadMultipleImages(files, 'Gallery');
        }
      });
    });
  }

  async uploadImage(file, imageType) {
    if (!this.validateFile(file)) return;
    const formData = new FormData();
    formData.append('BusinessId', this.businessId);
    formData.append('ImageType', imageType);
    formData.append('ImageFile', file);
    try {
      this.showProgress(true);
      const token = window.ApiClient?.getAntiForgeryToken?.();
      const response = await fetch('/Image/Upload', {
        method: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : undefined,
        body: formData
      });
      const result = await response.json();
      if (result.success) {
        this.notify('Image uploaded successfully!', 'success');
        window.location.reload();
      } else {
        this.notify(result.message || 'Upload failed', 'error');
      }
    } catch (error) {
      ErrorHandler.handle(error, 'Image upload');
      this.notify('Upload failed: ' + (error.message || 'Unknown error'), 'error');
    } finally {
      this.showProgress(false);
    }
  }

  async uploadMultipleImages(files, imageType) {
    const validFiles = files.filter((f) => this.validateFile(f));
    if (validFiles.length === 0) return;
    const formData = new FormData();
    formData.append('businessId', this.businessId);
    formData.append('imageType', imageType);
    validFiles.forEach((file) => formData.append('files', file));
    try {
      this.showProgress(true);
      const token = window.ApiClient?.getAntiForgeryToken?.();
      const response = await fetch('/Image/UploadMultiple', {
        method: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : undefined,
        body: formData
      });
      const result = await response.json();
      if (result.success) {
        this.notify(result.message || 'Images uploaded', 'success');
        window.location.reload();
      } else {
        this.notify(result.message || 'Upload failed', 'error');
      }
    } catch (error) {
      ErrorHandler.handle(error, 'Multiple image upload');
      this.notify('Upload failed: ' + (error.message || 'Unknown error'), 'error');
    } finally {
      this.showProgress(false);
    }
  }

  bindActionHandlers() {
    document.addEventListener('click', (e) => {
      const button = e.target.closest('[data-action]');
      if (!button) return;
      const action = button.dataset.action;
      const imageId = button.dataset.imageId;
      if (action === 'delete') {
        this.deleteImage(imageId);
      } else if (action === 'edit') {
        this.openEditModal(imageId);
      }
    });
  }

  async deleteImage(imageId) {
    const proceed = window.confirm('Are you sure you want to delete this image?');
    if (!proceed) return;
    try {
      const response = await fetch('/Image/Delete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ imageId })
      });
      const result = await response.json();
      if (result.success) {
        this.notify('Image deleted successfully!', 'success');
        const imageElement = document.querySelector(`[data-image-id="${imageId}"]`);
        imageElement?.remove();
      } else {
        this.notify(result.message || 'Delete failed', 'error');
      }
    } catch (error) {
      this.notify('Delete failed: ' + error.message, 'error');
    }
  }

  openEditModal(imageId) {
    const card = document.querySelector(`[data-image-id="${imageId}"]`);
    if (!card) return;
    const currentAlt = this.escapeHtml(card.dataset.altText || '');
    const currentOrder = card.dataset.displayOrder || '';

    if (window.FormModal) {
      window.FormModal.show({
        title: 'Edit Image Metadata',
        submitText: 'Save',
        cancelText: 'Cancel',
        fields: [
          { name: 'altText', label: 'Alt Text', type: 'text', value: currentAlt, fullWidth: true },
          { name: 'displayOrder', label: 'Display Order', type: 'number', value: currentOrder }
        ],
        onSubmit: async (values, close) => {
          const altText = (values.altText || '').trim();
          const displayOrderRaw = values.displayOrder || '';
          const displayOrder = displayOrderRaw === '' ? '' : parseInt(displayOrderRaw, 10);
          const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
          const form = new URLSearchParams();
          form.set('imageId', String(imageId));
          form.set('altText', altText);
          if (displayOrder !== '') form.set('displayOrder', String(displayOrder));
          if (tokenEl?.value) form.set('__RequestVerificationToken', tokenEl.value);
          const res = await fetch('/Image/UpdateMetadata', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: form.toString() });
          const result = await res.json();
          if (result.success) {
            this.notify('Image updated successfully', 'success');
            const img = card.querySelector('img');
            if (img) img.alt = altText;
            card.dataset.altText = altText;
            card.dataset.displayOrder = displayOrderRaw;
            close();
          } else {
            this.notify(result.message || 'Failed to update image', 'error');
          }
        }
      });
    }
  }

  escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
  }

  validateFile(file) {
    if (!file) return false;

    if (this.maxFileSize && file.size > this.maxFileSize) {
      this.notify(`File size must be less than ${(this.maxFileSize / (1024 * 1024)).toFixed(1)}MB`, 'error');
      return false;
    }

    // If no restrictions provided, allow
    if (!this.allowedMimeTypes?.length && !this.allowedExtensions?.length) {
      return true;
    }

    const mime = String(file.type || '').toLowerCase();
    const ext = `.${(file.name || '').split('.').pop()?.toLowerCase() || ''}`;

    const mimeAllowed = this.allowedMimeTypes?.length ? this.allowedMimeTypes.includes(mime) : false;
    const extAllowed = this.allowedExtensions?.length ? this.allowedExtensions.includes(ext) : false;

    if (!mimeAllowed && !extAllowed) {
      const allowList = [...(this.allowedMimeTypes || []), ...(this.allowedExtensions || [])].join(', ');
      this.notify(`File type not allowed. Allowed: ${allowList || 'images'}`, 'error');
      return false;
    }
    return true;
  }

  showProgress(show) {
    const progressElement = document.querySelector('.upload-progress');
    if (progressElement) progressElement.style.display = show ? 'block' : 'none';
  }

  notify(message, type) {
    if (window.NotificationManager) {
      const fn = type === 'success' ? 'success' : type === 'error' ? 'error' : 'info';
      window.NotificationManager[fn](message);
      return;
    }
    // Fallback toast
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
    toast.style.position = 'fixed';
    toast.style.top = '20px';
    toast.style.right = '20px';
    toast.style.zIndex = '9999';
    toast.textContent = String(message || '');
    const closeBtn = document.createElement('button');
    closeBtn.type = 'button';
    closeBtn.className = 'btn-close';
    closeBtn.setAttribute('data-bs-dismiss', 'alert');
    closeBtn.addEventListener('click', () => toast.remove());
    toast.appendChild(closeBtn);
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 5000);
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ImageGalleryManager;
}

// Global for auto-init
window.ImageGalleryManager = ImageGalleryManager;


