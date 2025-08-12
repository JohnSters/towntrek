/**
 * @fileoverview Centralized error handling system
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Centralized error handling class
 * @class
 */
class ErrorHandler {
  /**
   * Handle application errors consistently
   * @static
   * @param {Error} error - Error object
   * @param {string} context - Error context
   * @param {Object} options - Handling options
   * @param {boolean} options.showNotification - Show user notification
   * @param {string} options.userMessage - Custom user message
   * @param {Function} options.onError - Custom error handler
   * @param {boolean} options.logToConsole - Log to console
   * @param {boolean} options.logToService - Send to logging service
   */
  static handle(error, context = '', options = {}) {
    const defaultOptions = {
      showNotification: true,
      userMessage: null,
      onError: null,
      logToConsole: true,
      logToService: false
    };

    const config = { ...defaultOptions, ...options };

    // Create error info object
    const errorInfo = {
      message: error.message || 'Unknown error',
      context,
      timestamp: new Date().toISOString(),
      stack: error.stack,
      userAgent: navigator.userAgent,
      url: window.location.href,
      code: error.code || 'UNKNOWN_ERROR',
      details: error.details || {}
    };

    // Log to console
    if (config.logToConsole) {
      console.group(`🚨 Application Error${context ? ` - ${context}` : ''}`);
      console.error('Error:', error);
      console.error('Context:', context);
      console.error('Stack:', error.stack);
      console.error('Full Info:', errorInfo);
      console.groupEnd();
    }

    // Show user notification
    if (config.showNotification && window.NotificationManager) {
      const userMessage = config.userMessage || this.getUserFriendlyMessage(error);
      window.NotificationManager.show(userMessage, 'error');
    }

    // Send to logging service
    if (config.logToService && window.APP_CONFIG?.logging?.enabled) {
      this.logToService(errorInfo).catch(logError => {
        console.error('Failed to log error to service:', logError);
      });
    }

    // Call custom error handler
    if (config.onError && typeof config.onError === 'function') {
      try {
        config.onError(error, errorInfo);
      } catch (handlerError) {
        console.error('Error in custom error handler:', handlerError);
      }
    }

    return errorInfo;
  }

  /**
   * Create application-specific error
   * @static
   * @param {string} message - Error message
   * @param {string} code - Error code
   * @param {Object} details - Additional error details
   * @returns {Error} Application error
   */
  static createError(message, code = 'GENERIC_ERROR', details = {}) {
    const error = new Error(message);
    error.code = code;
    error.details = details;
    error.timestamp = new Date().toISOString();
    return error;
  }

  /**
   * Handle API errors specifically
   * @static
   * @param {Response|Error} error - API error
   * @param {string} endpoint - API endpoint
   * @param {Object} options - Error handling options
   * @returns {Object} Standardized error response
   */
  static async handleApiError(error, endpoint = '', options = {}) {
    let errorInfo;

    if (error instanceof Response) {
      // HTTP response error
      const status = error.status;
      const statusText = error.statusText;
      
      let errorData = {};
      try {
        errorData = await error.json();
      } catch (parseError) {
        errorData = { message: statusText };
      }

      const apiError = this.createError(
        errorData.message || this.getHttpErrorMessage(status),
        `HTTP_${status}`,
        {
          status,
          statusText,
          endpoint,
          response: errorData
        }
      );

      errorInfo = this.handle(apiError, `API Error - ${endpoint}`, {
        userMessage: this.getHttpUserMessage(status),
        ...options
      });
    } else {
      // Network or other error
      const networkError = this.createError(
        error.message || 'Network error occurred',
        'NETWORK_ERROR',
        { endpoint, originalError: error }
      );

      errorInfo = this.handle(networkError, `Network Error - ${endpoint}`, {
        userMessage: ERROR_MESSAGES.api.network,
        ...options
      });
    }

    return {
      success: false,
      error: errorInfo.message,
      code: errorInfo.code,
      details: errorInfo.details
    };
  }

