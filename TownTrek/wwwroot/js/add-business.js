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

    // Initialize for editing mode if category is pre-selected
    initializeEditingMode();

    // Test category sections exist
    testCategorySections();
}

function testCategorySections() {
    const sections = ['marketSection', 'tourSection', 'eventSection', 'restaurantSection', 'accommodationSection'];
    console.log('Testing category sections...');

    sections.forEach(sectionId => {
        const section = document.getElementById(sectionId);
        console.log(`${sectionId}: ${section ? 'Found' : 'NOT FOUND'}`);
        if (section) {
            console.log(`  - Current display style: ${section.style.display}`);
            console.log(`  - Computed display: ${window.getComputedStyle(section).display}`);
        }
    });
    
    // Also check if the container exists
    const container = document.getElementById('categorySpecificSections');
    console.log(`categorySpecificSections container: ${container ? 'Found' : 'NOT FOUND'}`);
    if (container) {
        console.log(`  - Container children count: ${container.children.length}`);
    }
}

function initializeEditingMode() {
    const categorySelect = document.getElementById('businessCategory');
    if (categorySelect && categorySelect.value) {
        console.log('Editing mode detected, initializing category:', categorySelect.value);
        
        // Hide all sections first
        hideAllCategorySections();
        
        // Show the appropriate section for the selected category
        showCategorySpecificSection(categorySelect.value);
        
        // Trigger subcategory loading
        const event = new Event('change');
        categorySelect.dispatchEvent(event);
    }
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
                // Show category-specific section immediately
                showCategorySpecificSection(selectedCategory);
                
                // Try to load subcategories (if endpoint exists)
                try {
                    console.log('Fetching subcategories for:', selectedCategory);
                    const response = await fetch(`/Client/GetSubCategories?category=${selectedCategory}`);
                    
                    if (response.ok) {
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
                        if (subCategoryContainer && subCategories.length > 0) {
                            subCategoryContainer.style.display = 'block';
                        }
                    } else {
                        console.log('Subcategories endpoint not available or returned error:', response.status);
                        // Hide subcategory container if endpoint fails
                        if (subCategoryContainer) {
                            subCategoryContainer.style.display = 'none';
                        }
                    }
                } catch (error) {
                    console.log('Subcategories not available:', error.message);
                    // Hide subcategory container if fetch fails
                    if (subCategoryContainer) {
                        subCategoryContainer.style.display = 'none';
                    }
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
    console.log('showCategorySpecificSection called with category:', category);
    hideAllCategorySections(); // make sure others are hidden

    const sectionMap = {
        'shops-retail': 'shopsSection',
        'markets-vendors': 'marketSection',
        'tours-experiences': 'tourSection',
        'events': 'eventSection',
        'restaurants-food': 'restaurantSection',
        'accommodation': 'accommodationSection'
    };

    const sectionId = sectionMap[category];
    console.log('Mapped category to sectionId:', sectionId);
    
    if (sectionId) {
        const section = document.getElementById(sectionId);
        console.log('Found section element:', !!section);
        
        if (section) {
            console.log('Showing section:', sectionId);
            console.log('Section before show:', section.style.display);
            section.style.display = 'block';
            console.log('Section after show:', section.style.display);

            // Restore 'required' attributes
            const inputs = section.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (input.dataset.originalRequired === 'true') {
                    input.setAttribute('required', '');
                }
            });

            updateStepNumbers();
        } else {
            console.error('Section element not found:', sectionId);
            console.log('Available elements with IDs:', Array.from(document.querySelectorAll('[id]')).map(el => el.id));
        }
    } else {
        console.log('No section mapping found for category:', category);
        console.log('Available categories:', Object.keys(sectionMap));
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

    dayCheckboxes.forEach((checkbox, index) => {
        checkbox.addEventListener('change', function () {
            const dayGroup = this.closest('.day-hours-group');
            const timeInputs = dayGroup.querySelectorAll('.time-input');

            timeInputs.forEach(input => {
                input.disabled = !this.checked;
                if (!this.checked) {
                    input.value = '';
                } else {
                    // Set default times when enabled
                    if (input.name.includes('OpenTime') && !input.value) {
                        input.value = '09:00';
                    } else if (input.name.includes('CloseTime') && !input.value) {
                        input.value = '17:00';
                    }
                }
            });
        });

        // Initialize state on page load
        const dayGroup = checkbox.closest('.day-hours-group');
        const timeInputs = dayGroup.querySelectorAll('.time-input');
        timeInputs.forEach(input => {
            input.disabled = !checkbox.checked;
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

    // Initialize existing image removal functionality for EditBusiness form
    initializeExistingImageRemoval();
}

function initializeExistingImageRemoval() {
    const removeButtons = document.querySelectorAll('.remove-image-btn');
    
    removeButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            const imageId = this.dataset.imageId;
            const imageItem = this.closest('.current-image-item');
            
            if (confirm('Are you sure you want to remove this image?')) {
                // Add to hidden input for removal
                const removalInput = document.createElement('input');
                removalInput.type = 'hidden';
                removalInput.name = 'ImagesToRemove';
                removalInput.value = imageId;
                document.querySelector('form').appendChild(removalInput);
                
                // Remove from display
                imageItem.remove();
                
                // Show notification
                showNotification('Image marked for removal', 'info');
            }
        });
    });
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
                    preview.className = 'image-preview-item';
                    preview.innerHTML = `
                        <div class="image-preview-wrapper">
                            <img src="${e.target.result}" alt="Preview" class="image-preview-img">
                            <div class="image-preview-overlay">
                                <button type="button" class="remove-preview-btn" data-index="${index}">
                                    <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                                    </svg>
                                </button>
                            </div>
                        </div>
                        <div class="image-preview-info">
                            <p class="image-preview-name">${file.name}</p>
                            <p class="image-preview-size">${formatFileSize(file.size)}</p>
                        </div>
                    `;
                    previewContainer.appendChild(preview);
                };
                reader.readAsDataURL(file);
            }
        });

        // Add event listeners for remove buttons
        previewContainer.querySelectorAll('.remove-preview-btn').forEach(btn => {
            btn.addEventListener('click', function() {
                const index = parseInt(this.dataset.index);
                removeFileFromInput(input, index);
                this.closest('.image-preview-item').remove();
            });
        });
    }
}

