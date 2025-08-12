/**
 * @fileoverview Authentication manager - handles login, register, and forgot password flows
 */

class AuthManager {
  constructor(options = {}) {
    this.options = {
      mode: options.mode || 'login', // 'login' | 'register' | 'forgot-password'
      autoValidate: true,
      showNotifications: true
    };

    this.isInitialized = false;
    this.elements = {};

    this.init();
  }

  init() {
    if (this.isInitialized) return;
    try {
      this.cacheElements();
      this.bindEvents();
      this.setupValidation();
      this.isInitialized = true;
      console.log(`✅ AuthManager initialized (${this.options.mode})`);
    } catch (error) {
      ErrorHandler.handle(error, 'AuthManager initialization');
    }
  }

  cacheElements() {
    this.elements = {
      form: document.querySelector('.auth-form'),
      submitBtn: document.querySelector('.auth-btn-primary'),
      // Register-only
      accountTypeCards: document.querySelectorAll('.account-type-card'),
      accountTypeInput: document.getElementById('accountType'),
      planSelection: document.getElementById('planSelection'),
      planCards: document.querySelectorAll('.plan-card'),
      selectedPlanInput: document.getElementById('selectedPlan'),
      phoneInput: document.querySelector('input[name="Phone"], #phone')
    };

    if (!this.elements.form) {
      throw ErrorHandler.createError('Auth form not found', 'ELEMENT_NOT_FOUND');
    }
  }

  bindEvents() {
    // Submit handling: add loading state if valid, otherwise prevent
    this.elements.form.addEventListener('submit', (event) => {
      const validation = this.validateForm();
      if (!validation.isValid) {
        event.preventDefault();
        return false;
      }

      // Additional register-only checks
      if (this.options.mode === 'register') {
        const acceptTerms = this.elements.form.querySelector('input[name="AcceptTerms"]');
        if (acceptTerms && !acceptTerms.checked) {
          event.preventDefault();
          NotificationManager.warning('Please accept the Terms of Service to continue.');
          return false;
        }
      }

      this.setLoadingState(true);
      return true;
    });

    // Register-only UI behaviors
    if (this.options.mode === 'register') {
      // Account type toggle
      this.elements.accountTypeCards.forEach((card) => {
        card.addEventListener('click', () => {
          const type = card.dataset.type;
          if (!type) return;
          if (this.elements.accountTypeInput) {
            this.elements.accountTypeInput.value = type;
          }
          // Show plans for business, hide for community
          if (this.elements.planSelection) {
            this.elements.planSelection.style.display = type === 'business' ? 'block' : 'none';
          }
          // Visual selection
          this.elements.accountTypeCards.forEach(c => c.classList.toggle('active', c === card));
        });
      });

      // Plan selection
      this.elements.planCards.forEach((card) => {
        card.addEventListener('click', () => {
          const planId = card.dataset.plan;
          if (this.elements.selectedPlanInput && planId) {
            this.elements.selectedPlanInput.value = planId;
          }
          this.elements.planCards.forEach(c => c.classList.toggle('selected', c === card));
        });
      });
    }

    // Phone formatting (register)
    if (this.elements.phoneInput) {
      this.elements.phoneInput.addEventListener('input', (e) => {
        const caretPos = e.target.selectionStart;
        const formatted = Utils.formatPhoneNumber(e.target.value);
        e.target.value = formatted;
        try { e.target.setSelectionRange(caretPos, caretPos); } catch {}
      });
    }
  }

  setupValidation() {
    if (!this.options.autoValidate) return;
    const rules = this.getValidationRules();
    ValidationManager.setupFormValidation(this.elements.form, rules, {
      showErrorsOnBlur: true,
      showErrorsOnSubmit: true
    });
  }

  getValidationRules() {
    if (this.options.mode === 'login') {
      return {
        email: ['required', 'email'],
        password: ['required']
      };
    }

    if (this.options.mode === 'forgot-password') {
      return {
        email: ['required', 'email']
      };
    }

    // register (view uses PascalCase names)
    return {
      FullName: ['required', { minLength: 2 }, { maxLength: 100 }],
      Email: ['required', 'email'],
      Phone: ['phone'],
      Location: [{ maxLength: 120 }],
      Password: ['required', { password: { minLength: 8, requireUppercase: true, requireLowercase: true, requireNumbers: true } }],
      ConfirmPassword: ['required', { confirmPassword: { field: 'Password' } }]
    };
  }

  validateForm() {
    const rules = this.getValidationRules();
    return ValidationManager.validateForm(this.elements.form, rules, {
      showErrorsOnSubmit: true
    });
  }

  setLoadingState(isLoading) {
    const button = this.elements.submitBtn;
    if (!button) return;
    if (isLoading) {
      button.disabled = true;
      if (!button.dataset.originalText) {
        button.dataset.originalText = button.textContent || '';
      }
      button.textContent = 'Processing...';
    } else {
      button.disabled = false;
      const original = button.dataset.originalText || 'Submit';
      button.textContent = original;
    }
  }

  destroy() {
    this.isInitialized = false;
    this.elements = {};
  }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = AuthManager;
}

// Global registration
window.AuthManager = AuthManager;


