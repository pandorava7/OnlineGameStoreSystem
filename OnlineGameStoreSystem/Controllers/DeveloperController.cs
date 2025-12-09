using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OnlineGameStoreSystem.Controllers;

public class DeveloperController : Controller
{
    private readonly DB db;

    // validation limits (tune to your needs)
    const long MaxThumbnailBytes = 5L * 1024 * 1024;   // 5 MB
    const long MaxPreviewImageBytes = 5L * 1024 * 1024; // 5 MB each
    const long MaxTrailerBytes = 50L * 1024 * 1024;    // 50 MB each
    const long MaxZipBytes = 200L * 1024 * 1024;       // 200 MB
    const int MaxPreviewImages = 8;
    const int MaxTrailers = 2;

    public DeveloperController(DB context)
    {
        db = context;
    }

    // helper: get current user id from claims (returns null if not authenticated/invalid)
    private int? GetCurrentUserId()
    {
        var claim = User?.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(claim)) return null;
        if (int.TryParse(claim, out var id)) return id;
        return null;
    }

    // GET: show registration form
    [HttpGet]
    public IActionResult DeveloperRegister()
    {
        // Render Views/Developer/DeveloperRegister.cshtml
        var vm = new DeveloperRegisterVM();
        return View("DeveloperRegister", vm);
    }

    // POST: Handle submission of the initial registration form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Apply(DeveloperRegisterVM model)
    {
        if (!ModelState.IsValid)
        {
            // return view with validation errors
            return View("DeveloperRegister", model);
        }

        // Get current logged in user id
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            TempData["FlashMessage"] = "Please log in before applying.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Login", "Account");
        }

        // Instead of making them a developer immediately, we redirect to the payment page.
        // We use TempData to pass key info to the next step.
        // Consider using a database to store this temporary registration data for better reliability.
        TempData["Reg_FullName"] = model.FullName;
        TempData["Reg_Email"] = model.NotifyEmail;
        TempData["Reg_UserId"] = userId.Value; // Keep track of which user is registering

        // Redirect to the payment action
        return RedirectToAction("DeveloperPayment");
    }

    // GET: Show the payment confirmation page
    [HttpGet]
    public IActionResult DeveloperPayment()
    {
        // Check if we have the necessary data from the previous step
        if (TempData["Reg_UserId"] == null)
        {
            // If not, redirect back to the start or show an error
            TempData["FlashMessage"] = "Session expired. Please start the registration again.";
            return RedirectToAction("DeveloperRegister");
        }

        // Retrieve data and preserve it for the POST action by calling Keep()
        string fullName = TempData["Reg_FullName"]?.ToString() ?? "";
        string email = TempData["Reg_Email"]?.ToString() ?? "";
        TempData.Keep("Reg_UserId"); // Important: Keep userId for the POST action

        // Pre-fill the payment view model
        var paymentVm = new DeveloperRegistrationViewModel
        {
            DeveloperName = fullName,
            Email = email,
            // Set default values or leave empty
            SelectedPaymentMethod = "Method 1",
            ConfirmPayment = false
        };

        // Render Views/Developer/DeveloperPayment.cshtml
        return View("DeveloperRegisterFee", paymentVm);
    }

    // POST: Process the payment and finalize registration
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcessPayment(DeveloperRegistrationViewModel model)
    {
        // Get the userId passed from the previous steps
        if (TempData["Reg_UserId"] is not int userId)
        {
            TempData["FlashMessage"] = "An error occurred. Please try again.";
            return RedirectToAction("DeveloperRegister");
        }

        if (!model.ConfirmPayment)
        {
            // The user must check the "Confirm" box
            ModelState.AddModelError("ConfirmPayment", "Please confirm your payment to proceed.");
            // Preserve userId again for re-displaying the form
            TempData.Keep("Reg_UserId");
            return View("DeveloperPayment", model);
        }

        // =================================================================
        // SIMULATE PAYMENT PROCESSING HERE
        // In a real app, you would integrate with a payment gateway (e.g., Stripe, PayPal).
        // =================================================================
        bool paymentSuccess = true; // Simulate success

        if (paymentSuccess)
        {
            // Find the user and update their role to 'Developer'
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.IsDeveloper = true;
                // You might also want to store other registration details from the first form here.
                // For example, update FullName, Email, or create a new 'DeveloperProfile' entity.
                user.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Payment successful! You are now a registered developer.";
                // Redirect to the developer console
                return RedirectToAction("Developer");
            }
            else
            {
                // User not found in DB (should be rare)
                TempData["FlashMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }
        }
        else
        {
            // Payment failed
            TempData["FlashMessage"] = "Payment failed. Please try again.";
            TempData.Keep("Reg_UserId");
            return View("DeveloperRegisterFee", model);
        }
    }

    // GET: Handle cancellation
    [HttpGet]
    public IActionResult RegistrationCancelled()
    {
        TempData["FlashMessage"] = "Registration cancelled.";
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Developer()
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var games = await db.Games
            .Where(g => g.DeveloperId == developerId)
            .Include(g => g.Media)
            .Include(g => g.Purchases)
            .Include(g => g.Reviews)
            .ToListAsync();

        var gameVms = games.Select(g => new DeveloperGameItemViewModel
        {
            Id = g.Id,
            Title = g.Title,
            ThumbnailUrl = g.Media.FirstOrDefault(m => m.MediaType == "thumb")?.MediaUrl ?? "/images/example/silksong.png",
            Price = g.Price,
            LikeRate = g.LikeCount > 0 ? Math.Round((double)g.LikeCount / Math.Max(1, g.Purchases.Count) * 100, 0) : 0,
            SalesCount = g.Purchases.Count(p => p.Status == PurchaseStatus.Completed),
            ReleaseDate = g.ReleaseDate,
            UpdatedAt = g.CreatedAt,
            Status = g.Status.ToString()
        }).ToList();

        // aggregate totals from DB data
        var purchasesAll = games.SelectMany(g => g.Purchases).ToList();
        var completedPurchases = purchasesAll.Where(p => p.Status == PurchaseStatus.Completed).ToList();

        var totalSales = completedPurchases.Count;
        var totalRevenue = completedPurchases.Sum(p => p.PriceAtPurchase);
        var totalLikes = games.Sum(g => g.LikeCount);
        var totalReviews = games.Sum(g => g.Reviews?.Count ?? 0);
        var totalDownloads = purchasesAll.Count; // all purchases entries count as downloads
        var totalExposure = totalSales * 200; // rough estimate; replace with real exposure table when available
        var netRevenue = totalRevenue; // keep same for now, apply platform fee if needed

        var vm = new DeveloperDashboardViewModel
        {
            Games = gameVms,
            TotalExposure = totalExposure,
            TotalSales = totalSales,
            TotalRevenue = totalRevenue,
            TotalLikes = totalLikes,
            TotalReviews = totalReviews,
            TotalDownloads = totalDownloads,
            NetRevenue = netRevenue
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData(int hours = 48)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return Unauthorized();

        var end = DateTime.UtcNow;
        var start = end.AddHours(-hours);

        // load purchases for developer in range
        var purchases = await db.Purchases
            .Include(p => p.Game)
            .Include(p => p.Payment) // Ensure Payment is included for CreatedAt
            .Where(p => p.Game.DeveloperId == developerId
                        && p.Payment.CreatedAt >= start && p.Payment.CreatedAt <= end
                        && p.Status == PurchaseStatus.Completed)
            .ToListAsync();

        int intervalHours = hours <= 48 ? 6 : 24;

        var labels = new List<string>();
        var exposureSeries = new List<int>();
        var salesSeries = new List<int>();
        var revenueSeries = new List<decimal>();

        for (var t = start; t <= end; t = t.AddHours(intervalHours))
        {
            var next = t.AddHours(intervalHours);
            labels.Add(t.ToLocalTime().ToString(hours <= 48 ? "HH:mm" : "dd MMM"));

            var bucket = purchases.Where(p => p.Payment.CreatedAt >= t && p.Payment.CreatedAt < next).ToList();
            var sales = bucket.Count;
            var revenue = bucket.Sum(p => p.PriceAtPurchase);

            salesSeries.Add(sales);
            revenueSeries.Add(revenue);
            exposureSeries.Add(sales * 200); // rough estimate if no exposure table
        }

        return Json(new
        {
            labels,
            exposure = exposureSeries,
            sales = salesSeries,
            revenue = revenueSeries
        });
    }

    [HttpGet]
    public async Task<IActionResult> GenerateReport()
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var games = await db.Games
            .Where(g => g.DeveloperId == developerId)
            .Include(g => g.Purchases)
            .Include(g => g.Reviews)
            .Include(g => g.Media)
            .ToListAsync();

        // Per-game stats and totals
        var sb = new StringBuilder();
        sb.AppendLine("GameId,Title,Sales,Revenue,Likes,Reviews");

        int totalSales = 0;
        decimal totalRevenue = 0;
        int totalLikes = 0;
        int totalReviews = 0;

        foreach (var g in games)
        {
            var sales = g.Purchases.Count(p => p.Status == PurchaseStatus.Completed);
            var revenue = g.Purchases.Where(p => p.Status == PurchaseStatus.Completed).Sum(p => p.PriceAtPurchase);
            var likes = g.LikeCount;
            var reviews = g.Reviews.Count;

            totalSales += sales;
            totalRevenue += revenue;
            totalLikes += likes;
            totalReviews += reviews;

            // escape title
            var titleEscaped = g.Title?.Replace("\"", "\'\"") ?? string.Empty;
            sb.AppendLine($"{g.Id},\"{titleEscaped}\",{sales},{revenue},{likes},{reviews}");
        }

        sb.AppendLine();
        sb.AppendLine($",Totals,{totalSales},{totalRevenue},{totalLikes},{totalReviews}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"developer_report_{developerId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    // GET: DeveloperUploadGame (optional id for editing)
    [HttpGet]
    public async Task<IActionResult> DeveloperUploadGame(int? id)
    {
        // view will use javascript to fetch data if id present
        return View();
    }

    // AJAX: get game details and media urls
    [HttpGet]
    public async Task<IActionResult> GetGame(int id)
    {
        var game = await db.Games
            .Include(g => g.Media)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null) return NotFound();

        var dto = new
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Status = game.Status.ToString(),
            Media = game.Media.Select(m => new { m.MediaUrl, m.MediaType, m.SortOrder }).OrderBy(m => m.SortOrder).ToList()
        };

        return Json(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeveloperUploadGame(string Title, string ShortDescription, string DetailDescription, decimal? Price, bool? IsFree)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var files = Request.Form.Files;
        var errors = new List<string>();

        if (files != null && files.Count > 0)
        {
            // counts for multi-file inputs
            var previewCount = files.Count(f => f.Name == "PreviewImages");
            var trailerCount = files.Count(f => f.Name == "Trailers");
            var thumbnailCount = files.Count(f => f.Name == "Thumbnail");
            var zipCount = files.Count(f => f.Name == "GameZip");

            if (previewCount > MaxPreviewImages) errors.Add($"Preview images: maximum {MaxPreviewImages} files allowed.");
            if (trailerCount > MaxTrailers) errors.Add($"Trailer videos: maximum {MaxTrailers} files allowed.");
            if (thumbnailCount > 1) errors.Add("Only one thumbnail is allowed.");
            if (zipCount > 1) errors.Add("Only one game ZIP is allowed.");

            foreach (var f in files)
            {
                if (f == null || f.Length == 0) { errors.Add($"File \"{f?.FileName}\" is empty."); continue; }

                if (f.Name == "Thumbnail")
                {
                    if (!f.ContentType?.StartsWith("image/") ?? true) errors.Add("Thumbnail must be an image.");
                    if (f.Length > MaxThumbnailBytes) errors.Add($"Thumbnail \"{f.FileName}\" exceeds {MaxThumbnailBytes / (1024 * 1024)} MB.");
                }
                else if (f.Name == "PreviewImages")
                {
                    if (!f.ContentType?.StartsWith("image/") ?? true) errors.Add($"Preview image \"{f.FileName}\" must be an image.");
                    if (f.Length > MaxPreviewImageBytes) errors.Add($"Preview image \"{f.FileName}\" exceeds {MaxPreviewImageBytes / (1024 * 1024)} MB.");
                }
                else if (f.Name == "Trailers")
                {
                    if (!f.ContentType?.StartsWith("video/") ?? true) errors.Add($"Trailer \"{f.FileName}\" must be a video.");
                    if (f.Length > MaxTrailerBytes) errors.Add($"Trailer \"{f.FileName}\" exceeds {MaxTrailerBytes / (1024 * 1024)} MB.");
                }
                else if (f.Name == "GameZip")
                {
                    var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();
                    if (ext != ".zip") errors.Add($"Game package \"{f.FileName}\" must be a .zip file.");
                    if (f.Length > MaxZipBytes) errors.Add($"Game package \"{f.FileName}\" exceeds {MaxZipBytes / (1024 * 1024)} MB.");
                }
                else
                {
                    // Optional: reject unexpected file inputs
                    // errors.Add($"Unexpected file input name: {f.Name}");
                }
            }
        }

        if (errors.Any())
        {
            // pass errors back to the GET view (simple string list). Adjust how you display in the Razor.
            TempData["UploadErrors"] = string.Join("||", errors);
            // preserve entered text fields in TempData if you want (optional)
            TempData["Title"] = Title;
            TempData["ShortDescription"] = ShortDescription;
            TempData["DetailDescription"] = DetailDescription;
            return RedirectToAction("DeveloperUploadGame");
        }

        // Create game entity
        var game = new Game
        {
            Title = Title ?? "Untitled",
            Description = DetailDescription ?? ShortDescription ?? string.Empty,
            Price = (IsFree ?? false) ? 0 : (Price ?? 0),
            ReleaseDate = DateTime.UtcNow,
            Status = GameStatus.Pending,
            DeveloperId = developerId.Value,
            CreatedAt = DateTime.UtcNow
        };

        db.Games.Add(game);
        await db.SaveChangesAsync(); // get game.Id

        // save uploaded files (same as before)
        if (files != null && files.Count > 0)
        {
            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "games", game.Id.ToString());
            Directory.CreateDirectory(uploadRoot);

            int sortOrder = 0;
            foreach (var f in files)
            {
                if (f == null || f.Length == 0) continue;

                var ext = Path.GetExtension(f.FileName);
                var fileName = Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(uploadRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await f.CopyToAsync(stream);
                }

                var relativeUrl = $"/uploads/games/{game.Id}/{fileName}";
                string mediaType = "image";

                if (f.Name == "Thumbnail") mediaType = "thumb";
                else if (f.Name == "GameZip") mediaType = "zip";
                else if (f.ContentType != null && f.ContentType.StartsWith("video")) mediaType = "video";

                var gm = new GameMedia
                {
                    GameId = game.Id,
                    MediaUrl = relativeUrl,
                    MediaType = mediaType,
                    SortOrder = sortOrder++
                };

                db.GameMedia.Add(gm);
            }

            await db.SaveChangesAsync();
        }

        // TODO: handle tags linking when tag UI is implemented.

        return RedirectToAction("Developer");
    }

    [HttpGet]
    // [Authorize(Roles = "Developer")] // enable if you have authentication/roles
    public async Task<IActionResult> DeveloperGameDetail(int id)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var game = await db.Games
            .Include(g => g.Media)
            .Include(g => g.Purchases)
            .Include(g => g.Reviews)
            .FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId);

        if (game == null) return NotFound();

        var vm = new DeveloperGameDetailViewModel
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Status = game.Status.ToString(),
            ReleaseDate = game.ReleaseDate,
            LikeCount = game.LikeCount,
            SalesCount = game.Purchases.Count(p => p.Status == PurchaseStatus.Completed),
            ReviewsCount = game.Reviews?.Count ?? 0,
            Media = game.Media.OrderBy(m => m.SortOrder)
                             .Select(m => new GameMediaItem { MediaUrl = m.MediaUrl, MediaType = m.MediaType, SortOrder = m.SortOrder })
                             .ToList()
        };

        return View(vm);
    }

    // POST: Delete game
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var game = await db.Games
            .Include(g => g.Media)
            .Include(g => g.Purchases)
            .Include(g => g.Reviews)
            .FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId.Value);

        if (game == null)
            return NotFound();

        // Remove files on disk (uploads/games/{game.Id})
        try
        {
            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "games", game.Id.ToString());
            if (Directory.Exists(uploadRoot))
            {
                Directory.Delete(uploadRoot, true);
            }
        }
        catch (Exception ex)
        {
            // don't throw to user; log if you have a logger. Continue with DB cleanup.
            Console.WriteLine("[dev] Error deleting upload folder: " + ex);
        }

        // Remove related DB rows (be explicit)
        if (game.Media != null && game.Media.Any()) db.GameMedia.RemoveRange(game.Media);
        if (game.Purchases != null && game.Purchases.Any()) db.Purchases.RemoveRange(game.Purchases);
        if (game.Reviews != null && game.Reviews.Any()) db.Reviews.RemoveRange(game.Reviews);

        db.Games.Remove(game);
        await db.SaveChangesAsync();

        TempData["FlashMessage"] = "Game removed successfully.";
        TempData["FlashMessageType"] = "success";
        return RedirectToAction("Developer");
    }

    [HttpGet]
    public async Task<IActionResult> SalesDetail()
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var games = await db.Games
            .Where(g => g.DeveloperId == developerId.Value)
            .Include(g => g.Media)
            .Include(g => g.Purchases).ThenInclude(p => p.Payment)
            .ToListAsync();

        var vm = new SalesDetailViewModel();

        foreach (var g in games)
        {
            var completed = g.Purchases.Where(p => p.Status == PurchaseStatus.Completed).ToList();
            var sales = completed.Count;
            var gross = completed.Sum(p => p.PriceAtPurchase);
            // example platform fee 33% (adjust as required)
            var fee = Math.Round(gross * 0.33m, 2);
            var net = gross - fee;

            vm.Items.Add(new SalesDetailItem
            {
                GameId = g.Id,
                Title = g.Title,
                ThumbnailUrl = g.Media.Where(m => m.MediaType == "thumb").OrderBy(m => m.SortOrder).Select(m => m.MediaUrl).FirstOrDefault() ?? "/images/default-thumbnail.png",
                SalesCount = sales,
                Gross = gross,
                PlatformFee = fee,
                Net = net
            });
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Analysis(int id)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null) return RedirectToAction("Login", "Account");

        var game = await db.Games
            .Include(g => g.Media)
            .FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId.Value);

        if (game == null) return NotFound();

        var vm = new GameAnalysisViewModel
        {
            GameId = game.Id,
            Title = game.Title,
            ThumbnailUrl = game.Media.Where(m => m.MediaType == "thumb").OrderBy(m => m.SortOrder).Select(m => m.MediaUrl).FirstOrDefault() ?? "/images/default-thumbnail.png"
        };

        return View("Analysis", vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetGameData(int id, int hours = 48)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null) return Unauthorized();

        // ensure developer owns the game
        var gameExists = await db.Games.AnyAsync(g => g.Id == id && g.DeveloperId == developerId.Value);
        if (!gameExists) return Forbid();

        var end = DateTime.UtcNow;
        var start = end.AddHours(-hours);

        var purchases = await db.Purchases
            .Include(p => p.Payment)
            .Where(p => p.GameId == id && p.Status == PurchaseStatus.Completed && p.Payment.CreatedAt >= start && p.Payment.CreatedAt <= end)
            .ToListAsync();

        int intervalHours = hours <= 48 ? 6 : 24;
        var labels = new List<string>();
        var exposureSeries = new List<int>();
        var salesSeries = new List<int>();
        var revenueSeries = new List<decimal>();

        for (var t = start; t <= end; t = t.AddHours(intervalHours))
        {
            var next = t.AddHours(intervalHours);
            labels.Add(t.ToLocalTime().ToString(hours <= 48 ? "HH:mm" : "dd MMM"));

            var bucket = purchases.Where(p => p.Payment.CreatedAt >= t && p.Payment.CreatedAt < next).ToList();
            var sales = bucket.Count;
            var revenue = bucket.Sum(p => p.PriceAtPurchase);

            salesSeries.Add(sales);
            revenueSeries.Add(revenue);
            exposureSeries.Add(sales * 200); // same exposure heuristic
        }

        return Json(new
        {
            labels,
            exposure = exposureSeries,
            sales = salesSeries,
            revenue = revenueSeries
        });
    }

    [HttpGet]
    public async Task<IActionResult> GenerateGameReport(int id)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null) return RedirectToAction("Login", "Account");

        var game = await db.Games
            .Include(g => g.Purchases)
                .ThenInclude(p => p.Payment)
            .FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId.Value);

        if (game == null) return NotFound();

        // Summarize purchases
        var completed = game.Purchases.Where(p => p.Status == PurchaseStatus.Completed).ToList();
        var totalSales = completed.Count;
        var totalRevenue = completed.Sum(p => p.PriceAtPurchase);

        var sb = new StringBuilder();
        sb.AppendLine("GameId,Title,TotalSales,TotalRevenue");
        var titleEscaped = game.Title?.Replace("\"", "\"\"") ?? string.Empty;
        sb.AppendLine($"{game.Id},\"{titleEscaped}\",{totalSales},{totalRevenue}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"game_report_{game.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    #region Edit Game

    // GET: Developer/DeveloperEditGame/{id}
    // 显示编辑游戏页面
    [HttpGet]
    public async Task<IActionResult> DeveloperEditGame(int id)
    {
        // 1. 获取当前开发者 ID 并验证登录
        var developerId = GetCurrentUserId();
        if (developerId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // 2. 查询游戏，确保该游戏属于当前开发者，并加载媒体信息
        var game = await db.Games
            .Include(g => g.Media)
            .FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId.Value);

        // 3. 如果找不到游戏或不属于该开发者，返回 404
        if (game == null)
        {
            return NotFound();
        }

        // 4. 将实体映射到 ViewModel
        var vm = new DeveloperGameDetailViewModel
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Status = game.Status.ToString(),
            // 获取缩略图 URL
            ThumbnailUrl = game.Media.FirstOrDefault(m => m.MediaType == "thumb")?.MediaUrl,
            // 获取其他媒体资源并排序
            Media = game.Media
                .Where(m => m.MediaType != "thumb") // 排除缩略图
                .OrderBy(m => m.SortOrder)
                .Select(m => new GameMediaItem { MediaUrl = m.MediaUrl, MediaType = m.MediaType, SortOrder = m.SortOrder })
                .ToList()
        };

        // 5. 返回视图
        return View(vm);
    }

    // POST: Developer/DeveloperEditGame
    // 处理编辑游戏的表单提交
    [HttpPost]
    [ValidateAntiForgeryToken] // 防止 CSRF 攻击
    public async Task<IActionResult> DeveloperEditGame(DeveloperGameDetailViewModel model,
        IFormFile? Thumbnail, // 新上传的缩略图
        List<IFormFile> newPreviewImages, // 新上传的预览图列表
        List<IFormFile> newTrailers, // 新上传的预告片列表
        IFormFile? GameZip, // 新上传的游戏文件包
        string DeletedMediaJson // 前端传来的需要删除的媒体 URL JSON 字符串
        )
    {
        // 1. 验证当前开发者
        var developerId = GetCurrentUserId();
        if (developerId == null) return RedirectToAction("Login", "Account");

        // 2. 查询数据库中要修改的游戏
        var game = await db.Games
            .Include(g => g.Media)
            .FirstOrDefaultAsync(g => g.Id == model.Id && g.DeveloperId == developerId.Value);

        if (game == null) return NotFound();

        // 3. 更新基本信息
        game.Title = model.Title;
        game.Description = model.Description;
        game.Price = model.IsFree ? 0 : model.Price; // 如果勾选免费，价格设为0
                                                     // 注意：通常不在这里修改 Status，除非有特定的业务逻辑

        // 4. 处理媒体文件更新

        // a. 处理要删除的媒体文件
        if (!string.IsNullOrEmpty(DeletedMediaJson))
        {
            try
            {
                // 反序列化前端传来的 URL 列表
                var mediaUrlsToDelete = JsonSerializer.Deserialize<List<string>>(DeletedMediaJson);
                if (mediaUrlsToDelete != null && mediaUrlsToDelete.Any())
                {
                    // 找出需要删除的数据库记录
                    var mediaToDelete = game.Media.Where(m => mediaUrlsToDelete.Contains(m.MediaUrl)).ToList();

                    foreach (var media in mediaToDelete)
                    {
                        // [重要] 在这里添加从服务器磁盘/云存储删除实际文件的代码
                        // DeleteFileFromServer(media.MediaUrl); 

                        // 从数据库上下文中移除
                        db.GameMedia.Remove(media);
                    }
                }
            }
            catch (JsonException)
            {
                // 处理 JSON 解析错误（可选）
                ModelState.AddModelError("", "Invalid deleted media data.");
            }
        }

        // b. 处理新上传的缩略图 (替换旧的)
        if (Thumbnail != null)
        {
            // 找到旧的缩略图记录
            var oldThumbnail = game.Media.FirstOrDefault(m => m.MediaType == "thumb");
            if (oldThumbnail != null)
            {
                // [重要] 删除旧文件
                // DeleteFileFromServer(oldThumbnail.MediaUrl);
                db.GameMedia.Remove(oldThumbnail);
            }

            // [重要] 上传新文件并获取 URL
            // string newThumbnailUrl = await UploadFile(Thumbnail, "thumbnails");
            string newThumbnailUrl = "/images/placeholder-new-thumb.jpg"; // 模拟 URL

            // 添加新记录
            game.Media.Add(new GameMedia { MediaType = "thumb", MediaUrl = newThumbnailUrl, SortOrder = 0 });
        }

        // 计算现有媒体的最大排序值，以便追加新媒体
        int nextSortOrder = (game.Media.Any(m => m.MediaType != "thumb") ? game.Media.Where(m => m.MediaType != "thumb").Max(m => m.SortOrder) : 0) + 1;

        // c. 处理新上传的预览图
        if (newPreviewImages != null)
        {
            foreach (var file in newPreviewImages)
            {
                // [重要] 上传文件
                // string url = await UploadFile(file, "previews");
                string url = "/images/placeholder-new-preview.jpg"; // 模拟 URL
                game.Media.Add(new GameMedia { MediaType = "image", MediaUrl = url, SortOrder = nextSortOrder++ });
            }
        }

        // d. 处理新上传的预告片
        if (newTrailers != null)
        {
            foreach (var file in newTrailers)
            {
                // [重要] 上传文件
                // string url = await UploadFile(file, "trailers");
                string url = "/videos/placeholder-new-trailer.mp4"; // 模拟 URL
                game.Media.Add(new GameMedia { MediaType = "video", MediaUrl = url, SortOrder = nextSortOrder++ });
            }
        }

        //// 判断用户是否在表单中选择了新的 ZIP 文件
        //if (GameZip != null && GameZip.Length > 0)
        //{
        //    // 这里的逻辑是“作假”：我们不实际保存用户上传的大文件。
        //    // 无论用户上传了什么，我们都把数据库里的路径指向一个固定的“示例文件”。

        //    // 1. 定义一个固定的示例文件路径。
        //    // 建议：在您的项目的 wwwroot/uploads/games/ 目录下实际放一个名为 example.zip 的空文件，
        //    // 这样如果以后有下载功能，不会报 404 错误。
        //    string fakeGameFilePath = "/uploads/games/example.zip";

        //    // 2. 更新数据库字段：指向假文件路径
        //    game.GameFilePath = fakeGameFilePath;

        //    // 3. 更新数据库字段：将文件所需存储空间设置为 0 MB
        //    // (根据您之前的迁移记录，您的 Game 表里应该有 StorageRequireMB 这个 int 字段)
        //    if (typeof(Game).GetProperty("StorageRequireMB") != null)
        //    {
        //        // 使用反射或者直接赋值 (如果您确定Game.cs里有这个属性)
        //        // game.StorageRequireMB = 0;
        //        // 为了安全起见，如果暂时没有Game.cs定义，我先注释掉直接赋值的方式，用动态方式演示意图：
        //        db.Entry(game).Property("StorageRequireMB").CurrentValue = 0;
        //    }

        //    // 选项：通常更新了游戏文件后，需要把状态改回“待审核”
        //    // game.Status = GameStatus.PendingReview;
        //}

        // 6. 保存所有更改到数据库
        await db.SaveChangesAsync();

        // 7. 添加成功提示消息并重定向
        TempData["SuccessMessage"] = "Game updated successfully!";
        // 重定向回开发者仪表盘或游戏详情页
        return RedirectToAction("Developer");
    }

    #endregion
}
