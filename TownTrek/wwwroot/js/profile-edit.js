// Profile Edit Form JavaScript
// Handles form validation, loading states, and profile picture preview

class ProfileEditManager {
    constructor() {
        this.form = document.querySelector('.add-business-form');
        this.submitBtn = this.form?.querySelector('button[type="submit"]');
        this.profilePictureInput = document.querySelector('input[name="ProfilePictureUrl"]');
        
        this.init();
    }

    init() {
        if (!this.form) return;
        
        this.setupFormSubmission();
        this.setupProfilePicturePreview();
    }

    setupFormSubmission() {
        if (!this.submitBtn) return;
        
        this.form.addEventListener('submit', () => {
            this.showLoadingState();
        });
    }

    showLoadingState() {
        this.submitBtn.disabled = true;
        this.submitBtn.innerHTML = `
            <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
            </svg>
            Saving...
        `;
    }

    setupProfilePicturePreview() {
        if (!this.profilePictureInput) return;
        
        this.profilePictureInput.addEventListener('blur', (e) => {
            const url = e.target.value.trim();
            if (url && this.isValidUrl(url)) {
                this.showImagePreview(url);
            } else {
                this.hideImagePreview();
            }
        });
    }

    isValidUrl(string) {
        try {
            new URL(string);
            return true;
        } catch (_) {
            return false;
        }
    }

    showImagePreview(url) {
        // Remove existing preview
        this.hideImagePreview();
        
        const preview = document.createElement('div');
        preview.className = 'image-preview';
        preview.innerHTML = `
            <img src="${url}" alt="Profile Picture Preview" class="profile-preview-image" 
                 onerror="this.parentElement.style.display='none';" />
        `;
        
        this.profilePictureInput.parentElement.appendChild(preview);
    }

    hideImagePreview() {
        const existingPreview = document.querySelector('.image-preview');
        if (existingPreview) {
            existingPreview.remove();
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ProfileEditManager();
});

// Export for potential use in other scripts
window.ProfileEditManager = ProfileEditManager;