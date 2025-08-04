// Authentication JavaScript
// Handles form validation, plan selection, and user interactions

class AuthManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupAccountTypeSelection();
        this.setupPlanSelection();
        this.setupFormValidation();
        this.setupPasswordToggle();
        this.setupFormSubmission();
    }

    // Account Type Selection Logic
    setupAccountTypeSelection() {
        const accountTypeCards = document.querySelectorAll('.account-type-card');
        const accountTypeInput = document.getElementById('accountType');
        const planSelection = document.getElementById('planSelection');
        const submitBtn = document.getElementById('submitBtn');

        accountTypeCards.forEach(card => {
            card.addEventListener('click', () => {
                // Remove selected class from all cards
                accountTypeCards.forEach(c => c.classList.remove('selected'));
                
                // Add selected class to clicked card
                card.classList.add('selected');
                
                // Update hidden input value
                const accountType = card.dataset.type;
                if (accountTypeInput) {
                    accountTypeInput.value = accountType;
                }

                // Show/hide plan selection based on account type
                if (planSelection) {
                    if (accountType === 'business') {
                        planSelection.style.display = 'block';
                        // Auto-select standard plan for business accounts
                        const standardPlan = document.querySelector('.plan-card.featured');
                        if (standardPlan) {
                            setTimeout(() => standardPlan.click(), 100);
                        }
                    } else {
                        planSelection.style.display = 'none';
                    }
                }

                // Update submit button text
                this.updateSubmitButtonForAccountType(accountType);
            });
        });

        // Auto-select business account on page load
        const businessCard = document.querySelector('.account-type-card[data-type="business"]');
        if (businessCard) {
            businessCard.click();
        }
    }

    updateSubmitButtonForAccountType(accountType) {
        const submitBtn = document.getElementById('submitBtn');
        if (!submitBtn) return;

        if (accountType === 'business') {
            submitBtn.textContent = 'Create Business Account';
            submitBtn.className = 'auth-btn auth-btn-cta';
        } else {
            submitBtn.textContent = 'Join Community';
            submitBtn.className = 'auth-btn auth-btn-primary';
        }
    }

    // Plan Selection Logic
    setupPlanSelection() {
        const planCards = document.querySelectorAll('.plan-card');
        const planInput = document.getElementById('selectedPlan');

        planCards.forEach(card => {
            card.addEventListener('click', () => {
                // Remove selected class from all cards
                planCards.forEach(c => c.classList.remove('selected'));
                
                // Add selected class to clicked card
                card.classList.add('selected');
                
                // Update hidden input value
                if (planInput) {
                    planInput.value = card.dataset.plan;
                }

                // Update form button text based on selection
                this.updateSubmitButton(card.dataset.plan);
            });
        });

        // Auto-select featured plan on page load
        const featuredPlan = document.querySelector('.plan-card.featured');
        if (featuredPlan && planInput) {
            featuredPlan.click();
        }
    }

    updateSubmitButton(planType) {
        const submitBtn = document.getElementById('submitBtn');
        const accountTypeInput = document.getElementById('accountType');
        
        if (!submitBtn || !accountTypeInput) return;

        // Only update for business accounts
        if (accountTypeInput.value === 'business') {
            const planPrices = {
                'basic': 'R199',
                'standard': 'R399',
                'premium': 'R599'
            };

            const price = planPrices[planType] || 'R399';
            submitBtn.textContent = `Start ${price}/month Plan`;
        }
    }

    // Form Validation
    setupFormValidation() {
        const forms = document.querySelectorAll('.auth-form');
        
        forms.forEach(form => {
            const inputs = form.querySelectorAll('.form-input');
            
            inputs.forEach(input => {
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', () => this.clearFieldError(input));
                
                // Format phone numbers as user types
                if (input.type === 'tel') {
                    input.addEventListener('input', () => this.formatPhoneNumber(input));
                }
            });
        });
    }

    validateField(field) {
        const value = field.value.trim();
        const type = field.type;
        const name = field.name;
        
        let isValid = true;
        let errorMessage = '';

        // Required field validation
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'This field is required';
        }

        // Email validation
        if (type === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                isValid = false;
                errorMessage = 'Please enter a valid email address';
            }
        }

        // Password validation
        if (type === 'password' && value) {
            if (value.length < 8) {
                isValid = false;
                errorMessage = 'Password must be at least 8 characters long';
            } else if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(value)) {
                isValid = false;
                errorMessage = 'Password must contain uppercase, lowercase, and number';
            }
        }

        // Confirm password validation
        if (name === 'confirmPassword' && value) {
            const passwordField = document.querySelector('input[name="password"]');
            if (passwordField && value !== passwordField.value) {
                isValid = false;
                errorMessage = 'Passwords do not match';
            }
        }

        // Phone validation (South African format)
        if (type === 'tel' && value) {
            const phoneRegex = /^(\+27|0)[0-9]{9}$/;
            if (!phoneRegex.test(value.replace(/\s/g, ''))) {
                isValid = false;
                errorMessage = 'Please enter a valid South African phone number';
            }
        }

        this.showFieldError(field, isValid, errorMessage);
        return isValid;
    }

    showFieldError(field, isValid, message) {
        const formGroup = field.closest('.form-group');
        let errorElement = formGroup.querySelector('.field-error');

        if (!isValid) {
            field.classList.add('error');
            
            if (!errorElement) {
                errorElement = document.createElement('div');
                errorElement.className = 'field-error';
                errorElement.style.cssText = `
                    color: var(--orange-pantone);
                    font-size: var(--body-xs);
                    margin-top: var(--space-xs);
                `;
                formGroup.appendChild(errorElement);
            }
            
            errorElement.textContent = message;
        } else {
            field.classList.remove('error');
            if (errorElement) {
                errorElement.remove();
            }
        }
    }

    clearFieldError(field) {
        field.classList.remove('error');
        const formGroup = field.closest('.form-group');
        const errorElement = formGroup.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    // Password Toggle
    setupPasswordToggle() {
        const passwordFields = document.querySelectorAll('input[type="password"]');
        
        passwordFields.forEach(field => {
            const toggleBtn = document.createElement('button');
            toggleBtn.type = 'button';
            toggleBtn.className = 'password-toggle';
            toggleBtn.textContent = 'Show';
            
            // Make parent relative
            const formGroup = field.closest('.form-group');
            formGroup.style.position = 'relative';
            
            // Add padding to input
            field.style.paddingRight = '60px';
            
            formGroup.appendChild(toggleBtn);

            toggleBtn.addEventListener('click', () => {
                const isPassword = field.type === 'password';
                field.type = isPassword ? 'text' : 'password';
                toggleBtn.textContent = isPassword ? 'Hide' : 'Show';
            });
        });
    }

    // Form Submission
    setupFormSubmission() {
        const forms = document.querySelectorAll('.auth-form');
        
        forms.forEach(form => {
            form.addEventListener('submit', (e) => this.handleFormSubmit(e));
        });
    }

    handleFormSubmit(e) {
        e.preventDefault();
        
        const form = e.target;
        const submitBtn = form.querySelector('.auth-btn-primary');
        const inputs = form.querySelectorAll('.form-input[required]');
        
        // Validate all required fields
        let isFormValid = true;
        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isFormValid = false;
            }
        });

        // Check terms acceptance for registration
        const termsCheckbox = form.querySelector('input[name="acceptTerms"]');
        if (termsCheckbox && !termsCheckbox.checked) {
            isFormValid = false;
            this.showMessage('Please accept the terms and conditions', 'error');
        }

        if (!isFormValid) {
            this.showMessage('Please correct the errors above', 'error');
            return;
        }

        // Show loading state
        this.setLoadingState(submitBtn, true);

        // Simulate form submission (replace with actual submission)
        setTimeout(() => {
            this.setLoadingState(submitBtn, false);
            
            // For demo purposes - replace with actual form submission
            if (form.id === 'loginForm') {
                this.showMessage('Login successful! Redirecting...', 'success');
                setTimeout(() => {
                    window.location.href = '/dashboard';
                }, 1500);
            } else if (form.id === 'registerForm') {
                this.showMessage('Registration successful! Please check your email.', 'success');
                setTimeout(() => {
                    window.location.href = '/login';
                }, 2000);
            }
        }, 2000);
    }

    setLoadingState(button, isLoading) {
        if (isLoading) {
            button.disabled = true;
            button.classList.add('loading');
            button.dataset.originalText = button.textContent;
            button.textContent = 'Processing...';
        } else {
            button.disabled = false;
            button.classList.remove('loading');
            button.textContent = button.dataset.originalText || button.textContent;
        }
    }

    showMessage(message, type = 'info') {
        // Remove existing messages
        const existingMessages = document.querySelectorAll('.auth-message');
        existingMessages.forEach(msg => msg.remove());

        // Create new message
        const messageDiv = document.createElement('div');
        messageDiv.className = `auth-message ${type === 'error' ? 'error-message' : 'success-message'}`;
        messageDiv.textContent = message;

        // Insert at top of form
        const form = document.querySelector('.auth-form');
        form.insertBefore(messageDiv, form.firstChild);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            messageDiv.remove();
        }, 5000);
    }

    // Utility Methods
    formatPhoneNumber(input) {
        let value = input.value.replace(/\D/g, '');
        
        // Handle different input formats
        if (value.startsWith('27') && value.length >= 3) {
            // International format starting with 27
            value = '+27 ' + value.substring(2, 4) + ' ' + value.substring(4, 7) + ' ' + value.substring(7, 11);
        } else if (value.startsWith('0') && value.length >= 2) {
            // Local format starting with 0
            const localNumber = value.substring(1);
            if (localNumber.length >= 2) {
                value = '+27 ' + localNumber.substring(0, 2) + ' ' + localNumber.substring(2, 5) + ' ' + localNumber.substring(5, 9);
            }
        } else if (value.length >= 9 && !value.startsWith('27') && !value.startsWith('0')) {
            // Assume it's a local number without leading 0
            value = '+27 ' + value.substring(0, 2) + ' ' + value.substring(2, 5) + ' ' + value.substring(5, 9);
        }
        
        // Clean up extra spaces and limit length
        value = value.replace(/\s+/g, ' ').trim();
        if (value.length > 17) { // +27 XX XXX XXXX = 17 chars max
            value = value.substring(0, 17);
        }
        
        input.value = value;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new AuthManager();
});

// Export for potential use in other scripts
window.AuthManager = AuthManager;