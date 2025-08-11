// Edit Business Form JavaScript
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing edit business form...');
    initializeEditBusinessForm();
});

function initializeEditBusinessForm() {
    console.log('Initializing edit form components...');

    // Initialize form components (reuse from add-business functionality)
    initializeCategoryHandling();
    initializeOperatingHours();
    initializeFileUploads();
    initializeAddressValidation();
    initializeFormValidation();

    // Initialize edit-specific functionality
    initializeEditMode();
    initializeExistingImageRemoval();

    console.log('Edit form initialization complete');
}

function initializeEditMode() {
    const categorySelect = document.getElementById('businessCategory');
    
    if (categorySelect && categorySelect.value) {
        console.log('Edit mode detected, initializing category:', categorySelect.value);
        
        // Hide all sections first
        hideAllCategorySections();
        
        // Show the appropriate section for the selected category
        showCategorySpecificSection(categorySelect.value);
        
        // Load subcategories and set current value
        loadSubcategoriesForEdit(categorySelect.value);
    }
}

async function loadSubcategoriesForEdit(category) {
    try {
        console.log('Loading subcategories for edit mode:', category);
        const response = await fetch(`/Client/GetSubCategories?category=${category}`);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const subCategories = await response.json();
        console.log('Subcategories loaded:', subCategories);

        const subCategorySelect = document.getElementById('subCategory');
        const subCategoryContainer = document.getElementById('subCategoryContainer');
        
        if (subCategorySelect && subCategories.length > 0) {
            // Clear existing options
            subCategorySelect.innerHTML = '<option value="">Select a subcategory (optional)</option>';
            
            // Add new options
            subCategories.forEach(subCat => {
                const option = document.createElement('option');
                option.value = subCat.value;
                option.textContent = subCat.text;
                subCategorySelect.appendChild(option);
            });
            
            // Set current subcategory value if it exists
            const currentSubCategory = subCategorySelect.dataset.currentValue;
            if (currentSubCategory) {
                subCategorySelect.value = currentSubCategory;
            }
            
            // Show subcategory container
            if (subCategoryContainer) {
                subCategoryContainer.style.display = 'block';
            }
        }
    } catch (error) {
        console.error('Error loading subcategories for edit:', error);
    }
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

// Utility function to show notifications
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 5000);
}