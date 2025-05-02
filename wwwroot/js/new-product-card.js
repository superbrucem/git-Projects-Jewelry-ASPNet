// New Product Card JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Initialize product description tooltips
    initializeProductDescriptions();

    // Initialize product carousels
    initializeProductCarousels();

    // Initialize quick view buttons
    initializeQuickView();
});

function initializeProductCarousels() {
    // Prevent carousel controls from triggering the product link
    const carouselControls = document.querySelectorAll('.carousel-control-prev, .carousel-control-next');
    carouselControls.forEach(control => {
        control.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
    });

    // Initialize Bootstrap carousels
    const carousels = document.querySelectorAll('.carousel');
    carousels.forEach(carousel => {
        // Pause carousel on hover
        carousel.addEventListener('mouseenter', function () {
            bootstrap.Carousel.getInstance(this)?.pause();
        });

        // Resume carousel when mouse leaves
        carousel.addEventListener('mouseleave', function () {
            bootstrap.Carousel.getInstance(this)?.cycle();
        });
    });
}

function initializeQuickView() {
    // Handle quick view button clicks
    const quickViewButtons = document.querySelectorAll('.quick-view-btn');
    quickViewButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            // Get product ID from the closest product card
            const productCard = this.closest('.product-card-container');
            const productLink = productCard.querySelector('.product-card-link');
            const productUrl = productLink.getAttribute('href');

            // Open product details in a modal or redirect to product page
            window.location.href = productUrl;
        });
    });
}

function initializeProductDescriptions() {
    // Adjust tooltip position based on screen size and scroll position
    const titleContainers = document.querySelectorAll('.product-title-container');

    titleContainers.forEach(container => {
        const tooltip = container.querySelector('.product-description-tooltip');

        if (tooltip) {
            container.addEventListener('mouseenter', function () {
                // Position the tooltip based on available space
                const rect = container.getBoundingClientRect();
                const windowHeight = window.innerHeight;

                // If there's not enough space above, show below
                if (rect.top < 150) {
                    tooltip.style.top = 'calc(100% + 5px)';
                } else {
                    tooltip.style.top = '-5px';
                    tooltip.style.transform = 'translateY(-100%)';
                }

                // Ensure tooltip doesn't go off-screen horizontally
                const tooltipRect = tooltip.getBoundingClientRect();
                if (tooltipRect.right > window.innerWidth) {
                    tooltip.style.left = 'auto';
                    tooltip.style.right = '0';
                }
            });
        }
    });
}
