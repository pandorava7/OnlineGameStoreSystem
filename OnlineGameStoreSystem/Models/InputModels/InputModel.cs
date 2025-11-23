public class ReviewInputModel
{
    public int SelectedRating { get; set; }
    public string Text { get; set; } = null!;
    //public int GameId { get; set; } // 可选，如果评论是针对某个游戏
}
