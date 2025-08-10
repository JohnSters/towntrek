// Client Administration JavaScript
// Handles sidebar navigation, responsive behavior, and admin interactions

class ClientAdminManager {
    constructor() {
        this.sidebar = document.getElementById('adminSidebar');
        this.sidebarToggle = document.getElementById('sidebarToggle');
        this.sidebarOverlay = document.getElementById('sidebarOverlay');
        this.isMobile = window.innerWidth <= 1024;

        this.init();
    }

    init() {
        this.setupSidebarToggle();
        this.setupResponsiveHandling();
        this.setupNavigationHighlighting();
        this.setupUserMenu();
        this.setupNotifications();
    }

    // Sidebar Toggle Functionality
    setupSidebarToggle() {
        if (this.sidebarToggle) {
            this.sidebarToggle.addEventListener('click', () => {
                this.toggleSidebar();
            });
        }

        if (this.sidebarOverlay) {
            this.sidebarOverlay.addEventListener('click', () => {
                this.closeSidebar();
            });
        }

        // Close sidebar on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.sidebar.classList.contains('open')) {
                this.closeSidebar();
            }
        });
    }

    toggleSidebar() {
        if (this.isMobile) {
            this.sidebar.classList.toggle('open');
            this.sidebarOverlay.classList.toggle('active');
            document.body.style.overflow = this.sidebar.classList.contains('open') ? 'hidden' : '';
        }
    }

    closeSidebar() {
        if (this.isMobile) {
            this.sidebar.classList.remove('open');
            this.sidebarOverlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    // Responsive Handling
    setupResponsiveHandling() {
        window.addEventListener('resize', () => {
            const wasMobile = this.isMobile;
            this.isMobile = window.innerWidth <= 1024;

            // If switching from mobile to desktop, clean up mobile states
            if (wasMobile && !this.isMobile) {
                this.sidebar.classList.remove('open');
                this.sidebarOverlay.classList.remove('active');
                document.body.style.overflow = '';
            }
        });
    }

    // Navigation Highlighting
    setupNavigationHighlighting() {
        const navLinks = document.querySelectorAll('.nav-link');
        const currentPath = window.location.pathname;

        navLinks.forEach(link => {
            // Remove existing active classes
            link.classList.remove('active');

            // Add active class if href matches current path
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    }

    // User Menu Functionality
    setupUserMenu() {
        const userMenu = document.querySelector('.user-menu');
        if (!userMenu) return;

        const dropdown = userMenu.querySelector('.user-dropdown');
        if (!dropdown) return;

        userMenu.addEventListener('click', () => {
            dropdown.classList.toggle('active');
        });

        // Close when clicking outside
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.user-menu')) {
                dropdown.classList.remove('active');
            }
        });

        // Close on Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                dropdown.classList.remove('active');
            }
        });
    }

    // Notifications
    setupNotifications() {
        const notificationBtn = document.querySelector('.header-btn[title="Notifications"]');

        if (notificationBtn) {
            notificationBtn.addEventListener('click', () => {
                this.showNotifications();
            });
        }
    }

    showNotifications() {
        // Simple notification demo
        this.showToast('No new notifications', 'info');
    }

    // Utility Methods
    showToast(message, type = 'info') {
        // Remove existing toasts
        const existingToasts = document.querySelectorAll('.admin-toast');
        existingToasts.forEach(toast => toast.remove());

        // Create toast
        const toast = document.createElement('div');
        toast.className = `admin-toast admin-toast-${type}`;
        toast.textContent = message;

        // Add toast styles if not already added
        if (!document.querySelector('#toast-styles')) {
            const style = document.createElement('style');
            style.id = 'toast-styles';
            style.textContent = `
                .admin-toast {
                    position: fixed;
                    top: var(--space-lg);
                    right: var(--space-lg);
                    background: var(--white);
                    border: 1px solid var(--medium-gray);
                    border-radius: var(--radius-lg);
                    padding: var(--space-md) var(--space-lg);
                    font-size: var(--body-small);
                    font-weight: var(--font-medium);
                    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
                    z-index: 1000;
                    animation: slideInRight 0.3s ease;
                }
                
                .admin-toast-info {
                    border-left: 4px solid var(--lapis-lazuli);
                    color: var(--lapis-lazuli);
                }
                
                .admin-toast-success {
                    border-left: 4px solid var(--carolina-blue);
                    color: var(--carolina-blue);
                }
                
                .admin-toast-warning {
                    border-left: 4px solid var(--hunyadi-yellow);
                    color: var(--charcoal);
                }
                
                .admin-toast-error {
                    border-left: 4px solid var(--orange-pantone);
                    color: var(--orange-pantone);
                }
                
                @keyframes slideInRight {
                    from {
                        transform: translateX(100%);
                        opacity: 0;
                    }
                    to {
                        transform: translateX(0);
                        opacity: 1;
                    }
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(toast);

        // Auto-remove after 3 seconds
        setTimeout(() => {
            toast.style.animation = 'slideInRight 0.3s ease reverse';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    // Loading States
    showLoading(element) {
        const spinner = document.createElement('div');
        spinner.className = 'loading-spinner';
        element.appendChild(spinner);
        element.classList.add('loading');
    }

    hideLoading(element) {
        const spinner = element.querySelector('.loading-spinner');
        if (spinner) {
            spinner.remove();
        }
        element.classList.remove('loading');
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ClientAdminManager();
});

// Export for potential use in other scripts
window.ClientAdminManager = ClientAdminManager;