/**
 * @fileoverview Notification system for user feedback
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Notification manager for displaying user messages
 * @class
 */
class NotificationManager {
  /**
   * @constructor
   */
  constructor() {
    this.notifications = new Map();
    this.container = null;
    this.init();
  }

  /**
   * Initialize the notification system
   * @private
   */
  init() {
    this.createContainer();
    // Styles are provided by CSS components (wwwroot/css/components/notifications.css)
  }

  /**
   * Create notification container
   * @private
   */
  createContainer() {
    this.container = document.createElement('div');
    this.container.id = 'notification-container';
    this.container.className = 'notification-container';
    document.body.appendChild(this.container);
  }

  // Removed inline style injection

  /**
   * Show notification
   * @param {string} message - Notification message
   * @param {string} type - Notification type (success, error, warning, info)
   * @param {Object} options - Additional options
   * @param {string} options.title - Notification title
   * @param {number} options.duration - Auto-hide duration in milliseconds
   * @param {boolean} options.closable - Whether notification can be closed manually
   * @param {Function} options.onClick - Click handler
   * @param {Function} options.onClose - Close handler
   * @returns {string} Notification ID
   */
  show(message, type = 'info', options = {}) {
    const defaultOptions = {
      title: this.getDefaultTitle(type),
      duration: window.APP_CONFIG?.ui?.notificationDuration || 5000,
      closable: true,
      onClick: null,
      onClose: null
    };

    const config = { ...defaultOptions, ...options };
    const id = Utils.generateId('notification');

    const notification = this.createNotification(id, message, type, config);
    this.container.appendChild(notification);

    // Store notification reference
    this.notifications.set(id, {
      element: notification,
      config,
      timeoutId: null
    });

    // Show notification with animation
    requestAnimationFrame(() => {
      notification.classList.add('show');
    });

    // Auto-hide if duration is set
    if (config.duration > 0) {
      this.scheduleAutoHide(id, config.duration);
    }

    return id;
  }

  /**
   * Create notification element
   * @private
   * @param {string} id - Notification ID
   * @param {string} message - Notification message
   * @param {string} type - Notification type
   * @param {Object} config - Notification configuration
   * @returns {HTMLElement} Notification element
   */
  createNotification(id, message, type, config) {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.dataset.id = id;

    const icon = this.getIcon(type);
    const closeButton = config.closable ? `
      <button class="notification-close" aria-label="Close notification">
        <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
        </svg>
      </button>
    ` : '';

    notification.innerHTML = `
      <div class="notification-header">
        <h4 class="notification-title">
          ${icon}
          ${Utils.escapeHtml(config.title)}
        </h4>
        ${closeButton}
      </div>
      <p class="notification-message">${Utils.escapeHtml(message)}</p>
      ${config.duration > 0 ? '<div class="notification-progress"></div>' : ''}
    `;

    // Add event listeners
    if (config.closable) {
      const closeBtn = notification.querySelector('.notification-close');
      closeBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        this.hide(id);
      });
    }

    if (config.onClick) {
      notification.style.cursor = 'pointer';
      notification.addEventListener('click', config.onClick);
    }

    return notification;
  }

  /**
   * Schedule auto-hide for notification
   * @private
   * @param {string} id - Notification ID
   * @param {number} duration - Duration in milliseconds
   */
  scheduleAutoHide(id, duration) {
    const notificationData = this.notifications.get(id);
    if (!notificationData) return;

    const progressBar = notificationData.element.querySelector('.notification-progress');
    
    if (progressBar) {
      // Animate progress bar
      progressBar.style.width = '100%';
      progressBar.style.transition = `width ${duration}ms linear`;
      
      requestAnimationFrame(() => {
        progressBar.style.width = '0%';
      });
    }

    notificationData.timeoutId = setTimeout(() => {
      this.hide(id);
    }, duration);
  }

  /**
   * Hide notification
   * @param {string} id - Notification ID
   */
  hide(id) {
    const notificationData = this.notifications.get(id);
    if (!notificationData) return;

    const { element, config, timeoutId } = notificationData;

    // Clear timeout
    if (timeoutId) {
      clearTimeout(timeoutId);
    }

    // Hide with animation
    element.classList.add('hide');

    // Remove from DOM after animation
    setTimeout(() => {
      if (element.parentNode) {
        element.parentNode.removeChild(element);
      }
      this.notifications.delete(id);

      // Call onClose callback
      if (config.onClose) {
        config.onClose(id);
      }
    }, 300);
  }

  /**
   * Hide all notifications
   */
  hideAll() {
    const ids = Array.from(this.notifications.keys());
    ids.forEach(id => this.hide(id));
  }

  /**
   * Get default title for notification type
   * @private
   * @param {string} type - Notification type
   * @returns {string} Default title
   */
  getDefaultTitle(type) {
    const titles = {
      success: 'Success',
      error: 'Error',
      warning: 'Warning',
      info: 'Information'
    };
    return titles[type] || 'Notification';
  }

  /**
   * Get icon for notification type
   * @private
   * @param {string} type - Notification type
   * @returns {string} Icon HTML
   */
  getIcon(type) {
    const icons = {
      success: `<svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
      </svg>`,
      error: `<svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
      </svg>`,
      warning: `<svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
      </svg>`,
      info: `<svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
      </svg>`
    };
    return icons[type] || icons.info;
  }

  /**
   * Convenience methods for different notification types
   */
  success(message, options = {}) {
    return this.show(message, 'success', options);
  }

  error(message, options = {}) {
    return this.show(message, 'error', { duration: 0, ...options });
  }

  warning(message, options = {}) {
    return this.show(message, 'warning', options);
  }

  info(message, options = {}) {
    return this.show(message, 'info', options);
  }
}

// Create global instance
const notificationManager = new NotificationManager();

// Static methods for convenience
NotificationManager.show = (message, type, options) => {
  return notificationManager.show(message, type, options);
};

NotificationManager.hide = (id) => {
  return notificationManager.hide(id);
};

NotificationManager.hideAll = () => {
  return notificationManager.hideAll();
};

NotificationManager.success = (message, options) => {
  return notificationManager.success(message, options);
};

NotificationManager.error = (message, options) => {
  return notificationManager.error(message, options);
};

NotificationManager.warning = (message, options) => {
  return notificationManager.warning(message, options);
};

NotificationManager.info = (message, options) => {
  return notificationManager.info(message, options);
};

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = NotificationManager;
}

// Global registration
window.NotificationManager = NotificationManager;