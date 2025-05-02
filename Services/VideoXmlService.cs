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
    public class VideoXmlService : IVideoService
    {
        private readonly string _xmlFilePath;
        private readonly IWebHostEnvironment _environment;
        private List<Video> _videos;
        private FileSystemWatcher _fileWatcher;
        private readonly object _lock = new object();

        public VideoXmlService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _xmlFilePath = Path.Combine(_environment.ContentRootPath, "Data", "Xml", "videos.xml");
            LoadVideos();
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
            LoadVideos();
        }

        private void LoadVideos()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_xmlFilePath))
                    {
                        XDocument doc = XDocument.Load(_xmlFilePath);
                        _videos = doc.Descendants("Video").Select(v => new Video
                        {
                            Id = int.Parse(v.Element("Id")?.Value ?? "0"),
                            Title = v.Element("Title")?.Value ?? "Untitled Video",
                            Description = v.Element("Description")?.Value ?? "No description available",
                            ThumbnailUrl = v.Element("ThumbnailUrl")?.Value ?? "",
                            VideoUrl = v.Element("VideoUrl")?.Value ?? "",
                            PublishedDate = DateTime.Parse(v.Element("PublishedDate")?.Value ?? DateTime.Now.ToString()),
                            ViewCount = int.Parse(v.Element("ViewCount")?.Value ?? "0"),
                            Category = v.Element("Category")?.Value ?? "Uncategorized",
                            IsFeatured = bool.Parse(v.Element("IsFeatured")?.Value ?? "false")
                        }).ToList();

                        Console.WriteLine($"Loaded {_videos.Count} videos from XML file");
                    }
                    else
                    {
                        _videos = new List<Video>();
                        Console.WriteLine($"XML file not found at: {_xmlFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading videos from XML: {ex.Message}");
                    _videos = _videos ?? new List<Video>();
                }
            }
        }

        public List<Video> GetAllVideos()
        {
            lock (_lock)
            {
                return new List<Video>(_videos);
            }
        }

        public Video? GetVideoById(int id)
        {
            lock (_lock)
            {
                return _videos.FirstOrDefault(v => v.Id == id);
            }
        }

        public List<Video> GetFeaturedVideos()
        {
            lock (_lock)
            {
                return _videos.Where(v => v.IsFeatured).ToList();
            }
        }

        public List<Video> GetVideosByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return GetAllVideos();
            }

            lock (_lock)
            {
                return _videos.Where(v => v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        public List<Video> SearchVideos(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Video>();
            }

            query = query.ToLower();
            lock (_lock)
            {
                return _videos.Where(v =>
                    v.Title.ToLower().Contains(query) ||
                    v.Description.ToLower().Contains(query) ||
                    v.Category.ToLower().Contains(query)
                ).ToList();
            }
        }

        public VideoViewModel GetVideoViewModel(string category = null, int page = 1, int itemsPerPage = 6, string searchQuery = null)
        {
            // Default to page 1 if invalid
            if (page < 1)
            {
                page = 1;
            }

            // Default to 6 items per page if invalid
            if (itemsPerPage < 1)
            {
                itemsPerPage = 6;
            }

            List<Video> videos;

            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    videos = SearchVideos(searchQuery);
                    if (!string.IsNullOrWhiteSpace(category) && category != "All")
                    {
                        videos = videos.Where(v => v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }
                else if (string.IsNullOrWhiteSpace(category) || category == "All")
                {
                    videos = GetAllVideos();
                }
                else
                {
                    videos = GetVideosByCategory(category);
                }

                var totalItems = videos.Count;
                var pagedVideos = videos
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();

                return new VideoViewModel
                {
                    Videos = pagedVideos,
                    CategoryName = category ?? "All",
                    TotalItems = totalItems,
                    CurrentPage = page,
                    ItemsPerPage = itemsPerPage,
                    SearchQuery = searchQuery ?? string.Empty
                };
            }
        }
    }
}
