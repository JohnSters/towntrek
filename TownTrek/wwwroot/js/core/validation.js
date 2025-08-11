/**
 * @fileoverview Unified validation system
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Validation manager for form validation
 * @class
 */
class ValidationManager {
  /**
   * @constructor
   * @param {Object} options - Configuration options
   */
  constructor(options = {}) {
    this.options = {
      showErrorsOnBlur: true,
      showErrorsOnSubmit: true,
      debounceDelay: 300,
      ...options
    };

    this.validators = new Map();
    this.setupDefaultValidators();
  }

  /**
   * Setup default validation rules
   * @private
   */
  setupDefaultValidators() {
    // Required field validator
    this.addValidator('required', (value, rule) => {
      const trimmedValue = typeof value === 'string' ? value.trim() : value;
      return trimmedValue !== '' && trimmedValue !== null && trimmedValue !== undefined;
    }, 'This field is required');

    // Email validator
    this.addValidator('email', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(value);
    }, 'Please enter a valid email address');

    // Phone validator (South African format)
    this.addValidator('phone', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      const phoneRegex = /^(\+27|0)[0-9]{9}$/;
      return phoneRegex.test(value.replace(/\s/g, ''));
    }, 'Please enter a valid South African phone number');

    // URL validator
    this.addValidator('url', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      try {
        const url = new URL(value);
        return ['http:', 'https:'].includes(url.protocol);
      } catch {
        return false;
      }
    }, 'Please enter a valid URL');

    // Minimum length validator
    this.addValidator('minLength', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      return value.length >= rule.value;
    }, 'Must be at least {value} characters long');

    // Maximum length validator
    this.addValidator('maxLength', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      return value.length <= rule.value;
    }, 'Must be no more than {value} characters long');

    // Pattern validator
    this.addValidator('pattern', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      const regex = typeof rule.value === 'string' ? new RegExp(rule.value) : rule.value;
      return regex.test(value);
    }, 'Invalid format');

    // Number validators
    this.addValidator('min', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      const num = parseFloat(value);
      return !isNaN(num) && num >= rule.value;
    }, 'Must be at least {value}');

    this.addValidator('max', (value, rule) => {
      if (!value) return true; // Allow empty if not required
      const num = parseFloat(value);
      return !isNaN(num) && num <= rule.value;
    }, 'Must be no more than {value}');

    // File size validator
    this.addValidator('fileSize', (files, rule) => {
      if (!files || files.length === 0) return true;
      const maxSize = rule.value;
      
      for (let file of files) {
        if (file.size > maxSize) {
          return false;
        }
      }
      return true;
    }, 'File size must be less than {value}');

    // File type validator
    this.addValidator('fileType', (files, rule) => {
      if (!files || files.length === 0) return true;
      const allowedTypes = Array.isArray(rule.value) ? rule.value : [rule.value];
      
      for (let file of files) {
        if (!allowedTypes.includes(file.type)) {
          return false;
        }
      }
      return true;
    }, 'File type not allowed');

    // Custom business name validator
    this.addValidator('businessName', (value, rule) => {
      if (!value) return true;
      // Business name should not contain special characters except spaces, hyphens, and apostrophes
      const businessNameRegex = /^[a-zA-Z0-9\s\-'&.]+$/;
      return businessNameRegex.test(value);
    }, 'Business name contains invalid characters');

    // Password strength validator
    this.addValidator('password', (value, rule) => {
      if (!value) return true;
      const minLength = rule.minLength || 8;
      const requireUppercase = rule.requireUppercase !== false;
      const requireLowercase = rule.requireLowercase !== false;
      const requireNumbers = rule.requireNumbers !== false;
      const requireSpecial = rule.requireSpecial || false;

      if (value.length < minLength) return false;
      if (requireUppercase && !/[A-Z]/.test(value)) return false;
      if (requireLowercase && !/[a-z]/.test(value)) return false;
      if (requireNumbers && !/\d/.test(value)) return false;
      if (requireSpecial && !/[!@#$%^&*(),.?":{}|<>]/.test(value)) return false;

      return true;
    }, 'Password does not meet requirements');

    // Confirm password validator
    this.addValidator('confirmPassword', (value, rule, allValues) => {
      if (!value) return true;
      const passwordField = rule.field || 'password';
      return value === allValues[passwordField];
    }, 'Passwords do not match');
  }

  /**
   * Add custom validator
   * @param {string} name - Validator name
   * @param {Function} validator - Validator function
   * @param {string} message - Error message template
   */
  addValidator(name, validator, message) {
    this.validators.set(name, { validator, message });
  }

  /**
   * Validate form data
   * @param {FormData|Object} data - Form data to validate
   * @param {Object} rules - Validation rules
   * @returns {Object} Validation result
   */
  validate(data, rules) {
    const errors = {};
    let isValid = true;

    // Convert FormData to object if needed
    const values = data instanceof FormData ? this.formDataToObject(data) : data;

    Object.entries(rules).forEach(([fieldName, fieldRules]) => {
      const fieldValue = values[fieldName];
      const fieldErrors = this.validateField(fieldName, fieldValue, fieldRules, values);

      if (fieldErrors.length > 0) {
        errors[fieldName] = fieldErrors;
        isValid = false;
      }
    });

    return { isValid, errors, values };
  }

  /**
   * Validate single field
   * @param {string} fieldName - Field name
   * @param {*} value - Field value
   * @param {Object|Array} rules - Field validation rules
   * @param {Object} allValues - All form values (for cross-field validation)
   * @returns {Array} Array of error messages
   */
  validateField(fieldName, value, rules, allValues = {}) {
    const errors = [];
    const rulesArray = Array.isArray(rules) ? rules : [rules];

    rulesArray.forEach(rule => {
      if (typeof rule === 'string') {
        // Simple rule name
        const validatorData = this.validators.get(rule);
        if (validatorData) {
          if (!validatorData.validator(value, {}, allValues)) {
            errors.push(validatorData.message);
          }
        }
      } else if (typeof rule === 'object') {
        // Rule with parameters
        Object.entries(rule).forEach(([ruleName, ruleValue]) => {
          const validatorData = this.validators.get(ruleName);
          if (validatorData) {
            const ruleConfig = typeof ruleValue === 'object' ? ruleValue : { value: ruleValue };
            if (!validatorData.validator(value, ruleConfig, allValues)) {
              let message = ruleConfig.message || validatorData.message;
              // Replace placeholders in message
              message = message.replace(/\{(\w+)\}/g, (match, key) => {
                return ruleConfig[key] !== undefined ? ruleConfig[key] : match;
              });
              errors.push(message);
            }
          }
        });
      }
    });

    return errors;
  }

  /**
   * Validate form element
   * @param {HTMLFormElement} form - Form element
   * @param {Object} rules - Validation rules
   * @param {Object} options - Validation options
   * @returns {Object} Validation result
   */
  validateForm(form, rules, options = {}) {
    const config = { ...this.options, ...options };
    const formData = new FormData(form);
    const result = this.validate(formData, rules);

    if (config.showErrorsOnSubmit && !result.isValid) {
      this.displayErrors(form, result.errors);
    }

    return result;
  }

  /**
   * Setup form validation
   * @param {HTMLFormElement} form - Form element
   * @param {Object} rules - Validation rules
   * @param {Object} options - Validation options
   */
  setupFormValidation(form, rules, options = {}) {
    const config = { ...this.options, ...options };

    // Validate on submit
    form.addEventListener('submit', (event) => {
      const result = this.validateForm(form, rules, config);
      if (!result.isValid) {
        event.preventDefault();
        return false;
      }
    });

    // Validate on blur if enabled
    if (config.showErrorsOnBlur) {
      Object.keys(rules).forEach(fieldName => {
        const field = form.querySelector(`[name="${fieldName}"]`);
        if (field) {
          const debouncedValidate = Utils.debounce(() => {
            this.validateSingleField(field, rules[fieldName], form);
          }, config.debounceDelay);

          field.addEventListener('blur', debouncedValidate);
          field.addEventListener('input', () => {
            this.clearFieldError(field);
          });
        }
      });
    }
  }

  /**
   * Validate single form field
   * @param {HTMLElement} field - Form field element
   * @param {Object|Array} rules - Field validation rules
   * @param {HTMLFormElement} form - Parent form
   */
  validateSingleField(field, rules, form) {
    const formData = new FormData(form);
    const values = this.formDataToObject(formData);
    const fieldValue = field.type === 'file' ? field.files : values[field.name];
    
    const errors = this.validateField(field.name, fieldValue, rules, values);
    
    if (errors.length > 0) {
      this.showFieldError(field, errors[0]);
    } else {
      this.clearFieldError(field);
    }
  }

  /**
   * Display validation errors on form
   * @param {HTMLFormElement} form - Form element
   * @param {Object} errors - Validation errors
   */
  displayErrors(form, errors) {
    // Clear existing errors
    this.clearFormErrors(form);

    Object.entries(errors).forEach(([fieldName, fieldErrors]) => {
      const field = form.querySelector(`[name="${fieldName}"]`);
      if (field && fieldErrors.length > 0) {
        this.showFieldError(field, fieldErrors[0]);
      }
    });

    // Focus first error field
    const firstErrorField = form.querySelector('.field-error');
    if (firstErrorField) {
      const field = firstErrorField.previousElementSibling;
      if (field && field.focus) {
        field.focus();
        Utils.scrollToElement(field);
      }
    }
  }

  /**
   * Show field error
   * @param {HTMLElement} field - Form field
   * @param {string} message - Error message
   */
  showFieldError(field, message) {
    this.clearFieldError(field);

    field.classList.add('error', 'is-invalid');
    
    const errorDiv = document.createElement('div');
    errorDiv.className = 'field-error invalid-feedback';
    errorDiv.textContent = message;
    errorDiv.setAttribute('role', 'alert');

    // Insert error message after the field
    field.parentNode.insertBefore(errorDiv, field.nextSibling);
  }

  /**
   * Clear field error
   * @param {HTMLElement} field - Form field
   */
  clearFieldError(field) {
    field.classList.remove('error', 'is-invalid');
    
    const existingError = field.parentNode.querySelector('.field-error');
    if (existingError) {
      existingError.remove();
    }
  }

  /**
   * Clear all form errors
   * @param {HTMLFormElement} form - Form element
   */
  clearFormErrors(form) {
    const errorElements = form.querySelectorAll('.field-error');
    errorElements.forEach(error => error.remove());

    const errorFields = form.querySelectorAll('.error, .is-invalid');
    errorFields.forEach(field => {
      field.classList.remove('error', 'is-invalid');
    });
  }

  /**
   * Convert FormData to plain object
   * @private
   * @param {FormData} formData - FormData object
   * @returns {Object} Plain object
   */
  formDataToObject(formData) {
    const obj = {};
    
    for (let [key, value] of formData.entries()) {
      if (obj[key]) {
        // Handle multiple values (like checkboxes)
        if (Array.isArray(obj[key])) {
          obj[key].push(value);
        } else {
          obj[key] = [obj[key], value];
        }
      } else {
        obj[key] = value;
      }
    }
    
    return obj;
  }

  /**
   * Get validation rules for business forms
   * @static
   * @returns {Object} Business form validation rules
   */
  static getBusinessFormRules() {
    return {
      businessName: [
        'required',
        { minLength: 2 },
        { maxLength: 100 },
        'businessName'
      ],
      email: [
        'required',
        'email'
      ],
      phone: [
        'required',
        'phone'
      ],
      physicalAddress: [
        'required',
        { minLength: 10 }
      ],
      businessCategory: [
        'required'
      ],
      description: [
        { maxLength: 1000 }
      ],
      website: [
        'url'
      ],
      businessLogo: [
        { fileSize: window.APP_CONFIG?.fileUpload?.maxFileSize || 5242880 },
        { fileType: window.APP_CONFIG?.fileUpload?.allowedImageTypes || ['image/jpeg', 'image/png'] }
      ],
      businessImages: [
        { fileSize: window.APP_CONFIG?.fileUpload?.maxFileSize || 5242880 },
        { fileType: window.APP_CONFIG?.fileUpload?.allowedImageTypes || ['image/jpeg', 'image/png'] }
      ]
    };
  }

  /**
   * Get validation rules for auth forms
   * @static
   * @returns {Object} Auth form validation rules
   */
  static getAuthFormRules() {
    return {
      email: [
        'required',
        'email'
      ],
      password: [
        'required',
        { password: { minLength: 8, requireUppercase: true, requireLowercase: true, requireNumbers: true } }
      ],
      confirmPassword: [
        'required',
        { confirmPassword: { field: 'password' } }
      ],
      firstName: [
        'required',
        { minLength: 2 },
        { maxLength: 50 }
      ],
      lastName: [
        'required',
        { minLength: 2 },
        { maxLength: 50 }
      ],
      phone: [
        'phone'
      ]
    };
  }
}

// Create global instance
const validationManager = new ValidationManager();

// Static methods for convenience
ValidationManager.validate = (data, rules) => {
  return validationManager.validate(data, rules);
};

ValidationManager.validateForm = (form, rules, options) => {
  return validationManager.validateForm(form, rules, options);
};

ValidationManager.setupFormValidation = (form, rules, options) => {
  return validationManager.setupFormValidation(form, rules, options);
};

ValidationManager.addValidator = (name, validator, message) => {
  return validationManager.addValidator(name, validator, message);
};

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ValidationManager;
}

// Global registration
window.ValidationManager = ValidationManager;