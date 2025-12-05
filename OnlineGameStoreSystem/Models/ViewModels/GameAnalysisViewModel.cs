using System;

namespace OnlineGameStoreSystem.Models.ViewModels
{
    public class GameAnalysisViewModel
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}