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
        
        if (userMenu) {
            userMenu.addEventListener('click', () => {
                this.showUserDropdown();
            });
        }
    }

    showUserDropdown() {
        // Create dropdown if it doesn't exist
        let dropdown = document.querySelector('.user-dropdown');
        
        if (!dropdown) {
            dropdown = this.createUserDropdown();
            document.querySelector('.user-menu').appendChild(dropdown);
        }

        dropdown.classList.toggle('active');

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.user-menu')) {
                dropdown.classList.remove('active');
            }
        }, { once: true });
    }

    createUserDropdown() {
        const dropdown = document.createElement('div');
        dropdown.className = 'user-dropdown';
        dropdown.innerHTML = `
            <div class="dropdown-menu">
                <a href="/Client/Profile" class="dropdown-item">
                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                    </svg>
                    View Profile
                </a>
                <a href="/Client/Settings" class="dropdown-item">
                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    </svg>
                    Account Settings
                </a>
                <div class="dropdown-divider"></div>
                <a href="/Home/Login" class="dropdown-item">
                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
                    </svg>
                    Sign Out
                </a>
            </div>
        `;

        // Add dropdown styles
        const style = document.createElement('style');
        style.textContent = `
            .user-dropdown {
                position: absolute;
                top: 100%;
                right: 0;
                margin-top: var(--space-sm);
                opacity: 0;
                visibility: hidden;
                transform: translateY(-10px);
                transition: all var(--transition-normal);
                z-index: 1000;
            }
            
            .user-dropdown.active {
                opacity: 1;
                visibility: visible;
                transform: translateY(0);
            }
            
            .dropdown-menu {
                background: var(--white);
                border: 1px solid var(--medium-gray);
                border-radius: var(--radius-lg);
                padding: var(--space-sm);
                min-width: 200px;
                box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
            }
            
            .dropdown-item {
                display: flex;
                align-items: center;
                gap: var(--space-sm);
                padding: var(--space-sm) var(--space-md);
                color: var(--charcoal);
                text-decoration: none;
                font-size: var(--body-small);
                border-radius: var(--radius-md);
                transition: background-color var(--transition-normal);
            }
            
            .dropdown-item:hover {
                background-color: var(--light-gray);
            }
            
            .dropdown-divider {
                height: 1px;
                background-color: var(--medium-gray);
                margin: var(--space-sm) 0;
            }
            
            .user-menu {
                position: relative;
            }
        `;
        
        if (!document.querySelector('#user-dropdown-styles')) {
            style.id = 'user-dropdown-styles';
            document.head.appendChild(style);
        }

        return dropdown;
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