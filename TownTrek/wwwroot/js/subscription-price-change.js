// Subscription Price Change JavaScript
document.addEventListener('DOMContentLoaded', function() {
    initializePriceChangeForm();
});

function initializePriceChangeForm() {
    console.log('Initializing subscription price change form...');
    
    const currentPriceElement = document.querySelector('[data-current-price]');
    const affectedCustomersElement = document.querySelector('[data-affected-customers]');
    
    if (!currentPriceElement || !affectedCustomersElement) {
        console.error('Required data attributes not found');
        return;
    }
    
    const currentPrice = parseFloat(currentPriceElement.dataset.currentPrice);
    const affectedCustomers = parseInt(affectedCustomersElement.dataset.affectedCustomers);
    
    const newPriceInput = document.getElementById('NewPrice');
    const priceChangeInfo = document.getElementById('priceChangeInfo');
    const priceChangeAlert = document.getElementById('priceChangeAlert');
    const priceChangeText = document.getElementById('priceChangeText');
    const revenueImpact = document.getElementById('revenueImpact');
    const submitBtn = document.getElementById('submitBtn');
    const effectiveDateInput = document.getElementById('EffectiveDate');
    
    if (!newPriceInput) {
        console.error('Price input elements not found');
        return;
    }
    
    function updatePriceChangeInfo() {
        const newPrice = parseFloat(newPriceInput.value) || 0;
        const priceDiff = newPrice - currentPrice;
        const revenueChange = priceDiff * affectedCustomers;
        
        if (newPrice > 0 && newPrice !== currentPrice) {
            if (priceChangeInfo) priceChangeInfo.style.display = 'block';
            
            if (priceDiff > 0) {
                // Price increase
                if (priceChangeAlert) priceChangeAlert.className = 'alert alert-error';
                if (priceChangeText) {
                    priceChangeText.innerHTML = `
                        <strong>Price Increase:</strong> R${priceDiff.toFixed(2)} per month (+${((priceDiff/currentPrice)*100).toFixed(1)}%)<br>
                        <strong>Important:</strong> Price increases require at least 30 days notice to customers.
                    `;
                }
                if (submitBtn) submitBtn.textContent = 'Schedule Price Increase';
                
                // Set minimum effective date to 30 days from now
                if (effectiveDateInput) {
                    const minDate = new Date();
                    minDate.setDate(minDate.getDate() + 30);
                    effectiveDateInput.min = minDate.toISOString().split('T')[0];
                }
                
            } else {
                // Price decrease
                if (priceChangeAlert) priceChangeAlert.className = 'alert alert-success';
                if (priceChangeText) {
                    priceChangeText.innerHTML = `
                        <strong>Price Decrease:</strong> R${Math.abs(priceDiff).toFixed(2)} per month (${((Math.abs(priceDiff)/currentPrice)*100).toFixed(1)}% reduction)<br>
                        <strong>Good news:</strong> Price decreases can take effect immediately.
                    `;
                }
                if (submitBtn) submitBtn.textContent = 'Apply Price Decrease';
                
                // Remove minimum date restriction
                if (effectiveDateInput) {
                    effectiveDateInput.min = new Date().toISOString().split('T')[0];
                }
            }
            
            // Update revenue impact
            if (revenueImpact) {
                if (revenueChange > 0) {
                    revenueImpact.innerHTML = `+R${revenueChange.toFixed(0)}`;
                    revenueImpact.style.color = 'var(--lapis-lazuli)';
                } else if (revenueChange < 0) {
                    revenueImpact.innerHTML = `-R${Math.abs(revenueChange).toFixed(0)}`;
                    revenueImpact.style.color = 'var(--orange-pantone)';
                } else {
                    revenueImpact.innerHTML = 'R0';
                    revenueImpact.style.color = 'var(--dark-gray)';
                }
            }
        } else {
            if (priceChangeInfo) priceChangeInfo.style.display = 'none';
            if (submitBtn) submitBtn.textContent = 'Update Price';
            if (revenueImpact) {
                revenueImpact.innerHTML = 'R0';
                revenueImpact.style.color = 'var(--dark-gray)';
            }
        }
    }
    
    newPriceInput.addEventListener('input', updatePriceChangeInfo);
    
    // Initial calculation
    updatePriceChangeInfo();
    
    console.log('Price change form initialization complete');
}