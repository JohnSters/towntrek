/**
 * @fileoverview Business form management - replaces add-business.js
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * Business form manager - handles business creation and editing
 * @class
 */
class BusinessFormManager {
  /**
   * @constructor
   * @param {Object} options - Configuration options
   */
  constructor(options = {}) {
    this.options = {
      formSelector: options.mode === 'edit' ? '.edit-business-form' : '.add-business-form',
      mode: options.mode || 'create', // 'create' or 'edit'
      autoValidate: true,
      showNotifications: true,
      ...options
    };

    this.isInitialized = false;
    this.elements = {};
    this.handlers = {};
    this.validationRules = ValidationManager.getBusinessFormRules();

    this.init();
  }

  /**
   * Initialize the business form manager
   * @public
   */
  init() {
    if (this.isInitialized) return;

    try {
      this.cacheElements();
      this.initializeComponents();
      this.bindEvents();
      this.setupValidation();
      
      // Initialize for editing mode if needed
      if (this.options.mode === 'edit') {
        this.initializeEditMode();
      }

      this.isInitialized = true;
      console.log('✅ BusinessFormManager initialized successfully');
    } catch (error) {
      ErrorHandler.handle(error, 'BusinessFormManager initialization');
    }
  }

  /**
   * Cache DOM elements
   * @private
   */
  cacheElements() {
    this.elements = {
      form: document.querySelector(this.options.formSelector),
      // Use actual CTA button class from views: .auth-btn-cta
      submitBtn: document.querySelector('.auth-btn-cta'),
      categorySelect: document.getElementById('businessCategory'),
      subCategoryContainer: document.getElementById('subCategoryContainer'),
      subCategorySelect: document.getElementById('subCategory'),
      categorySpecificSections: document.getElementById('categorySpecificSections'),
      
      // File upload elements
      logoUpload: document.getElementById('businessLogo'),
      imagesUpload: document.getElementById('businessImages'),
      logoPreview: document.getElementById('logoPreview'),
      imagesPreview: document.getElementById('imagesPreview'),
      
      // Address validation
      addressInput: document.getElementById('physicalAddress'),
      // These elements do not exist in the current views; leave null-safe
      validateAddressBtn: document.getElementById('validateAddress'),
      addressValidationResult: document.getElementById('addressValidationResult'),
      // Hidden lat/long are rendered without IDs, only names
      latitudeInput: document.querySelector('input[name="Latitude"]'),
      longitudeInput: document.querySelector('input[name="Longitude"]'),
      
      // Operating hours
      dayCheckboxes: document.querySelectorAll('.day-checkbox'),
      
      // Existing images (for edit mode)
      existingImages: document.querySelectorAll('.current-image-item')
    };

    // Validate required elements
    if (!this.elements.form) {
      throw ErrorHandler.createError(
        `Business form not found: ${this.options.formSelector}`,
        'ELEMENT_NOT_FOUND'
      );
    }

    // Normalize category sections initial visibility
    this.hideAllCategorySections();
  }

  /**
   * Initialize components
   * @private
   */
  initializeComponents() {
    this.initializeCategoryHandling();
    this.initializeOperatingHours();
    this.initializeFileUploads();
    this.initializeAddressValidation();
    this.initializeExistingImageRemoval();
  }

  /**
   * Initialize category handling
   * @private
   */
  initializeCategoryHandling() {
    if (!this.elements.categorySelect) return;

    this.elements.categorySelect.addEventListener('change', async (event) => {
      const selectedCategory = event.target.value;
      
      try {
        // Hide all category-specific sections first
        this.hideAllCategorySections();

        if (selectedCategory) {
          // Show category-specific section
          this.showCategorySpecificSection(selectedCategory);

          // Load subcategories
          await this.loadSubCategories(selectedCategory);
        } else {
          this.hideSubCategoryContainer();
        }
      } catch (error) {
        ErrorHandler.handle(error, 'Category change handling');
      }
    });
  }

  /**
   * Hide all category-specific sections
   * @private
   */
  hideAllCategorySections() {
    const sections = document.querySelectorAll('.category-section');
    sections.forEach(section => {
      section.style.display = 'none';

      // Remove 'required' from all inputs in hidden sections
      const inputs = section.querySelectorAll('input, select, textarea');
      inputs.forEach(input => {
        if (input.hasAttribute('required')) {
          input.dataset.originalRequired = 'true';
          input.removeAttribute('required');
        }
      });
    });
  }

