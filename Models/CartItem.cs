using System;

namespace OttawaOpalShop.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageColor { get; set; } = string.Empty;
        public string ImageText { get; set; } = string.Empty;
        public bool IsDarkText { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        
        public decimal Total => Price * Quantity;
    }
}
