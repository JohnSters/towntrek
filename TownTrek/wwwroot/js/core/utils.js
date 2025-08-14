/**
 * @fileoverview Core utility functions
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Utility functions for common operations
 * @namespace
 */
const Utils = {
  /**
   * Debounce function execution
   * @param {Function} func - Function to debounce
   * @param {number} wait - Wait time in milliseconds
   * @param {boolean} immediate - Execute immediately on first call
   * @returns {Function} Debounced function
   */
  debounce(func, wait, immediate = false) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        timeout = null;
        if (!immediate) func(...args);
      };
      const callNow = immediate && !timeout;
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
      if (callNow) func(...args);
    };
  },

  /**
   * Throttle function execution
   * @param {Function} func - Function to throttle
   * @param {number} limit - Time limit in milliseconds
   * @returns {Function} Throttled function
   */
  throttle(func, limit) {
    let inThrottle;
    return function executedFunction(...args) {
      if (!inThrottle) {
        func.apply(this, args);
        inThrottle = true;
        setTimeout(() => inThrottle = false, limit);
      }
    };
  },

  /**
   * Deep merge objects
   * @param {Object} target - Target object
   * @param {...Object} sources - Source objects
   * @returns {Object} Merged object
   */
  deepMerge(target, ...sources) {
    if (!sources.length) return target;
    const source = sources.shift();

    if (this.isObject(target) && this.isObject(source)) {
      for (const key in source) {
        if (this.isObject(source[key])) {
          if (!target[key]) Object.assign(target, { [key]: {} });
          this.deepMerge(target[key], source[key]);
        } else {
          Object.assign(target, { [key]: source[key] });
        }
      }
    }

    return this.deepMerge(target, ...sources);
  },

  /**
   * Check if value is an object
   * @param {*} item - Item to check
   * @returns {boolean} True if object
   */
  isObject(item) {
    return item && typeof item === 'object' && !Array.isArray(item);
  },

  /**
   * Generate unique ID
   * @param {string} prefix - Optional prefix
   * @returns {string} Unique ID
   */
  generateId(prefix = 'id') {
    return `${prefix}_${Date.now()}_${Math.random().toString(36).slice(2, 11)}`;
  },

  /**
   * Format file size in human readable format
   * @param {number} bytes - File size in bytes
   * @returns {string} Formatted file size
   */
  formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  },

  /**
   * Format phone number to South African format
   * @param {string} phone - Phone number
   * @returns {string} Formatted phone number
   */
  formatPhoneNumber(phone) {
    if (!phone) return '';
    const digits = String(phone).replace(/\D/g, '');

    // Normalize to ZA format (E.164-ish display): +27 XX XXX XXXX
    // Accept inputs: 0XXXXXXXXX, 27XXXXXXXXX, +27XXXXXXXXX
    let local;
    if (digits.startsWith('27')) {
      local = digits.slice(2);
    } else if (digits.startsWith('0')) {
      local = digits.slice(1);
    } else {
      local = digits;
    }

    // Only digits now, expect 9 for mobile/landline without leading 0
    if (local.length < 9) return phone.trim();
    const a = local.slice(0, 2);
    const b = local.slice(2, 5);
    const c = local.slice(5, 9);
    return `+27 ${a} ${b} ${c}`.trim();
  },

  /**
   * Sanitize HTML string
   * @param {string} str - String to sanitize
   * @returns {string} Sanitized string
   */
  sanitizeHtml(str) {
    if (!str) return '';
    const temp = document.createElement('div');
    temp.textContent = str;
    return temp.innerHTML;
  },

  /**
   * Escape HTML entities
   * @param {string} str - String to escape
   * @returns {string} Escaped string
   */
  escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
  },

  /**
   * Get query parameter value
   * @param {string} name - Parameter name
   * @param {string} url - URL to parse (defaults to current URL)
   * @returns {string|null} Parameter value
   */
  getQueryParam(name, url = window.location.href) {
    const urlObj = new URL(url);
    return urlObj.searchParams.get(name);
  },

  /**
   * Set query parameter
   * @param {string} name - Parameter name
   * @param {string} value - Parameter value
   * @param {boolean} pushState - Whether to update browser history
   */
  setQueryParam(name, value, pushState = true) {
    const url = new URL(window.location.href);
    url.searchParams.set(name, value);
    
    if (pushState) {
      window.history.pushState({}, '', url);
    } else {
      window.history.replaceState({}, '', url);
    }
  },

  /**
   * Check if element is visible in viewport
   * @param {HTMLElement} element - Element to check
   * @returns {boolean} True if visible
   */
  isElementVisible(element) {
    if (!element) return false;
    const rect = element.getBoundingClientRect();
    return (
      rect.top >= 0 &&
      rect.left >= 0 &&
      rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
      rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
  },

  /**
   * Scroll element into view smoothly
   * @param {HTMLElement} element - Element to scroll to
   * @param {Object} options - Scroll options
   */
  scrollToElement(element, options = {}) {
    if (!element) return;
    
    const defaultOptions = {
      behavior: 'smooth',
      block: 'center',
      inline: 'nearest'
    };
    
    element.scrollIntoView({ ...defaultOptions, ...options });
  },

  /**
   * Copy text to clipboard
   * @param {string} text - Text to copy
   * @returns {Promise<boolean>} Success status
   */
  async copyToClipboard(text) {
    try {
      // Preferred modern API in secure contexts
      if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function' && window.isSecureContext) {
        await navigator.clipboard.writeText(String(text));
        return true;
      }

      // Secondary modern API: ClipboardItem support
      if (navigator.clipboard && typeof navigator.clipboard.write === 'function' && window.isSecureContext) {
        const blob = new Blob([String(text)], { type: 'text/plain' });
        const item = new ClipboardItem({ 'text/plain': blob });
        await navigator.clipboard.write([item]);
        return true;
      }

      // Non-secure or unsupported environments: present a selectable field and instruct user
      const container = document.createElement('div');
      container.style.position = 'fixed';
      container.style.left = '-9999px';
      const input = document.createElement('input');
      input.value = String(text);
      input.setAttribute('readonly', '');
      container.appendChild(input);
      document.body.appendChild(container);
      input.select();
      // Inform user to press keyboard shortcut since programmatic copy is not available
      if (window.NotificationManager) {
        NotificationManager.info('Press Ctrl+C (⌘+C on macOS) to copy the selected text.');
      }
      // Keep selection momentarily for the user
      setTimeout(() => { document.body.removeChild(container); }, 3000);
      return false;
    } catch (error) {
      console.error('Failed to copy text:', error);
      return false;
    }
  },

  /**
   * Wait for specified time
   * @param {number} ms - Milliseconds to wait
   * @returns {Promise} Promise that resolves after delay
   */
  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  },

  /**
   * Retry function with exponential backoff
   * @param {Function} fn - Function to retry
   * @param {number} maxAttempts - Maximum retry attempts
   * @param {number} baseDelay - Base delay in milliseconds
   * @returns {Promise} Promise that resolves with function result
   */
  async retry(fn, maxAttempts = 3, baseDelay = 1000) {
    let lastError;
    
    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
      try {
        return await fn();
      } catch (error) {
        lastError = error;
        
        if (attempt === maxAttempts) {
          throw lastError;
        }
        
        const delay = baseDelay * Math.pow(2, attempt - 1);
        await this.sleep(delay);
      }
    }
  }
};

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = Utils;
}

// Global registration for legacy compatibility
window.Utils = Utils;