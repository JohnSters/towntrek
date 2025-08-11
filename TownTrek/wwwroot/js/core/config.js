/**
 * @fileoverview Application configuration and constants
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Application configuration object
 * @namespace
 */
const APP_CONFIG = {
  /**
   * API configuration
   */
  api: {
    baseUrl: '/api',
    timeout: 30000,
    retryAttempts: 3
  },

  /**
   * Validation configuration
   */
  validation: {
    debounceDelay: 300,
    showErrorsOnBlur: true,
    showErrorsOnSubmit: true
  },

  /**
   * File upload configuration
   */
  fileUpload: {
    maxFileSize: 5242880, // 5MB
    allowedImageTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'],
    maxFiles: 10
  },

  /**
   * UI configuration
   */
  ui: {
    notificationDuration: 5000,
    loadingDelay: 200,
    animationDuration: 300
  },

  /**
   * Business form configuration
   */
  business: {
    categories: {
      'shops-retail': 'shopsSection',
      'markets-vendors': 'marketSection', 
      'tours-experiences': 'tourSection',
      'events': 'eventSection',
      'restaurants-food': 'restaurantSection',
      'accommodation': 'accommodationSection'
    },
    defaultOperatingHours: {
      open: '09:00',
      close: '17:00'
    }
  },

  /**
   * Validation rules
   */
  validationRules: {
    businessName: {
      minLength: 2,
      maxLength: 100
    },
    email: {
      pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    },
    phone: {
      pattern: /^(\+27|0)[0-9]{9}$/,
      formatPattern: /^(\+27\s\d{2}\s\d{3}\s\d{4}|0\d{2}\s\d{3}\s\d{4})$/
    },
    url: {
      pattern: /^https?:\/\/.+/
    }
  }
};

/**
 * API endpoints
 */
const API_ENDPOINTS = {
  businesses: {
    create: '/Client/Business/Create',
    update: '/Client/Business/Edit',
    delete: '/Client/Business/Delete',
    getSubCategories: '/Client/Business/GetSubCategories'
  },
  validation: {
    address: '/Client/ValidateAddress'
  },
  auth: {
    login: '/Auth/Login',
    register: '/Auth/Register',
    forgotPassword: '/Auth/ForgotPassword'
  }
};

/**
 * CSS selectors used throughout the application
 */
const SELECTORS = {
  forms: {
    business: '#businessForm',
    auth: '.auth-form',
    town: '#townForm'
  },
  buttons: {
    submit: '.submit-btn',
    cancel: '.cancel-btn',
    delete: '.delete-btn'
  },
  inputs: {
    required: '[required]',
    formInput: '.form-input',
    fileInput: 'input[type="file"]'
  },
  notifications: {
    container: '.notification-container',
    toast: '.toast'
  }
};

/**
 * Error messages
 */
const ERROR_MESSAGES = {
  validation: {
    required: 'This field is required',
    email: 'Please enter a valid email address',
    phone: 'Please enter a valid South African phone number',
    url: 'Please enter a valid URL',
    minLength: 'Must be at least {min} characters long',
    maxLength: 'Must be no more than {max} characters long',
    fileSize: 'File size must be less than {size}',
    fileType: 'File type not allowed'
  },
  api: {
    network: 'Network error. Please check your connection.',
    server: 'Server error. Please try again later.',
    timeout: 'Request timed out. Please try again.',
    unauthorized: 'You are not authorized to perform this action.',
    notFound: 'The requested resource was not found.'
  },
  general: {
    unexpected: 'An unexpected error occurred. Please try again.',
    formSubmission: 'Failed to submit form. Please try again.',
    fileUpload: 'Failed to upload file. Please try again.'
  }
};

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { APP_CONFIG, API_ENDPOINTS, SELECTORS, ERROR_MESSAGES };
}

// Global registration for legacy compatibility
window.APP_CONFIG = APP_CONFIG;
window.API_ENDPOINTS = API_ENDPOINTS;
window.SELECTORS = SELECTORS;
window.ERROR_MESSAGES = ERROR_MESSAGES;