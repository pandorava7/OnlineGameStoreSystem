#region 主页VM
using System.ComponentModel.DataAnnotations;

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

#region 社区VM

public class CommunityPostViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string ContentSnippet { get; set; } = null!; // 截取的内容预览
    public string AuthorName { get; set; } = null!;
    public string AuthorAvatarUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

public class CommunityViewModel
{
    public List<CommunityPostViewModel> Posts { get; set; } = null!;
}

public class PostDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public string AuthorAvatarUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public List<CommentViewModel> Comments { get; set; } = null!;
}

public class CommentViewModel
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = null!;
    public string AuthorAvatarUrl { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

// 自己的帖子列表VM
public class UserPostsViewModel
{
    public List<UserPostViewModel> Posts { get; set; } = null!;
}

public class UserPostViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

// 发布Post时使用的VM
public class CreatePostViewModel
{
    public int? Id { get; set; }          // 新帖 Id 为 null，编辑帖有值
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public IFormFile? Thumbnail { get; set; }
    public string? ThumbnailUrl { get; set; } = null!; // 编辑时显示已有缩略图
}
public class PaymentInputViewModel
{
    [RegularExpression(@"Paypal & TnG", ErrorMessage = "Payment method must be either 'Paypal' or 'TnG'.")]
    public bool ChoicePayment { get; set; }
}
#endregion