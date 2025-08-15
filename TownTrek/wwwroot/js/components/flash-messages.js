/**
 * Flash messages bridge: convert server-rendered alerts to NotificationManager toasts
 * - Looks for .alert.alert-success and .alert.alert-error in the DOM
 * - Shows corresponding NotificationManager toasts
 * - Removes the legacy alert nodes to avoid duplicate UI
 */
(function () {
  if (typeof window === 'undefined') return;

  function showFromAlerts() {
    try {
      const successAlerts = Array.from(document.querySelectorAll('.alert.alert-success'));
      const errorAlerts = Array.from(document.querySelectorAll('.alert.alert-error'));

      successAlerts.forEach((el) => {
        const msg = el.textContent.trim();
        if (msg) {
          window.NotificationManager && NotificationManager.success(msg, { duration: 4000 });
        }
        el.remove();
      });

      errorAlerts.forEach((el) => {
        const msg = el.textContent.trim();
        if (msg) {
          window.NotificationManager && NotificationManager.error(msg, { duration: 0 });
        }
        el.remove();
      });
    } catch (err) {
      // Non-fatal
      console.warn('FlashMessages initialization failed:', err);
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', showFromAlerts);
  } else {
    showFromAlerts();
  }
})();


