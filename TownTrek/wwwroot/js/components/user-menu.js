/**
 * @fileoverview User menu dropdown component for TownTrek
 * Handles the user menu dropdown functionality across all pages
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class UserMenuComponent {
    constructor() {
        this.userMenu = null;
        this.dropdown = null;
        this.isInitialized = false;
        this.boundClickOutside = null;
        this.boundKeyDown = null;
        this.init();
    }

    init() {
        if (this.isInitialized) return;

        this.userMenu = document.querySelector('.user-menu');
        if (!this.userMenu) {
            // User menu not present (user not authenticated)
            return;
        }

        this.dropdown = this.userMenu.querySelector('.user-dropdown');
        if (!this.dropdown) {
            console.warn('User menu found but dropdown not present');
            return;
        }

        this.bindEventListeners();
        this.isInitialized = true;
        console.log('✅ User menu component initialized');
    }

    bindEventListeners() {
        // Find the trigger button
        const trigger = this.userMenu.querySelector('.user-menu-trigger');
        if (!trigger) {
            console.warn('User menu trigger button not found');
            return;
        }

        // Toggle dropdown on trigger click
        trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            this.toggleDropdown();
        });

        // Store bound functions for cleanup
        this.boundClickOutside = (e) => {
            if (!e.target.closest('.user-menu')) {
                this.closeDropdown();
            }
        };

        this.boundKeyDown = (e) => {
            if (e.key === 'Escape') {
                this.closeDropdown();
            }
        };

        // Close dropdown when clicking outside
        document.addEventListener('click', this.boundClickOutside);

        // Close dropdown on escape key
        document.addEventListener('keydown', this.boundKeyDown);

        // Handle dropdown item clicks
        this.dropdown.addEventListener('click', (e) => {
            // Allow normal navigation for links and form submissions
            // Only prevent default for custom actions if needed
            const target = e.target.closest('[data-user-action]');
            if (target) {
                const action = target.dataset.userAction;
                this.handleUserAction(action, target, e);
            }
        });
    }

    toggleDropdown() {
        if (this.dropdown.classList.contains('active')) {
            this.closeDropdown();
        } else {
            this.openDropdown();
        }
    }

    openDropdown() {
        this.dropdown.classList.add('active');
        this.dropdown.setAttribute('aria-hidden', 'false');
        
        // Update trigger aria-expanded
        const trigger = this.userMenu.querySelector('.user-menu-trigger');
        if (trigger) {
            trigger.setAttribute('aria-expanded', 'true');
        }

        // Focus first focusable element in dropdown
        const firstFocusable = this.dropdown.querySelector('a, button, [tabindex]:not([tabindex="-1"])');
        if (firstFocusable) {
            setTimeout(() => firstFocusable.focus(), 100);
        }
    }

    closeDropdown() {
        this.dropdown.classList.remove('active');
        this.dropdown.setAttribute('aria-hidden', 'true');
        
        // Update trigger aria-expanded
        const trigger = this.userMenu.querySelector('.user-menu-trigger');
        if (trigger) {
            trigger.setAttribute('aria-expanded', 'false');
        }
    }

    handleUserAction(action, target, event) {
        switch (action) {
            case 'close-menu':
                this.closeDropdown();
                break;
            // Add more custom actions as needed
            default:
                console.log(`Unhandled user action: ${action}`);
        }
    }

    destroy() {
        if (this.boundClickOutside) {
            document.removeEventListener('click', this.boundClickOutside);
        }
        if (this.boundKeyDown) {
            document.removeEventListener('keydown', this.boundKeyDown);
        }
        this.isInitialized = false;
    }

    // Test function to verify dropdown is working
    test() {
        console.log('🧪 Testing user menu dropdown...');
        if (!this.isInitialized) {
            console.error('❌ User menu not initialized');
            return false;
        }

        console.log('✅ User menu initialized');
        console.log('🎯 User menu element:', this.userMenu);
        console.log('📋 Dropdown element:', this.dropdown);

        // Test toggle
        this.openDropdown();
        console.log('📂 Dropdown opened, active class:', this.dropdown.classList.contains('active'));

        setTimeout(() => {
            this.closeDropdown();
            console.log('📁 Dropdown closed, active class:', this.dropdown.classList.contains('active'));
        }, 1000);

        return true;
    }
};

// Initialize immediately when script loads
function initUserMenu() {
    if (window.__userMenuInitialized) {
        return;
    }

    const userMenu = document.querySelector('.user-menu');
    if (userMenu) {
        window.UserMenuComponent = new UserMenuComponent();
        window.__userMenuInitialized = true;
    }
}

// Try to initialize immediately
initUserMenu();

// Also try when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initUserMenu);
} else {
    // DOM is already ready, try again in case elements were added dynamically
    setTimeout(initUserMenu, 50);
}

// Global test function for debugging
window.testUserMenu = function () {
    if (window.UserMenuComponent && window.UserMenuComponent.test) {
        return window.UserMenuComponent.test();
    } else {
        console.error('❌ User menu component not available for testing');
        return false;
    }
};

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UserMenuComponent;
}