using System;
using System.Collections.Generic;
using System.Linq;

namespace OttawaOpalShop.Models
{
    public class ShoppingCart
    {
        public string CartId { get; set; } = Guid.NewGuid().ToString();
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        public decimal Total => Items.Sum(item => item.Total);
        public int ItemCount => Items.Sum(item => item.Quantity);
        public bool IsEmpty => !Items.Any();
    }
}
