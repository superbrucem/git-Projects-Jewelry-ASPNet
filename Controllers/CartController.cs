using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OttawaOpalShop.Models;
using OttawaOpalShop.Services;

namespace OttawaOpalShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ShoppingCartService _cartService;
        private readonly ProductService _productService;
        private readonly ILogger<CartController> _logger;
        private readonly PayPalService _paypalService;

        public CartController(ShoppingCartService cartService, ProductService productService, ILogger<CartController> logger, PayPalService paypalService)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
            _paypalService = paypalService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1, string returnUrl = null)
        {
            try
            {
                _cartService.AddToCart(productId, quantity);
                TempData["SuccessMessage"] = "Item added to cart successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            // If returnUrl is provided, use it
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Otherwise, try to get the referring URL
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            // Fallback to Home/Index if no referrer is available
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int itemId, int quantity)
        {
            try
            {
                _cartService.UpdateCartItem(itemId, quantity);
                TempData["SuccessMessage"] = "Cart updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            try
            {
                _cartService.RemoveFromCart(itemId);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            TempData["SuccessMessage"] = "Cart cleared.";
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();

            if (cart.IsEmpty)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            if (!_cartService.ValidateCartStock())
            {
                TempData["ErrorMessage"] = "Some items in your cart are out of stock or have insufficient quantity.";
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string FirstName, string LastName, string Email, string Phone, string PaymentMethod, string TransactionId = null)
        {
            var cart = _cartService.GetCart();

            if (cart.IsEmpty)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            if (!_cartService.ValidateCartStock())
            {
                TempData["ErrorMessage"] = "Some items in your cart are out of stock or have insufficient quantity.";
                return RedirectToAction("Index");
            }

            // Store customer information in TempData for retrieval after PayPal redirect
            TempData["CustomerFirstName"] = FirstName;
            TempData["CustomerLastName"] = LastName;
            TempData["CustomerEmail"] = Email;
            TempData["CustomerPhone"] = Phone;

            // If we have a transaction ID, it means PayPal payment is complete
            if (!string.IsNullOrEmpty(TransactionId))
            {
                // Log customer information
                _logger.LogInformation($"Order placed by {FirstName} {LastName} ({Email})");
                _logger.LogInformation($"Payment method: {PaymentMethod}");
                _logger.LogInformation($"Order total: ${cart.Total}");
                _logger.LogInformation($"PayPal Transaction ID: {TransactionId}");

                // Generate a unique order ID
                string orderId = $"OOS-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                _logger.LogInformation($"Generated order ID: {orderId}");

                // Update inventory (reduce stock quantities)
                foreach (var item in cart.Items)
                {
                    var product = _productService.GetProductById(item.ProductId);
                    if (product != null)
                    {
                        // In a real app, you would update the database
                        // For now, we're just simulating the stock reduction
                        _logger.LogInformation($"Reducing stock for product {product.Id} from {product.StockQuantity} to {product.StockQuantity - item.Quantity}");

                        // Update the product stock quantity
                        _productService.UpdateProductStock(product.Id, product.StockQuantity - item.Quantity);
                    }
                }

                // Clear the cart after successful order placement
                _cartService.ClearCart();

                // Store order information in TempData
                TempData["SuccessMessage"] = "Your order has been placed successfully!";
                TempData["OrderId"] = orderId;
                TempData["CustomerName"] = $"{FirstName} {LastName}";
                TempData["CustomerEmail"] = Email;
                TempData["PayPalMessage"] = "Your PayPal payment has been processed successfully.";
                TempData["TransactionId"] = TransactionId;

                return RedirectToAction("OrderConfirmation");
            }
            else
            {
                try
                {
                    // Create a PayPal order
                    string paypalOrderId = await _paypalService.CreateOrderAsync(cart.Total);

                    // Store the PayPal order ID in TempData
                    TempData["PayPalOrderId"] = paypalOrderId;

                    // Get the approval URL
                    string approvalUrl = await _paypalService.GetApprovalUrlAsync(paypalOrderId);

                    // Redirect to PayPal for payment
                    return Redirect(approvalUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create PayPal order");
                    TempData["ErrorMessage"] = "There was an error processing your PayPal payment. Please try again.";
                    return RedirectToAction("Checkout");
                }
            }
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        public async Task<IActionResult> CapturePayment(string token)
        {
            try
            {
                // Capture the payment
                bool success = await _paypalService.CaptureOrderAsync(token);

                if (success)
                {
                    // Retrieve customer information from TempData
                    string firstName = TempData["CustomerFirstName"]?.ToString() ?? "";
                    string lastName = TempData["CustomerLastName"]?.ToString() ?? "";
                    string email = TempData["CustomerEmail"]?.ToString() ?? "";
                    string phone = TempData["CustomerPhone"]?.ToString() ?? "";

                    // Process the payment with the PayPal order ID as the transaction ID
                    return await ProcessPayment(firstName, lastName, email, phone, "PayPal", token);
                }
                else
                {
                    TempData["ErrorMessage"] = "PayPal payment was not completed successfully.";
                    return RedirectToAction("Checkout");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture PayPal payment");
                TempData["ErrorMessage"] = "There was an error processing your PayPal payment. Please try again.";
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult CancelPayment()
        {
            TempData["ErrorMessage"] = "PayPal payment was cancelled.";
            return RedirectToAction("Checkout");
        }
    }
}
