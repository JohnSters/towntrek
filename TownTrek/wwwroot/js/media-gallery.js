// Media Gallery JavaScript
// Handles filtering, navigation, and overview functionality

class MediaGalleryManager {
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
    }

    setupFilterToggle() {
        if (this.filterToggle && this.filterPanel) {
            this.filterToggle.addEventListener('click', () => {
                this.toggleFilterPanel();
            });
        }
    }

    toggleFilterPanel() {
        const isVisible = this.filterPanel.style.display !== 'none';
        this.filterPanel.style.display = isVisible ? 'none' : 'block';
        
        // Update button text
        const buttonText = isVisible ? 'Filter' : 'Hide Filter';
        this.filterToggle.querySelector('svg').nextSibling.textContent = buttonText;
    }

    setupFilters() {
        if (this.applyFiltersBtn) {
            this.applyFiltersBtn.addEventListener('click', () => {
                this.applyFilters();
            });
        }

        if (this.clearFiltersBtn) {
            this.clearFiltersBtn.addEventListener('click', () => {
                this.clearFilters();
            });
        }

        // Apply filters on select change
        if (this.businessFilter) {
            this.businessFilter.addEventListener('change', () => {
                this.applyFilters();
            });
        }

        if (this.typeFilter) {
            this.typeFilter.addEventListener('change', () => {
                this.applyFilters();
            });
        }
    }

    applyFilters() {
        const selectedBusinessId = this.businessFilter?.value || '';
        const selectedImageType = this.typeFilter?.value || '';

        // Filter business sections
        const businessSections = document.querySelectorAll('.business-section');
        businessSections.forEach(section => {
            const businessId = section.dataset.businessId;
            let showSection = true;

            // Filter by business
            if (selectedBusinessId && businessId !== selectedBusinessId) {
                showSection = false;
            }

            section.style.display = showSection ? 'block' : 'none';

            // Filter images within visible sections
            if (showSection && selectedImageType) {
                const imageCards = section.querySelectorAll('.image-card[data-image-type]');
                imageCards.forEach(card => {
                    const imageType = card.dataset.imageType;
                    card.style.display = imageType === selectedImageType ? 'block' : 'none';
                });
            } else if (showSection) {
                // Show all images if no type filter
                const imageCards = section.querySelectorAll('.image-card[data-image-type]');
                imageCards.forEach(card => {
                    card.style.display = 'block';
                });
            }
        });

        this.updateFilterStatus();
    }

    clearFilters() {
        if (this.businessFilter) this.businessFilter.value = '';
        if (this.typeFilter) this.typeFilter.value = '';
        
        // Show all sections and images
        const businessSections = document.querySelectorAll('.business-section');
        businessSections.forEach(section => {
            section.style.display = 'block';
            
            const imageCards = section.querySelectorAll('.image-card[data-image-type]');
            imageCards.forEach(card => {
                card.style.display = 'block';
            });
        });

        this.updateFilterStatus();
    }

    updateFilterStatus() {
        const activeFilters = [];
        
        if (this.businessFilter?.value) {
            const selectedOption = this.businessFilter.options[this.businessFilter.selectedIndex];
            activeFilters.push(`Business: ${selectedOption.text}`);
        }
        
        if (this.typeFilter?.value) {
            activeFilters.push(`Type: ${this.typeFilter.value}`);
        }

        // Update filter button to show active state
        if (activeFilters.length > 0) {
            this.filterToggle.classList.add('active');
            this.filterToggle.title = `Active filters: ${activeFilters.join(', ')}`;
        } else {
            this.filterToggle.classList.remove('active');
            this.filterToggle.title = 'Filter images';
        }
    }

    setupImageCards() {
        const imageCards = document.querySelectorAll('.image-card img');
        imageCards.forEach(img => {
            img.addEventListener('click', (e) => {
                this.showImagePreview(e.target);
            });
        });
    }

    showImagePreview(img) {
        // Create modal for image preview
        const modal = document.createElement('div');
        modal.className = 'image-preview-modal';
        modal.innerHTML = `
            <div class="modal-backdrop" onclick="this.parentElement.remove()"></div>
            <div class="modal-content">
                <button class="modal-close" onclick="this.closest('.image-preview-modal').remove()">
                    <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
                <img src="${img.src.replace('thumbnail', 'full')}" alt="${img.alt}" />
                <div class="modal-info">
                    <h4>${img.alt || 'Business Image'}</h4>
                    <p>Click outside to close</p>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
        document.body.style.overflow = 'hidden';

        // Remove modal when clicking backdrop or close button
        modal.addEventListener('click', (e) => {
            if (e.target === modal || e.target.classList.contains('modal-backdrop')) {
                modal.remove();
                document.body.style.overflow = '';
            }
        });
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new MediaGalleryManager();
});

// Export for potential use in other scripts
window.MediaGalleryManager = MediaGalleryManager;