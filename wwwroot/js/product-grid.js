// Product Grid JavaScript Functions

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize sort select
    const sortSelect = document.getElementById('sort-select');
    if (sortSelect) {
        sortSelect.addEventListener('change', applyFilters);
    }

    // Initialize per page select
    const perPageSelect = document.getElementById('per-page-select');
    if (perPageSelect) {
        perPageSelect.addEventListener('change', applyFilters);
    }

    // Set all product cards to use 3 columns by default
    initializeProductCards();
});

// Apply filters function
function applyFilters() {
    console.log('Applying filters...');

    // Get values
    const sortSelect = document.getElementById('sort-select');
    const perPageSelect = document.getElementById('per-page-select');

    if (!sortSelect || !perPageSelect) {
        console.error('Required elements not found');
        return;
    }

    const sortValue = sortSelect.value;
    const perPageValue = perPageSelect.value;

    console.log('Sort:', sortValue, 'PerPage:', perPageValue);

    // Get current URL and parameters
    let url = new URL(window.location.href);
    let params = new URLSearchParams(url.search);

    // Update parameters
    params.set('sort', sortValue);
    params.set('perPage', perPageValue);
    params.set('page', '1'); // Reset to page 1 when filters change

    // Preserve category parameter if it exists in the model
    const categoryElement = document.getElementById('category-name');
    if (categoryElement && categoryElement.value) {
        params.set('category', categoryElement.value);
    }

    // Preserve search parameter if it exists in the model
    const searchElement = document.getElementById('search-query');
    if (searchElement && searchElement.value) {
        params.set('search', searchElement.value);
    }

    // Create the new URL and navigate to it
    const newUrl = url.pathname + '?' + params.toString();
    console.log('Navigating to:', newUrl);
    window.location.href = newUrl;
}

// Initialize product cards to use 3 columns
function initializeProductCards() {
    // Update product card classes
    const productCards = document.querySelectorAll('.col-md-3.col-lg-3.mb-4, .product-card-container');
    productCards.forEach(card => {
        card.classList.add('product-card-container');

        // Remove existing column classes
        card.classList.remove('col-md-12', 'col-md-6', 'col-md-4', 'col-md-3', 'col-md-2');

        // Set to 4 columns (col-md-3)
        card.classList.add('col-md-3');
    });
}
