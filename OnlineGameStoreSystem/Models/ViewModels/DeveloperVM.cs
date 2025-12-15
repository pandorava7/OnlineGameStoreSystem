using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels;

// ViewModel for publishing a new game
public class GamePublishViewModel
{
    // Basic Info
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide between 1 and 10 tags.")]
    //[MinLength(1, ErrorMessage = "Please provide between 1 and 10 tags.")]
    //[MaxLength(10, ErrorMessage = "Please provide between 1 and 10 tags.")]
    public List<string> Tags { get; set; } = new List<string>();

    public string ShortDescription { get; set; } = string.Empty;

    // Media
    [Required]
    public IFormFile Thumbnail { get; set; } = null!;
    public List<IFormFile> PreviewImages { get; set; } = new List<IFormFile>();
    public List<IFormFile> Trailers { get; set; } = new List<IFormFile>();

    // Game Detail
    public string DetailDescription { get; set; } = null!;
    [Required]
    public IFormFile GameZip { get; set; } = null!;

    // Price & Publish
    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }
    public bool IsFree { get; set; }
}

public class DeveloperEditGameViewModel
{
    public int Id { get; set; }

    // Basic
    [Required]
    public string Title { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public string ShortDescription { get; set; } = string.Empty;
    public string DetailDescription { get; set; } = string.Empty;

    // Existing media (for display)
    public string? ThumbnailUrl { get; set; }
    public List<string> PreviewImageUrls { get; set; } = new();
    public List<string> TrailersUrls { get; set; } = new();

    // New uploads (all optional)
    public IFormFile? NewThumbnail { get; set; }
    public List<IFormFile> NewPreviewImages { get; set; } = new();
    public List<IFormFile> NewTrailers { get; set; } = new();
    public IFormFile? NewGameZip { get; set; }

    // Deletions
    public string? DeletedMediaJson { get; set; }

    // Price
    public decimal? Price { get; set; }
    public bool IsFree { get; set; }
}


//public class DeveloperGameDetailViewModel
//{
//    public int Id { get; set; }
//        // Basic Info
//    [Required]
//    public string Title { get; set; } = string.Empty;

//    [Required(ErrorMessage = "Please provide between 1 and 10 tags.")]
//    //[MinLength(1, ErrorMessage = "Please provide between 1 and 10 tags.")]
//    //[MaxLength(10, ErrorMessage = "Please provide between 1 and 10 tags.")]
//    public List<string> Tags { get; set; } = new List<string>();

//    public string ShortDescription { get; set; } = string.Empty;

//    // Media
//    public IFormFile Thumbnail { get; set; } = null!;
//    public List<IFormFile> PreviewImages { get; set; } = new List<IFormFile>();
//    public List<IFormFile> Trailers { get; set; } = new List<IFormFile>();
//    public string? ThumbnailUrl { get; set; } = string.Empty;
//    public List<string> PreviewImageUrls { get; set; } = new List<string>();

//    // Game Detail
//    public string DetailDescription { get; set; } = null!;
//    public IFormFile GameZip { get; set; } = null!;

//    // Price & Publish
//    [Range(0, double.MaxValue)]
//    public decimal? Price { get; set; }
//    public bool IsFree { get; set; }
//}

//public class DeveloperGameDetailViewModel
//{
//    public int Id { get; set; }
//    public string Title { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public decimal Price { get; set; }
//    public bool IsFree { get; set; }
//    public string Status { get; set; } = string.Empty;
//    public DateTime ReleaseDate { get; set; }
//    public int LikeCount { get; set; }
//    public int SalesCount { get; set; }
//    public int ReviewsCount { get; set; }
//    public List<GameMediaItem> Media { get; set; } = new();
//    public string? ThumbnailUrl { get; set; }
//}

public class GameMediaItem
{
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

#region 开发者面板VM

public class DeveloperDashboardViewModel
{
    public string UserName { get; set; } = "";
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
