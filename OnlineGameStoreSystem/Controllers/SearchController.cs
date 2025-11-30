using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using System;

public class SearchController : Controller
{
    private readonly DB _db;

    public SearchController(DB db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? term, string? price, bool hideFree = false, bool onlyDiscount = false, bool hideOwned = false, bool hideWishlist = false, string? tags = null)
    {
        var currentUserId = User.GetUserId();

        var query = _db.Games
            .Include(g => g.Media)
            .Include(g => g.Tags).ThenInclude(gt => gt.Tag)
            .Include(g => g.Reviews)
            .Include(g => g.Developer)
            .AsQueryable();

        // ---------------- 搜索词 ----------------
        if (!string.IsNullOrWhiteSpace(term))
        {
            string t = term.ToLower();
            query = query.Where(g => g.Title.ToLower().Contains(t) || g.Developer.Username.ToLower().Contains(t));
        }

        // ---------------- 价格 ----------------
        if (!string.IsNullOrWhiteSpace(price) && price != "Any Price")
        {
            decimal upper = price switch
            {
                "under RM15" => 15m,
                "under RM30" => 30m,
                "under RM60" => 60m,
                "under RM150" => 150m,
                "under RM300" => 300m,
                _ => decimal.MaxValue
            };
            query = query.Where(g => (g.DiscountPrice ?? g.Price) <= upper);
        }

        // ---------------- 开关 ----------------
        if (hideFree)
            query = query.Where(g => (g.DiscountPrice ?? g.Price) > 0);
        if (onlyDiscount)
            query = query.Where(g => g.DiscountPrice.HasValue && g.DiscountPrice.Value < g.Price);
        if (hideOwned)
            query = query.Where(g => !_db.Purchases.Any(p => p.UserId == currentUserId && p.GameId == g.Id));
        if (hideWishlist)
            query = query.Where(g => !_db.Wishlists.Any(w => w.UserId == currentUserId && w.GameId == g.Id));

        // ---------------- 标签 ----------------
        if (!string.IsNullOrWhiteSpace(tags))
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower()).ToList();
            query = query.Where(g => g.Tags.Any(gt => tagList.Contains(gt.Tag.Name.ToLower())));
        }

        // ---------------- 投影结果 ----------------
        var vm = new SearchPageVM
        {
            SearchTerm = term ?? "",

            Results = await query
            .Select(g => new SearchResultVM
            {
                Title = g.Title,
                Cover = g.Media
                    .FirstOrDefault(m => m.MediaType == "thumb")!.MediaUrl,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                DiscountPrice = g.DiscountPrice,
                PositiveRate = g.Likes.Count == 0
                    ? 0
                    : (double)g.Likes.Count(l => l.IsLike) / g.Likes.Count,
                DeveloperId = g.DeveloperId,
                DeveloperName = g.Developer.Username
            })
            .ToListAsync()
        };

        return View("/Views/Home/Search.cshtml", vm);
    }
}
