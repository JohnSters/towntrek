/**
 * @fileoverview Lightweight FileUploadComponent placeholder to satisfy app auto-initialization
 * without interfering with module-specific upload logic.
 * This can be expanded in Phase 2 to provide generic drag/drop, size/type validation, etc.
 */

class FileUploadComponent {
  constructor() {
    this.isInitialized = true;
    // Intentionally minimal to avoid conflicting with specialized upload logic
    // in feature modules like BusinessFormManager.
  }

  destroy() {
    this.isInitialized = false;
  }
}

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = FileUploadComponent;
}

// Global registration
window.FileUploadComponent = FileUploadComponent;


