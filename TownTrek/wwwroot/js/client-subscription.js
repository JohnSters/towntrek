// Client Subscription Management JavaScript
document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing client subscription page...');
    initializeSubscriptionPage();
});

function initializeSubscriptionPage() {
    // Initialize any subscription-specific functionality
    setupPlanSelection();
    console.log('Client subscription page initialization complete');
}

function setupPlanSelection() {
    // Add event listeners for plan selection buttons
    const planButtons = document.querySelectorAll('.plan-actions .btn-cta[data-tier-id]');

    planButtons.forEach(button => {
        const tierId = button.getAttribute('data-tier-id');
        if (tierId) {
            button.addEventListener('click', function () {
                selectPlan(parseInt(tierId));
            });
        }
    });
}

function selectPlan(tierId) {
    console.log('Plan selection requested for tier ID:', tierId);

    // Show loading state
    const button = event.target;
    const originalText = button.textContent;
    button.disabled = true;
    button.textContent = 'Processing...';

    // TODO: This would integrate with payment processing
    // For now, show a placeholder alert
    setTimeout(() => {
        alert('Payment integration would be implemented here for tier ID: ' + tierId);

        // Reset button state
        button.disabled = false;
        button.textContent = originalText;
    }, 1000);

    // Future implementation would:
    // 1. Validate the selected plan
    // 2. Redirect to payment gateway
    // 3. Handle payment success/failure
    // 4. Update subscription status
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