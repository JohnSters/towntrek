// Add Town Form JavaScript
document.addEventListener('DOMContentLoaded', function () {
    initializeFormValidation();
});

// Form Validation
function initializeFormValidation() {
    const form = document.querySelector('.add-business-form');
    const requiredFields = form.querySelectorAll('[required]');

    // Real-time validation - only after user interaction
    requiredFields.forEach(field => {
        let hasInteracted = false;

        // Mark field as interacted when user focuses and then leaves
        field.addEventListener('blur', function () {
            hasInteracted = true;
            if (hasInteracted) {
                validateField(this);
            }
        });

        // Clear errors on input, but only validate if already interacted
        field.addEventListener('input', function () {
            clearFieldError(this);
            if (hasInteracted) {
                validateField(this);
            }
        });

        // Mark as interacted on first input
        field.addEventListener('input', function () {
            hasInteracted = true;
        }, { once: true });
    });

    // Form submission
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        if (validateForm()) {
            submitForm();
        }
    });
}

function validateField(field) {
    const value = field.value.trim();

    // Clear previous errors
    clearFieldError(field);

    // Required field validation
    if (field.hasAttribute('required') && !value) {
        showFieldError(field, 'This field is required');
        return false;
    }

    // Email validation
    if (field.type === 'email' && value && !isValidEmail(value)) {
        showFieldError(field, 'Please enter a valid email address');
        return false;
    }

    // Phone validation
    if (field.type === 'tel' && value && !isValidPhone(value)) {
        showFieldError(field, 'Please enter a valid phone number');
        return false;
    }

    // URL validation
    if (field.type === 'url' && value && !isValidUrl(value)) {
        showFieldError(field, 'Please enter a valid URL');
        return false;
    }

    // Number validation for population
    if (field.type === 'number' && value && isNaN(value)) {
        showFieldError(field, 'Please enter a valid number');
        return false;
    }

    // Coordinate validation
    if (field.name === 'Latitude' && value) {
        const lat = parseFloat(value);
        if (lat < -90 || lat > 90) {
            showFieldError(field, 'Latitude must be between -90 and 90');
            return false;
        }
    }

    if (field.name === 'Longitude' && value) {
        const lng = parseFloat(value);
        if (lng < -180 || lng > 180) {
            showFieldError(field, 'Longitude must be between -180 and 180');
            return false;
        }
    }

    // Show success state
    field.classList.add('success');
    return true;
}

function validateForm() {
    const form = document.querySelector('.add-business-form');
    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        if (!validateField(field)) {
            isValid = false;
        }
    });

    return isValid;
}

function showFieldError(field, message) {
    field.classList.add('error');
    field.classList.remove('success');

    // Use existing ASP.NET validation span or create new one
    let errorElement = field.parentNode.querySelector('.error-message');
    if (!errorElement) {
        errorElement = document.createElement('span');
        errorElement.className = 'error-message';
        field.parentNode.appendChild(errorElement);
    }
    errorElement.textContent = message;
}

function clearFieldError(field) {
    field.classList.remove('error');
    const errorMessage = field.parentNode.querySelector('.error-message');
    if (errorMessage) {
        errorMessage.textContent = '';
    }
}

function showError(message) {
    // You can implement a toast notification or alert here
    alert(message);
}

function showSuccess(message) {
    // You can implement a toast notification here
    alert(message);
}

// Validation Helpers
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isValidPhone(phone) {
    const phoneRegex = /^[\+]?[0-9\s\-\(\)]{10,}$/;
    return phoneRegex.test(phone);
}

function isValidUrl(url) {
    try {
        new URL(url);
        return true;
    } catch {
        return false;
    }
}

// Form Submission
function submitForm() {
    const form = document.querySelector('.add-business-form');
    const submitButton = form.querySelector('button[type="submit"]');

    // Show loading state
    submitButton.disabled = true;
    const isEdit = submitButton.textContent.includes('Update');

    submitButton.innerHTML = `
        <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
        </svg>
        ${isEdit ? 'Updating Town...' : 'Creating Town...'}
    `;

    // Submit the form
    form.submit();
}