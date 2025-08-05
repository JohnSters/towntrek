// Add Business Form JavaScript
document.addEventListener('DOMContentLoaded', function() {
    initializeOperatingHours();
    initializeFileUploads();
    initializeFormValidation();
});

// Operating Hours Functionality
function initializeOperatingHours() {
    const dayCheckboxes = document.querySelectorAll('.day-checkbox');
    
    dayCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const dayName = this.value.toLowerCase();
            const openInput = document.querySelector(`input[name="${dayName}Open"]`);
            const closeInput = document.querySelector(`input[name="${dayName}Close"]`);
            
            if (this.checked) {
                openInput.disabled = false;
                closeInput.disabled = false;
                openInput.required = true;
                closeInput.required = true;
            } else {
                openInput.disabled = true;
                closeInput.disabled = true;
                openInput.required = false;
                closeInput.required = false;
                openInput.value = '';
                closeInput.value = '';
            }
        });
    });
}

// File Upload Functionality
function initializeFileUploads() {
    const logoUpload = document.getElementById('businessLogo');
    const imagesUpload = document.getElementById('businessImages');
    
    // Logo upload
    logoUpload.addEventListener('change', function(e) {
        handleFileUpload(e, 'logo');
    });
    
    // Images upload
    imagesUpload.addEventListener('change', function(e) {
        handleFileUpload(e, 'images');
    });
    
    // Drag and drop functionality
    const uploadAreas = document.querySelectorAll('.file-upload-area');
    uploadAreas.forEach(area => {
        area.addEventListener('dragover', handleDragOver);
        area.addEventListener('dragleave', handleDragLeave);
        area.addEventListener('drop', handleDrop);
    });
}

function handleFileUpload(event, type) {
    const files = event.target.files;
    const uploadArea = event.target.closest('.file-upload-area');
    
    if (files.length > 0) {
        updateUploadAreaDisplay(uploadArea, files, type);
        validateFileSize(files);
    }
}

function updateUploadAreaDisplay(uploadArea, files, type) {
    const uploadContent = uploadArea.querySelector('.file-upload-content');
    const fileCount = files.length;
    
    if (type === 'logo' && fileCount === 1) {
        uploadContent.innerHTML = `
            <svg width="48" height="48" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="upload-icon">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <p class="upload-text">Logo uploaded: ${files[0].name}</p>
            <p class="upload-hint">Click to change</p>
        `;
        uploadArea.style.borderColor = '#28a745';
        uploadArea.style.backgroundColor = '#f8f9fa';
    } else if (type === 'images' && fileCount > 0) {
        uploadContent.innerHTML = `
            <svg width="48" height="48" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="upload-icon">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <p class="upload-text">${fileCount} image${fileCount > 1 ? 's' : ''} uploaded</p>
            <p class="upload-hint">Click to change or add more</p>
        `;
        uploadArea.style.borderColor = '#28a745';
        uploadArea.style.backgroundColor = '#f8f9fa';
    }
}

function validateFileSize(files) {
    const maxSize = 2 * 1024 * 1024; // 2MB
    let hasError = false;
    
    Array.from(files).forEach(file => {
        if (file.size > maxSize) {
            showError(`File "${file.name}" is too large. Maximum size is 2MB.`);
            hasError = true;
        }
    });
    
    return !hasError;
}

// Drag and Drop Handlers
function handleDragOver(e) {
    e.preventDefault();
    e.currentTarget.style.borderColor = '#86bbd8';
    e.currentTarget.style.backgroundColor = '#f8f9fa';
}

function handleDragLeave(e) {
    e.preventDefault();
    e.currentTarget.style.borderColor = '#e9ecef';
    e.currentTarget.style.backgroundColor = 'transparent';
}

function handleDrop(e) {
    e.preventDefault();
    const uploadArea = e.currentTarget;
    const fileInput = uploadArea.querySelector('.file-input');
    const files = e.dataTransfer.files;
    
    // Reset styles
    uploadArea.style.borderColor = '#e9ecef';
    uploadArea.style.backgroundColor = 'transparent';
    
    if (files.length > 0) {
        fileInput.files = files;
        const event = new Event('change', { bubbles: true });
        fileInput.dispatchEvent(event);
    }
}

// Form Validation
function initializeFormValidation() {
    const form = document.querySelector('.add-business-form');
    const requiredFields = form.querySelectorAll('[required]');
    
    // Real-time validation
    requiredFields.forEach(field => {
        field.addEventListener('blur', function() {
            validateField(this);
        });
        
        field.addEventListener('input', function() {
            clearFieldError(this);
        });
    });
    
    // Form submission
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        if (validateForm()) {
            submitForm();
        }
    });
}

function validateField(field) {
    const value = field.value.trim();
    const fieldName = field.name;
    
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
    
    // Validate operating hours
    if (!validateOperatingHours()) {
        isValid = false;
        showError('Please select at least one operating day with valid hours');
    }
    
    return isValid;
}

function validateOperatingHours() {
    const checkedDays = document.querySelectorAll('.day-checkbox:checked');
    
    if (checkedDays.length === 0) {
        return false;
    }
    
    let hasValidHours = true;
    checkedDays.forEach(checkbox => {
        const dayName = checkbox.value.toLowerCase();
        const openInput = document.querySelector(`input[name="${dayName}Open"]`);
        const closeInput = document.querySelector(`input[name="${dayName}Close"]`);
        
        if (!openInput.value || !closeInput.value) {
            hasValidHours = false;
        }
    });
    
    return hasValidHours;
}

function showFieldError(field, message) {
    field.classList.add('error');
    field.classList.remove('success');
    
    // Remove existing error message
    const existingError = field.parentNode.querySelector('.error-message');
    if (existingError) {
        existingError.remove();
    }
    
    // Add new error message
    const errorElement = document.createElement('span');
    errorElement.className = 'error-message';
    errorElement.textContent = message;
    field.parentNode.appendChild(errorElement);
}

function clearFieldError(field) {
    field.classList.remove('error');
    const errorMessage = field.parentNode.querySelector('.error-message');
    if (errorMessage) {
        errorMessage.remove();
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
    submitButton.innerHTML = `
        <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
        </svg>
        Creating Business...
    `;
    
    // Submit the form
    form.submit();
}