/**
 * @fileoverview Minimal ClientManager stub to satisfy app auto-init until full migration
 */

class ClientManager {
  constructor() {
    this.isInitialized = true;
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


