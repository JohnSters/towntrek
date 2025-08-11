// Admin Subscription Index JavaScript
document.addEventListener('DOMContentLoaded', function() {
    console.log('Initializing admin subscription index...');
    initializeSubscriptionIndex();
});

function initializeSubscriptionIndex() {
    setupDeactivationConfirmations();
    setupTableInteractions();
    console.log('Admin subscription index initialization complete');
}

function setupDeactivationConfirmations() {
    // Replace inline onclick confirmations with proper event listeners
    const deactivateButtons = document.querySelectorAll('.btn-action.btn-danger[type="submit"]');
    
    deactivateButtons.forEach(button => {
        // Remove inline onclick if it exists
        button.removeAttribute('onclick');
        
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            const form = this.closest('form');
            const tierName = this.closest('tr').querySelector('.tier-name').textContent.trim();
            
            showDeactivationConfirmation(tierName, form);
        });
    });
}

function showDeactivationConfirmation(tierName, form) {
    const confirmed = confirm(`Are you sure you want to deactivate the "${tierName}" tier?\n\nThis action will prevent new subscriptions to this tier, but existing subscriptions will remain active.`);
    
    if (confirmed) {
        // Show loading state
        const submitButton = form.querySelector('button[type="submit"]');
        const originalContent = submitButton.innerHTML;
        
        submitButton.disabled = true;
        submitButton.innerHTML = `
            <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
            </svg>
        `;
        
        // Submit the form
        form.submit();
    }
}

function setupTableInteractions() {
    // Add hover effects and click handlers for table rows
    const tableRows = document.querySelectorAll('.admin-table tbody tr');
    
    tableRows.forEach(row => {
        // Add click handler to make entire row clickable (navigate to details)
        row.addEventListener('click', function(e) {
            // Don't trigger if clicking on action buttons
            if (e.target.closest('.action-buttons')) {
                return;
            }
            
            const detailsLink = this.querySelector('a[href*="Details"]');
            if (detailsLink) {
                window.location.href = detailsLink.href;
            }
        });
        
        // Add visual feedback for clickable rows
        row.style.cursor = 'pointer';
        row.addEventListener('mouseenter', function() {
            this.style.backgroundColor = 'var(--light-gray)';
        });
        
        row.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '';
        });
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

// Add CSS for spinning animation
const style = document.createElement('style');
style.textContent = `
    .animate-spin {
        animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
        from {
            transform: rotate(0deg);
        }
        to {
            transform: rotate(360deg);
        }
    }
`;
document.head.appendChild(style);