using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OttawaOpalShop.Models;
using OttawaOpalShop.Services;

namespace OttawaOpalShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ProductService _productService;

    public HomeController(ILogger<HomeController> logger, ProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public IActionResult Index(int page = 1, string category = "Featured")
    {
        var viewModel = _productService.GetProductViewModel(category, page);
        return View(viewModel);
    }

    public IActionResult Collections(int page = 1, string sort = "featured", int perPage = 8)
    {
        var viewModel = _productService.GetProductViewModel("Collections", page, perPage);

        // Apply sorting if needed
        if (!string.IsNullOrEmpty(sort))
        {
            viewModel.Products = SortProducts(viewModel.Products, sort);
        }

        return View(viewModel);
    }

    public IActionResult Signature(int page = 1, string sort = "featured", int perPage = 8)
    {
        var viewModel = _productService.GetProductViewModel("Signature", page, perPage);

        // Apply sorting if needed
        if (!string.IsNullOrEmpty(sort))
        {
            viewModel.Products = SortProducts(viewModel.Products, sort);
        }

        return View(viewModel);
    }

    private List<Product> SortProducts(List<Product> products, string sortOption)
    {
        return sortOption switch
        {
            "price-asc" => products.OrderBy(p => p.Price).ToList(),
            "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
            "newest" => products.OrderByDescending(p => p.Id).ToList(), // Assuming newer products have higher IDs
            _ => products // Default: featured (no change)
        };
    }

    public IActionResult Search(string query, string category = "All", int page = 1, string sort = "featured", int perPage = 8)
    {
        var viewModel = _productService.GetProductViewModel(category, page, perPage, query);
        viewModel.SearchQuery = query;

        // Apply sorting if needed
        if (!string.IsNullOrEmpty(sort))
        {
            viewModel.Products = SortProducts(viewModel.Products, sort);
        }

        return View(viewModel);
    }

    public IActionResult Videos()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Testimonials()
    {
        return View();
    }

    public IActionResult CustomerService()
    {
        return View();
    }

    public IActionResult ShippingReturns()
    {
        return View();
    }

    public IActionResult ProductCare()
    {
        return View();
    }

    public IActionResult Warranty()
    {
        return View();
    }

    public IActionResult FAQ()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
