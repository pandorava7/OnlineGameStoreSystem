public class GameCategoryViewModel
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public List<GameViewModel> Games { get; set; }
}

public class GameViewModel
{
    public string Title { get; set; }
    public string CoverUrl { get; set; }
    public decimal Price { get; set; }
}
