using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OnlineGameStoreSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options)
        : base(options)
    {
    }

    // User & Preferences
    public DbSet<User> Users { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }

    // Shopping Cart
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    // Wishlist
    public DbSet<Wishlist> Wishlists { get; set; }

    // Tag
    public DbSet<Tag> Tags { get; set; }
    public DbSet<FavouriteTags> FavouriteTags { get; set; }
    public DbSet<GameTag> GameTags { get; set; }

    // Game
    public DbSet<Game> Games { get; set; }
    public DbSet<GameMedia> GameMedia { get; set; }

    // Community
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<ReviewLike> ReviewLikes { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<GameLike> GameLikes { get; set; }
    public DbSet<Comment> Comments { get; set; }

    // Review
    public DbSet<Review> Reviews { get; set; }

    // Notification
    public DbSet<Notification> Notifications { get; set; }

    // Payment & Purchase
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Purchase> Purchases { get; set; }

    // Developer Revenue
    public DbSet<DeveloperRevenue> DeveloperRevenues { get; set; }

    // OTP
    public DbSet<OtpEntry> OtpEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many: FavouriteTags (用户 - 标签)
        modelBuilder.Entity<FavouriteTags>()
            .HasKey(ft => new { ft.UserId, ft.TagId });

        modelBuilder.Entity<FavouriteTags>()
            .HasOne(ft => ft.User)
            .WithMany(u => u.FavouriteTags)
            .HasForeignKey(ft => ft.UserId);

        modelBuilder.Entity<FavouriteTags>()
            .HasOne(ft => ft.Tag)
            .WithMany(t => t.UserTags)
            .HasForeignKey(ft => ft.TagId);

        // Many-to-Many: GameTag (游戏 - 标签)
        modelBuilder.Entity<GameTag>()
            .HasKey(gt => new { gt.GameId, gt.TagId });

        modelBuilder.Entity<GameTag>()
            .HasOne(gt => gt.Game)
            .WithMany(g => g.Tags)
            .HasForeignKey(gt => gt.GameId);

        modelBuilder.Entity<GameTag>()
            .HasOne(gt => gt.Tag)
            .WithMany(t => t.Games)
            .HasForeignKey(gt => gt.TagId);

        // ShoppingCart One-to-One
        modelBuilder.Entity<ShoppingCart>()
            .HasOne(c => c.User)
            .WithOne(u => u.ShoppingCart)
            .HasForeignKey<ShoppingCart>(c => c.UserId);

        // User Preferences One-to-One
        modelBuilder.Entity<UserPreferences>()
            .HasOne(p => p.User)
            .WithOne(u => u.Preferences)
            .HasForeignKey<UserPreferences>(p => p.UserId);

        // Game Developer (User)
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Developer)
            .WithMany(u => u.GamesDeveloped)
            .HasForeignKey(g => g.DeveloperId);

        // Developer Revenue
        modelBuilder.Entity<DeveloperRevenue>()
            .HasOne(r => r.Developer)
            .WithMany(u => u.Revenues)
            .HasForeignKey(r => r.DeveloperId);

        modelBuilder.Entity<DeveloperRevenue>()
            .HasOne(r => r.Game)
            .WithMany()
            .HasForeignKey(r => r.GameId);

        modelBuilder.Entity<DeveloperRevenue>()
            .HasOne(r => r.Purchase)
            .WithMany()
            .HasForeignKey(r => r.PurchaseId);

        // -------------------------
        // User One-to-One
        // -------------------------
        modelBuilder.Entity<UserPreferences>()
            .HasOne(p => p.User)
            .WithOne(u => u.Preferences)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShoppingCart>()
            .HasOne(c => c.User)
            .WithOne(u => u.ShoppingCart)
            .HasForeignKey<ShoppingCart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // Wishlist
        // -------------------------
        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany(u => u.Wishlists)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // CartItem
        // -------------------------
        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade); // 删除购物车 → 删除项

        // -------------------------
        // Tags (Many-to-Many)
        // -------------------------
        modelBuilder.Entity<FavouriteTags>()
            .HasKey(ft => new { ft.UserId, ft.TagId });

        modelBuilder.Entity<FavouriteTags>()
            .HasOne(ft => ft.User)
            .WithMany(u => u.FavouriteTags)
            .HasForeignKey(ft => ft.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FavouriteTags>()
            .HasOne(ft => ft.Tag)
            .WithMany(t => t.UserTags)
            .HasForeignKey(ft => ft.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------
        // GameTags (Many-to-Many)
        // -------------------------
        modelBuilder.Entity<GameTag>()
            .HasKey(gt => new { gt.GameId, gt.TagId });

        modelBuilder.Entity<GameTag>()
            .HasOne(gt => gt.Game)
            .WithMany(g => g.Tags)
            .HasForeignKey(gt => gt.GameId)
            .OnDelete(DeleteBehavior.Restrict); // 避免删 Game 时把关联 Tag 也删掉

        modelBuilder.Entity<GameTag>()
            .HasOne(gt => gt.Tag)
            .WithMany(t => t.Games)
            .HasForeignKey(gt => gt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------
        // Game
        // -------------------------
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Developer)
            .WithMany(u => u.GamesDeveloped)
            .HasForeignKey(g => g.DeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        // GameMedia
        modelBuilder.Entity<GameMedia>()
            .HasOne(m => m.Game)
            .WithMany(g => g.Media)
            .HasForeignKey(m => m.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------
        // Review
        // -------------------------
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Game)
            .WithMany(g => g.Reviews)
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // Post / Comment / Like
        // -------------------------
        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade); // 删除帖子 → 删除评论

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User 删除 → 各类 Like 级联删除
        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.User)
            .WithMany(u => u.PostLikes)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReviewLike>()
            .HasOne(rl => rl.User)
            .WithMany(u => u.ReviewLikes)
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentLike>()
            .HasOne(cl => cl.User)
            .WithMany(u => u.CommentLikes)
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameLike>()
            .HasOne(gl => gl.User)
            .WithMany(u => u.GameLikes)
            .HasForeignKey(gl => gl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 对象删除 → 相应 Like 级联删除
        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReviewLike>()
            .HasOne(rl => rl.Review)
            .WithMany(r => r.Likes)
            .HasForeignKey(rl => rl.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentLike>()
            .HasOne(cl => cl.Comment)
            .WithMany(c => c.Likes)
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameLike>()
            .HasOne(gl => gl.Game)
            .WithMany(g => g.Likes)
            .HasForeignKey(gl => gl.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------
        // Notification
        // -------------------------
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // Payment
        // -------------------------
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // Purchase
        // -------------------------
        modelBuilder.Entity<Purchase>()
            .HasOne(p => p.User)
            .WithMany(u => u.Purchases)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Purchase>()
            .HasOne(p => p.Game)
            .WithMany(g => g.Purchases)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Purchase>()
            .HasOne(p => p.Payment)
            .WithMany(pay => pay.Purchases)
            .HasForeignKey(p => p.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------
        // DeveloperRevenue
        // -------------------------
        modelBuilder.Entity<DeveloperRevenue>()
            .HasOne(r => r.Developer)
            .WithMany(u => u.Revenues)
            .HasForeignKey(r => r.DeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeveloperRevenue>()
            .HasOne(r => r.Game)
            .WithMany()
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeveloperRevenue>()
            .HasOne(r => r.Purchase)
            .WithMany()
            .HasForeignKey(r => r.PurchaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public bool IsAdmin { get; set; }
    public bool IsDeveloper { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Summary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string Status { get; set; } = "active"; // active / banned

    // Navigation
    public UserPreferences? Preferences { get; set; }
    public ShoppingCart? ShoppingCart { get; set; }

    public List<Wishlist> Wishlists { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Post> Posts { get; set; } = new();
    public List<PostLike> PostLikes { get; set; } = new();
    public List<ReviewLike> ReviewLikes { get; set; } = new();
    public List<CommentLike> CommentLikes { get; set; } = new();
    public List<GameLike> GameLikes { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public List<Purchase> Purchases { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<Game> GamesDeveloped { get; set; } = new();
    public List<FavouriteTags> FavouriteTags { get; set; } = new();
    public List<DeveloperRevenue> Revenues { get; set; } = new();
    public OtpEntry? OtpEntry { get; set; }
}

public class OtpEntry
{
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }  // Foreign key to User table
    [Required]
    public string OtpCode { get; set; }
    public DateTime Expiry { get; set; }
    public User User { get; set; }    // Navigation property
}

public class UserPreferences
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool PublicProfile { get; set; }

    // Nav
    public User User { get; set; } = null!;
}

public class ShoppingCart
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }
    public ShoppingCart Cart { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class Wishlist
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class Tag
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public List<FavouriteTags> UserTags { get; set; } = new();
    public List<GameTag> Games { get; set; } = new();
}

public class FavouriteTags
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}

public class GameTag
{
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}

public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int Rating { get; set; } // 1 to 5
    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int LikeCount { get; set; }
    public List<ReviewLike> Likes { get; set; } = new List<ReviewLike>();
}

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public string Title { get; set; } = null!;

    public string? Thumbnail { get; set; }

    public string Content { get; set; } = null!;

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public int LikeCount { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class PostLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? PostId { get; set; }
    public Post? Post { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class ReviewLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? ReviewId { get; set; }
    public Review? Review { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CommentLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? CommentId { get; set; }
    public Comment? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class GameLike
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    // 新增字段：true = Like，false = Dislike
    public bool IsLike { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Content { get; set; } = null!;
    public int GameId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int LikeCount { get; set; }
    public List<CommentLike> Likes { get; set; } = new List<CommentLike>();

    // 导航属性
    public Game Game { get; set; } = null!;   // 留言属于哪个游戏
}

public class Purchase
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public decimal PriceAtPurchase { get; set; }

    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
}

public class Payment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string TransactionId { get; set; } = null!;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Purchase> Purchases { get; set; } = new();
}

public class Game
{
    public int Id { get; set; }

    public int DeveloperId { get; set; }
    public User Developer { get; set; } = null!;

    [Required]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }

    public DateTime ReleaseDate { get; set; }

    public GameStatus Status { get; set; } = GameStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<GameMedia> Media { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Purchase> Purchases { get; set; } = new();
    public List<GameTag> Tags { get; set; } = new();
    public int LikeCount { get; set; }
    public List<GameLike> Likes { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}

public class GameMedia
{
    public int Id { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!; // "image", "video", etc.

    public int SortOrder { get; set; } = 0;
}

public class DeveloperRevenue
{
    public int Id { get; set; }

    public int DeveloperId { get; set; }
    public User Developer { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;

    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