  /**
   * Show category-specific section
   * @private
   * @param {string} category - Selected category
   */
  showCategorySpecificSection(category) {
    const sectionMap = APP_CONFIG.business.categories;
    const sectionId = sectionMap[category];

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

        this.updateStepNumbers();
      } else {
        console.warn(`Category section not found: ${sectionId}`);
      }
    }
  }

  /**
   * Load subcategories for selected category
   * @private
   * @param {string} category - Selected category
   */
  async loadSubCategories(category) {
    if (!this.elements.subCategorySelect) return;

    try {
      const response = await ApiClient.get(API_ENDPOINTS.businesses.getSubCategories, { category });
      
      // Clear existing options
      this.elements.subCategorySelect.innerHTML = '<option value="">Select a subcategory (optional)</option>';

      // Add new options
      if (response && Array.isArray(response)) {
        response.forEach(subCat => {
          const option = document.createElement('option');
          option.value = subCat.value;
          option.textContent = subCat.text;
          this.elements.subCategorySelect.appendChild(option);
        });

        // Show subcategory container if there are options
        if (response.length > 0 && this.elements.subCategoryContainer) {
          this.elements.subCategoryContainer.style.display = 'block';
        }
      }
    } catch (error) {
      console.log('Subcategories not available:', error.message);
      this.hideSubCategoryContainer();
    }
  }

  /**
   * Hide subcategory container
   * @private
   */
  hideSubCategoryContainer() {
    if (this.elements.subCategoryContainer) {
      this.elements.subCategoryContainer.style.display = 'none';
    }
  }

  /**
   * Update step numbers for visible sections
   * @private
   */
  updateStepNumbers() {
       const visibleSections = Array.from(document.querySelectorAll('.form-section'))
         .filter(section => section.style.display !== 'none');
    let stepNumber = 1;

    visibleSections.forEach(section => {
      const stepElement = section.querySelector('.step-number');
      if (stepElement) {
        stepElement.textContent = stepNumber;
        stepNumber++;
      }
    });
  }

  /**
   * Initialize operating hours management
   * @private
   */
  initializeOperatingHours() {
    this.elements.dayCheckboxes.forEach(checkbox => {
      checkbox.addEventListener('change', (event) => {
        const dayGroup = event.target.closest('.day-hours-group');
        if (!dayGroup) return;
        const timeInputs = dayGroup.querySelectorAll('.time-input');

        timeInputs.forEach(input => {
          input.disabled = !event.target.checked;
          if (!event.target.checked) {
            input.value = '';
          } else {
            // Set default times when enabled
            if (input.name.includes('OpenTime') && !input.value) {
              input.value = APP_CONFIG.business.defaultOperatingHours.open;
            } else if (input.name.includes('CloseTime') && !input.value) {
              input.value = APP_CONFIG.business.defaultOperatingHours.close;
            }
          }
        });
      });

      // Initialize state on page load
       const dayGroup = checkbox.closest('.day-hours-group');
       if (!dayGroup) return;
       const timeInputs = dayGroup.querySelectorAll('.time-input');
      timeInputs.forEach(input => {
        input.disabled = !checkbox.checked;
      });
    });
  }

  /**
   * Initialize file uploads
   * @private
   */
  initializeFileUploads() {
    if (this.elements.logoUpload) {
      this.elements.logoUpload.addEventListener('change', (event) => {
        this.handleFilePreview(event.target, 'logoPreview', true);
      });
    }

    if (this.elements.imagesUpload) {
      this.elements.imagesUpload.addEventListener('change', (event) => {
        this.handleFilePreview(event.target, 'imagesPreview', false);
      });
    }
  }

  /**
   * Handle file preview
   * @private
   * @param {HTMLInputElement} input - File input element
   * @param {string} previewContainerId - Preview container ID
   * @param {boolean} isSingle - Whether single file upload
   */
  handleFilePreview(input, previewContainerId, isSingle) {
    const previewContainer = document.getElementById(previewContainerId);
    if (!previewContainer) return;

    // Reset previews for new selection
    previewContainer.innerHTML = '';

    if (input.files && input.files.length > 0) {
      Array.from(input.files).forEach((file, index) => {
        if (file.type.startsWith('image/')) {
          const reader = new FileReader();
          reader.onload = (e) => {
            const preview = this.createImagePreview(e.target.result, file, index);
            previewContainer.appendChild(preview);
          };
          reader.readAsDataURL(file);
        }
      });
    }

    // Delegate remove-clicks once per container
    previewContainer.onclick = (event) => {
      const btn = event.target.closest('.remove-preview-btn');
      if (!btn) return;
      const index = parseInt(btn.dataset.index, 10);
      if (Number.isFinite(index)) {
        this.removeFileFromInput(input, index);
        btn.closest('.image-preview-item')?.remove();
      }
    };
  }

  /**
   * Create image preview element
   * @private
   * @param {string} src - Image source
   * @param {File} file - File object
   * @param {number} index - File index
   * @returns {HTMLElement} Preview element
   */
  createImagePreview(src, file, index) {
    const preview = document.createElement('div');
    preview.className = 'image-preview-item';
    preview.innerHTML = `
      <div class="image-preview-wrapper">
        <img src="${src}" alt="Preview" class="image-preview-img">
        <div class="image-preview-overlay">
          <button type="button" class="remove-preview-btn" data-index="${index}">
            <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>
      </div>
      <div class="image-preview-info">
        <p class="image-preview-name">${Utils.escapeHtml(file.name)}</p>
        <p class="image-preview-size">${Utils.formatFileSize(file.size)}</p>
      </div>
    `;
    return preview;
  }

  /**
   * Remove file from input
   * @private
   * @param {HTMLInputElement} input - File input
   * @param {number} index - File index to remove
   */
  removeFileFromInput(input, index) {
    const dt = new DataTransfer();
    const { files } = input;

    for (let i = 0; i < files.length; i++) {
      if (i !== index) {
        dt.items.add(files[i]);
      }
    }

    input.files = dt.files;
  }

  /**
   * Initialize existing image removal (for edit mode)
   * @private
   */
  initializeExistingImageRemoval() {
    this.elements.existingImages.forEach(imageItem => {
      const removeBtn = imageItem.querySelector('.remove-image-btn');
      if (removeBtn) {
        removeBtn.addEventListener('click', (event) => {
          const imageId = event.target.dataset.imageId;
          
          if (confirm('Are you sure you want to remove this image?')) {
            // Add to hidden input for removal
            const removalInput = document.createElement('input');
            removalInput.type = 'hidden';
            removalInput.name = 'ImagesToRemove';
            removalInput.value = imageId;
            this.elements.form.appendChild(removalInput);

            // Remove from display
            imageItem.remove();

            if (this.options.showNotifications) {
              NotificationManager.info('Image marked for removal');
            }
          }
        });
      }
    });
  }

  /**
   * Initialize address validation
   * @private
   */
  initializeAddressValidation() {
    if (!this.elements.validateAddressBtn) return;

    this.elements.validateAddressBtn.addEventListener('click', async () => {
      const address = this.elements.addressInput.value.trim();
      if (!address) {
        NotificationManager.warning('Please enter an address first.');
        return;
      }

      try {
        this.setLoadingState(this.elements.validateAddressBtn, true);
        
        const response = await ApiClient.post(API_ENDPOINTS.validation.address, { address });

        if (response.isValid) {
          // Update hidden latitude/longitude fields
          if (this.elements.latitudeInput) {
            this.elements.latitudeInput.value = response.latitude;
          }
          if (this.elements.longitudeInput) {
            this.elements.longitudeInput.value = response.longitude;
          }

          this.showAddressValidationResult('Address validated successfully!', 'success');
          NotificationManager.success('Address validated successfully!');
        } else {
          this.showAddressValidationResult('Could not validate address. Please check and try again.', 'error');
          NotificationManager.error('Could not validate address. Please check and try again.');
        }
      } catch (error) {
        ErrorHandler.handle(error, 'Address validation');
        this.showAddressValidationResult('Error validating address. Please try again.', 'error');
      } finally {
        this.setLoadingState(this.elements.validateAddressBtn, false);
      }
    });
  }

  /**
   * Show address validation result
   * @private
   * @param {string} message - Result message
   * @param {string} type - Result type (success/error)
   */
  showAddressValidationResult(message, type) {
    if (this.elements.addressValidationResult) {
      this.elements.addressValidationResult.innerHTML = `
        <div class="alert alert-${type}">${Utils.escapeHtml(message)}</div>
      `;
      setTimeout(() => {
        this.elements.addressValidationResult.innerHTML = '';
      }, 5000);
    }
  }

  /**
   * Initialize edit mode
   * @private
   */
  initializeEditMode() {
    if (this.elements.categorySelect && this.elements.categorySelect.value) {
      console.log('Edit mode detected, initializing category:', this.elements.categorySelect.value);

      // Hide all sections first
      this.hideAllCategorySections();

      // Show the appropriate section for the selected category
      this.showCategorySpecificSection(this.elements.categorySelect.value);

      // Trigger subcategory loading
      const event = new Event('change');
      this.elements.categorySelect.dispatchEvent(event);
    }
  }

  /**
   * Bind event listeners
   * @private
   */
  bindEvents() {
    if (this.elements.form) {
      this.elements.form.addEventListener('submit', this.handleFormSubmit.bind(this));
    }
  }

  /**
   * Setup form validation
   * @private
   */
  setupValidation() {
    if (this.options.autoValidate && this.elements.form) {
      ValidationManager.setupFormValidation(
        this.elements.form,
        this.validationRules,
        {
          showErrorsOnBlur: true,
          showErrorsOnSubmit: true
        }
      );
    }
  }

  /**
   * Handle form submission
   * @private
   * @param {Event} event - Submit event
   */
  async handleFormSubmit(event) {
    event.preventDefault();

    try {
      // Ensure hidden fields are disabled before validation
      this.ensureHiddenFieldsAreDisabled();

      // Validate form
      const validation = ValidationManager.validateForm(
        this.elements.form,
        this.validationRules
      );

      if (!validation.isValid) {
        return false;
      }

      // Additional business-specific validation
      if (!this.validateOperatingHours()) {
        return false;
      }

      // Show loading state
      this.setLoadingState(this.elements.submitBtn, true);

      // Submit the form via standard POST so MVC can handle redirects/validation
      // This avoids relying on a JSON API response for traditional Razor actions
      this.elements.form.submit();
      return true;
    } catch (error) {
      ErrorHandler.handle(error, 'Business form submission', {
        userMessage: 'Failed to save business. Please try again.'
      });
    } finally {
      this.setLoadingState(this.elements.submitBtn, false);
    }
  }

  /**
   * Ensure hidden fields are disabled
   * @private
   */
  ensureHiddenFieldsAreDisabled() {
    const hiddenSections = document.querySelectorAll('.category-section[style*="display: none"]');
    hiddenSections.forEach(section => {
      const requiredFields = section.querySelectorAll('[required]');
      requiredFields.forEach(field => {
        field.removeAttribute('required');
      });

      // Ensure hidden file inputs do not contribute undefined strings to validation
      const fileInputs = section.querySelectorAll('input[type="file"]');
      fileInputs.forEach(input => {
        // No value to clear for security reasons; just ensure not required
        input.removeAttribute('required');
      });
    });
  }

  /**
   * Validate operating hours
   * @private
   * @returns {boolean} Validation result
   */
  validateOperatingHours() {
    const operatingDays = document.querySelectorAll('.day-checkbox:checked');
    
    if (operatingDays.length === 0) {
      NotificationManager.error('Please select at least one operating day.');
      return false;
    }

    // Validate that checked days have both open and close times
    for (let dayCheckbox of operatingDays) {
      const dayGroup = dayCheckbox.closest('.day-hours-group');
       const openTimeInput = dayGroup.querySelector('input[name*="OpenTime"]');
       const closeTimeInput = dayGroup.querySelector('input[name*="CloseTime"]');

      if (!openTimeInput.value || !closeTimeInput.value) {
        const dayName = dayGroup.querySelector('.day-label').textContent;
        NotificationManager.error(`Please set both opening and closing times for ${dayName}.`);
        return false;
      }
    }

    return true;
  }

  /**
   * Set loading state for button
   * @private
   * @param {HTMLElement} button - Button element
   * @param {boolean} isLoading - Loading state
   */
  setLoadingState(button, isLoading) {
    if (!button) return;

    if (isLoading) {
      button.disabled = true;
      if (!button.dataset.originalText) {
        button.dataset.originalText = button.textContent || '';
      }
      const labelSpan = button.querySelector('span');
      if (labelSpan) {
        labelSpan.textContent = 'Processing...';
      } else {
        button.textContent = 'Processing...';
      }
      button.classList.add('loading');
    } else {
      button.disabled = false;
      const original = button.dataset.originalText || 'Save Business';
      const labelSpan = button.querySelector('span');
      if (labelSpan) {
        labelSpan.textContent = original;
      } else {
        button.textContent = original;
      }
      button.classList.remove('loading');
    }
  }

  /**
   * Handle successful submission
   * @private
   * @param {Object} result - Success result
   */
  handleSuccess(result) {
    if (this.options.showNotifications) {
      const message = this.options.mode === 'edit' 
        ? 'Business updated successfully!' 
        : 'Business created successfully!';
      NotificationManager.success(message);
    }

    // Redirect if URL provided
    if (result.redirectUrl) {
      setTimeout(() => {
        window.location.href = result.redirectUrl;
      }, 1500);
    }
  }

  /**
   * Handle submission error
   * @private
   * @param {Error} error - Error object
   */
  handleError(error) {
    ErrorHandler.handle(error, 'Business form submission');
  }

  /**
   * Destroy the manager and cleanup
   * @public
   */
  destroy() {
    // Remove event listeners
    if (this.elements.form) {
      this.elements.form.removeEventListener('submit', this.handleFormSubmit);
    }

    // Clear references
    this.elements = {};
    this.handlers = {};
    this.isInitialized = false;
  }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = BusinessFormManager;
}

// Global registration
window.BusinessFormManager = BusinessFormManager;

// Note: Initialization is handled by core/app.js to avoid double-binding events