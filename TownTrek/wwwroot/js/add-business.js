// Add Business Form JavaScript
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing add business form...');
    initializeAddBusinessForm();
});

function initializeAddBusinessForm() {
    console.log('Initializing form components...');

    // Initialize form components
    initializeCategoryHandling();
    initializeOperatingHours();
    initializeFileUploads();
    initializeAddressValidation();
    initializeFormValidation();

    console.log('Form initialization complete');

    // Test category sections exist
    testCategorySections();
}

function testCategorySections() {
    const sections = ['marketSection', 'tourSection', 'eventSection', 'restaurantSection', 'accommodationSection'];
    console.log('Testing category sections...');

    sections.forEach(sectionId => {
        const section = document.getElementById(sectionId);
        console.log(`${sectionId}: ${section ? 'Found' : 'NOT FOUND'}`);
    });
}

// Category and subcategory handling
function initializeCategoryHandling() {
    const categorySelect = document.getElementById('businessCategory');
    const subCategoryContainer = document.getElementById('subCategoryContainer');
    const subCategorySelect = document.getElementById('subCategory');

    console.log('Category elements found:', {
        categorySelect: !!categorySelect,
        subCategoryContainer: !!subCategoryContainer,
        subCategorySelect: !!subCategorySelect
    });

    if (categorySelect) {
        categorySelect.addEventListener('change', async function () {
            const selectedCategory = this.value;

            // Hide all category-specific sections first
            hideAllCategorySections();

            if (selectedCategory) {
                try {
                    console.log('Fetching subcategories for:', selectedCategory);
                    const response = await fetch(`/Client/GetSubCategories?category=${selectedCategory}`);
                    console.log('Response status:', response.status);

                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }

                    const subCategories = await response.json();
                    console.log('Subcategories received:', subCategories);

                    // Clear existing options (with null check)
                    if (subCategorySelect) {
                        subCategorySelect.innerHTML = '<option value="">Select a subcategory (optional)</option>';

                        // Add new options
                        subCategories.forEach(subCat => {
                            const option = document.createElement('option');
                            option.value = subCat.value;
                            option.textContent = subCat.text;
                            subCategorySelect.appendChild(option);
                        });
                    }

                    // Show subcategory container if there are options (with null check)
                    if (subCategoryContainer) {
                        if (subCategories.length > 0) {
                            subCategoryContainer.style.display = 'block';
                        } else {
                            subCategoryContainer.style.display = 'none';
                        }
                    }

                    // Show category-specific section
                    showCategorySpecificSection(selectedCategory);

                } catch (error) {
                    console.error('Error loading subcategories:', error);
                }
            } else {
                if (subCategoryContainer) {
                    subCategoryContainer.style.display = 'none';
                }
            }
        });
    }
}

function hideAllCategorySections () {
    const sections = document.querySelectorAll('.category-section');
    sections.forEach(section => {
        section.style.display = 'none';

        // Remove 'required' from all inputs in hidden sections
        const inputs = section.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            if (input.hasAttribute('required')) {
                input.dataset.originalRequired = 'true'; // store original state
                input.removeAttribute('required');
            }
        });
    });
}

function showCategorySpecificSection (category) {
    hideAllCategorySections(); // make sure others are hidden

    const sectionMap = {
        'markets-vendors': 'marketSection',
        'tours-experiences': 'tourSection',
        'events': 'eventSection',
        'restaurants-food': 'restaurantSection',
        'accommodation': 'accommodationSection'
    };

    const sectionId = sectionMap[ category ];
    if (sectionId) {
        const section = document.getElementById(sectionId);
        if (section) {
            section.style.display = 'block';

            // Restore 'required' attributes
            const inputs = section.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (input.dataset.originalRequired === 'true') {
                    input.setAttribute('required', '');
                }
            });

            updateStepNumbers();
        }
    }
}

function updateStepNumbers() {
    const visibleSections = document.querySelectorAll('.form-section:not([style*="display: none"])');
    let stepNumber = 1;

    visibleSections.forEach(section => {
        const stepElement = section.querySelector('.step-number');
        if (stepElement) {
            stepElement.textContent = stepNumber;
            stepNumber++;
        }
    });
}

// Operating hours management
function initializeOperatingHours() {
    const dayCheckboxes = document.querySelectorAll('.day-checkbox');

    dayCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function () {
            const dayName = this.value;
            const timeInputs = document.querySelectorAll(`input[name="${dayName}Open"], input[name="${dayName}Close"]`);

            timeInputs.forEach(input => {
                input.disabled = !this.checked;
                if (!this.checked) {
                    input.value = '';
                } else {
                    // Set default times when enabled
                    if (input.name.includes('Open') && !input.value) {
                        input.value = '09:00';
                    } else if (input.name.includes('Close') && !input.value) {
                        input.value = '17:00';
                    }
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
        logoUpload.addEventListener('change', function () {
            handleFilePreview(this, 'logoPreview', true);
        });
    }

    if (imagesUpload) {
        imagesUpload.addEventListener('change', function () {
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
                reader.onload = function (e) {
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
        validateButton.addEventListener('click', async function () {
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
        form.addEventListener('submit', function (e) {
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