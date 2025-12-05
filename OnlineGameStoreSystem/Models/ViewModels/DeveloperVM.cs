using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace OnlineGameStoreSystem.Models.ViewModels
{
    public class DeveloperGameDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int LikeCount { get; set; }
        public int SalesCount { get; set; }
        public int ReviewsCount { get; set; }
        public List<GameMediaItem> Media { get; set; } = new();
        public string? ThumbnailUrl { get; set; }
    }

    public class GameMediaItem
    {
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    #region 开发者面板VM

    public class DeveloperDashboardViewModel
    {
        public List<DeveloperGameItemViewModel> Games { get; set; } = new();

        // aggregated summary (calculated server-side)
        public int TotalExposure { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalDownloads { get; set; }
        public int TotalLikes { get; set; }
        public int TotalReviews { get; set; }
        public decimal NetRevenue { get; set; }
    }

    public class DeveloperGameItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public decimal Price { get; set; }
        public double LikeRate { get; set; }
        public int SalesCount { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = "";
    }

    #endregion
}
