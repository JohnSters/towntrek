// Manage Businesses JavaScript

// Initialize page functionality
document.addEventListener('DOMContentLoaded', function() {
    console.log('Manage Businesses page loaded');
    
    // Any additional initialization can go here
    initializeTableActions();
});

// Initialize table action handlers
function initializeTableActions() {
    // Filter button functionality (if needed)
    const filterBtn = document.querySelector('.header-btn[title="Filter"]');
    if (filterBtn) {
        filterBtn.addEventListener('click', function() {
            // Implement filter functionality if needed
            console.log('Filter clicked');
        });
    }
    
    // Refresh button functionality
    const refreshBtn = document.querySelector('.header-btn[title="Refresh"]');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', function() {
            // Refresh the page
            window.location.reload();
        });
    }
}

// Client-specific delete business confirmation
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