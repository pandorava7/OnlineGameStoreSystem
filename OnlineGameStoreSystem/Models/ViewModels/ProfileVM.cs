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
        public List<ReviewItem> UserComments { get; set; } = new();
        public List<PurchasedGameVM> PurchasedGames { get; set; } = new();


        public string GameLibraryUrl { get; set; } = null!;
        public string FavoriteTagGamesUrl { get; set; } = null!;
        public string PrivacySettingsUrl { get; set; } = null!;
        public string GameRatingUrl { get; set; } = null!;
        public string EditProfileUrl { get; set; } = null!;
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
}
