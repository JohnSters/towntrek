/**
 * @fileoverview Public browsing manager for TownTrek
 * Handles public browsing/search; enables member-only actions (favorites, reviews) when signed in
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class PublicManager {
  constructor() {
    this.apiClient = window.ApiClient;
    this.notifications = window.NotificationManager;
    this.modalComponent = window.ModalComponent;
    this.init();
  }

  init() {
    this.bindEventListeners();
    this.initializeCurrentPage();
    this.initializeReviewResponses();
  }

  bindEventListeners() {
    // Global event delegation for public features
    document.addEventListener('click', this.handleClick.bind(this));
    document.addEventListener('submit', this.handleSubmit.bind(this));
    document.addEventListener('change', this.handleChange.bind(this));
  }

  handleClick(e) {
    const target = e.target.closest('[data-public-action]');
    if (!target) return;

    const action = target.dataset.publicAction;
    e.preventDefault();

    switch (action) {
      case 'toggle-favorite':
        this.toggleFavorite(target);
        break;
      case 'submit-review':
        this.showReviewModal(target);
        break;
      case 'apply-filters':
        this.applyFilters();
        break;
      case 'clear-filters':
        this.clearFilters();
        break;
      case 'remove-filter':
        this.removeFilter(target.dataset.filter);
        break;
      case 'load-page':
        this.loadPage(parseInt(target.dataset.page));
        break;
    }
  }

  handleSubmit(e) {
    if (e.target.matches('[data-public-form]')) {
      e.preventDefault();
      const formType = e.target.dataset.publicForm;

      switch (formType) {
        case 'review':
          this.submitReview(e.target);
          break;
        case 'search':
          this.performSearch(e.target);
          break;
      }
    }
  }

  handleChange(e) {
    if (e.target.matches('[data-public-filter]')) {
      const filterType = e.target.dataset.publicFilter;

      switch (filterType) {
        case 'category':
          this.updateSubCategories(e.target.value);
          break;
      }
    }
  }

  initializeCurrentPage() {
    // Initialize favorite states for all business cards on page load
    this.initializeFavoriteStates();

    // Prefer explicit body data attribute if set
    const explicitPage = document.body.dataset.page;
    if (explicitPage) {
      this.routeToPage(explicitPage);
      return;
    }

    // Fallback to DOM-based detection (no inline scripts required in views)
    if (document.querySelector('.public-dashboard-page')) {
      this.routeToPage('public-dashboard');
    } else if (document.querySelector('.business-search-page')) {
      this.routeToPage('public-search');
    } else if (document.querySelector('.favorites-page')) {
      this.routeToPage('public-favorites');
    } else if (document.querySelector('.town-businesses-page')) {
      this.routeToPage('public-town-businesses');
    } else if (document.querySelector('.business-details-page')) {
      this.routeToPage('public-business-details');
    }
  }

  routeToPage(pageKey) {
    switch (pageKey) {
      case 'public-dashboard':
        this.initializeDashboard();
        break;
      case 'public-town-businesses':
        this.initializeTownBusinesses();
        break;
      case 'public-search':
        this.initializeSearch();
        break;
      case 'public-business-details':
        this.initializeBusinessDetails();
        break;
      case 'public-favorites':
        this.initializeFavorites();
        break;
    }
  }

  initializeDashboard() {
    // Initialize search functionality
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
      searchInput.addEventListener('focus', function () {
        this.parentElement.classList.add('focused');
      });

      searchInput.addEventListener('blur', function () {
        this.parentElement.classList.remove('focused');
      });
    }
  }

  initializeTownBusinesses() {
    // Use global categories data if available
    this.categoriesData = window.PublicCategoriesData || [];
    this.townId = window.PublicTownId;

    // Initialize selected filters
    this.initializeFilters();
  }

  initializeSearch() {
    // Load categories data
    this.loadCategoriesData();

    // Initialize selected filters
    this.initializeFilters();
  }

  initializeBusinessDetails() {
    // Initialize image gallery if exists
    this.initializeImageGallery();
  }

  initializeFavorites() {
    // Initialize filtering and sorting
    this.initializeFavoritesFiltering();
  }

  async toggleFavorite(button) {
    const businessId = button.dataset.businessId;
    const icon = button.querySelector('svg');

    try {
      const response = await this.apiClient.post('/Public/ToggleFavorite', {
        businessId: businessId
      });

      if (response.success) {
        // Update the visual state
        if (response.isFavorite) {
          icon.setAttribute('fill', 'currentColor');
          button.title = 'Remove from favorites';
          button.classList.add('is-favorite');
        } else {
          icon.setAttribute('fill', 'none');
          button.title = 'Add to favorites';
          button.classList.remove('is-favorite');
        }

        this.notifications.show(response.message, 'success');

        // If on favorites page, remove the card with animation
        if ((document.body.dataset.page || '').toLowerCase() === 'public-favorites' && !response.isFavorite) {
          this.removeFavoriteCard(button.closest('.business-card'));
        }
      } else {
        this.notifications.show(response.message || 'Error updating favorite', 'error');
      }
    } catch (error) {
      console.error('Error toggling favorite:', error);
      this.notifications.show('Error updating favorite', 'error');
    }
  }

  showReviewModal(button) {
    const businessId = button.dataset.businessId;

    // Prefer existing FormModal component if available
    if (typeof window.showFormModal === 'function') {
      window.showFormModal({
        title: 'Write a Review',
        width: '600px',
        fields: [
          { name: 'Rating', label: 'Your Rating (1-5 stars)', type: 'number', value: '5', min: 1, max: 5 },
          { name: 'Comment', label: 'Your Review (optional)', type: 'text', value: '', placeholder: 'Share your experience with this business...' }
        ],
        submitText: 'Submit Review',
        cancelText: 'Cancel',
        onSubmit: async (values, close) => {
          await this.submitReviewData(businessId, values);
          close();
        }
      });
      return;
    }

    // Lightweight fallback modal if FormModal is not present
    const modal = document.createElement('div');
    modal.className = 'tt-form-modal';
    modal.innerHTML = `
      <div class="tt-form-modal__overlay"></div>
      <div class="tt-form-modal__content" role="dialog" aria-modal="true" aria-label="Write a Review">
        <div class="tt-form-modal__header">
          <h3>Write a Review</h3>
          <button type="button" class="tt-form-modal__close" aria-label="Close">×</button>
        </div>
        <div class="tt-form-modal__body">
          <form class="tt-form-modal__form">
            <div class="tt-form-field">
              <label for="tt-review-rating">Your Rating (1-5 stars)</label>
              <input id="tt-review-rating" name="Rating" type="number" min="1" max="5" value="5" class="form-input" />
            </div>
            <div class="tt-form-field">
              <label for="tt-review-comment">Your Review (optional)</label>
              <textarea id="tt-review-comment" name="Comment" rows="4" class="form-input" placeholder="Share your experience with this business..."></textarea>
            </div>
            <div class="tt-form-modal__actions">
              <button type="submit" class="btn btn-cta">Submit Review</button>
              <button type="button" class="btn btn-secondary tt-form-cancel">Cancel</button>
            </div>
          </form>
        </div>
      </div>
    `;
    document.body.appendChild(modal);

    const closeModal = () => modal.remove();
    modal.querySelector('.tt-form-modal__overlay')?.addEventListener('click', closeModal);
    modal.querySelector('.tt-form-modal__close')?.addEventListener('click', closeModal);
    modal.querySelector('.tt-form-cancel')?.addEventListener('click', closeModal);
    const form = modal.querySelector('form');
    form?.addEventListener('submit', async (evt) => {
      evt.preventDefault();
      const values = {
        Rating: form.querySelector('[name="Rating"]').value,
        Comment: form.querySelector('[name="Comment"]').value
      };
      await this.submitReviewData(businessId, values);
      closeModal();
    });
  }

  async submitReviewData(businessId, data) {
    try {
      const response = await this.apiClient.post('/Public/AddReview', {
        BusinessId: businessId,
        Rating: parseInt(data.Rating),
        Comment: data.Comment || ''
      });

      if (response.success) {
        this.notifications.show('Review submitted successfully!', 'success');

        // Reload page to show new review
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      } else {
        this.notifications.show(response.message || 'Error submitting review', 'error');
      }
    } catch (error) {
      console.error('Error submitting review:', error);
      this.notifications.show('Error submitting review', 'error');
    }
  }

  async loadCategoriesData() {
    // Prefer embedded data in page to avoid extra network and keep logic in JS directory
    const dataEl = document.getElementById('searchPageData');
    if (dataEl && dataEl.dataset.categories) {
      try {
        this.categoriesData = JSON.parse(dataEl.dataset.categories);
        return;
      } catch (e) {
        console.warn('Failed to parse embedded categories JSON. Falling back to API.', e);
      }
    }

    try {
      const response = await fetch('/Public/Api/Categories');
      this.categoriesData = await response.json();
    } catch (error) {
      console.error('Error loading categories:', error);
      this.categoriesData = [];
    }
  }

  updateSubCategories(categoryKey) {
    const subCategorySelect = document.getElementById('subCategoryFilter');
    const subCategoryGroup = document.getElementById('subCategoryGroup') || document.getElementById('subCategoryField');

    if (!subCategorySelect) return;

    // Clear existing options
    subCategorySelect.innerHTML = '<option value="">All Sub-Categories</option>';

    if (categoryKey && this.categoriesData) {
      const category = this.categoriesData.find(c => c.key === categoryKey);
      if (category && category.subCategories) {
        category.subCategories.forEach(sub => {
          const option = document.createElement('option');
          option.value = sub.key;
          option.textContent = sub.name;
          subCategorySelect.appendChild(option);
        });
      }
    }

    // Show/hide sub-category group
    if (subCategoryGroup) {
      subCategoryGroup.style.display = categoryKey ? 'block' : 'none';
      // Also support utility class if present on element
      if (categoryKey) {
        subCategoryGroup.classList.remove('d-none');
      } else {
        subCategoryGroup.classList.add('d-none');
      }
    }
  }

  initializeFilters() {
    const categoryFilter = document.getElementById('categoryFilter');
    if (categoryFilter && categoryFilter.value) {
      this.updateSubCategories(categoryFilter.value);
      const dataEl = document.getElementById('searchPageData');
      const selectedSubCategory = (dataEl && dataEl.dataset.selectedSubcategory) ||
        new URLSearchParams(window.location.search).get('subCategory');
      if (selectedSubCategory) {
        setTimeout(() => {
          const subCategorySelect = document.getElementById('subCategoryFilter');
          if (subCategorySelect) {
            subCategorySelect.value = selectedSubCategory;
          }
        }, 100);
      }
    }
  }

  applyFilters() {
    const form = document.querySelector('[data-public-form="search"]') ||
      document.querySelector('.advanced-search-form') ||
      document.querySelector('.filters-bar');

    if (form) {
      this.performSearch(form);
    }
  }

  clearFilters() {
    // Clear all filter inputs
    const inputs = document.querySelectorAll('[data-public-filter], .search-input, .filter-select');
    inputs.forEach(input => {
      if (input.type === 'select-one') {
        input.selectedIndex = 0;
      } else {
        input.value = '';
      }
    });

    // Hide sub-category group
    const subCategoryGroup = document.getElementById('subCategoryGroup') || document.getElementById('subCategoryField');
    if (subCategoryGroup) {
      subCategoryGroup.style.display = 'none';
    }

    // Apply cleared filters
    this.applyFilters();
  }

  async performSearch(form, page = 1) {
    const formData = new FormData(form);
    formData.set('page', page);

    const params = new URLSearchParams(formData);
    const currentPath = window.location.pathname;
    const actionUrl = (form.getAttribute('action') || '').trim() || currentPath;

    // Find submit button - the global app.js will handle the processing state
    const submitButton = form.querySelector('button[type="submit"]');

    console.log('Submit button found:', !!submitButton);
    console.log('Button dataset originalText:', submitButton ? submitButton.dataset.originalText : 'N/A');
    console.log('Button current state:', submitButton ? submitButton.textContent : 'N/A');

    try {
      // Button state is already handled by global app.js form submission handler
      console.log('Starting search request');

      // Show loading state
      const resultsContainer = document.getElementById('businessResults') ||
        document.getElementById('searchResults');

      if (resultsContainer) {
        resultsContainer.innerHTML = '<div class="loading-state">Loading businesses...</div>';
      }

      // Make AJAX request
      const response = await fetch(`${actionUrl}?${params}`, {
        headers: {
          'X-Requested-With': 'XMLHttpRequest'
        }
      });

      if (response.ok) {
        const html = await response.text();
        if (resultsContainer) {
          resultsContainer.innerHTML = html;
          // Reinitialize favorite states for newly loaded content
          this.initializeFavoriteStates();
        }

        // Update URL without page reload
        const newUrl = `${actionUrl}?${params}`;
        window.history.pushState({}, '', newUrl);
      } else {
        throw new Error('Search request failed');
      }
    } catch (error) {
      console.error('Error performing search:', error);
      const resultsContainer = document.getElementById('businessResults') ||
        document.getElementById('searchResults');
      if (resultsContainer) {
        resultsContainer.innerHTML = '<div class="error-state">Error loading businesses. Please try again.</div>';
      }
    } finally {
      // Reset button state using the original text stored by app.js
      console.log('Finally block executing');
      if (submitButton) {
        console.log('Resetting button state');
        submitButton.disabled = false;
        submitButton.textContent = submitButton.dataset.originalText || 'Search';
        console.log('Button reset complete');
      } else {
        console.log('No submit button to reset');
      }
    }
  }

  loadPage(page) {
    const form = document.querySelector('[data-public-form="search"]') ||
      document.querySelector('.advanced-search-form') ||
      document.querySelector('.filters-bar');

    if (form) {
      this.performSearch(form, page);
    }
  }

  removeFilter(filterType) {
    const searchForm = document.querySelector('.advanced-search-form');
    if (!searchForm) return;
    switch (filterType) {
      case 'search':
        const searchInput = document.getElementById('searchTerm');
        if (searchInput) searchInput.value = '';
        break;
      case 'town':
        const townSelect = document.getElementById('townFilter');
        if (townSelect) townSelect.value = '';
        break;
      case 'category':
        const categorySelect = document.getElementById('categoryFilter');
        const subCategorySelect = document.getElementById('subCategoryFilter');
        if (categorySelect) categorySelect.value = '';
        if (subCategorySelect) subCategorySelect.value = '';
        const subCategoryGroup = document.getElementById('subCategoryGroup') || document.getElementById('subCategoryField');
        if (subCategoryGroup) {
          subCategoryGroup.style.display = 'none';
          subCategoryGroup.classList.add('d-none');
        }
        break;
      case 'subcategory':
        const s = document.getElementById('subCategoryFilter');
        if (s) s.value = '';
        break;
    }
    this.performSearch(searchForm);
  }

  initializeImageGallery() {
    const galleryImages = document.querySelectorAll('.gallery-image img');
    galleryImages.forEach(img => {
      img.addEventListener('click', function () {
        // Use existing modal system for image viewing
        const imageUrl = this.dataset.full || this.src;
        const altText = this.alt;

        // Create simple image modal
        const modal = document.createElement('div');
        modal.className = 'image-modal';
        modal.innerHTML = `
          <div class="image-modal-overlay" onclick="this.parentElement.remove()"></div>
          <div class="image-modal-content">
            <img src="${imageUrl}" alt="${altText}" />
            <button class="image-modal-close" onclick="this.closest('.image-modal').remove()">×</button>
          </div>
        `;
        document.body.appendChild(modal);
      });
    });
  }

  removeFavoriteCard(card) {
    card.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
    card.style.opacity = '0';
    card.style.transform = 'scale(0.8)';

    setTimeout(() => {
      card.remove();

      // Update favorites count
      const statNumber = document.querySelector('.stat-number');
      if (statNumber) {
        const currentCount = parseInt(statNumber.textContent);
        const newCount = currentCount - 1;
        statNumber.textContent = newCount;

        const statLabel = document.querySelector('.stat-label');
        if (statLabel) {
          statLabel.textContent = newCount === 1 ? 'Favorite' : 'Favorites';
        }

        // Show no favorites message if none left
        if (newCount === 0) {
          setTimeout(() => {
            window.location.reload();
          }, 500);
        }
      }
    }, 300);
  }

  initializeFavoritesFiltering() {
    const businessCards = Array.from(document.querySelectorAll('.business-card'));
    const townFilter = document.getElementById('townFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const sortFilter = document.getElementById('sortFilter');
    const favoritesGrid = document.getElementById('favoritesGrid');

    if (!favoritesGrid) return;

    // Store original business data for filtering/sorting
    const businessData = businessCards.map(card => ({
      element: card,
      name: card.querySelector('.business-name a')?.textContent.trim() || '',
      town: card.querySelector('.business-location span')?.textContent.trim() || '',
      category: card.querySelector('.category-main')?.textContent.trim() || '',
      rating: parseFloat(card.querySelector('.rating-value')?.textContent || '0'),
      businessId: card.dataset.businessId
    }));

    this.favoritesContext = { businessData, townFilter, categoryFilter, sortFilter, favoritesGrid };
    this.applyFavoritesFilters();

    // Event listeners
    if (townFilter) townFilter.addEventListener('change', () => this.applyFavoritesFilters());
    if (categoryFilter) categoryFilter.addEventListener('change', () => this.applyFavoritesFilters());
    if (sortFilter) sortFilter.addEventListener('change', () => this.applyFavoritesFilters());
  }

  applyFavoritesFilters() {
    if (!this.favoritesContext) return;
    const { businessData, townFilter, categoryFilter, sortFilter, favoritesGrid } = this.favoritesContext;
    const townValue = (townFilter?.value || '').toLowerCase();
    const categoryValue = (categoryFilter?.value || '').toLowerCase();
    const sortValue = (sortFilter?.value || 'recent');

    // Filter
    let filteredBusinesses = businessData.filter(business => {
      const townMatch = !townValue || business.town.toLowerCase().includes(townValue);
      const categoryMatch = !categoryValue || business.category.toLowerCase().includes(categoryValue);
      return townMatch && categoryMatch;
    });

    // Sort
    filteredBusinesses.sort((a, b) => {
      switch (sortValue) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'rating':
          return b.rating - a.rating;
        case 'town':
          return a.town.localeCompare(b.town);
        case 'recent':
        default:
          return 0;
      }
    });

    // Render
    favoritesGrid.innerHTML = '';
    filteredBusinesses.forEach(business => favoritesGrid.appendChild(business.element));
    if (filteredBusinesses.length === 0) {
      this.showNoResultsMessage(favoritesGrid);
      // No-results button will call clear-filters; we handle that to reset and re-apply
    }
  }

  showNoResultsMessage(container) {
    container.innerHTML = `
      <div class="no-results">
        <div class="no-results-icon">🔍</div>
        <h3>No businesses match your filters</h3>
        <p>Try adjusting your filter criteria.</p>
        <button type="button" data-public-action="clear-filters" class="btn btn-primary">Clear Filters</button>
      </div>
    `;
  }

  initializeFavoriteStates() {
    // Ensure all favorite buttons have correct visual state based on server-rendered data
    const favoriteButtons = document.querySelectorAll('[data-public-action="toggle-favorite"]');
    
    favoriteButtons.forEach(button => {
      const icon = button.querySelector('svg');
      if (!icon) return;

      // Check if the icon is already filled (server-rendered as favorite)
      const currentFill = icon.getAttribute('fill');
      const isFavorite = currentFill === 'currentColor';
      
      // Ensure the button title matches the state
      button.title = isFavorite ? 'Remove from favorites' : 'Add to favorites';
      
      // Ensure consistent styling
      if (isFavorite) {
        button.classList.add('is-favorite');
      } else {
        button.classList.remove('is-favorite');
      }
    });
  }

  // Review Response Functionality
  initializeReviewResponses() {
    // Bind event listeners for review response functionality
    document.addEventListener('click', (e) => {
      if (e.target.matches('.respond-to-review')) {
        this.showResponseForm(e.target);
      } else if (e.target.matches('.cancel-response')) {
        this.hideResponseForm(e.target);
      } else if (e.target.matches('.submit-response')) {
        this.submitResponse(e.target);
      }
    });

    // Bind textarea events for character counting
    document.addEventListener('input', (e) => {
      if (e.target.matches('.response-textarea')) {
        this.updateCharacterCount(e.target);
      }
    });
  }

  showResponseForm(button) {
    const reviewId = button.dataset.reviewId;
    const formContainer = document.querySelector(`[data-review-id="${reviewId}"]`);
    
    if (formContainer) {
      formContainer.style.display = 'block';
      formContainer.classList.add('show');
      button.style.display = 'none';
      
      // Focus on textarea
      const textarea = formContainer.querySelector('.response-textarea');
      if (textarea) {
        textarea.focus();
      }
    }
  }

  hideResponseForm(button) {
    const formContainer = button.closest('.response-form-container');
    const reviewId = formContainer.dataset.reviewId;
    const respondButton = document.querySelector(`.respond-to-review[data-review-id="${reviewId}"]`);
    
    if (formContainer && respondButton) {
      formContainer.classList.remove('show');
      setTimeout(() => {
        formContainer.style.display = 'none';
      }, 300);
      respondButton.style.display = 'inline-flex';
      
      // Clear textarea
      const textarea = formContainer.querySelector('.response-textarea');
      if (textarea) {
        textarea.value = '';
        this.updateCharacterCount(textarea);
      }
    }
  }

  async submitResponse(button) {
    const reviewId = button.dataset.reviewId;
    const formContainer = button.closest('.response-form-container');
    const textarea = formContainer.querySelector('.response-textarea');
    const responseText = textarea.value.trim();

    if (!responseText) {
      this.notifications.show('Please enter a response', 'error');
      return;
    }

    if (responseText.length > 1000) {
      this.notifications.show('Response cannot exceed 1000 characters', 'error');
      return;
    }

    // Show loading state
    const form = button.closest('.response-form');
    form.classList.add('loading');
    button.disabled = true;

    try {
      const response = await this.apiClient.post('/Public/AddReviewResponse', {
        ReviewId: parseInt(reviewId),
        Response: responseText
      });

      if (response.success) {
        this.notifications.show('Response posted successfully!', 'success');
        
        // Reload page to show new response
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      } else {
        this.notifications.show(response.message || 'Error posting response', 'error');
        form.classList.remove('loading');
        button.disabled = false;
      }
    } catch (error) {
      console.error('Error submitting response:', error);
      this.notifications.show('Error posting response', 'error');
      form.classList.remove('loading');
      button.disabled = false;
    }
  }

  updateCharacterCount(textarea) {
    const maxLength = 1000;
    const currentLength = textarea.value.length;
    const remaining = maxLength - currentLength;
    
    let counterElement = textarea.parentNode.querySelector('.character-counter');
    if (!counterElement) {
      counterElement = document.createElement('div');
      counterElement.className = 'character-counter';
      textarea.parentNode.appendChild(counterElement);
    }
    
    counterElement.textContent = `${remaining} characters remaining`;
    
    // Update styling based on remaining characters
    counterElement.classList.remove('warning', 'error');
    if (remaining < 100) {
      counterElement.classList.add('warning');
    }
    if (remaining < 0) {
      counterElement.classList.add('error');
    }
  }
}

// Guard against double registration and duplicate class definitions
window.__publicManagerInitialized = window.__publicManagerInitialized || false;
if (!window.__publicManagerInitialized) {
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
      if (!window.__publicManagerInitialized) {
        window.PublicManager = new PublicManager();
        window.__publicManagerInitialized = true;
      }
    });
  } else {
    window.PublicManager = new PublicManager();
    window.__publicManagerInitialized = true;
  }
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
  module.exports = PublicManager;
}
