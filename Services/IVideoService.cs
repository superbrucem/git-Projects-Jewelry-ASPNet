using System.Collections.Generic;
using OttawaOpalShop.Models;

namespace OttawaOpalShop.Services
{
    public interface IVideoService
    {
        List<Video> GetAllVideos();
        Video? GetVideoById(int id);
        List<Video> GetFeaturedVideos();
        List<Video> GetVideosByCategory(string category);
        List<Video> SearchVideos(string query);
        VideoViewModel GetVideoViewModel(string category = null, int page = 1, int itemsPerPage = 6, string searchQuery = null);
    }
}
