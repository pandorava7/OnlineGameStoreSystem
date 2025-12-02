using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Security.Claims;

namespace OnlineGameStoreSystem.Controllers
{
    public class ProfileController : Controller
    {
        private readonly DB db;
        private readonly IHttpContextAccessor http;

        public ProfileController(DB context, IHttpContextAccessor accessor)
        {
            db = context;
            http = accessor;
        }

        public IActionResult Index()
        {
            // 从 Claims 取 UserId
            var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account"); // 未登入 → 返回登入页

            int userId = int.Parse(userIdString);

            // 找到当前登入的 user
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // 找 Favorite Tags
            var favTags = db.FavouriteTags
                             .Where(f => f.UserId == userId)
                             .Select(f => f.Tag)
                             .ToList();
            // 获取用户留言
            var comments = db.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Game)
                .ThenInclude(g => g.Media)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentItem
                {
                    GameId = c.Game.Id,
                    GameTitle = c.Game.Title,
                    CoverUrl = c.Game.Media
                        .Where(m => m.MediaType == "image")
                        .OrderBy(m => m.SortOrder)
                        .Select(m => m.MediaUrl)
                        .FirstOrDefault() ?? "/images/default-cover.jpg",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToList();

        var purchasedGames = db.Purchases
        .Where(p => p.UserId == userId && p.Status == PurchaseStatus.Completed)
        .Join(
            db.Games,
            p => p.GameId,
            g => g.Id,
            (p, g) => new PurchasedGameVM
            {
                GameId = g.Id,
                Title = g.Title,
                CoverUrl = g.Media
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.MediaUrl)
                    .FirstOrDefault() ?? "/img/no-cover.png",
                PriceAtPurchase = p.PriceAtPurchase,
                PurchasedAt = p.Payment.CreatedAt
            }
        )
        .ToList();

            // 建立 ViewModel（跟你要求的完全一致）
            var vm = new ProfileViewModel
            {
                AvatarUrl = user.AvatarUrl,
                Username = user.Username,
                Summary = string.IsNullOrEmpty(user.Summary) ? "Empty" : user.Summary,
                IsDeveloper = user.IsDeveloper,
                CreatedAt = user.CreatedAt,
                FavoriteTags = favTags,
                UserComments = comments,
                PurchasedGames = purchasedGames,

                GameLibraryUrl = Url.Action("Library", "Game", new { id = user.Id }),
                FavoriteTagGamesUrl = Url.Action("FavoriteTags", "Game", new { id = user.Id }),
                PrivacySettingsUrl = Url.Action("Settings", "Profile", new { id = user.Id }),
                GameRatingUrl = Url.Action("MyRatings", "Game", new { id = user.Id }),
                EditProfileUrl = Url.Action("Edit", "Profile", new { id = user.Id })
            };

            return View(vm);
        }



    }

}
