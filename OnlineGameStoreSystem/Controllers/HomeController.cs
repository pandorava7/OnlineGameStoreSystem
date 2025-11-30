using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DB db;

    public HomeController(ILogger<HomeController> logger, DB context)
    {
        _logger = logger;
        db = context;
    }

    public IActionResult Index()
    {
        var categories = new List<GameCategoryViewModel>()
        {
            new GameCategoryViewModel {
                Title = "Free to Play",
                Slug = "free-to-play",
                Games = new List<GameViewModel> {
                    new GameViewModel { Title = "Cyberpunk2077", CoverUrl="/images/example/cyberpunk2077.png", Price=0 },
                    new GameViewModel { Title = "Silksong", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Wuthering Waves", CoverUrl="/images/example/WutheringWaves.png", Price=0 },
                    new GameViewModel { Title = "Hollow Knight", CoverUrl="/images/example/hollow knight.png", Price=0 },
                    new GameViewModel { Title = "Clair Obscur: Expedition 33", CoverUrl="/images/example/Expedition33.png", Price=0 },
                    new GameViewModel { Title = "Hollow Knight", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Silksong", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Hollow Knight", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Silksong", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Hollow Knight", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Silksong", CoverUrl="/images/example/silksong.png", Price=0 },
                    new GameViewModel { Title = "Hollow Knight", CoverUrl="/images/example/silksong.png", Price=0 },
                }
            },

            new GameCategoryViewModel {
                Title = "RPG Games",
                Slug = "rpg",
                Games = new List<GameViewModel> {
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                    new GameViewModel { Title="RPG A", CoverUrl="/images/example/silksong.png", Price=59 },
                    new GameViewModel { Title="RPG B", CoverUrl="/images/example/silksong.png", Price=79 },
                }
            }
        };


        var topGamesBySales = db.Games
            .Include(g => g.Media)
            .Select(g => new
            {
                Game = g,
                SalesCount = g.Purchases.Count(p => p.Status == PurchaseStatus.Completed)
            })
            .OrderByDescending(x => x.SalesCount)
            .Take(10)
            .ToList();

        var homeViewModel = new HomeViewModel
        {
            topGamesBySales = topGamesBySales.Select(x => new TopGameViewModel
            {
                Title = x.Game.Title,
                ThumbnailUrl = x.Game.Media
                    .Where(m => m.MediaType == "thumb")
                    .Select(m => m.MediaUrl)
                    .FirstOrDefault() ?? string.Empty,
                ImageUrls = x.Game.Media
                    .Where(m => m.MediaType == "image")
                    .Select(m => m.MediaUrl)
                    .ToList(),
            }).ToList(),

            Categories = categories
        };

        Console.WriteLine("Top 10 Games by Sales:" + topGamesBySales.Count);
        foreach (var item in topGamesBySales)
        {
            Console.WriteLine($"Game: {item.Game.Title}, Sales: {item.SalesCount}");
        }
        foreach (var item in homeViewModel.topGamesBySales)
        {
            foreach (var image in item.ImageUrls)
            {
                Console.WriteLine($"Game: {item.Title}, Image URL: {image}");
            }
        }

        return View(homeViewModel);
    }

    [Route("cart")]
    public IActionResult ShoppingCart()
    {
        // 1. 获取当前用户的 ID (假设是 1)
        int userId = 1;

        // 2. 从数据库获取该用户的购物车
        var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);

        // 如果还没有购物车，给个空的
        if (cart == null)
        {
            return View(new ShoppingCartViewModel
            {
                Items = new List<CartItemViewModel>(),
                TotalPrice = 0
            });
        }

        // 3. ✅ 关键修改：从数据库查询真实的 CartItems (不要用 new List<CartItem>...)
        // 必须加上 .Include(c => c.Game) 否则游戏信息是空的
        var dbItems = db.CartItems
            .Where(c => c.CartId == cart.Id)
            .Include(c => c.Game)
            .ToList();

        // 4. 转换数据格式给页面用
        var viewModelItems = dbItems.Select(item => new CartItemViewModel
        {
            Item = item, // 这里把真实的 ID (比如 5, 6) 传给页面
            ThumbnailUrl = $"/images/example/silksong.png"
        }).ToList();

        var viewModel = new ShoppingCartViewModel
        {
            Items = viewModelItems,
            TotalPrice = viewModelItems.Sum(x => x.Item.Game.Price) // 简单计算总价
        };

        return View();
    }

    [Route("game/{name}")]
    public IActionResult GameDetail(string name)
    {
        // 将 '_' 转回 ' '
        string realName = name.Replace("_", " ");

        // Search game by name from database
        var game = db.Games
            .Include(g => g.Media)
            .Include(g => g.Tags)
            .Include(g => g.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefault(g => g.Title.ToLower() == realName.ToLower());

        Console.WriteLine("Searching for game: " + name);

        if (game == null)
        {
            // Game not found, return 404
            return NotFound();
        }

        var gameDetailViewModel = new GameDetailViewModel
        {
            Id = game.Id,
            Title = game.Title,
            Price = game.Price,
            DiscountPrice = game.DiscountPrice,
            Description = game.Description,
            VideoUrls = game.Media
            .Where(g => g.MediaType == "video")
            .Select(m => m.MediaUrl)
            .ToList(),
            ImageUrls = game.Media
            .Where(g => g.MediaType == "image")
            .Select(m => m.MediaUrl)
            .ToList(),
            Genres = game.Tags.Count > 0 ? game.Tags.Select(t => t.Tag.Name).ToList() : new List<string> { "No Genre" },
            Reviews = game.Reviews
                .Select(r => new Review
                {
                    User = r.User,
                    Rating = r.Rating,
                    Content = r.Content,
                    Likes = r.Likes,
                    CreatedAt = r.CreatedAt
                })
                .ToList()
        };
        return View(gameDetailViewModel);
    }

    




    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
