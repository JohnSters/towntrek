/**
 * @fileoverview FormHandlerComponent - generic client-side form validation
 * Detects forms with [data-form-handler] and applies validation on blur/input and submit
 */

class FormHandlerComponent {
  constructor() {
    this.forms = Array.from(document.querySelectorAll('form[data-form-handler]'));
    this.initializeForms();
  }

  initializeForms() {
    this.forms.forEach((form) => this.setupForm(form));
  }

  setupForm(formElement) {
    const requiredFields = formElement.querySelectorAll('[required]');

    requiredFields.forEach((field) => {
      let userInteracted = false;

      field.addEventListener('blur', () => {
        userInteracted = true;
        this.validateField(field);
      });

      field.addEventListener('input', () => {
        this.clearFieldError(field);
        if (userInteracted) this.validateField(field);
      });

      field.addEventListener('input', () => {
        userInteracted = true;
      }, { once: true });
    });

    formElement.addEventListener('submit', (event) => {
      const isValid = this.validateForm(formElement);
      if (!isValid) {
        event.preventDefault();
        if (window.NotificationManager) {
          NotificationManager.error('Please correct the highlighted errors and try again.');
        }
      }
    });
  }

  validateForm(formElement) {
    const requiredFields = formElement.querySelectorAll('[required]');
    let formIsValid = true;
    requiredFields.forEach((field) => {
      if (!this.validateField(field)) {
        formIsValid = false;
      }
    });
    return formIsValid;
  }

  validateField(fieldElement) {
    const value = (fieldElement.value || '').trim();
    this.clearFieldError(fieldElement);

    if (fieldElement.hasAttribute('required') && !value) {
      this.showFieldError(fieldElement, 'This field is required');
      return false;
    }

    if (fieldElement.type === 'email' && value && !this.isValidEmail(value)) {
      this.showFieldError(fieldElement, 'Please enter a valid email address');
      return false;
    }

    if (fieldElement.type === 'tel' && value && !this.isValidPhone(value)) {
      this.showFieldError(fieldElement, 'Please enter a valid phone number');
      return false;
    }

    if (fieldElement.type === 'url' && value && !this.isValidUrl(value)) {
      this.showFieldError(fieldElement, 'Please enter a valid URL');
      return false;
    }

    if (fieldElement.type === 'number' && value && Number.isNaN(Number(value))) {
      this.showFieldError(fieldElement, 'Please enter a valid number');
      return false;
    }

    if (fieldElement.name === 'Latitude' && value) {
      const lat = parseFloat(value);
      if (lat < -90 || lat > 90) {
        this.showFieldError(fieldElement, 'Latitude must be between -90 and 90');
        return false;
      }
    }

    if (fieldElement.name === 'Longitude' && value) {
      const lng = parseFloat(value);
      if (lng < -180 || lng > 180) {
        this.showFieldError(fieldElement, 'Longitude must be between -180 and 180');
        return false;
      }
    }

    fieldElement.classList.add('success');
    return true;
  }

  showFieldError(fieldElement, message) {
    fieldElement.classList.add('error');
    fieldElement.classList.remove('success');
    let errorElement = fieldElement.parentElement?.querySelector('.error-message');
    if (!errorElement) {
      errorElement = document.createElement('span');
      errorElement.className = 'error-message';
      fieldElement.parentElement?.appendChild(errorElement);
    }
    errorElement.textContent = message;
  }

  clearFieldError(fieldElement) {
    fieldElement.classList.remove('error');
    const errorElement = fieldElement.parentElement?.querySelector('.error-message');
    if (errorElement) errorElement.textContent = '';
  }

  isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  isValidPhone(phone) {
    return /^(\+?[0-9\s\-()]{10,})$/.test(phone);
  }

  isValidUrl(urlString) {
    try {
      // eslint-disable-next-line no-new
      new URL(urlString);
      return true;
    } catch {
      return false;
    }
  }

  destroy() {
    this.forms = [];
  }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = FormHandlerComponent;
}

// Global registration
window.FormHandlerComponent = FormHandlerComponent;


