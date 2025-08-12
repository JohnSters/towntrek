/**
 * @fileoverview Member functionality manager for TownTrek
 * Handles business browsing, searching, reviews, and favorites for members
 * @author TownTrek Development Team
 * @version 1.0.0
 */

class MemberManager {
  constructor() {
    this.apiClient = window.ApiClient;
    this.notifications = window.NotificationManager;
    this.modalComponent = window.ModalComponent;
    this.init();
  }

  init() {
    this.bindEventListeners();
    this.initializeCurrentPage();
  }

  bindEventListeners() {
    // Global event delegation for member features
    document.addEventListener('click', this.handleClick.bind(this));
    document.addEventListener('submit', this.handleSubmit.bind(this));
    document.addEventListener('change', this.handleChange.bind(this));
  }

  handleClick(e) {
    const target = e.target.closest('[data-member-action]');
    if (!target) return;

    const action = target.dataset.memberAction;
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
    if (e.target.matches('[data-member-form]')) {
      e.preventDefault();
      const formType = e.target.dataset.memberForm;
      
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
    if (e.target.matches('[data-member-filter]')) {
      const filterType = e.target.dataset.memberFilter;
      
      switch (filterType) {
        case 'category':
          this.updateSubCategories(e.target.value);
          break;
      }
    }
  }

  initializeCurrentPage() {
    // Prefer explicit body data attribute if set
    const explicitPage = document.body.dataset.page;
    if (explicitPage) {
      this.routeToPage(explicitPage);
      return;
    }

    // Fallback to DOM-based detection (no inline scripts required in views)
    if (document.querySelector('.business-search-page')) {
      this.routeToPage('member-search');
    } else if (document.querySelector('.favorites-page')) {
      this.routeToPage('member-favorites');
    } else if (document.querySelector('.town-businesses-page')) {
      this.routeToPage('member-town-businesses');
    } else if (document.querySelector('.business-details-page')) {
      this.routeToPage('member-business-details');
    } else if (document.querySelector('.member-dashboard')) {
      this.routeToPage('member-dashboard');
    }
  }

  routeToPage(pageKey) {
    switch (pageKey) {
      case 'member-dashboard':
        this.initializeDashboard();
        break;
      case 'member-town-businesses':
        this.initializeTownBusinesses();
        break;
      case 'member-search':
        this.initializeSearch();
        break;
      case 'member-business-details':
        this.initializeBusinessDetails();
        break;
      case 'member-favorites':
        this.initializeFavorites();
        break;
    }
  }

  initializeDashboard() {
    // Initialize search functionality
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
      searchInput.addEventListener('focus', function() {
        this.parentElement.classList.add('focused');
      });
      
      searchInput.addEventListener('blur', function() {
        this.parentElement.classList.remove('focused');
      });
    }
  }

  initializeTownBusinesses() {
    // Use global categories data if available
    this.categoriesData = window.memberCategoriesData || [];
    this.townId = window.memberTownId;
    
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
      const response = await this.apiClient.post('/Member/ToggleFavorite', {
        businessId: businessId
      });
      
      if (response.success) {
        if (response.isFavorite) {
          icon.setAttribute('fill', 'currentColor');
          button.title = 'Remove from favorites';
        } else {
          icon.setAttribute('fill', 'none');
          button.title = 'Add to favorites';
        }
        
        this.notifications.show(response.message, 'success');
        
        // If on favorites page, remove the card with animation
        if (document.body.dataset.page === 'member-favorites' && !response.isFavorite) {
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
    
    // Use existing FormModal component
    window.showFormModal({
      title: 'Write a Review',
      width: '600px',
      fields: [
        { 
          name: 'Rating', 
          label: 'Your Rating (1-5 stars)', 
          type: 'number', 
          value: '5',
          min: 1,
          max: 5
        },
        { 
          name: 'Comment', 
          label: 'Your Review (optional)', 
          type: 'text', 
          value: '',
          placeholder: 'Share your experience with this business...'
        }
      ],
      submitText: 'Submit Review',
      cancelText: 'Cancel',
      onSubmit: async (values, close) => {
        await this.submitReviewData(businessId, values);
        close();
      }
    });
  }

  async submitReviewData(businessId, data) {
    try {
      const response = await this.apiClient.post('/Member/AddReview', {
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
      const response = await fetch('/Member/Api/Categories');
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
    const form = document.querySelector('[data-member-form="search"]') || 
                 document.querySelector('.advanced-search-form') ||
                 document.querySelector('.filters-bar');
    
    if (form) {
      this.performSearch(form);
    }
  }

  clearFilters() {
    // Clear all filter inputs
    const inputs = document.querySelectorAll('[data-member-filter], .search-input, .filter-select');
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
    
    try {
      // Show loading state
      const resultsContainer = document.getElementById('businessResults') || 
                              document.getElementById('searchResults');
      
      if (resultsContainer) {
        resultsContainer.innerHTML = '<div class="loading-state">Loading businesses...</div>';
      }
      
      // Make AJAX request
      const response = await fetch(`${currentPath}?${params}`, {
        headers: {
          'X-Requested-With': 'XMLHttpRequest'
        }
      });
      
      if (response.ok) {
        const html = await response.text();
        if (resultsContainer) {
          resultsContainer.innerHTML = html;
        }
        
        // Update URL without page reload
        const newUrl = `${currentPath}?${params}`;
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
    }
  }

  loadPage(page) {
    const form = document.querySelector('[data-member-form="search"]') || 
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
      img.addEventListener('click', function() {
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
        <button type="button" data-member-action="clear-filters" class="btn btn-primary">Clear Filters</button>
      </div>
    `;
  }
}

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => {
    window.MemberManager = new MemberManager();
  });
} else {
  window.MemberManager = new MemberManager();
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
  module.exports = MemberManager;
}