  /**
   * Handle validation errors
   * @static
   * @param {Object} validationErrors - Validation error object
   * @param {string} context - Validation context
   * @returns {Object} Formatted validation errors
   */
  static handleValidationErrors(validationErrors, context = 'Form validation') {
    const formattedErrors = {};
    
    Object.entries(validationErrors).forEach(([field, errors]) => {
      formattedErrors[field] = Array.isArray(errors) ? errors : [errors];
    });

    this.handle(
      this.createError('Validation failed', 'VALIDATION_ERROR', formattedErrors),
      context,
      {
        showNotification: false, // Don't show global notification for validation errors
        logToConsole: false // Don't log validation errors to console
      }
    );

    return formattedErrors;
  }

  /**
   * Get user-friendly error message
   * @static
   * @private
   * @param {Error} error - Error object
   * @returns {string} User-friendly message
   */
  static getUserFriendlyMessage(error) {
    if (error.code && ERROR_MESSAGES.api[error.code.toLowerCase()]) {
      return ERROR_MESSAGES.api[error.code.toLowerCase()];
    }

    if (error.message.includes('network') || error.message.includes('fetch')) {
      return ERROR_MESSAGES.api.network;
    }

    if (error.message.includes('timeout')) {
      return ERROR_MESSAGES.api.timeout;
    }

    return ERROR_MESSAGES.general.unexpected;
  }

  /**
   * Get HTTP error message
   * @static
   * @private
   * @param {number} status - HTTP status code
   * @returns {string} Error message
   */
  static getHttpErrorMessage(status) {
    const messages = {
      400: 'Bad request - invalid data provided',
      401: 'Unauthorized - please log in',
      403: 'Forbidden - insufficient permissions',
      404: 'Resource not found',
      409: 'Conflict - resource already exists',
      422: 'Validation failed',
      429: 'Too many requests - please try again later',
      500: 'Internal server error',
      502: 'Bad gateway - server unavailable',
      503: 'Service unavailable',
      504: 'Gateway timeout'
    };

    return messages[status] || `HTTP error ${status}`;
  }

  /**
   * Get user-friendly HTTP error message
   * @static
   * @private
   * @param {number} status - HTTP status code
   * @returns {string} User-friendly message
   */
  static getHttpUserMessage(status) {
    const messages = {
      400: 'Please check your input and try again.',
      401: 'Please log in to continue.',
      403: 'You don\'t have permission to perform this action.',
      404: 'The requested item could not be found.',
      409: 'This item already exists.',
      422: 'Please correct the errors and try again.',
      429: 'Too many requests. Please wait a moment and try again.',
      500: 'Server error. Please try again later.',
      502: 'Service temporarily unavailable. Please try again later.',
      503: 'Service temporarily unavailable. Please try again later.',
      504: 'Request timed out. Please try again.'
    };

    return messages[status] || ERROR_MESSAGES.general.unexpected;
  }

  /**
   * Log error to external service
   * @static
   * @private
   * @param {Object} errorInfo - Error information
   * @returns {Promise} Logging promise
   */
  static async logToService(errorInfo) {
    if (!window.APP_CONFIG?.logging?.endpoint) {
      return;
    }

    try {
      await fetch(window.APP_CONFIG.logging.endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          level: 'error',
          timestamp: errorInfo.timestamp,
          message: errorInfo.message,
          context: errorInfo.context,
          stack: errorInfo.stack,
          userAgent: errorInfo.userAgent,
          url: errorInfo.url,
          code: errorInfo.code,
          details: errorInfo.details
        })
      });
    } catch (logError) {
      // Silently fail logging errors to avoid infinite loops
      console.warn('Failed to log error to service:', logError);
    }
  }

  /**
   * Set up global error handlers
   * @static
   */
  static setupGlobalHandlers() {
    // Handle unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      this.handle(
        event.reason instanceof Error ? event.reason : new Error(event.reason),
        'Unhandled Promise Rejection',
        {
          userMessage: ERROR_MESSAGES.general.unexpected
        }
      );
    });

    // Handle uncaught errors
    window.addEventListener('error', (event) => {
      this.handle(
        new Error(event.message),
        'Uncaught Error',
        {
          userMessage: ERROR_MESSAGES.general.unexpected,
          details: {
            filename: event.filename,
            lineno: event.lineno,
            colno: event.colno
          }
        }
      );
    });
  }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ErrorHandler;
}

// Global registration
window.ErrorHandler = ErrorHandler;

// Setup global error handlers when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  ErrorHandler.setupGlobalHandlers();
});