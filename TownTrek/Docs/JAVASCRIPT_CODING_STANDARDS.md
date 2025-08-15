# JavaScript Coding Standards

## Overview
This document defines the coding standards and best practices for JavaScript development in the TownTrek application. All JavaScript code must adhere to these standards to ensure consistency, maintainability, and quality.

## Table of Contents
1. [General Principles](#general-principles)
2. [File Organization](#file-organization)
3. [Naming Conventions](#naming-conventions)
4. [Code Structure](#code-structure)
5. [Documentation Standards](#documentation-standards)
6. [Error Handling](#error-handling)
7. [Performance Guidelines](#performance-guidelines)
8. [Security Guidelines](#security-guidelines)
9. [Testing Requirements](#testing-requirements)
10. [Code Examples](#code-examples)

## General Principles

### 1. Modern JavaScript (ES6+)
- Use ES6+ features consistently
- Prefer `const` and `let` over `var`
- Use arrow functions for callbacks and short functions
- Use template literals for string interpolation
- Use destructuring for object and array operations

### 2. Code Quality
- Write self-documenting code with clear variable and function names
- Keep functions small and focused (max 50 lines)
- Avoid deep nesting (max 3 levels)
- Use early returns to reduce complexity
- Follow the DRY (Don't Repeat Yourself) principle

### 3. Consistency
- Use consistent indentation (2 spaces)
- Use semicolons consistently
- Use consistent quote style (single quotes preferred)
- Follow consistent bracket placement

## File Organization

### Directory Structure
```
wwwroot/js/
├── core/                    # Core functionality
│   ├── app.js              # Application bootstrap
│   ├── config.js           # Configuration constants
│   ├── utils.js            # Utility functions
│   ├── api-client.js       # API communication
│   ├── validation.js       # Validation utilities
│   └── notifications.js    # Notification system
├── components/             # Reusable UI components
│   ├── modal.js           # Modal component
│   ├── form-handler.js    # Form handling
│   ├── file-upload.js     # File upload component
│   └── data-table.js      # Data table component
├── modules/               # Feature modules
│   ├── auth/             # Authentication
│   ├── business/         # Business management
│   ├── admin/            # Admin functionality
│   └── client/           # Client functionality
└── shared/               # Shared utilities (optional; use core where possible)
    ├── constants.js      # Application constants (if not using core/config.js)
    ├── helpers.js        # Helper functions (prefer core/utils.js)
    └── mixins.js         # Reusable mixins
```

### File Naming
- Use kebab-case for file names: `business-manager.js`
- Use descriptive names that indicate purpose
- Avoid abbreviations unless commonly understood
- Group related files in directories

### File Structure Template
```javascript
/**
 * @fileoverview Brief description of file purpose
 * @author Your Name
 * @version 1.0.0
 */

// Imports (if using modules)
import { ApiClient } from '../core/api-client.js';
import { ValidationManager } from '../core/validation.js';

// Constants
const DEFAULT_OPTIONS = {
  // default configuration
};

const API_ENDPOINTS = {
  // API endpoints
};

/**
 * Main class description
 */
class ClassName {
  // Class implementation
}

// Export (UMD-lite style for legacy compatibility)
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ClassName;
}
window.ClassName = ClassName;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  // Initialization code
});
```

## Naming Conventions

### Variables and Functions
- Use camelCase: `userName`, `calculateTotal()`
- Use descriptive names: `isFormValid` instead of `valid`
- Use verbs for functions: `getUserData()`, `validateForm()`
- Use nouns for variables: `userData`, `formElement`

### Classes
- Use PascalCase: `BusinessManager`, `FormValidator`
- Use descriptive names that indicate purpose
- Avoid generic names like `Manager` or `Handler` alone

### Constants
- Use UPPER_SNAKE_CASE: `API_BASE_URL`, `MAX_FILE_SIZE`
- Group related constants in objects:
```javascript
const VALIDATION_RULES = {
  MIN_PASSWORD_LENGTH: 8,
  MAX_FILE_SIZE: 5242880, // 5MB
  ALLOWED_FILE_TYPES: ['image/jpeg', 'image/png']
};
```

### DOM Elements
- Use descriptive prefixes:
  - `$element` for jQuery objects (if used)
  - `elementRef` for DOM references
  - `elementSelector` for CSS selectors

### Event Handlers
- Use descriptive names with `handle` prefix:
  - `handleFormSubmit()`
  - `handleFileUpload()`
  - `handleModalClose()`

## Code Structure

### Class Structure
```javascript
/**
 * Class description
 * @class
 */
class BusinessManager {
  /**
   * @constructor
   * @param {Object} options - Configuration options
   */
  constructor(options = {}) {
    // Merge options with defaults
    this.options = { ...BusinessManager.defaultOptions, ...options };
    
    // Initialize properties
    this.isInitialized = false;
    this.elements = {};
    
    // Initialize
    this.init();
  }

  /**
   * Default configuration options
   * @static
   * @returns {Object} Default options
   */
  static get defaultOptions() {
    return {
      autoInit: true,
      validateOnChange: true,
      showNotifications: true
    };
  }

  // Public methods first
  /**
   * Initialize the manager
   * @public
   */
  init() {
    if (this.isInitialized) return;
    
    try {
      this.cacheElements();
      this.bindEvents();
      this.setupComponents();
      this.isInitialized = true;
    } catch (error) {
      this.handleError(error, 'Initialization failed');
    }
  }

  /**
   * Destroy the manager and cleanup
   * @public
   */
  destroy() {
    this.unbindEvents();
    this.cleanup();
    this.isInitialized = false;
  }

  // Private methods last (prefixed with _)
  /**
   * Cache DOM elements
   * @private
   */
  cacheElements() {
    this.elements = {
      form: document.getElementById('businessForm'),
      submitBtn: document.querySelector('.submit-btn'),
      // ... other elements
    };

    // Validate required elements exist
    this.validateElements();
  }

  /**
   * Validate that required elements exist
   * @private
   * @throws {Error} If required elements are missing
   */
  validateElements() {
    const required = ['form', 'submitBtn'];
    const missing = required.filter(key => !this.elements[key]);
    
    if (missing.length > 0) {
      throw new Error(`Required elements missing: ${missing.join(', ')}`);
    }
  }

  /**
   * Bind event listeners
   * @private
   */
  bindEvents() {
    if (!this.elements.form) return;

    this.elements.form.addEventListener('submit', this.handleFormSubmit.bind(this));
    this.elements.submitBtn.addEventListener('click', this.handleSubmitClick.bind(this));
  }

  /**
   * Unbind event listeners
   * @private
   */
  unbindEvents() {
    // Remove event listeners to prevent memory leaks
    if (this.elements.form) {
      this.elements.form.removeEventListener('submit', this.handleFormSubmit);
    }
  }

  /**
   * Handle form submission
   * @private
   * @param {Event} event - Form submit event
   */
  async handleFormSubmit(event) {
    event.preventDefault();
    
    try {
      const formData = new FormData(this.elements.form);
      const result = await this.submitData(formData);
      
      if (result.success) {
        this.handleSuccess(result);
      } else {
        this.handleError(new Error(result.message));
      }
    } catch (error) {
      this.handleError(error, 'Form submission failed');
    }
  }

  /**
   * Handle errors consistently
   * @private
   * @param {Error} error - Error object
   * @param {string} [context=''] - Error context
   */
  handleError(error, context = '') {
    const errorMessage = `${this.constructor.name}${context ? ` - ${context}` : ''}: ${error.message}`;
    console.error(errorMessage, error);
    
    if (this.options.showNotifications) {
      NotificationManager.show('An error occurred. Please try again.', 'error');
    }
  }
}
```

### Function Structure
```javascript
/**
 * Function description
 * @param {type} param1 - Parameter description
 * @param {type} [param2] - Optional parameter description
 * @returns {type} Return value description
 * @throws {Error} Error conditions
 */
async function processBusinessData(data, options = {}) {
  // Input validation
  if (!data || typeof data !== 'object') {
    throw new Error('Invalid data provided');
  }

  // Default options
  const config = {
    validate: true,
    transform: true,
    ...options
  };

  try {
    // Main logic
    let result = data;

    if (config.validate) {
      result = await validateData(result);
    }

    if (config.transform) {
      result = transformData(result);
    }

    return {
      success: true,
      data: result
    };
  } catch (error) {
    console.error('processBusinessData failed:', error);
    return {
      success: false,
      error: error.message
    };
  }
}
```

## Documentation Standards

### JSDoc Comments
All public methods, classes, and complex functions must have JSDoc comments:

```javascript
/**
 * Validates business form data
 * @param {HTMLFormElement} form - The form element to validate
 * @param {Object} rules - Validation rules
 * @param {boolean} rules.required - Whether field is required
 * @param {string} rules.type - Field type (email, phone, etc.)
 * @param {number} [rules.minLength] - Minimum length requirement
 * @param {boolean} [showErrors=true] - Whether to display errors
 * @returns {Promise<Object>} Validation result
 * @returns {boolean} returns.isValid - Whether validation passed
 * @returns {Object} returns.errors - Validation errors by field name
 * @throws {Error} When form element is invalid
 * @example
 * const result = await validateBusinessForm(form, {
 *   name: { required: true, minLength: 3 },
 *   email: { required: true, type: 'email' }
 * });
 * 
 * if (result.isValid) {
 *   // Process form
 * } else {
 *   // Handle errors
 *   console.log(result.errors);
 * }
 */
```

### Inline Comments
- Use inline comments sparingly for complex logic
- Explain "why" not "what"
- Keep comments up-to-date with code changes

```javascript
// Calculate business score based on multiple factors
// Uses weighted algorithm: reviews (40%) + ratings (30%) + activity (30%)
const businessScore = (reviews * 0.4) + (ratings * 0.3) + (activity * 0.3);
```

### File Headers
```javascript
/**
 * @fileoverview Business management functionality for TownTrek
 * Handles business creation, editing, validation, and API communication
 * 
 * @author TownTrek Development Team
 * @version 2.0.0
 * @since 1.0.0
 * 
 * @requires ApiClient
 * @requires ValidationManager
 * @requires NotificationManager
 */
```

## Error Handling

### Error Handling Strategy
Use the simplified `ClientErrorHandler` for client-side error handling. Server-side errors are handled by ASP.NET middleware. For AJAX calls, use `ClientErrorHandler.handleApiError()`. For user notifications, use `ClientErrorHandler.showError()` or `ClientErrorHandler.showSuccess()`. Avoid complex client-side error logging - let the server handle that.

### Try-Catch Usage
```javascript
// Always use try-catch for async operations
async function saveBusinessData(data) {
  try {
    const response = await ApiClient.post('/api/businesses', data);
    return { success: true, data: response };
  } catch (error) {
    ErrorHandler.handle(error, 'saveBusinessData');
    return { success: false, error: error.message };
  }
}

// Use try-catch for operations that might fail
function parseJsonData(jsonString) {
  try {
    return JSON.parse(jsonString);
  } catch (error) {
    ErrorHandler.handle(error, 'parseJsonData', {
      userMessage: 'Invalid data format received'
    });
    return null;
  }
}
```

## Performance Guidelines

### DOM Manipulation
```javascript
// Cache DOM elements
class FormManager {
  constructor() {
    // Cache elements once
    this.elements = {
      form: document.getElementById('businessForm'),
      inputs: document.querySelectorAll('.form-input'),
      submitBtn: document.querySelector('.submit-btn')
    };
  }

  // Use event delegation for dynamic content
  bindEvents() {
    this.elements.form.addEventListener('click', (event) => {
      if (event.target.matches('.remove-btn')) {
        this.handleRemove(event);
      }
    });
  }

  // Batch DOM updates
  updateMultipleElements(updates) {
    // Use DocumentFragment for multiple insertions
    const fragment = document.createDocumentFragment();
    
    updates.forEach(update => {
      const element = document.createElement(update.tag);
      element.textContent = update.text;
      fragment.appendChild(element);
    });
    
    this.elements.container.appendChild(fragment);
  }
}
```

### Async Operations
```javascript
// Use Promise.all for parallel operations
async function loadBusinessData(businessId) {
  try {
    const [business, reviews, images] = await Promise.all([
      ApiClient.get(`/api/businesses/${businessId}`),
      ApiClient.get(`/api/businesses/${businessId}/reviews`),
      ApiClient.get(`/api/businesses/${businessId}/images`)
    ]);

    return { business, reviews, images };
  } catch (error) {
    ErrorHandler.handle(error, 'loadBusinessData');
    throw error;
  }
}

// Use debouncing for frequent operations
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

// Usage
const debouncedSearch = debounce(performSearch, 300);
searchInput.addEventListener('input', debouncedSearch);
```

### Memory Management
```javascript
class ComponentManager {
  constructor() {
    this.eventListeners = [];
    this.timers = [];
  }

  // Track event listeners for cleanup
  addEventListener(element, event, handler) {
    element.addEventListener(event, handler);
    this.eventListeners.push({ element, event, handler });
  }

  // Track timers for cleanup
  setTimeout(callback, delay) {
    const timerId = setTimeout(callback, delay);
    this.timers.push(timerId);
    return timerId;
  }

  // Cleanup method
  destroy() {
    // Remove event listeners
    this.eventListeners.forEach(({ element, event, handler }) => {
      element.removeEventListener(event, handler);
    });

    // Clear timers
    this.timers.forEach(timerId => {
      clearTimeout(timerId);
    });

    // Clear references
    this.eventListeners = [];
    this.timers = [];
  }
}
```

## Security Guidelines

### Input Validation
```javascript
// Always validate and sanitize input
function validateBusinessName(name) {
  if (typeof name !== 'string') {
    throw new Error('Business name must be a string');
  }

  // Remove potentially dangerous characters
  const sanitized = name.replace(/[<>\"'&]/g, '');
  
  if (sanitized.length < 2 || sanitized.length > 100) {
    throw new Error('Business name must be between 2 and 100 characters');
  }

  return sanitized.trim();
}

// Validate URLs
function validateUrl(url) {
  try {
    const urlObj = new URL(url);
    // Only allow http and https protocols
    if (!['http:', 'https:'].includes(urlObj.protocol)) {
      throw new Error('Invalid URL protocol');
    }
    return urlObj.href;
  } catch (error) {
    throw new Error('Invalid URL format');
  }
}
```

### XSS Prevention
```javascript
// Use textContent instead of innerHTML when possible
function displayUserContent(content) {
  const element = document.getElementById('content');
  element.textContent = content; // Safe from XSS
}

// If HTML is needed, sanitize it
function displaySafeHtml(html) {
  // Use a library like DOMPurify for HTML sanitization
  const clean = DOMPurify.sanitize(html);
  document.getElementById('content').innerHTML = clean;
}
```

### CSRF Protection
Always include the anti-forgery token on non-GET requests. Prefer using `core/api-client.js` methods which add the `RequestVerificationToken` header automatically for JSON and FormData. For direct `fetch` (e.g., raw FormData uploads), read the token from `ApiClient.getAntiForgeryToken()` and set the header explicitly.

## Testing Requirements

### Unit Test Structure
```javascript
// business-manager.test.js
describe('BusinessManager', () => {
  let businessManager;
  let mockElements;

  beforeEach(() => {
    // Setup DOM elements
    document.body.innerHTML = `
      <form id="businessForm">
        <input name="businessName" required>
        <button class="submit-btn">Submit</button>
      </form>
    `;

    businessManager = new BusinessManager();
  });

  afterEach(() => {
    businessManager.destroy();
    document.body.innerHTML = '';
  });

  describe('initialization', () => {
    it('should initialize with default options', () => {
      expect(businessManager.options).toEqual(BusinessManager.defaultOptions);
    });

    it('should cache required elements', () => {
      expect(businessManager.elements.form).toBeTruthy();
      expect(businessManager.elements.submitBtn).toBeTruthy();
    });
  });

  describe('form validation', () => {
    it('should validate required fields', async () => {
      const result = await businessManager.validateForm();
      expect(result.isValid).toBe(false);
      expect(result.errors).toHaveProperty('businessName');
    });
  });
});
```

### Integration Test Example
```javascript
// business-form.integration.test.js
describe('Business Form Integration', () => {
  it('should create business successfully', async () => {
    // Setup
    const formData = {
      businessName: 'Test Business',
      category: 'restaurant',
      email: 'test@example.com'
    };

    // Mock API response
    jest.spyOn(ApiClient, 'post').mockResolvedValue({
      success: true,
      data: { id: 1, ...formData }
    });

    // Execute
    const businessManager = new BusinessManager();
    const result = await businessManager.createBusiness(formData);

    // Assert
    expect(result.success).toBe(true);
    expect(ApiClient.post).toHaveBeenCalledWith('/api/businesses', formData);
  });
});
```

## Code Examples

### Complete Class Example
```javascript
/**
 * @fileoverview Business form management
 * @author TownTrek Team
 * @version 2.0.0
 */

import { ApiClient } from '../core/api-client.js';
import { ValidationManager } from '../core/validation.js';
import { NotificationManager } from '../core/notifications.js';
import { ErrorHandler } from '../core/error-handler.js';

/**
 * Manages business form operations including validation, submission, and UI updates
 * @class
 */
class BusinessFormManager {
  /**
   * @constructor
   * @param {Object} options - Configuration options
   * @param {string} options.formSelector - CSS selector for the form
   * @param {boolean} options.autoValidate - Enable automatic validation
   */
  constructor(options = {}) {
    this.options = {
      formSelector: '#businessForm',
      autoValidate: true,
      showNotifications: true,
      ...options
    };

    this.isInitialized = false;
    this.elements = {};
    this.validationRules = this.getValidationRules();

    this.init();
  }

  /**
   * Initialize the form manager
   * @public
   */
  init() {
    try {
      this.cacheElements();
      this.bindEvents();
      this.setupValidation();
      this.isInitialized = true;
    } catch (error) {
      ErrorHandler.handle(error, 'BusinessFormManager initialization');
    }
  }

  /**
   * Cache DOM elements
   * @private
   */
  cacheElements() {
    this.elements = {
      form: document.querySelector(this.options.formSelector),
      submitBtn: document.querySelector('.submit-btn'),
      inputs: document.querySelectorAll('.form-input'),
      categorySelect: document.getElementById('businessCategory')
    };

    if (!this.elements.form) {
      throw ErrorHandler.createError(
        `Form not found: ${this.options.formSelector}`,
        'ELEMENT_NOT_FOUND'
      );
    }
  }

  /**
   * Bind event listeners
   * @private
   */
  bindEvents() {
    this.elements.form.addEventListener('submit', this.handleSubmit.bind(this));
    
    if (this.options.autoValidate) {
      this.elements.inputs.forEach(input => {
        input.addEventListener('blur', this.handleFieldValidation.bind(this));
        input.addEventListener('input', this.handleFieldChange.bind(this));
      });
    }

    if (this.elements.categorySelect) {
      this.elements.categorySelect.addEventListener('change', this.handleCategoryChange.bind(this));
    }
  }

  /**
   * Handle form submission
   * @private
   * @param {Event} event - Submit event
   */
  async handleSubmit(event) {
    event.preventDefault();

    try {
      // Validate form
      const validation = await this.validateForm();
      if (!validation.isValid) {
        this.displayValidationErrors(validation.errors);
        return;
      }

      // Show loading state
      this.setLoadingState(true);

      // Submit data
      const formData = new FormData(this.elements.form);
      const result = await ApiClient.post('/api/businesses', formData);

      if (result.success) {
        this.handleSuccess(result);
      } else {
        this.handleError(new Error(result.message || 'Submission failed'));
      }
    } catch (error) {
      ErrorHandler.handle(error, 'Form submission', {
        userMessage: 'Failed to save business. Please try again.'
      });
    } finally {
      this.setLoadingState(false);
    }
  }

  /**
   * Validate the entire form
   * @private
   * @returns {Promise<Object>} Validation result
   */
  async validateForm() {
    const formData = new FormData(this.elements.form);
    return ValidationManager.validate(formData, this.validationRules);
  }

  /**
   * Get validation rules for the form
   * @private
   * @returns {Object} Validation rules
   */
  getValidationRules() {
    return {
      businessName: {
        required: true,
        minLength: 2,
        maxLength: 100
      },
      email: {
        required: true,
        type: 'email'
      },
      phone: {
        required: true,
        type: 'phone'
      },
      category: {
        required: true
      }
    };
  }

  /**
   * Set loading state
   * @private
   * @param {boolean} isLoading - Loading state
   */
  setLoadingState(isLoading) {
    if (this.elements.submitBtn) {
      this.elements.submitBtn.disabled = isLoading;
      this.elements.submitBtn.textContent = isLoading ? 'Saving...' : 'Save Business';
    }
  }

  /**
   * Handle successful submission
   * @private
   * @param {Object} result - Success result
   */
  handleSuccess(result) {
    if (this.options.showNotifications) {
      NotificationManager.show('Business saved successfully!', 'success');
    }

    // Redirect or update UI as needed
    if (result.redirectUrl) {
      window.location.href = result.redirectUrl;
    }
  }

  /**
   * Cleanup and destroy
   * @public
   */
  destroy() {
    // Remove event listeners
    if (this.elements.form) {
      this.elements.form.removeEventListener('submit', this.handleSubmit);
    }

    // Clear references
    this.elements = {};
    this.isInitialized = false;
  }
}

// Export for module use
export default BusinessFormManager;

// Global registration for legacy compatibility
window.BusinessFormManager = BusinessFormManager;

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  if (document.querySelector('#businessForm')) {
    new BusinessFormManager();
  }
});
```

## Enforcement

### Code Review Checklist
- [ ] Follows naming conventions
- [ ] Has proper JSDoc documentation
- [ ] Includes error handling
- [ ] Uses modern JavaScript features consistently
- [ ] Has no hardcoded values
- [ ] Includes input validation
- [ ] Has proper memory cleanup
- [ ] Follows security guidelines
- [ ] Includes unit tests
- [ ] Performance considerations addressed

### Automated Tools
- **ESLint**: For code style and error detection
- **Prettier**: For code formatting
- **JSDoc**: For documentation generation
- **Jest** (or equivalent): For unit testing where applicable
- **Lighthouse**: For performance auditing

### Configuration Files
Create `.eslintrc.js`:
```javascript
module.exports = {
  env: {
    browser: true,
    es2021: true,
    jest: true
  },
  extends: [
    'eslint:recommended'
  ],
  parserOptions: {
    ecmaVersion: 12,
    sourceType: 'module'
  },
  rules: {
    'indent': ['error', 2],
    'quotes': ['error', 'single'],
    'semi': ['error', 'always'],
    'no-unused-vars': 'error',
    'no-console': 'warn',
    'prefer-const': 'error',
    'no-var': 'error'
  }
};
```

This coding standards document ensures all JavaScript code in the TownTrek application maintains high quality, consistency, and maintainability.