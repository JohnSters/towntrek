/**
 * @fileoverview Simple client-side error handling for AJAX calls and user feedback
 * @author TownTrek Development Team
 * @version 3.0.0
 */

/**
 * Simple client-side error handler - focused on user experience
 * @class
 */
class ClientErrorHandler {
  /**
   * Handle API/AJAX errors
   * @static
   * @param {Response|Error} error - Error object or Response
   * @param {string} context - Error context for logging
   * @returns {Object} Standardized error response
   */
  static async handleApiError(error, context = '') {
    let message = 'Something went wrong. Please try again.';
    let shouldRedirect = false;

    if (error instanceof Response) {
      switch (error.status) {
        case 401:
          message = 'Please log in to continue.';
          shouldRedirect = true;
          break;
        case 403:
          message = 'You don\'t have permission to perform this action.';
          break;
        case 404:
          message = 'The requested item could not be found.';
          break;
        case 422:
          message = 'Please check your input and try again.';
          break;
        case 429:
          message = 'Too many requests. Please wait a moment and try again.';
          break;
        case 500:
        case 502:
        case 503:
          message = 'Server error. Please try again later.';
          break;
      }

      // Try to get more specific error message from response
      try {
        const errorData = await error.json();
        if (errorData.message) {
          message = errorData.message;
        }
      } catch (parseError) {
        // Use default message
      }
    }

    // Show user notification if NotificationManager is available
    if (window.NotificationManager) {
      window.NotificationManager.error(message);
    } else {
      // Fallback to alert
      alert(message);
    }

    // Log to console in development
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
      console.error(`Client Error [${context}]:`, error);
    }

    // Redirect to login if unauthorized
    if (shouldRedirect) {
      setTimeout(() => {
        window.location.href = '/Auth/Login';
      }, 2000);
    }

    return {
      success: false,
      message: message,
      shouldRedirect: shouldRedirect
    };
  }

  /**
   * Show a simple error message to the user
   * @static
   * @param {string} message - Error message
   */
  static showError(message) {
    if (window.NotificationManager) {
      window.NotificationManager.error(message);
    } else {
      alert(message);
    }
  }

  /**
   * Show a simple success message to the user
   * @static
   * @param {string} message - Success message
   */
  static showSuccess(message) {
    if (window.NotificationManager) {
      window.NotificationManager.success(message);
    } else {
      // Could use a simple toast or just log
      console.log('Success:', message);
    }
  }

  /**
   * Handle form validation errors
   * @static
   * @param {Object} validationErrors - Validation errors object
   * @param {HTMLFormElement} form - Form element
   */
  static handleValidationErrors(validationErrors, form = null) {
    if (!validationErrors || typeof validationErrors !== 'object') {
      return;
    }

    // Clear existing validation messages
    if (form) {
      const existingErrors = form.querySelectorAll('.field-validation-error');
      existingErrors.forEach(error => error.textContent = '');
    }

    // Display new validation errors
    Object.entries(validationErrors).forEach(([field, errors]) => {
      const errorMessages = Array.isArray(errors) ? errors : [errors];
      const errorText = errorMessages.join(', ');

      if (form) {
        // Try to find validation span for this field
        const validationSpan = form.querySelector(`[data-valmsg-for="${field}"]`);
        if (validationSpan) {
          validationSpan.textContent = errorText;
          validationSpan.className = 'field-validation-error';
        }
      }
    });
  }
}

// Global registration for backward compatibility
window.ErrorHandler = ClientErrorHandler;
window.ClientErrorHandler = ClientErrorHandler;