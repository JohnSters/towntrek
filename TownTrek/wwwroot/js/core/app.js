/**
 * @fileoverview Main application bootstrap and initialization
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Main application class
 * @class
 */
class TownTrekApp {
  /**
   * @constructor
   */
  constructor() {
    this.isInitialized = false;
    this.modules = new Map();
    this.components = new Map();

    this.init();
  }

  /**
   * Initialize the application
   * @private
   */
  init() {
    if (this.isInitialized) return;

    try {
      this.setupGlobalErrorHandling();
      this.initializeCore();
      this.autoInitializeModules();
      this.setupGlobalEventListeners();

      this.isInitialized = true;
      console.log('🚀 TownTrek application initialized successfully');
    } catch (error) {
      console.error('❌ Failed to initialize TownTrek application:', error);
      ClientErrorHandler.showError('Failed to initialize application. Please refresh the page.');
    }
  }

  /**
   * Setup global error handling
   * @private
   */
  setupGlobalErrorHandling() {
    // Simple global error handling for unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      console.error('Unhandled promise rejection:', event.reason);
      // Don't show user notification for every unhandled promise
      // Let the server-side error handling deal with it
    });
  }

  /**
   * Initialize core systems
   * @private
   */
  initializeCore() {
    // Initialize notification system
    if (!window.NotificationManager) {
      console.warn('NotificationManager not available');
    }

    // Initialize API client
    if (!window.ApiClient) {
      console.warn('ApiClient not available');
    }

    // Set up CSRF token refresh
    this.setupCsrfTokenRefresh();
  }

  /**
   * Setup CSRF token refresh
   * @private
   */
  setupCsrfTokenRefresh() {
    // Refresh CSRF token on focus (in case of long sessions)
    window.addEventListener('focus', () => {
      const token = ApiClient.getAntiForgeryToken();
      if (!token) {
        console.warn('CSRF token not found - may need to refresh page');
      }
    });
  }

  /**
   * Auto-initialize modules based on page content
   * @private
   */
  autoInitializeModules() {
    const currentPath = window.location.pathname.toLowerCase();

    // Business-related pages
    if (currentPath.includes('/client/business/create')) {
      this.initializeModule('business-form', 'create');
    } else if (currentPath.includes('/client/business/edit')) {
      this.initializeModule('business-form', 'edit');
    } else if (
      (currentPath.includes('/client/business/manage') || currentPath.includes('/client/business/index')) &&
      (document.querySelector('[data-business-list]') || document.querySelector('.business-list') || document.querySelector('.admin-table'))
    ) {
      this.initializeModule('business-list');
    }

    // Auth pages
    if (currentPath.includes('/auth/login')) {
      this.initializeModule('auth', 'login');
    } else if (currentPath.includes('/auth/register')) {
      this.initializeModule('auth', 'register');
    } else if (currentPath.includes('/auth/forgotpassword')) {
      this.initializeModule('auth', 'forgot-password');
    }

    // Admin pages (route-based detection only; client layout may reuse "admin" styles)
    const isAdminRoute =
      currentPath.startsWith('/admin') ||
      currentPath.startsWith('/adminusers') ||
      currentPath.startsWith('/admintowns') ||
      currentPath.startsWith('/adminsubscription');
    if (isAdminRoute) {
      this.initializeModule('admin');

      const isUsers = currentPath.includes('/admin/users') || currentPath.includes('/adminusers');
      const isTowns = currentPath.includes('/admin/towns') || currentPath.includes('/admintowns');
      const isSubscriptions = currentPath.includes('/admin/subscriptions') || currentPath.includes('/adminsubscription');

      if (isUsers) {
        this.initializeModule('admin-users');
      } else if (isTowns) {
        this.initializeModule('admin-towns');
      } else if (isSubscriptions) {
        this.initializeModule('admin-subscriptions');
      }
    }

    // Client pages
    if (currentPath.includes('/client/')) {
      this.initializeModule('client');

      // Initialize trial countdown if trial elements are present
      if (document.querySelector('[data-trial-end]') || document.querySelector('.trial-countdown-card')) {
        this.initializeModule('trial-countdown');
      }

      if (currentPath.includes('/client/analytics')) {
        // Initialize analytics dashboard module
        this.initializeModule('client-analytics');
      } else if (currentPath.includes('/client/profile')) {
        this.initializeModule('client-profile');
      } else if (currentPath.includes('/client/subscription')) {
        this.initializeModule('client-subscription');
      }
    }

    // Gallery pages
    if (currentPath.includes('/image/gallery')) {
      this.initializeModule('image-gallery');
    } else if (currentPath.includes('/image/mediagallery')) {
      this.initializeModule('media-gallery');
    }

    // Initialize common components based on DOM elements
    this.initializeCommonComponents();
  }

  /**
   * Initialize common components based on DOM presence
   * @private
   */
  initializeCommonComponents() {
    // User menu component is now self-initializing, no need to initialize here
    // if (document.querySelector('.user-menu')) {
    //   this.initializeComponent('user-menu');
    // }

    // Modal component
    if (document.querySelector('.modal') || document.querySelector('[data-modal]')) {
      this.initializeComponent('modal');
    }

    // File upload component
    if (document.querySelector('input[type="file"]')) {
      this.initializeComponent('file-upload');
    }

    // Data tables
    if (document.querySelector('.data-table') || document.querySelector('[data-table]')) {
      this.initializeComponent('data-table');
    }

    // Form handlers
    if (document.querySelector('form[data-form-handler]')) {
      this.initializeComponent('form-handler');
    }
  }

  /**
   * Initialize a specific module
   * @param {string} moduleName - Module name
   * @param {string} mode - Module mode/variant
   */
  initializeModule(moduleName, mode = 'default') {
    try {
      const moduleKey = `${moduleName}-${mode}`;

      if (this.modules.has(moduleKey)) {
        console.warn(`Module ${moduleKey} already initialized`);
        return;
      }

      // Try to find and initialize the module
      const moduleClass = this.findModuleClass(moduleName, mode);

      if (moduleClass) {
        const instance = new moduleClass({ mode });
        this.modules.set(moduleKey, instance);
        console.log(`✅ Initialized module: ${moduleKey}`);
      } else {
        console.warn(`⚠️ Module class not found: ${moduleName} (${mode})`);
      }
    } catch (error) {
      console.error(`❌ Failed to initialize module ${moduleName}:`, error);
      ClientErrorHandler.showError(`Failed to initialize ${moduleName}. Some features may not work properly.`);
    }
  }

  /**
   * Initialize a specific component
   * @param {string} componentName - Component name
   */
  initializeComponent(componentName) {
    try {
      if (this.components.has(componentName)) {
        console.warn(`Component ${componentName} already initialized`);
        return;
      }

      const componentClass = this.findComponentClass(componentName);

      if (componentClass) {
        const instance = new componentClass();
        this.components.set(componentName, instance);

        // Set global flags for specific components
        if (componentName === 'user-menu') {
          window.__userMenuInitialized = true;
        }

        console.log(`✅ Initialized component: ${componentName}`);
      } else {
        console.warn(`⚠️ Component class not found: ${componentName}`);
      }
    } catch (error) {
      console.error(`❌ Failed to initialize component ${componentName}:`, error);
      ClientErrorHandler.showError(`Failed to initialize ${componentName}. Some features may not work properly.`);
    }
  }

  /**
   * Find module class by name and mode
   * @private
   * @param {string} moduleName - Module name
   * @param {string} mode - Module mode
   * @returns {Function|null} Module class
   */
  findModuleClass(moduleName, mode) {
    // Map module names to class names
    const moduleClassMap = {
      'business-form': 'BusinessFormManager',
      'business-list': 'BusinessListManager',
      'auth': 'AuthManager',
      'admin': 'AdminManager',
      // Admin feature modules (aligned with refactoring plan naming)
      'admin-users': 'AdminUserManagement',
      'admin-towns': 'AdminTownManagement',
      'admin-subscriptions': 'AdminSubscriptionManagement',
      'client': 'ClientManager',
      'client-profile': 'ClientProfileManager',
      'client-subscription': 'ClientSubscriptionManager',
      'trial-countdown': 'TrialCountdownManager',
      'image-gallery': 'ImageGalleryManager',
      'media-gallery': 'MediaGalleryOverviewManager',
      // Client analytics dashboard
      'client-analytics': 'ClientAnalyticsManager'
    };

    const className = moduleClassMap[moduleName];
    return className ? window[className] : null;
  }

  /**
   * Find component class by name
   * @private
   * @param {string} componentName - Component name
   * @returns {Function|null} Component class
   */
  findComponentClass(componentName) {
    // Map component names to class names
    const componentClassMap = {
      'user-menu': 'UserMenuComponent',
      'modal': 'ModalComponent',
      'file-upload': 'FileUploadComponent',
      'data-table': 'DataTableComponent',
      'form-handler': 'FormHandlerComponent'
    };

    const className = componentClassMap[componentName];
    return className ? window[className] : null;
  }

  /**
   * Setup global event listeners
   * @private
   */
  setupGlobalEventListeners() {
    // Handle page visibility changes
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'visible') {
        this.onPageVisible();
      } else {
        this.onPageHidden();
      }
    });

    // Handle online/offline status
    window.addEventListener('online', () => {
      NotificationManager.success('Connection restored', { duration: 3000 });
    });

    window.addEventListener('offline', () => {
      NotificationManager.warning('Connection lost - some features may not work', { duration: 0 });
    });

    // Handle form submissions globally
    document.addEventListener('submit', (event) => {
      this.handleGlobalFormSubmit(event);
    });

    // Handle clicks on elements with data attributes
    document.addEventListener('click', (event) => {
      this.handleGlobalClick(event);
    });
  }

  /**
   * Handle page becoming visible
   * @private
   */
  onPageVisible() {
    // Refresh any stale data
    this.modules.forEach((module, key) => {
      if (module.onPageVisible && typeof module.onPageVisible === 'function') {
        module.onPageVisible();
      }
    });
  }

  /**
   * Handle page becoming hidden
   * @private
   */
  onPageHidden() {
    // Pause any ongoing operations
    this.modules.forEach((module, key) => {
      if (module.onPageHidden && typeof module.onPageHidden === 'function') {
        module.onPageHidden();
      }
    });
  }

  /**
   * Handle global form submissions
   * @private
   * @param {Event} event - Submit event
   */
  handleGlobalFormSubmit(event) {
    const form = event.target;

    // Add loading state to submit buttons
    const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
    submitButtons.forEach(button => {
      if (!button.disabled) {
        button.dataset.originalText = button.textContent || button.value;
        button.textContent = 'Processing...';
        button.disabled = true;

        // Re-enable after a timeout as fallback
        setTimeout(() => {
          if (button.disabled) {
            button.textContent = button.dataset.originalText;
            button.disabled = false;
          }
        }, 30000);
      }
    });
  }

  /**
   * Handle global clicks
   * @private
   * @param {Event} event - Click event
   */
  handleGlobalClick(event) {
    const target = event.target.closest('[data-action]');
    if (!target) return;

    const action = target.dataset.action;
    const moduleTarget = target.dataset.module;

    if (moduleTarget && this.modules.has(moduleTarget)) {
      const module = this.modules.get(moduleTarget);
      if (module.handleAction && typeof module.handleAction === 'function') {
        module.handleAction(action, target, event);
      }
    }
  }

  /**
   * Get module instance
   * @param {string} moduleName - Module name
   * @param {string} mode - Module mode
   * @returns {Object|null} Module instance
   */
  getModule(moduleName, mode = 'default') {
    const moduleKey = `${moduleName}-${mode}`;
    return this.modules.get(moduleKey) || null;
  }

  /**
   * Get component instance
   * @param {string} componentName - Component name
   * @returns {Object|null} Component instance
   */
  getComponent(componentName) {
    return this.components.get(componentName) || null;
  }

  /**
   * Destroy module
   * @param {string} moduleName - Module name
   * @param {string} mode - Module mode
   */
  destroyModule(moduleName, mode = 'default') {
    const moduleKey = `${moduleName}-${mode}`;
    const module = this.modules.get(moduleKey);

    if (module) {
      if (module.destroy && typeof module.destroy === 'function') {
        module.destroy();
      }
      this.modules.delete(moduleKey);
      console.log(`🗑️ Destroyed module: ${moduleKey}`);
    }
  }

  /**
   * Destroy component
   * @param {string} componentName - Component name
   */
  destroyComponent(componentName) {
    const component = this.components.get(componentName);

    if (component) {
      if (component.destroy && typeof component.destroy === 'function') {
        component.destroy();
      }
      this.components.delete(componentName);
      console.log(`🗑️ Destroyed component: ${componentName}`);
    }
  }

  /**
   * Destroy all modules and components
   */
  destroy() {
    // Destroy all modules
    this.modules.forEach((module, key) => {
      if (module.destroy && typeof module.destroy === 'function') {
        module.destroy();
      }
    });
    this.modules.clear();

    // Destroy all components
    this.components.forEach((component, key) => {
      if (component.destroy && typeof component.destroy === 'function') {
        component.destroy();
      }
    });
    this.components.clear();

    this.isInitialized = false;
    console.log('🗑️ TownTrek application destroyed');
  }
}

// Create global app instance
let app = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  app = new TownTrekApp();
});

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = TownTrekApp;
}

// Global registration
window.TownTrekApp = TownTrekApp;
window.getApp = () => app;