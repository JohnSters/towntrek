// Confirmation Modal JavaScript

let currentModalConfig = null;
let currentFormData = null;

// Global function to show confirmation modal
function showConfirmationModal(config) {
    currentModalConfig = config;
    currentFormData = config.formData || null; // Set the form data
    
    // Create modal HTML dynamically
    const modalHtml = `
        <div id="confirmationModal" class="confirmation-modal">
            <div class="confirmation-modal-overlay" onclick="closeConfirmationModal()"></div>
            <div class="confirmation-modal-content">
                <div class="confirmation-modal-header">
                    <div class="confirmation-modal-icon ${config.iconType || 'info'}">
                        ${getIconHtml(config.iconType || 'info')}
                    </div>
                    <h3 class="confirmation-modal-title">${config.title || 'Confirm Action'}</h3>
                </div>
                
                <div class="confirmation-modal-body">
                    <p class="confirmation-modal-message">${config.message || 'Are you sure you want to proceed?'}</p>
                    ${config.details ? `<div class="confirmation-modal-details"><p class="confirmation-modal-details-text">${config.details}</p></div>` : ''}
                </div>
                
                <div class="confirmation-modal-actions">
                    <button type="button" class="confirmation-modal-btn confirmation-modal-btn-secondary" onclick="closeConfirmationModal()">
                        ${config.cancelText || 'Cancel'}
                    </button>
                    <button type="button" class="confirmation-modal-btn confirmation-modal-btn-${config.confirmButtonType || 'primary'}" onclick="confirmAction()">
                        ${config.confirmText || 'Confirm'}
                    </button>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    const existingModal = document.getElementById('confirmationModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add modal to body
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Show modal with animation
    const modal = document.getElementById('confirmationModal');
    modal.style.display = 'flex';
    
    // Focus on confirm button for accessibility
    setTimeout(() => {
        const confirmBtn = modal.querySelector('.confirmation-modal-btn:not(.confirmation-modal-btn-secondary)');
        if (confirmBtn) {
            confirmBtn.focus();
        }
    }, 100);
    
    // Handle escape key
    document.addEventListener('keydown', handleEscapeKey);
}

// Close modal function
function closeConfirmationModal() {
    const modal = document.getElementById('confirmationModal');
    if (modal) {
        modal.style.display = 'none';
        setTimeout(() => {
            modal.remove();
        }, 200);
    }
    
    // Clean up
    currentModalConfig = null;
    currentFormData = null;
    document.removeEventListener('keydown', handleEscapeKey);
}

// Confirm action function
function confirmAction() {
    if (!currentModalConfig) {
        closeConfirmationModal();
        return;
    }
    
    // If form action is specified, submit the form
    if (currentModalConfig.formAction) {
        submitForm();
    } else if (currentModalConfig.onConfirm) {
        // If callback is specified, call it
        currentModalConfig.onConfirm();
    }
    
    closeConfirmationModal();
}

// Submit form function
function submitForm() {
    if (!currentModalConfig || !currentFormData) {
        console.error('Missing modal config or form data');
        return;
    }
    
    // Create form element
    const form = document.createElement('form');
    form.method = currentModalConfig.formMethod || 'post';
    form.action = currentModalConfig.formAction;
    form.style.display = 'none';
    
    // Add anti-forgery token if it's a POST request
    if (currentModalConfig.formMethod === 'post') {
        // Try multiple ways to find the token
        let token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) {
            token = document.querySelector('input[name="__RequestVerificationToken"]');
        }
        if (!token) {
            // Look for token in meta tag
            const metaToken = document.querySelector('meta[name="__RequestVerificationToken"]');
            if (metaToken) {
                token = { value: metaToken.getAttribute('content') };
            }
        }
        
        if (token) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token.value;
            form.appendChild(tokenInput);
        } else {
            console.error('Anti-forgery token not found');
        }
    }
    
    // Add hidden fields
    if (currentFormData) {
        Object.keys(currentFormData).forEach(key => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = currentFormData[key];
            form.appendChild(input);
        });
    }
    
    // Submit form
    document.body.appendChild(form);
    console.log('Submitting form to:', form.action, 'with data:', currentFormData);
    form.submit();
}

// Helper function to get icon HTML
function getIconHtml(iconType) {
    const icons = {
        success: `<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>`,
        warning: `<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
        </svg>`,
        danger: `<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
        </svg>`,
        info: `<svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>`
    };
    
    return icons[iconType] || icons.info;
}

// Handle escape key
function handleEscapeKey(event) {
    if (event.key === 'Escape') {
        closeConfirmationModal();
    }
}

// Convenience functions for common confirmation types
function confirmApproveBusiness(businessName, businessId) {
    showConfirmationModal({
        title: 'Approve Business',
        message: `Are you sure you want to approve "${businessName}"?`,
        details: 'This business will become live and visible to users.',
        confirmText: 'Approve',
        cancelText: 'Cancel',
        iconType: 'success',
        confirmButtonType: 'success',
        formAction: '/AdminBusinesses/Approve',
        formMethod: 'post',
        formData: { id: businessId }
    });
}

function confirmRejectBusiness(businessName, businessId) {
    showConfirmationModal({
        title: 'Reject Business',
        message: `Are you sure you want to reject "${businessName}"?`,
        details: 'This business will be marked as inactive and hidden from users.',
        confirmText: 'Reject',
        cancelText: 'Cancel',
        iconType: 'warning',
        confirmButtonType: 'warning',
        formAction: '/AdminBusinesses/Reject',
        formMethod: 'post',
        formData: { id: businessId }
    });
}

function confirmSuspendBusiness(businessName, businessId) {
    showConfirmationModal({
        title: 'Suspend Business',
        message: `Are you sure you want to suspend "${businessName}"?`,
        details: 'This business will be temporarily hidden from users until reactivated.',
        confirmText: 'Suspend',
        cancelText: 'Cancel',
        iconType: 'warning',
        confirmButtonType: 'warning',
        formAction: '/AdminBusinesses/Suspend',
        formMethod: 'post',
        formData: { id: businessId }
    });
}

function confirmDeleteBusiness(businessName, businessId) {
    showConfirmationModal({
        title: 'Delete Business',
        message: `Are you sure you want to delete "${businessName}"?`,
        details: 'This action cannot be undone. The business will be permanently removed.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        iconType: 'danger',
        confirmButtonType: 'danger',
        formAction: '/AdminBusinesses/Delete',
        formMethod: 'post',
        formData: { id: businessId }
    });
}

// Client-specific delete function
function confirmDeleteClientBusiness(businessName, businessId) {
    showConfirmationModal({
        title: 'Delete Business',
        message: `Are you sure you want to delete "${businessName}"?`,
        details: 'This action cannot be undone. The business will be permanently removed.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        iconType: 'danger',
        confirmButtonType: 'danger',
        formAction: '/Client/Business/DeleteBusiness',
        formMethod: 'post',
        formData: { id: businessId }
    });
} 