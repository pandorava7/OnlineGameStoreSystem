#region 主页VM
public class HomeViewModel
{
    public List<TopGameViewModel> topGamesBySales { get; set; } = null!;
    public List<GameCategoryViewModel> Categories { get; set; } = null!;
}

public class TopGameViewModel
{
    public string Title { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public List<string> ImageUrls { get; set; } = null!;
}


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
#endregion

#region 游戏详情VM & 购物车VM

public class GameDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string Description { get; set; } = null!;
    public List<string> VideoUrls { get; set; } = null!;
    public List<string> ImageUrls { get; set; } = null!;
    public List<string> Genres { get; set; } = null!;
    public List<Review> Reviews { get; set; } = null!;
}

public class ShoppingCartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = null!;
    public decimal TotalPrice { get; set; }
}

public class CartItemViewModel
{
    public CartItem Item { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
}

#endregion