using System.Collections.Generic;
using OttawaOpalShop.Models;

namespace OttawaOpalShop.Services
{
    public interface IProductService
    {
        Product? GetProductById(int id);
        List<Product> GetAllProducts();
        List<Product> GetProductsByCategory(string category);
        List<Product> GetFeaturedProducts();
        List<Product> SearchProducts(string query);
        void UpdateProductStock(int productId, int newStockQuantity);
        ProductViewModel GetProductViewModel(string category, int page = 1, int itemsPerPage = 8, string searchQuery = null);
    }
}
