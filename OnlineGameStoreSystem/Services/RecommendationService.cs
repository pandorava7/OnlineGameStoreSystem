using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Models;
using System;

namespace OnlineGameStoreSystem.Services;

public class RecommendationService
{
    private readonly DB _context;
    const double WishlistWeight = 0.3;     // 愿望单 30%
    const double TagMatchWeight = 0.25;    // 标签兴趣 25%
    const double ViewCountWeight = 0.2;    // 浏览次数 20%
    const double GlobalHotWeight = 0.1;    // 全局热度 10%


    public RecommendationService(DB context)
    {
        _context = context;
    }

    public async Task<List<RecommendedGameDto>> GetRecommendedGames(int userId)
    {
        // 取数据库资料
        var games = await _context.Games
            .Include(g => g.Tags)
            .ThenInclude(gt => gt.Tag)
            .Where(g => g.Status == GameStatus.Published)
            .ToListAsync();

        var wishlist = await _context.Wishlists
                                    .Where(w => w.UserId == userId)
                                    .Select(w => w.GameId)
                                    .ToListAsync();

        var interestTags = await _context.FavouriteTags
                                         .Where(t => t.UserId == userId)
                                         .ToListAsync();

        //var viewRecords = await _context.GameViews
        //                                .Where(v => v.UserId == userId)
        //                                .ToListAsync();

        // Calculate score for each game
        var result = new List<RecommendedGameDto>();

        foreach (var game in games)
        {
            // maximum is 5
            if (result.Count >= 5)
                break;

            double score = 0;

            // 1. wishlist score
            if (wishlist.Contains(game.Id))
                score += 1 * WishlistWeight;

            // 2. tag match and give score
            var gameTags = game.Tags?
                .Where(t => t.Tag != null && t.Tag.Name != null)
                .Select(t => t.Tag.Name)
                .ToList() ?? new List<string>();
            var userTags = interestTags?
                .Where(t => t.Tag != null && t.Tag.Name != null)
                .Select(t => t.Tag.Name)
                .ToList() ?? new List<string>();
            var tagMatch = gameTags.Intersect(userTags).Count();

            if (interestTags != null && interestTags.Count > 0)
                score += ((double)tagMatch / interestTags.Count) * TagMatchWeight;

            // 3. view count score
            //var view = viewRecords.FirstOrDefault(v => v.GameId == game.GameId)?.ViewCount ?? 0;
            //score += NormalizeView(view) * ViewCountWeight;

            // 4. global hot score
            //score += NormalizeHot(game.GlobalViews) * GlobalHotWeight;

            result.Add(new RecommendedGameDto
            {
                GameId = game.Id,
                Title = game.Title,
                Score = score
            });

            // increase exposure count
            // 轮播图推荐，曝光+5
            game.ExposureCount += 5;
            ConsoleHelper.WriteRed($"Increased exposure for game '{game.Title}'. New ExposureCount: {game.ExposureCount}");
        }

        // save exposure count
        await _context.SaveChangesAsync();

        // sort by score
        return result.OrderByDescending(r => r.Score).ToList();
    }

    // 可调节归一化函数：将数值压缩到 0~1 之间
    private double NormalizeView(int viewCount)
    {
        return Math.Min(1, viewCount / 50.0); // 例如浏览 50 次=满分
    }

    private double NormalizeHot(int globalViews)
    {
        return Math.Min(1, globalViews / 1000.0); // 热度1000次=满分
    }


