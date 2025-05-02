using System;

namespace OttawaOpalShop.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public int ViewCount { get; set; } = 0;
        public string Category { get; set; } = string.Empty;
        public bool IsFeatured { get; set; } = false;
    }
}
