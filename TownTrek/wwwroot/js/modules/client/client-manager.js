/**
 * @fileoverview ClientManager - handles client portal shell behaviors
 */

class ClientManager {
  constructor() {
    this.elements = {
      sidebar: document.getElementById('adminSidebar'),
      sidebarToggle: document.getElementById('sidebarToggle'),
      sidebarOverlay: document.getElementById('sidebarOverlay'),
      navLinks: document.querySelectorAll('.nav-link'),
      userMenu: document.querySelector('.user-menu')
    };
    this.isMobile = window.innerWidth <= 1024;
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized) return;
    this.setupSidebarToggle();
    this.setupResponsiveHandling();
    this.setupNavigationHighlighting();
    this.setupUserMenu();
    this.setupNotifications();
    this.isInitialized = true;
    console.log('✅ ClientManager initialized');
  }

  setupSidebarToggle() {
    const { sidebarToggle, sidebarOverlay } = this.elements;
    if (sidebarToggle) {
      sidebarToggle.addEventListener('click', () => this.toggleSidebar());
    }
    if (sidebarOverlay) {
      sidebarOverlay.addEventListener('click', () => this.closeSidebar());
    }
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && this.elements.sidebar?.classList.contains('open')) {
        this.closeSidebar();
      }
    });
  }

  toggleSidebar() {
    if (!this.isMobile) return;
    const { sidebar, sidebarOverlay } = this.elements;
    sidebar?.classList.toggle('open');
    sidebarOverlay?.classList.toggle('active');
    document.body.style.overflow = sidebar?.classList.contains('open') ? 'hidden' : '';
  }

  closeSidebar() {
    if (!this.isMobile) return;
    const { sidebar, sidebarOverlay } = this.elements;
    sidebar?.classList.remove('open');
    sidebarOverlay?.classList.remove('active');
    document.body.style.overflow = '';
  }

  setupResponsiveHandling() {
    window.addEventListener('resize', () => {
      const wasMobile = this.isMobile;
      this.isMobile = window.innerWidth <= 1024;
      if (wasMobile && !this.isMobile) {
        this.closeSidebar();
      }
    });
  }

  setupNavigationHighlighting() {
    const currentPath = window.location.pathname;
    this.elements.navLinks.forEach((link) => {
      link.classList.remove('active');
      if (link.getAttribute('href') === currentPath) {
        link.classList.add('active');
      }
    });
  }

  setupUserMenu() {
    const userMenu = this.elements.userMenu;
    if (!userMenu) return;
    const dropdown = userMenu.querySelector('.user-dropdown');
    if (!dropdown) return;
    userMenu.addEventListener('click', () => dropdown.classList.toggle('active'));
    document.addEventListener('click', (e) => {
      if (!e.target.closest('.user-menu')) dropdown.classList.remove('active');
    });
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') dropdown.classList.remove('active');
    });
  }

  setupNotifications() {
    const notificationBtn = document.querySelector('.header-btn[title="Notifications"]');
    if (notificationBtn) {
      notificationBtn.addEventListener('click', () => {
        NotificationManager.info('No new notifications');
      });
    }
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ClientManager;
}

// Global
window.ClientManager = ClientManager;