    // RecommendationService 内新增方法
    public async Task<List<GameCategoryViewModel>> GetCategoryRecommendationGamesAsync(int userId, int limit = 20)
    {
        // 取用户数据：愿望单 + 兴趣标签（最多取10个标签）
        var wishlist = await _context.Wishlists
                            .Where(w => w.UserId == userId)
                            .Select(w => w.GameId)
                            .ToListAsync();

        var interestTags = await _context.FavouriteTags
                                 .Where(t => t.UserId == userId)
                                 //.OrderByDescending(t => t.Score) // 如果有分数字段可优先
                                 .Select(t => t.Tag.Name)
                                 .ToListAsync();

        var wishlistSet = new HashSet<int>(wishlist);

        // 计算单个游戏推荐分数（不使用浏览次数）
        double ComputeScore(Game game, List<string> userTagList)
        {
            double score = 0;

            // 1. 愿望单
            if (wishlistSet.Contains(game.Id))
                score += 1 * WishlistWeight;

            // 2. 标签匹配
            var gameTagNames = game.Tags?.Select(gt => gt.Tag.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                               ?? new List<string>();
            int tagMatchCount = 0;
            if (userTagList.Count > 0)
            {
                var userTagsLower = new HashSet<string>(userTagList, StringComparer.OrdinalIgnoreCase);
                tagMatchCount = gameTagNames.Count(t => userTagsLower.Contains(t));
                score += ((double)tagMatchCount / userTagList.Count) * TagMatchWeight;
            }

            // 3. 全局热度
            //if (game.GlobalViews >= 0)
            //{
            //    score += NormalizeHot(game.GlobalViews) * GlobalHotWeight;
            //}

            return score;
        }

        var categories = new List<GameCategoryViewModel>();

        // ---------- 1) Free to play ----------
        var freeGames = await _context.Games
            .Where(g => g.Price <= 0)
            .Include(g => g.Media)
            .Include(g => g.Tags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.Status == GameStatus.Published)
            .Take(20)
            .ToListAsync();

        // increase exposure count
        // 普通推荐，曝光+1
        foreach (var game in freeGames)
        {
            game.ExposureCount += 1;
            ConsoleHelper.WriteRed($"Increased exposure for free game '{game.Title}'. New ExposureCount: {game.ExposureCount}");
        }

        categories.Add(new GameCategoryViewModel
        {
            Title = "Free to Play",
            Slug = "price=Free",
            Games = freeGames
                .Select(g => new { Game = g, Score = ComputeScore(g, interestTags) })
                .OrderByDescending(x => x.Score)
                .Take(limit > 0 ? limit : int.MaxValue)
                .Select(x => new GameViewModel
                {
                    Title = x.Game.Title,
                    CoverUrl = x.Game.Media?.Where(m => m.MediaType == "thumb").Select(m => m.MediaUrl).FirstOrDefault() ?? string.Empty,
                    Price = x.Game.Price,
                    DiscountPrice = x.Game.DiscountPrice
                })
                .ToList()
        });

        // ---------- 2) Discount ----------
        var discountGames = await _context.Games
            .Where(g => g.Price > 0 && g.DiscountPrice.HasValue && g.DiscountPrice < g.Price)
            .Include(g => g.Media)
            .Include(g => g.Tags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.Status == GameStatus.Published)
            .Take(20)
            .ToListAsync();

        // increase exposure count
        // 普通推荐，曝光+1
        foreach (var game in discountGames)
        {
            game.ExposureCount += 1;
            ConsoleHelper.WriteRed($"Increased exposure for discount game '{game.Title}'. New ExposureCount: {game.ExposureCount}");
        }

        categories.Add(new GameCategoryViewModel
        {
            Title = "Discounts",
            Slug = "onlyDiscount=true",
            Games = discountGames
                .Select(g => new { Game = g, Score = ComputeScore(g, interestTags) })
                .OrderByDescending(x => x.Score)
                .Take(limit > 0 ? limit : int.MaxValue)
                .Select(x => new GameViewModel
                {
                    Title = x.Game.Title,
                    CoverUrl = x.Game.Media?.Where(m => m.MediaType == "thumb").Select(m => m.MediaUrl).FirstOrDefault() ?? string.Empty,
                    Price = x.Game.Price,
                    DiscountPrice = x.Game.DiscountPrice
                })
                .ToList()
        });

        // ---------- 3) 基于用户兴趣标签 ----------
        int tagCategoryCount = 0;
        int maxTagCategories = 5; // 最多推荐5个标签分类
        foreach (var tagName in interestTags)
        {
            var tagGames = await _context.Games
                  .Where(g => g.Tags.Any(gt => gt.Tag.Name.ToLower() == tagName.ToLower()))
                  .Include(g => g.Media)
                  .Include(g => g.Tags)
                      .ThenInclude(gt => gt.Tag)
                  .Where(g => g.Status == GameStatus.Published)
                  .Take(20)
                  .ToListAsync();

            if(tagGames.Count == 0)
                continue;
            tagCategoryCount++;
            if (tagCategoryCount > maxTagCategories)
                break;

            // increase exposure count
            // 普通推荐，曝光+1
            foreach (var game in tagGames)
            {
                game.ExposureCount += 1;
                ConsoleHelper.WriteRed($"Increased exposure for game '{game.Title}' due to tag '{tagName}'. New ExposureCount: {game.ExposureCount}");
            }

            categories.Add(new GameCategoryViewModel
            {
                Title = tagName,
                Slug = $"tags={Slugify(tagName)}",
                Games = tagGames
                    .Select(g => new { Game = g, Score = ComputeScore(g, interestTags) })
                    .OrderByDescending(x => x.Score)
                    .Take(limit > 0 ? limit : int.MaxValue)
                    .Select(x => new GameViewModel
                    {
                        Title = x.Game.Title,
                        CoverUrl = x.Game.Media?.Where(m => m.MediaType == "thumb").Select(m => m.MediaUrl).FirstOrDefault() ?? string.Empty,
                        Price = x.Game.Price,
                        DiscountPrice = x.Game.DiscountPrice
                    })
                    .ToList()
            });
        }

        // save exposure count
        await _context.SaveChangesAsync();

        return categories;
    }

    // Slugify 简单实现
    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Trim().Replace(" ", "+");
    }


}

public class RecommendedGameDto
{
    public int GameId { get; set; }
    public string Title { get; set; } = null!;
    public double Score { get; set; }
}