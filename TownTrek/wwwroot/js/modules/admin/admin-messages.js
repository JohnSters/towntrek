/**
 * Admin Messages Management
 * Handles bulk actions, filtering, and message management
 */

class AdminMessagesManager {
    constructor() {
        this.selectedMessages = new Set();
        this.bulkActionsContainer = document.querySelector('.bulk-actions');
        this.bulkActionSelect = document.getElementById('bulkActionSelect');
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.updateBulkActionsVisibility();
    }

    bindEvents() {
        // Select all checkbox
        const selectAllCheckbox = document.getElementById('selectAll');
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener('change', (e) => {
                this.toggleSelectAll(e.target.checked);
            });
        }

        // Individual message checkboxes
        const messageCheckboxes = document.querySelectorAll('.message-checkbox');
        messageCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                this.toggleMessageSelection(e.target.value, e.target.checked);
            });
        });

        // Auto-refresh every 30 seconds
        setInterval(() => {
            this.refreshMessages();
        }, 30000);
    }

    toggleSelectAll(checked) {
        const messageCheckboxes = document.querySelectorAll('.message-checkbox');
        messageCheckboxes.forEach(checkbox => {
            checkbox.checked = checked;
            this.toggleMessageSelection(checkbox.value, checked);
        });
    }

    toggleMessageSelection(messageId, selected) {
        if (selected) {
            this.selectedMessages.add(messageId);
        } else {
            this.selectedMessages.delete(messageId);
        }
        
        this.updateBulkActionsVisibility();
        this.updateSelectAllState();
    }

    updateSelectAllState() {
        const selectAllCheckbox = document.getElementById('selectAll');
        const messageCheckboxes = document.querySelectorAll('.message-checkbox');
        
        if (selectAllCheckbox && messageCheckboxes.length > 0) {
            const checkedCount = document.querySelectorAll('.message-checkbox:checked').length;
            
            if (checkedCount === 0) {
                selectAllCheckbox.indeterminate = false;
                selectAllCheckbox.checked = false;
            } else if (checkedCount === messageCheckboxes.length) {
                selectAllCheckbox.indeterminate = false;
                selectAllCheckbox.checked = true;
            } else {
                selectAllCheckbox.indeterminate = true;
                selectAllCheckbox.checked = false;
            }
        }
    }

    updateBulkActionsVisibility() {
        if (this.bulkActionsContainer) {
            if (this.selectedMessages.size > 0) {
                this.bulkActionsContainer.style.display = 'flex';
                this.bulkActionsContainer.classList.add('show');
            } else {
                this.bulkActionsContainer.style.display = 'none';
                this.bulkActionsContainer.classList.remove('show');
            }
        }
    }

    async performBulkAction() {
        if (this.selectedMessages.size === 0) {
            alert('Please select at least one message.');
            return;
        }

        const action = this.bulkActionSelect?.value;
        if (!action) {
            alert('Please select an action.');
            return;
        }

        const actionNames = {
            'resolve': 'mark as resolved',
            'close': 'mark as closed',
            'delete': 'delete'
        };

        const actionName = actionNames[action] || action;
        const messageCount = this.selectedMessages.size;
        
        if (!confirm(`Are you sure you want to ${actionName} ${messageCount} message(s)?`)) {
            return;
        }

        try {
            const messageIds = Array.from(this.selectedMessages);
            const formData = new FormData();
            
            // Add anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }
            
            formData.append('action', action);
            messageIds.forEach(id => formData.append('messageIds', id));

            const response = await fetch('/Admin/Messages/BulkAction', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                this.showNotification(result.message, 'success');
                // Refresh the page to show updated data
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                this.showNotification(result.message || 'An error occurred', 'error');
            }
        } catch (error) {
            console.error('Bulk action error:', error);
            this.showNotification('An error occurred while performing the bulk action', 'error');
        }
    }

    refreshMessages() {
        // Simple page refresh for now
        // In a more advanced implementation, this could use AJAX to update just the table
        const currentUrl = new URL(window.location);
        const params = new URLSearchParams(currentUrl.search);
        
        // Add a timestamp to prevent caching
        params.set('_t', Date.now().toString());
        
        // Silently refresh without showing loading indicator
        fetch(currentUrl.pathname + '?' + params.toString())
            .then(response => response.text())
            .then(html => {
                // Parse the response and update just the table content
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newTable = doc.querySelector('.admin-table');
                const currentTable = document.querySelector('.admin-table');
                
                if (newTable && currentTable) {
                    currentTable.parentNode.replaceChild(newTable, currentTable);
                    // Rebind events for the new table
                    this.bindEvents();
                }
            })
            .catch(error => {
                console.error('Error refreshing messages:', error);
            });
    }

    showNotification(message, type = 'info') {
        // Create a simple notification
        const notification = document.createElement('div');
        notification.className = `alert alert-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            max-width: 400px;
            padding: 1rem;
            border-radius: var(--radius-lg);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    }
}

// Global functions for inline event handlers
window.toggleSelectAll = function(checkbox) {
    if (window.adminMessagesManager) {
        window.adminMessagesManager.toggleSelectAll(checkbox.checked);
    }
};

window.updateBulkActions = function() {
    if (window.adminMessagesManager) {
        const checkbox = event.target;
        window.adminMessagesManager.toggleMessageSelection(checkbox.value, checkbox.checked);
    }
};

window.performBulkAction = function() {
    if (window.adminMessagesManager) {
        window.adminMessagesManager.performBulkAction();
    }
};

window.refreshMessages = function() {
    if (window.adminMessagesManager) {
        window.adminMessagesManager.refreshMessages();
    }
};

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.adminMessagesManager = new AdminMessagesManager();
});

// Export for potential use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminMessagesManager;
}