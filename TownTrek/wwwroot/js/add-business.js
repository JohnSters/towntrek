// Add Business Form JavaScript
document.addEventListener('DOMContentLoaded', function() {
    initializeAddBusinessForm();
});

function initializeAddBusinessForm() {
    // Initialize form components
    initializeCategoryHandling();
    initializeOperatingHours();
    initializeFileUploads();
    initializeAddressValidation();
    initializeFormValidation();
}

// Category and subcategory handling
function initializeCategoryHandling() {
    const categorySelect = document.getElementById('businessCategory');
    const subCategoryContainer = document.getElementById('subCategoryContainer');
    const subCategorySelect = document.getElementById('subCategory');

    if (categorySelect) {
        categorySelect.addEventListener('change', async function() {
            const selectedCategory = this.value;
            
            if (selectedCategory) {
                try {
                    const response = await fetch(`/Client/GetSubCategories?category=${selectedCategory}`);
                    const subCategories = await response.json();
                    
                    // Clear existing options
                    subCategorySelect.innerHTML = '<option value="">Select a subcategory (optional)</option>';
                    
                    // Add new options
                    subCategories.forEach(subCat => {
                        const option = document.createElement('option');
                        option.value = subCat.value;
                        option.textContent = subCat.text;
                        subCategorySelect.appendChild(option);
                    });
                    
                    // Show subcategory container if there are options
                    if (subCategories.length > 0) {
                        subCategoryContainer.style.display = 'block';
                    } else {
                        subCategoryContainer.style.display = 'none';
                    }
                } catch (error) {
                    console.error('Error loading subcategories:', error);
                }
            } else {
                subCategoryContainer.style.display = 'none';
            }
        });
    }
}

// Operating hours management
function initializeOperatingHours() {
    const dayCheckboxes = document.querySelectorAll('.day-checkbox');
    
    dayCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const dayName = this.value.toLowerCase();
            const timeInputs = document.querySelectorAll(`input[name="${dayName}Open"], input[name="${dayName}Close"]`);
            
            timeInputs.forEach(input => {
                input.disabled = !this.checked;
                if (!this.checked) {
                    input.value = '';
                }
            });
        });
    });
}

// File upload handling
function initializeFileUploads() {
    const logoUpload = document.getElementById('businessLogo');
    const imagesUpload = document.getElementById('businessImages');
    
    if (logoUpload) {
        logoUpload.addEventListener('change', function() {
            handleFilePreview(this, 'logoPreview', true);
        });
    }
    
    if (imagesUpload) {
        imagesUpload.addEventListener('change', function() {
            handleFilePreview(this, 'imagesPreview', false);
        });
    }
}

function handleFilePreview(input, previewContainerId, isSingle) {
    const previewContainer = document.getElementById(previewContainerId);
    if (!previewContainer) return;
    
    previewContainer.innerHTML = '';
    
    if (input.files && input.files.length > 0) {
        Array.from(input.files).forEach((file, index) => {
            if (file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const preview = document.createElement('div');
                    preview.className = 'file-preview';
                    preview.innerHTML = `
                        <img src="${e.target.result}" alt="Preview" style="max-width: 100px; max-height: 100px; object-fit: cover;">
                        <p>${file.name}</p>
                    `;
                    previewContainer.appendChild(preview);
                };
                reader.readAsDataURL(file);
            }
        });
    }
}

// Address validation and geocoding
function initializeAddressValidation() {
    const addressInput = document.getElementById('physicalAddress');
    const validateButton = document.getElementById('validateAddress');
    
    if (validateButton) {
        validateButton.addEventListener('click', async function() {
            const address = addressInput.value.trim();
            if (!address) {
                alert('Please enter an address first.');
                return;
            }
            
            try {
                const response = await fetch('/Client/ValidateAddress', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ address: address })
                });
                
                const result = await response.json();
                
                if (result.isValid) {
                    // Update hidden latitude/longitude fields
                    document.getElementById('latitude').value = result.latitude;
                    document.getElementById('longitude').value = result.longitude;
                    
                    // Show success message
                    showAddressValidationResult('Address validated successfully!', 'success');
                } else {
                    showAddressValidationResult('Could not validate address. Please check and try again.', 'error');
                }
            } catch (error) {
                console.error('Address validation error:', error);
                showAddressValidationResult('Error validating address. Please try again.', 'error');
            }
        });
    }
}

function showAddressValidationResult(message, type) {
    const resultDiv = document.getElementById('addressValidationResult');
    if (resultDiv) {
        resultDiv.innerHTML = `<div class="alert alert-${type}">${message}</div>`;
        setTimeout(() => {
            resultDiv.innerHTML = '';
        }, 5000);
    }
}

// Form validation
function initializeFormValidation() {
    const form = document.querySelector('.add-business-form');
    
    if (form) {
        form.addEventListener('submit', function(e) {
            if (!validateForm()) {
                e.preventDefault();
                return false;
            }
        });
    }
}

function validateForm() {
    let isValid = true;
    const requiredFields = document.querySelectorAll('[required]');
    
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            showFieldError(field, 'This field is required.');
            isValid = false;
        } else {
            clearFieldError(field);
        }
    });
    
    // Validate at least one operating day is selected
    const operatingDays = document.querySelectorAll('.day-checkbox:checked');
    if (operatingDays.length === 0) {
        alert('Please select at least one operating day.');
        isValid = false;
    }
    
    return isValid;
}

function showFieldError(field, message) {
    clearFieldError(field);
    
    const errorDiv = document.createElement('div');
    errorDiv.className = 'field-error';
    errorDiv.textContent = message;
    
    field.parentNode.appendChild(errorDiv);
    field.classList.add('error');
}

function clearFieldError(field) {
    const existingError = field.parentNode.querySelector('.field-error');
    if (existingError) {
        existingError.remove();
    }
    field.classList.remove('error');
}

// Utility functions
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 5000);
}

// Initialize Google Maps (if API key is available)
function initializeGoogleMaps() {
    // This would be implemented when Google Maps API is integrated
    // For now, we'll use the address validation endpoint
}