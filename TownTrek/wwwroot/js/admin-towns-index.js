// Admin Towns Index JavaScript
document.addEventListener('DOMContentLoaded', function() {
    console.log('Initializing admin towns index...');
    initializeTownsIndex();
});

function initializeTownsIndex() {
    setupTableInteractions();
    setupFilterAndExport();
    console.log('Admin towns index initialization complete');
}

function setupTableInteractions() {
    // Add hover effects for table rows
    const tableRows = document.querySelectorAll('.admin-table tbody tr');
    
    tableRows.forEach(row => {
        // Add visual feedback for clickable rows
        row.addEventListener('mouseenter', function() {
            this.style.backgroundColor = 'var(--light-gray)';
        });
        
        row.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '';
        });
    });
}

function setupFilterAndExport() {
    // Add functionality for filter and export buttons
    const filterBtn = document.querySelector('.header-btn[title="Filter"]');
    const exportBtn = document.querySelector('.header-btn[title="Export"]');
    
    if (filterBtn) {
        filterBtn.addEventListener('click', function() {
            // TODO: Implement filter functionality
            showNotification('Filter functionality coming soon', 'info');
        });
    }
    
    if (exportBtn) {
        exportBtn.addEventListener('click', function() {
            // TODO: Implement export functionality
            showNotification('Export functionality coming soon', 'info');
        });
    }
}

// Town-specific confirmation function using the existing modal system
function confirmDeleteTown(townName, townId) {
    showConfirmationModal({
        title: 'Delete Town',
        message: `Are you sure you want to delete "${townName}"?`,
        details: 'This action cannot be undone. The town will be permanently removed.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        iconType: 'danger',
        confirmButtonType: 'danger',
        formAction: '/AdminTowns/Delete',
        formMethod: 'post',
        formData: { id: townId }
    });
}

// Utility function to show notifications
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type}`;
    notification.innerHTML = `
        <div class="alert-content">
            <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <div>
                <strong>${type.charAt(0).toUpperCase() + type.slice(1)}</strong>
                <p>${message}</p>
            </div>
        </div>
    `;
    
    // Insert at the top of the page content
    const pageHeader = document.querySelector('.page-header');
    if (pageHeader) {
        pageHeader.insertAdjacentElement('afterend', notification);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            notification.remove();
        }, 5000);
    }
}