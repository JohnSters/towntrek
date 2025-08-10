(function () {
  const form = document.getElementById('userSearchForm');
  const input = document.getElementById('userSearchInput');
  if (!form || !input) return;

  let debounceTimer;
  input.addEventListener('input', function () {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      // Always reset page to 1 when searching
      const pageField = form.querySelector('input[name="page"]');
      if (pageField) pageField.value = '1';
      form.submit();
    }, 350);
  });
})();


