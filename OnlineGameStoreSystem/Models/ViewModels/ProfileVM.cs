using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels
{
    #region Profile 

    public class ProfileViewModel
    {
        public string AvatarUrl { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public bool IsDeveloper { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Tag> FavoriteTags { get; set; } = new();

        // 用户所有留言
        public List<ReviewItem> UserReviews { get; set; } = new();
        public List<PurchasedGameVM> PurchasedGames { get; set; } = new();
        // Privacy Settings
        public UserPreferences UserPreferences { get; set; } = new();

    }


    #endregion

    #region Comment 
    public class ReviewItem
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; } = null!;
        public string CoverUrl { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
    #endregion

    #region Purchase Games
    public class PurchasedGameVM
    {
        public int GameId { get; set; }
        public string Title { get; set; } = null!;
        public string CoverUrl { get; set; } = null!;
        public decimal PriceAtPurchase { get; set; }
        public DateTime PurchasedAt { get; set; }
    }
    #endregion

    #region Edit Profile
    public class EditProfileViewModel
    {
        // User Details
        public int Id { get; set; }

        [Required(ErrorMessage = "Gamer Tag is required.")]
        [StringLength(50, ErrorMessage = "Gamer Tag cannot exceed 50 characters.")]
        public string Username { get; set; } = null!;

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? AvatarUrl { get; set; }
        // NEW: 用于接收上传的文件
        public IFormFile? NewAvatarFile { get; set; }

        [StringLength(500, ErrorMessage = "User Description cannot exceed 500 characters.")]
        public string? Summary { get; set; }

        // Favorite Tags
        public List<int> SelectedTagIds { get; set; } = new List<int>();

        // For displaying all available tags in the view
        public List<Tag> AvailableTags { get; set; } = new List<Tag>();
    }
    #endregion

    #region Public Profile
    public class PublicProfileViewModel
    {
        public string AvatarUrl { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public bool IsDeveloper { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Tag> FavoriteTags { get; set; } = new();
        public List<ReviewItem> UserReviews { get; set; } = new();
        public List<PurchasedGameVM> PurchasedGames { get; set; } = new();

        public bool PublicProfile { get; set; } = true; // 只读 Privacy 信息
        public List<PublishedGameVM> PublishedGames { get; set; } = new();
        public class PublishedGameVM
        {
            public int GameId { get; set; }
            public string Title { get; set; } = null!;
            public string CoverUrl { get; set; } = null!;
            public decimal Price { get; set; }
            public DateTime ReleaseDate { get; set; }
        }

    }
    #endregion

}
