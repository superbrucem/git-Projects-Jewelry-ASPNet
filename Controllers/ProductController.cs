using Microsoft.AspNetCore.Mvc;
using OttawaOpalShop.Models;
using OttawaOpalShop.Services;

namespace OttawaOpalShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Details(int id)
        {
            var product = _productService.GetProductById(id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }
    }
}
