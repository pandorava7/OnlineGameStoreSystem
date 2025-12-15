using System.ComponentModel.DataAnnotations;

#region Test

public class UploadTestViewModel
{
    [Required]
    public IFormFile Thumbnail { get; set; } = null!;

    public List<IFormFile> PreviewImages { get; set; } = new List<IFormFile>();
    public List<IFormFile> Trailers { get; set; } = new List<IFormFile>();

    [Required]
    public IFormFile GameZip { get; set; } = null!;
}

#endregion


#region 主页VM

public class HomeViewModel
{
    public List<TopGameViewModel> TopGamesBySales { get; set; } = null!;
    public List<GameCategoryViewModel> Categories { get; set; } = null!;
}

public class TopGameViewModel
{
    public string Title { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public List<string> ImageUrls { get; set; } = null!;
    public List<string> TagName { get; set; } = null!;
}


public class GameCategoryViewModel
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public List<GameViewModel> Games { get; set; } = null!;
}
public class GameViewModel
{
    public string Title { get; set; } = null!;
    public string CoverUrl { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
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
    public DateTime ReleasedDate { get; set; }
    public string DeveloperName { get; set; } = null!;
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
    public int UserId { get; set; }
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
#endregion

#region 搜索界面

public class SearchPageVM
{
    public string SearchTerm { get; set; } = "";
    public List<SearchResultVM> Results { get; set; } = new List<SearchResultVM>();
}

public class SearchResultVM
{
    public string Title { get; set; } = "";
    public string? Cover { get; set; }
    public DateTime ReleaseDate { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public double PositiveRate { get; set; }
    public int DeveloperId { get; set; }
    public string DeveloperName { get; set; } = "";
}

#endregion

#region GameLibrary

public class GameLibraryViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public DateTime PurchasedDate { get; set; }
    public int RequireMB { get; set; }

    public string StorageDisplay
    {
        get
        {
            if (RequireMB >= 1024)
                return $"{(RequireMB / 1024.0):0.0} GB";

            return $"{RequireMB} MB";
        }
    }
}

#endregion

#region Support

public class PurchaseItemVM
{
    public int PaymentId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public Dictionary<string, string> ItemAndStatus { get; set; } = new Dictionary<string, string>();
    public string PurchasePurpose { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class PurchaseDetailVM
{
    public int PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty ;

    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public decimal Subtotal { get; set; }   
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PurchaseItem
{
    public int PurchaseId { get; set; } 
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class RefundPurchaseItemVM
{
    public int PurchaseId { get; set; }
    public string? GameThumbnailUrl { get; set; } = string.Empty;
    public string GameTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class RefundPaymentVM
{
    public int PaymentId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<RefundPurchaseItemVM> Purchases { get; set; } = new();
    public string? Reason { get; set; }
}

#endregion

#region Wishlist

public class WishlistItemVM
{
    public int WishlistId { get; set; }
    public int GameId { get; set; }   
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }      
}

public class WishlistVM
{
    public List<WishlistItemVM> Items { get; set; } = new();
}

#endregion

#region Invoice

public class InvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime IssuedAt { get; set; }

    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;

    public List<InvoiceItemDto> Items { get; set; } = new();

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}

public class InvoiceItemDto
{
    public int No { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Total => Price - Discount;
}


#endregion