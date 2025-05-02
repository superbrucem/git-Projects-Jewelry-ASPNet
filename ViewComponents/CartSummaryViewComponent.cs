using Microsoft.AspNetCore.Mvc;
using OttawaOpalShop.Services;

namespace OttawaOpalShop.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ShoppingCartService _cartService;

        public CartSummaryViewComponent(ShoppingCartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }
    }
}
