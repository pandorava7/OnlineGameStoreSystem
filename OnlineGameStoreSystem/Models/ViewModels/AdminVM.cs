using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels;


public class StatusManageVM
{
    public int UserId { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;
}

public class GameReleaseReviewVM
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GameReviewVM
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ThumbnailUrl {  get; set; } = string.Empty;
    public string[] PreviewUrls { get; set; } = new string[0];
    public string[] VideoUrls { get; set;  } = new string[0];
    public string[] Tags { get; set;  } = new string[0];
}

public class GameManagementVM
{
    public int GameId { get; set; }
    public string? ThumbnailUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string[] Tags { get; set; } = new string[0];
    public string Status {  get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // 目前没有updated at属性
    //public DateTime UpdatedAt { get; set; }
}

public class RefundHandlingVM
{
    public int PurchaseId { get; set; }
    public int UserId { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int RefundGameId { get; set; }
    public string RefundGameName { get; set;} = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime RefundRequestDate { get; set; }
    public string RefundReason {  get; set; } = string.Empty;
}