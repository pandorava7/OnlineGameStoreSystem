using System.Collections.Generic;

namespace OnlineGameStoreSystem.Models.ViewModels
{
    public class SalesDetailItem
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Gross { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal Net { get; set; }
    }

    public class SalesDetailViewModel
    {
        public List<SalesDetailItem> Items { get; set; } = new List<SalesDetailItem>();
    }
}