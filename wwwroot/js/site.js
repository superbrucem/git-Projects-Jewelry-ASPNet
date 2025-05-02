// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Ottawa Opal Shop JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Sidebar toggle functionality
    const sidebarHeaders = document.querySelectorAll('.sidebar-header');

    sidebarHeaders.forEach(header => {
        header.addEventListener('click', function() {
            const content = this.nextElementSibling;

            // Toggle content visibility
            if (content.style.display === 'none') {
                content.style.display = 'block';
            } else {
                content.style.display = 'none';
            }
        });
    });

    // Initialize submenu functionality
    const hasSubmenuItems = document.querySelectorAll('.has-submenu > a');

    // Handle window resize to adjust menu behavior
    function handleMenuBehavior() {
        // For mobile devices, we need to handle click events to show/hide submenus
        if (window.innerWidth < 992) {
            hasSubmenuItems.forEach(item => {
                // Remove any existing event listeners first
                item.removeEventListener('click', mobileMenuHandler);
                // Add the mobile click handler
                item.addEventListener('click', mobileMenuHandler);
            });
        } else {
            // For desktop, remove click handlers (will use CSS hover instead)
            hasSubmenuItems.forEach(item => {
                item.removeEventListener('click', mobileMenuHandler);
            });
        }
    }

    // Mobile menu click handler
    function mobileMenuHandler(e) {
        // Only prevent default if this is a parent menu item
        if (this.parentElement.classList.contains('has-submenu')) {
            e.preventDefault();
            const submenu = this.nextElementSibling;

            // Toggle submenu visibility
            if (submenu.style.display === 'block') {
                submenu.style.display = 'none';
            } else {
                // Hide all other submenus first
                document.querySelectorAll('.submenu').forEach(menu => {
                    menu.style.display = 'none';
                });
                submenu.style.display = 'block';
            }
        }
    }

    // Initialize menu behavior
    handleMenuBehavior();

    // Update menu behavior on window resize
    window.addEventListener('resize', handleMenuBehavior);

    // Add ripple effect to buttons
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.classList.add('ripple');
    });

    // Add hover effect to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });

        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Quantity selector functionality
    // Quantity decrease buttons
    document.querySelectorAll('.quantity-decrease').forEach(function (button) {
        button.addEventListener('click', function () {
            const input = this.parentNode.querySelector('.quantity-input');
            const currentValue = parseInt(input.value);
            if (currentValue > 1) {
                input.value = currentValue - 1;
            }
        });
    });

    // Quantity increase buttons
    document.querySelectorAll('.quantity-increase').forEach(function (button) {
        button.addEventListener('click', function () {
            const input = this.parentNode.querySelector('.quantity-input');
            const currentValue = parseInt(input.value);
            const maxValue = parseInt(input.getAttribute('max'));
            if (currentValue < maxValue) {
                input.value = currentValue + 1;
            }
        });
    });

    // Validate quantity inputs
    document.querySelectorAll('.quantity-input').forEach(function (input) {
        // Center the text in the input
        input.style.textAlign = 'center';

        // Prevent non-numeric input
        input.addEventListener('keypress', function(e) {
            if (!/[0-9]/.test(e.key)) {
                e.preventDefault();
            }
        });

        // Validate on change
        input.addEventListener('change', function () {
            const minValue = parseInt(this.getAttribute('min'));
            const maxValue = parseInt(this.getAttribute('max'));
            let currentValue = parseInt(this.value);

            if (isNaN(currentValue) || currentValue < minValue) {
                this.value = minValue;
            } else if (currentValue > maxValue) {
                this.value = maxValue;
            }
        });

        // Validate on blur
        input.addEventListener('blur', function() {
            if (this.value === '') {
                this.value = 1;
            }
        });
    });

    // Initialize PayPal button if we're on the checkout page
    if (document.getElementById('paypal-button-container')) {
        console.log('PayPal button container found, initializing...');
        // Add a slight delay to ensure PayPal SDK is loaded
        setTimeout(function() {
            console.log('Attempting to initialize PayPal button...');
            initPayPalButton();
        }, 1000);
    }
});

// PayPal integration
function initPayPalButton() {
    console.log('initPayPalButton called');

    if (typeof paypal === 'undefined') {
        console.error('PayPal SDK not loaded!');
        const container = document.getElementById('paypal-button-container');
        if (container) {
            container.innerHTML = '<div class="alert alert-danger">PayPal payment system is currently unavailable. Please try again later.</div>';
        }
        return;
    }

    console.log('PayPal SDK loaded, creating buttons...');
    paypal.Buttons({
            style: {
                shape: 'rect',
                color: 'gold',
                layout: 'vertical',
                label: 'paypal',
            },
            createOrder: function(data, actions) {
                // Get total from the page
                const total = document.getElementById('order-total').textContent.replace('$', '');

                return actions.order.create({
                    purchase_units: [{
                        amount: {
                            currency_code: 'USD',
                            value: total
                        },
                        description: 'Ottawa Opal Shop Purchase'
                    }]
                });
            },
            onApprove: function(data, actions) {
                // Show a loading message
                const paypalContainer = document.getElementById('paypal-button-container');
                const loadingMessage = document.createElement('div');
                loadingMessage.className = 'alert alert-info text-center';
                loadingMessage.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Processing your payment...';
                paypalContainer.appendChild(loadingMessage);

                return actions.order.capture().then(function(orderData) {
                    console.log('PayPal Capture Result:', orderData);

                    // Create a hidden input for the PayPal order ID
                    const paypalOrderIdInput = document.createElement('input');
                    paypalOrderIdInput.type = 'hidden';
                    paypalOrderIdInput.name = 'paypalOrderId';
                    paypalOrderIdInput.value = orderData.id;

                    // Add the input to the form
                    const form = document.getElementById('checkout-form');
                    form.appendChild(paypalOrderIdInput);

                    // Update the loading message
                    loadingMessage.innerHTML = '<i class="fas fa-check-circle me-2"></i> Payment successful! Completing your order...';

                    // Submit the form to process the payment on the server
                    setTimeout(() => {
                        form.submit();
                    }, 1500);
                });
            },
            onCancel: function(data) {
                console.log('PayPal payment cancelled:', data);
                alert('You have cancelled the PayPal payment. Your order has not been processed.');
            },
            onError: function(err) {
                console.error('PayPal Error:', err);
                alert('There was an error processing your payment. Please try again or contact customer support.');
            }
        }).render('#paypal-button-container');
    }
}
