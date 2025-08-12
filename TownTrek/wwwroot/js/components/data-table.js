/**
 * @fileoverview DataTableComponent - lightweight enhancements for tables
 * Features: hover highlight, optional clickable rows via data-href, zebra striping
 */

class DataTableComponent {
  constructor() {
    this.tables = Array.from(document.querySelectorAll('table[data-table], table.admin-table[data-table]'));
    if (this.tables.length === 0) {
      // As a convenience, enhance admin tables even without the attribute
      this.tables = Array.from(document.querySelectorAll('table.admin-table'));
    }
    this.initializeTables();
  }

  initializeTables() {
    this.tables.forEach((table) => this.enhanceTable(table));
  }

  enhanceTable(tableElement) {
    this.applyHoverHighlight(tableElement);
    this.applyClickableRows(tableElement);
    this.applyZebraStriping(tableElement);
  }

  applyHoverHighlight(tableElement) {
    const rows = tableElement.querySelectorAll('tbody tr');
    rows.forEach((row) => {
      row.addEventListener('mouseenter', function () {
        this.style.backgroundColor = 'var(--light-gray)';
      });
      row.addEventListener('mouseleave', function () {
        this.style.backgroundColor = '';
      });
    });
  }

  applyClickableRows(tableElement) {
    const rows = tableElement.querySelectorAll('tbody tr[data-href]');
    rows.forEach((row) => {
      row.style.cursor = 'pointer';
      row.addEventListener('click', function (event) {
        // Avoid clicks on interactive controls
        if (event.target.closest('a, button, .action-buttons, .table-actions')) return;
        const href = this.getAttribute('data-href');
        if (href) window.location.href = href;
      });
    });
  }

  applyZebraStriping(tableElement) {
    const rows = Array.from(tableElement.querySelectorAll('tbody tr'));
    rows.forEach((row, index) => {
      if (index % 2 === 1) {
        row.style.backgroundColor = row.style.backgroundColor || 'rgba(0,0,0,0.015)';
      }
    });
  }

  destroy() {
    this.tables = [];
  }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
  module.exports = DataTableComponent;
}

// Global for auto-init
window.DataTableComponent = DataTableComponent;


