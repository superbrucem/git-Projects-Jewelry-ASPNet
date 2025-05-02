using System.Collections.Generic;

namespace OttawaOpalShop.Models
{
    public class VideoViewModel
    {
        public List<Video> Videos { get; set; } = new List<Video>();
        public string CategoryName { get; set; } = "All";
        public int TotalItems { get; set; } = 0;
        public int CurrentPage { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 6;
        public string SearchQuery { get; set; } = string.Empty;
        public string ChannelUrl { get; set; } = "https://www.youtube.com/channel/UCdQw4DLkKNxRgTWkGtCVvvg";
    }
}