function removeFileFromInput(input, index) {
    const dt = new DataTransfer();
    const { files } = input;
    
    for (let i = 0; i < files.length; i++) {
        if (i !== index) {
            dt.items.add(files[i]);
        }
    }
    
    input.files = dt.files;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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
            console.log('Form submission started...');
            
            // Ensure all hidden required fields are disabled before submission
            ensureHiddenFieldsAreDisabled();
            
            if (!validateForm()) {
                console.log('Form validation failed');
                e.preventDefault();
                return false;
            }
            
            console.log('Form validation passed, submitting...');
        });
    }
}

function ensureHiddenFieldsAreDisabled() {
    const hiddenSections = document.querySelectorAll('.category-section[style*="display: none"]');
    hiddenSections.forEach(section => {
        const requiredFields = section.querySelectorAll('[required]');
        requiredFields.forEach(field => {
            field.removeAttribute('required');
            console.log('Removed required attribute from hidden field:', field.name);
        });
    });
}

function validateForm() {
    let isValid = true;
    
    // Only validate required fields that are visible (not in hidden sections)
    const requiredFields = document.querySelectorAll('[required]');

    requiredFields.forEach(field => {
        // Check if the field is in a visible section
        const isVisible = field.offsetParent !== null;
        
        if (isVisible && !field.value.trim()) {
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

    // Validate that checked days have both open and close times
    operatingDays.forEach(dayCheckbox => {
        const dayGroup = dayCheckbox.closest('.day-hours-group');
        const openTimeInput = dayGroup.querySelector('input[name*="OpenTime"]');
        const closeTimeInput = dayGroup.querySelector('input[name*="CloseTime"]');
        
        if (!openTimeInput.value || !closeTimeInput.value) {
            const dayName = dayGroup.querySelector('.day-label').textContent;
            alert(`Please set both opening and closing times for ${dayName}.`);
            isValid = false;
        }
    });

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