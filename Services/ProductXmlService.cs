using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using OttawaOpalShop.Models;

namespace OttawaOpalShop.Services
{
    public class ProductXmlService : IProductService
    {
        private readonly string _xmlFilePath;
        private readonly IWebHostEnvironment _environment;
        private List<Product> _products;
        private FileSystemWatcher _fileWatcher;
        private readonly object _lock = new object();

        public ProductXmlService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _xmlFilePath = Path.Combine(_environment.ContentRootPath, "Data", "Xml", "products.xml");
            LoadProducts();
            SetupFileWatcher();
        }

        private void SetupFileWatcher()
        {
            string directory = Path.GetDirectoryName(_xmlFilePath);
            string filename = Path.GetFileName(_xmlFilePath);

            _fileWatcher = new FileSystemWatcher(directory, filename);
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;

            _fileWatcher.Changed += OnXmlFileChanged;
            _fileWatcher.Created += OnXmlFileChanged;

            _fileWatcher.EnableRaisingEvents = true;

            Console.WriteLine($"File watcher set up for {_xmlFilePath}");
        }

        private void OnXmlFileChanged(object sender, FileSystemEventArgs e)
        {
            // Add a small delay to ensure the file is not locked
            Thread.Sleep(500);

            Console.WriteLine($"XML file changed: {e.FullPath}");
            LoadProducts();
        }

        private List<string> LoadCategories(XElement productElement)
        {
            var categories = new List<string>();

            // First, add the main category if it exists
            var mainCategory = productElement.Element("Category")?.Value;
            if (!string.IsNullOrWhiteSpace(mainCategory))
            {
                categories.Add(mainCategory);
            }

            // Then, check for additional categories
            var categoriesElement = productElement.Element("Categories");
            if (categoriesElement != null)
            {
                var additionalCategories = categoriesElement.Elements("Category")
                    .Select(c => c.Value)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                categories.AddRange(additionalCategories);
            }

            // Ensure we have at least one category
            if (categories.Count == 0)
            {
                categories.Add("Uncategorized");
            }

            // Remove duplicates
            return categories.Distinct().ToList();
        }

        private void LoadProducts()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_xmlFilePath))
                    {
                        XDocument doc = XDocument.Load(_xmlFilePath);
                        _products = doc.Descendants("Product").Select(p => new Product
                        {
                            Id = int.Parse(p.Element("Id")?.Value ?? "0"),
                            Name = p.Element("Name")?.Value ?? p.Element("n")?.Value ?? "Unknown Product",
                            Description = p.Element("Description")?.Value ?? "No description available",
                            Price = decimal.Parse(p.Element("Price")?.Value ?? "0"),
                            ImageColor = p.Element("ImageColor")?.Value ?? "bg-secondary",
                            ImageText = p.Element("ImageText")?.Value ?? "",
                            IsDarkText = bool.Parse(p.Element("IsDarkText")?.Value ?? "false"),
                            Category = p.Element("Category")?.Value ?? "Uncategorized",
                            Categories = LoadCategories(p),
                            Tags = p.Element("Tags")?.Elements("Tag").Select(t => t.Value).ToList() ?? new List<string>(),
                            StockQuantity = int.Parse(p.Element("StockQuantity")?.Value ?? "0"),
                            VideoUrl = p.Element("VideoUrl")?.Value
                        }).ToList();

                        Console.WriteLine($"Loaded {_products.Count} products from XML file");
                    }
                    else
                    {
                        _products = new List<Product>();
                        Console.WriteLine($"XML file not found at: {_xmlFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading products from XML: {ex.Message}");
                    _products = _products ?? new List<Product>();
                }
            }
        }

        public Product? GetProductById(int id)
        {
            lock (_lock)
            {
                return _products.FirstOrDefault(p => p.Id == id);
            }
        }

        public List<Product> GetAllProducts()
        {
            lock (_lock)
            {
                return new List<Product>(_products);
            }
        }

        public List<Product> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return GetAllProducts();
            }

            lock (_lock)
            {
                return _products.Where(p =>
                    p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) ||
                    p.Categories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
        }

        public List<Product> GetFeaturedProducts()
        {
            lock (_lock)
            {
                return _products.Where(p =>
                    p.Category.Equals("Featured", StringComparison.OrdinalIgnoreCase) ||
                    p.Categories.Any(c => c.Equals("Featured", StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
        }

        public List<Product> SearchProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Product>();
            }

            query = query.ToLower();
            lock (_lock)
            {
                return _products.Where(p =>
                    p.Name.ToLower().Contains(query) ||
                    p.Description.ToLower().Contains(query) ||
                    p.Tags.Any(t => t.ToLower().Contains(query))
                ).ToList();
            }
        }

        public void UpdateProductStock(int productId, int newStockQuantity)
        {
            lock (_lock)
            {
                var product = _products.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    // Ensure stock doesn't go below 0
                    product.StockQuantity = Math.Max(0, newStockQuantity);
                    SaveChanges();
                }
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

            lock (_lock)
            {
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

        private void SaveChanges()
        {
            lock (_lock)
            {
                try
                {
                    XDocument doc = new XDocument(
                        new XElement("Products",
                            _products.Select(p => new XElement("Product",
                                new XElement("Id", p.Id),
                                new XElement("Name", p.Name),
                                new XElement("Description", p.Description),
                                new XElement("Price", p.Price),
                                new XElement("ImageColor", p.ImageColor),
                                new XElement("ImageText", p.ImageText),
                                new XElement("IsDarkText", p.IsDarkText),
                                new XElement("Category", p.Category),
                                p.Categories.Count > 1 ? new XElement("Categories",
                                    p.Categories.Where(c => c != p.Category).Select(c => new XElement("Category", c))
                                ) : null,
                                new XElement("Tags",
                                    p.Tags.Select(t => new XElement("Tag", t))
                                ),
                                new XElement("StockQuantity", p.StockQuantity),
                                p.VideoUrl != null ? new XElement("VideoUrl", p.VideoUrl) : null
                            ))
                        )
                    );

                    doc.Save(_xmlFilePath);
                    Console.WriteLine($"XML file saved to: {_xmlFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving products to XML: {ex.Message}");
                }
            }
        }
    }
}
