using System;
using System.Collections.Generic;

namespace OttawaOpalShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageColor { get; set; } = "bg-secondary"; // For the colored box background
        public string ImageText { get; set; } = string.Empty; // Text to display in the colored box
        public bool IsDarkText { get; set; } // Whether to use dark text on the colored background
        public string Category { get; set; } = "Uncategorized"; // Collections, Signature, or Featured
        public List<string> Tags { get; set; } = new List<string>(); // For search functionality
        public int StockQuantity { get; set; } = 0; // Available stock quantity
        public bool IsInStock => StockQuantity > 0;
    }

    public class ProductViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public string CategoryName { get; set; } = "All";
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 8;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);
        public string SearchQuery { get; set; } = string.Empty;
    }
}
