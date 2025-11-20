using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            var hasher = new PasswordHasher<IdentityUser>();
            var user = new IdentityUser();

            // 这里填你想要的密码，例如 "Test123!"
            var passwordHash = hasher.HashPassword(user, "123");

            Console.WriteLine(passwordHash);

            var categories = new List<GameCategoryViewModel>()
            {
                new GameCategoryViewModel {
                    Title = "Free to Play",
                    Slug = "free-to-play",
                    Games = new List<GameViewModel> {
                        new GameViewModel { Title = "Game A", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game B", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game A", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game B", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game A", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game B", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game A", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game B", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game A", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game B", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game A", CoverUrl="/images/example/silksong.png", Price=0 },
                        new GameViewModel { Title = "Game B", CoverUrl="/images/example/silksong.png", Price=0 },
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


            return View(categories);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
