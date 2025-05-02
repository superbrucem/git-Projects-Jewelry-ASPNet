using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using OttawaOpalShop.Models;

namespace OttawaOpalShop.Services
{
    public class ProductService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<Product> _products = new List<Product>();

        public ProductService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            InitializeProducts();
        }

        private void InitializeProducts()
        {
            // Create sample products directly in code instead of loading from JSON
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "1.00 MM ROUND 100 PIECES BRILLIANT AAA+ NATURAL CHROME GREEN DIOPSIDE [FLAWLESS-VVS]",
                    Description = "High quality natural chrome green diopside gemstones with brilliant cut.",
                    Price = 8.99m,
                    ImageColor = "bg-secondary",
                    ImageText = "GREEN DIOPSIDE",
                    IsDarkText = false,
                    Category = "Collections",
                    Tags = new List<string> { "green", "diopside", "chrome", "brilliant", "natural", "round", "flawless" },
                    StockQuantity = 25
                },
                new Product
                {
                    Id = 2,
                    Name = "1.40 MM ROUND 70 PIECES NATURAL UNHEATED CHROME GREEN DIOPSIDE [FLAWLESS-VVS]",
                    Description = "Premium unheated chrome green diopside gemstones with excellent clarity.",
                    Price = 11.99m,
                    ImageColor = "bg-secondary",
                    ImageText = "GREEN DIOPSIDE",
                    IsDarkText = false,
                    Category = "Collections",
                    Tags = new List<string> { "green", "diopside", "chrome", "unheated", "natural", "round", "flawless" },
                    StockQuantity = 15
                },
                new Product
                {
                    Id = 3,
                    Name = "1.50 MM ROUND MACHINE CUT 70 PCS AAA LUSTER NATURAL INTENSE ICE BLUE APATITE [VVS]-AAA GRADE",
                    Description = "Stunning ice blue apatite gemstones with exceptional luster and clarity.",
                    Price = 12.99m,
                    ImageColor = "bg-primary",
                    ImageText = "BLUE APATITE",
                    IsDarkText = false,
                    Category = "Collections",
                    Tags = new List<string> { "blue", "apatite", "ice", "machine cut", "natural", "round", "luster", "AAA grade" },
                    StockQuantity = 10
                },
                new Product
                {
                    Id = 4,
                    Name = "1.50 MM ROUND MACHINE CUT 70 PCS AAA LUSTER NATURAL NEON GREEN BLUE APATITE [VVS]-AAA GRADE",
                    Description = "Vibrant neon green-blue apatite gemstones with excellent cut and clarity.",
                    Price = 12.99m,
                    ImageColor = "bg-info",
                    ImageText = "NEON GREEN BLUE APATITE",
                    IsDarkText = true,
                    Category = "Collections",
                    Tags = new List<string> { "neon", "green", "blue", "apatite", "machine cut", "natural", "round", "luster", "AAA grade" },
                    StockQuantity = 5
                },
                new Product
                {
                    Id = 5,
                    Name = "1.50 MM ROUND 70 PCS AAA LUSTER NATURAL GOLDEN ORANGE CITRINE [FLAWLESS-VVS]",
                    Description = "Radiant golden orange citrine gemstones with exceptional clarity and color.",
                    Price = 6.99m,
                    ImageColor = "bg-warning",
                    ImageText = "ORANGE CITRINE",
                    IsDarkText = false,
                    Category = "Signature",
                    Tags = new List<string> { "orange", "citrine", "golden", "natural", "round", "luster", "flawless" },
                    StockQuantity = 30
                },
                new Product
                {
                    Id = 6,
                    Name = "1.50 MM ROUND 60 PIECES NATURAL UNHEATED CHROME GREEN DIOPSIDE [FLAWLESS-VVS]",
                    Description = "Premium unheated chrome green diopside gemstones with excellent clarity.",
                    Price = 11.99m,
                    ImageColor = "bg-success",
                    ImageText = "GREEN DIOPSIDE",
                    IsDarkText = false,
                    Category = "Signature",
                    Tags = new List<string> { "green", "diopside", "chrome", "unheated", "natural", "round", "flawless" },
                    StockQuantity = 0 // Out of stock
                },
                new Product
                {
                    Id = 7,
                    Name = "1.60 MM ROUND 70 PCS AAA LUSTER NATURAL GOLDEN YELLOW CITRINE [FLAWLESS-VVS]",
                    Description = "Brilliant golden yellow citrine gemstones with exceptional clarity and luster.",
                    Price = 4.99m,
                    ImageColor = "bg-warning",
                    ImageText = "YELLOW CITRINE",
                    IsDarkText = true,
                    Category = "Featured",
                    Tags = new List<string> { "yellow", "citrine", "golden", "natural", "round", "luster", "flawless" },
                    StockQuantity = 50
                },
                new Product
                {
                    Id = 8,
                    Name = "1.70-1.80 MM ROUND 40 PIECES NATURAL UNHEATED CHROME GREEN DIOPSIDE [FLAWLESS-VVS]",
                    Description = "Large size premium unheated chrome green diopside gemstones with excellent clarity.",
                    Price = 11.99m,
                    ImageColor = "bg-success",
                    ImageText = "GREEN DIOPSIDE",
                    IsDarkText = false,
                    Category = "Featured",
                    Tags = new List<string> { "green", "diopside", "chrome", "unheated", "natural", "round", "flawless" },
                    StockQuantity = 8
                },
                // Additional Collections products
                new Product
                {
                    Id = 9,
                    Name = "2.00 MM ROUND 50 PIECES NATURAL RUBY RED GARNET [FLAWLESS-VVS]",
                    Description = "Exquisite ruby red garnet gemstones with exceptional clarity and brilliance.",
                    Price = 14.99m,
                    ImageColor = "bg-danger",
                    ImageText = "RUBY RED GARNET",
                    IsDarkText = false,
                    Category = "Collections",
                    Tags = new List<string> { "red", "garnet", "ruby", "natural", "round", "flawless" },
                    StockQuantity = 12
                },
                new Product
                {
                    Id = 10,
                    Name = "1.80 MM ROUND 60 PIECES NATURAL DEEP BLUE SAPPHIRE [VVS]",
                    Description = "Stunning deep blue sapphire gemstones with excellent clarity and color.",
                    Price = 19.99m,
                    ImageColor = "bg-primary",
                    ImageText = "BLUE SAPPHIRE",
                    IsDarkText = false,
                    Category = "Collections",
                    Tags = new List<string> { "blue", "sapphire", "deep", "natural", "round", "VVS" },
                    StockQuantity = 7
                },
                new Product
                {
                    Id = 11,
                    Name = "1.50 MM ROUND 80 PIECES NATURAL PURPLE AMETHYST [AAA GRADE]",
                    Description = "Beautiful purple amethyst gemstones with excellent color saturation.",
                    Price = 9.99m,
                    ImageColor = "bg-purple",
                    ImageText = "PURPLE AMETHYST",
                    IsDarkText = false,
                    Category = "Collections",
                    Tags = new List<string> { "purple", "amethyst", "natural", "round", "AAA grade" },
                    StockQuantity = 20
                },
                new Product
                {
                    Id = 12,
                    Name = "1.20 MM ROUND 90 PIECES NATURAL PINK TOURMALINE [FLAWLESS]",
                    Description = "Delicate pink tourmaline gemstones with exceptional clarity and color.",
                    Price = 15.99m,
                    ImageColor = "bg-pink",
                    ImageText = "PINK TOURMALINE",
                    IsDarkText = true,
                    Category = "Collections",
                    Tags = new List<string> { "pink", "tourmaline", "natural", "round", "flawless" },
                    StockQuantity = 15
                },
                new Product
                {
                    Id = 13,
                    Name = "2.50 MM ROUND 30 PIECES NATURAL WHITE MOONSTONE [AAA GRADE]",
                    Description = "Ethereal white moonstone gemstones with beautiful adularescence.",
                    Price = 13.99m,
                    ImageColor = "bg-light",
                    ImageText = "WHITE MOONSTONE",
                    IsDarkText = true,
                    Category = "Collections",
                    Tags = new List<string> { "white", "moonstone", "natural", "round", "AAA grade" },
                    StockQuantity = 10
                },
                // Additional Signature products
                new Product
                {
                    Id = 14,
                    Name = "2.00 MM ROUND 40 PIECES NATURAL FIRE OPAL [5 ELEMENTS COLLECTION]",
                    Description = "Vibrant fire opal gemstones from our exclusive 5 Elements Collection.",
                    Price = 24.99m,
                    ImageColor = "bg-danger",
                    ImageText = "FIRE OPAL",
                    IsDarkText = false,
                    Category = "Signature",
                    Tags = new List<string> { "fire", "opal", "natural", "round", "5 elements" },
                    StockQuantity = 5
                },
                new Product
                {
                    Id = 15,
                    Name = "1.80 MM ROUND 50 PIECES NATURAL AQUA BLUE OPAL [5 ELEMENTS COLLECTION]",
                    Description = "Serene aqua blue opal gemstones from our exclusive 5 Elements Collection.",
                    Price = 22.99m,
                    ImageColor = "bg-info",
                    ImageText = "AQUA BLUE OPAL",
                    IsDarkText = true,
                    Category = "Signature",
                    Tags = new List<string> { "aqua", "blue", "opal", "natural", "round", "5 elements" },
                    StockQuantity = 8
                },
                new Product
                {
                    Id = 16,
                    Name = "2.20 MM ROUND 35 PIECES NATURAL FOREST GREEN OPAL [5 ELEMENTS COLLECTION]",
                    Description = "Rich forest green opal gemstones from our exclusive 5 Elements Collection.",
                    Price = 23.99m,
                    ImageColor = "bg-success",
                    ImageText = "FOREST GREEN OPAL",
                    IsDarkText = false,
                    Category = "Signature",
                    Tags = new List<string> { "forest", "green", "opal", "natural", "round", "5 elements" },
                    StockQuantity = 6
                },
                new Product
                {
                    Id = 17,
                    Name = "1.90 MM ROUND 45 PIECES NATURAL BLACK ONYX [UNDERWORLD COLLECTION]",
                    Description = "Mysterious black onyx gemstones from our exclusive Underworld Collection.",
                    Price = 18.99m,
                    ImageColor = "bg-dark",
                    ImageText = "BLACK ONYX",
                    IsDarkText = false,
                    Category = "Signature",
                    Tags = new List<string> { "black", "onyx", "natural", "round", "underworld" },
                    StockQuantity = 12
                },
                new Product
                {
                    Id = 18,
                    Name = "2.10 MM ROUND 40 PIECES NATURAL GOLDEN AMBER [GREEK GODS COLLECTION]",
                    Description = "Radiant golden amber gemstones from our exclusive Greek Gods Collection.",
                    Price = 21.99m,
                    ImageColor = "bg-warning",
                    ImageText = "GOLDEN AMBER",
                    IsDarkText = true,
                    Category = "Signature",
                    Tags = new List<string> { "golden", "amber", "natural", "round", "greek gods" },
                    StockQuantity = 9
                },
                new Product
                {
                    Id = 19,
                    Name = "1.70 MM ROUND 60 PIECES NATURAL SILVER PEARL [GREEK GODS COLLECTION]",
                    Description = "Lustrous silver pearl gemstones from our exclusive Greek Gods Collection.",
                    Price = 25.99m,
                    ImageColor = "bg-secondary",
                    ImageText = "SILVER PEARL",
                    IsDarkText = true,
                    Category = "Signature",
                    Tags = new List<string> { "silver", "pearl", "natural", "round", "greek gods" },
                    StockQuantity = 7
                }
            };
        }

        public Product? GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public List<Product> GetAllProducts()
        {
            return _products;
        }

        public List<Product> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return GetAllProducts();
            }

            return _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Product> GetFeaturedProducts()
        {
            return _products.Where(p => p.Category.Equals("Featured", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Product> SearchProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Product>();
            }

            query = query.ToLower();
            return _products.Where(p =>
                p.Name.ToLower().Contains(query) ||
                p.Description.ToLower().Contains(query) ||
                p.Tags.Any(t => t.ToLower().Contains(query))
            ).ToList();
        }

        public void UpdateProductStock(int productId, int newStockQuantity)
        {
            var product = GetProductById(productId);
            if (product != null)
            {
                // Ensure stock doesn't go below 0
                product.StockQuantity = Math.Max(0, newStockQuantity);
            }
        }

        public ProductViewModel GetProductViewModel(string category, int page = 1, int itemsPerPage = 8, string searchQuery = null)
        {
            // Default to page 1 if invalid
            if (page < 1)
            {
                page = 1;
            }

            // Default to 8 items per page if invalid
            if (itemsPerPage < 1)
            {
                itemsPerPage = 8;
            }

            List<Product> products;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                products = SearchProducts(searchQuery);
                if (!string.IsNullOrWhiteSpace(category) && category != "All")
                {
                    products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            else if (string.IsNullOrWhiteSpace(category) || category == "All")
            {
                products = GetAllProducts();
            }
            else
            {
                products = GetProductsByCategory(category);
            }

            var totalItems = products.Count;
            var pagedProducts = products
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return new ProductViewModel
            {
                Products = pagedProducts,
                CategoryName = category ?? "All",
                TotalItems = totalItems,
                CurrentPage = page,
                ItemsPerPage = itemsPerPage,
                SearchQuery = searchQuery ?? string.Empty
            };
        }
    }
}
