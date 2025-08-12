/**
 * @fileoverview MediaGalleryOverviewManager - manages media gallery overview filters and preview
 */

class MediaGalleryOverviewManager {
  constructor() {
    this.filterPanel = document.getElementById('filterPanel');
    this.filterToggle = document.getElementById('filterToggle');
    this.businessFilter = document.getElementById('businessFilter');
    this.typeFilter = document.getElementById('typeFilter');
    this.applyFiltersBtn = document.getElementById('applyFilters');
    this.clearFiltersBtn = document.getElementById('clearFilters');
    this.init();
  }

  init() {
    this.setupFilterToggle();
    this.setupFilters();
    this.setupImageCards();
    console.log('✅ MediaGalleryOverviewManager initialized');
  }

  setupFilterToggle() {
    if (!this.filterToggle || !this.filterPanel) return;
    this.filterToggle.addEventListener('click', () => this.toggleFilterPanel());
  }

  toggleFilterPanel() {
    const isVisible = this.filterPanel.style.display !== 'none';
    this.filterPanel.style.display = isVisible ? 'none' : 'block';
    const textNode = this.filterToggle.querySelector('svg')?.nextSibling;
    if (textNode) textNode.textContent = isVisible ? 'Filter' : 'Hide Filter';
  }

  setupFilters() {
    this.applyFiltersBtn?.addEventListener('click', () => this.applyFilters());
    this.clearFiltersBtn?.addEventListener('click', () => this.clearFilters());
    this.businessFilter?.addEventListener('change', () => this.applyFilters());
    this.typeFilter?.addEventListener('change', () => this.applyFilters());
  }

  applyFilters() {
    const selectedBusinessId = this.businessFilter?.value || '';
    const selectedImageType = this.typeFilter?.value || '';
    const sections = document.querySelectorAll('.business-section');
    sections.forEach((section) => {
      const businessId = section.dataset.businessId;
      const showSection = !selectedBusinessId || businessId === selectedBusinessId;
      section.style.display = showSection ? 'block' : 'none';
      if (showSection) {
        const cards = section.querySelectorAll('.image-card[data-image-type]');
        cards.forEach((card) => {
          card.style.display = selectedImageType ? (card.dataset.imageType === selectedImageType ? 'block' : 'none') : 'block';
        });
      }
    });
    this.updateFilterStatus();
  }

  clearFilters() {
    if (this.businessFilter) this.businessFilter.value = '';
    if (this.typeFilter) this.typeFilter.value = '';
    const sections = document.querySelectorAll('.business-section');
    sections.forEach((section) => {
      section.style.display = 'block';
      const cards = section.querySelectorAll('.image-card[data-image-type]');
      cards.forEach((card) => (card.style.display = 'block'));
    });
    this.updateFilterStatus();
  }

  updateFilterStatus() {
    const active = [];
    if (this.businessFilter?.value) {
      const opt = this.businessFilter.options[this.businessFilter.selectedIndex];
      active.push(`Business: ${opt.text}`);
    }
    if (this.typeFilter?.value) active.push(`Type: ${this.typeFilter.value}`);
    if (active.length > 0) {
      this.filterToggle?.classList.add('active');
      if (this.filterToggle) this.filterToggle.title = `Active filters: ${active.join(', ')}`;
    } else {
      this.filterToggle?.classList.remove('active');
      if (this.filterToggle) this.filterToggle.title = 'Filter images';
    }
  }

  setupImageCards() {
    const imgs = document.querySelectorAll('.image-card img');
    imgs.forEach((img) => img.addEventListener('click', (e) => this.showImagePreview(e.target)));
  }

  showImagePreview(img) {
    const modal = document.createElement('div');
    modal.className = 'image-preview-modal';
    const safeAlt = this.escapeHtml(img.alt || 'Business Image');
    modal.innerHTML = `
      <div class="modal-backdrop" data-close></div>
      <div class="modal-content">
        <button class="modal-close" data-close>
          <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
        <img src="${img.src.replace('thumbnail', 'full')}" alt="${safeAlt}" />
        <div class="modal-info">
          <h4>${safeAlt}</h4>
          <p>Click outside to close</p>
        </div>
      </div>`;
    document.body.appendChild(modal);
    document.body.style.overflow = 'hidden';
    modal.addEventListener('click', (e) => {
      if (e.target.dataset.close !== undefined || e.target === modal) {
        modal.remove();
        document.body.style.overflow = '';
      }
    });
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = MediaGalleryOverviewManager;
}

// Global for auto-init
window.MediaGalleryOverviewManager = MediaGalleryOverviewManager;


