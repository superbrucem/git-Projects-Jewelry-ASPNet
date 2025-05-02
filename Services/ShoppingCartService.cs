using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using OttawaOpalShop.Models;

namespace OttawaOpalShop.Services
{
    public class ShoppingCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProductService _productService;
        private readonly string _cartSessionKey = "ShoppingCart";

        public ShoppingCartService(IHttpContextAccessor httpContextAccessor, ProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        public ShoppingCart GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString(_cartSessionKey);
            
            if (string.IsNullOrEmpty(cartJson))
            {
                var cart = new ShoppingCart();
                SaveCart(cart);
                return cart;
            }
            
            return JsonSerializer.Deserialize<ShoppingCart>(cartJson) ?? new ShoppingCart();
        }

        private void SaveCart(ShoppingCart cart)
        {
            cart.LastUpdated = DateTime.Now;
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = JsonSerializer.Serialize(cart);
            session.SetString(_cartSessionKey, cartJson);
        }

        public void AddToCart(int productId, int quantity = 1)
        {
            var cart = GetCart();
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {productId} not found.");
            }
            
            if (product.StockQuantity < quantity)
            {
                throw new InvalidOperationException($"Not enough stock available. Only {product.StockQuantity} items left.");
            }
            
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                // Check if we have enough stock for the increased quantity
                if (product.StockQuantity < existingItem.Quantity + quantity)
                {
                    throw new InvalidOperationException($"Not enough stock available. Only {product.StockQuantity} items left.");
                }
                
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    Id = cart.Items.Count > 0 ? cart.Items.Max(i => i.Id) + 1 : 1,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageColor = product.ImageColor,
                    ImageText = product.ImageText,
                    IsDarkText = product.IsDarkText,
                    DateAdded = DateTime.Now
                });
            }
            
            SaveCart(cart);
        }

        public void UpdateCartItem(int itemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            
            if (item == null)
            {
                throw new ArgumentException($"Cart item with ID {itemId} not found.");
            }
            
            var product = _productService.GetProductById(item.ProductId);
            
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {item.ProductId} not found.");
            }
            
            if (product.StockQuantity < quantity)
            {
                throw new InvalidOperationException($"Not enough stock available. Only {product.StockQuantity} items left.");
            }
            
            if (quantity <= 0)
            {
                RemoveFromCart(itemId);
                return;
            }
            
            item.Quantity = quantity;
            SaveCart(cart);
        }

        public void RemoveFromCart(int itemId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            
            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            var cart = GetCart();
            cart.Items.Clear();
            SaveCart(cart);
        }

        public bool ValidateCartStock()
        {
            var cart = GetCart();
            var allInStock = true;
            
            foreach (var item in cart.Items)
            {
                var product = _productService.GetProductById(item.ProductId);
                
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    allInStock = false;
                    break;
                }
            }
            
            return allInStock;
        }
    }
}
