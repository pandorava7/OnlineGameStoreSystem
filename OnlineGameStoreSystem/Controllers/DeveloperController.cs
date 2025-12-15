using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OnlineGameStoreSystem.Controllers;

[RequireDeveloper]
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

    #region Developer Dashboard
    public async Task<IActionResult> Developer()
    {
        var user = db.Users.FirstOrDefault(u => u.Id == User.GetUserId());
        if (user == null || !user.IsDeveloper)
        {
            TempData["FlashMessage"] = "You must be a registered developer to access this page.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Index", "Home");
        }

        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var games = await db.Games
            .Where(g => g.DeveloperId == developerId && g.Status != GameStatus.Removed)
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
        var totalExposure = completedPurchases.Sum(p => p.Game.ExposureCount);
        var totalLikes = games.Sum(g => g.LikeCount);
        var totalReviews = games.Sum(g => g.Reviews?.Count ?? 0);
        var totalDownloads = purchasesAll.Count; // all purchases entries count as downloads
        var netRevenue = totalRevenue; // keep same for now, apply platform fee if needed

        var vm = new DeveloperDashboardViewModel
        {
            UserName = user.Username,
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

        var purchases = await db.Purchases
            .Include(p => p.Game)
            .Include(p => p.Payment)
            .Where(p => p.Game.DeveloperId == developerId
                        && p.Payment.CreatedAt >= start && p.Payment.CreatedAt <= end
                        && p.Status == PurchaseStatus.Completed)
            .ToListAsync();

        List<string> labels = new List<string>();
        List<int> exposureSeries = new List<int>();
        List<int> salesSeries = new List<int>();
        List<decimal> revenueSeries = new List<decimal>();

        if (hours >= 8760)
        {
            var now = DateTime.UtcNow;
            var monthlyData = new Dictionary<string, List<Purchase>>();

            for (int i = 11; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var key = $"{monthStart.Year}-{monthStart.Month:D2}";
                monthlyData[key] = new List<Purchase>();
            }

            foreach (var p in purchases)
            {
                var key = $"{p.Payment.CreatedAt.Year}-{p.Payment.CreatedAt.Month:D2}";
                if (monthlyData.ContainsKey(key))
                    monthlyData[key].Add(p);
            }

            foreach (var kv in monthlyData.OrderBy(k => k.Key))
            {
                labels.Add(kv.Key);
                salesSeries.Add(kv.Value.Count);
                revenueSeries.Add(kv.Value.Sum(p => p.PriceAtPurchase));

                // 用 Game.ExposureCount 总数
                var exposureSum = kv.Value.Sum(p => p.Game.ExposureCount);
                exposureSeries.Add(exposureSum);
            }
        }
        else
        {
            var intervalHours = hours <= 48 ? 6 : 24;
            var grouped = purchases
                .GroupBy(p =>
                {
                    var delta = (p.Payment.CreatedAt - start).TotalHours;
                    return Math.Floor(delta / intervalHours);
                })
                .ToDictionary(g => (int)g.Key, g => g.ToList());

            for (int i = 0; i <= Math.Ceiling(hours / (double)intervalHours); i++)
            {
                var t = start.AddHours(i * intervalHours);
                labels.Add(t.ToString("o")); // UTC 时间 ISO 8601

                if (grouped.TryGetValue(i, out var bucket))
                {
                    salesSeries.Add(bucket.Count);
                    revenueSeries.Add(bucket.Sum(p => p.PriceAtPurchase));
                    exposureSeries.Add(bucket.Sum(p => p.Game.ExposureCount));
                }
                else
                {
                    salesSeries.Add(0);
                    revenueSeries.Add(0);
                    exposureSeries.Add(0);
                }
            }
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
        int totalExposure = 0;

        foreach (var g in games)
        {
            var sales = g.Purchases.Count(p => p.Status == PurchaseStatus.Completed);
            var revenue = g.Purchases.Where(p => p.Status == PurchaseStatus.Completed).Sum(p => p.PriceAtPurchase);
            var likes = g.LikeCount;
            var reviews = g.Reviews.Count;
            var exposure = g.ExposureCount;

            totalSales += sales;
            totalRevenue += revenue;
            totalLikes += likes;
            totalReviews += reviews;
            totalExposure += exposure;

            // escape title
            var titleEscaped = g.Title?.Replace("\"", "\'\"") ?? string.Empty;
            sb.AppendLine($"{g.Id},\"{titleEscaped}\",{sales},{revenue},{likes},{reviews},{exposure}");
        }

        sb.AppendLine();
        sb.AppendLine($",Totals,{totalSales},{totalRevenue},{totalLikes},{totalReviews},{totalExposure}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"developer_report_{developerId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    #endregion

    #region Game Publishing 
    // GET: 显示发布页面
    [HttpGet]
    public IActionResult DeveloperUploadGame()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    public async Task<IActionResult> DeveloperUploadGame(GamePublishViewModel model)
    {
        // 1. 获取当前开发者 ID 并验证登录
        var developerId = User.GetUserId();
        if (developerId == -1)
            return RedirectToAction("Login", "Account");

        var errors = new List<string>();

        if (!ModelState.IsValid)
        {
            ModelStateHelper.LogErrors(ModelState);
            return View(model);
        }

        // =================== 1. 模型验证 ===================
        ValidateGameModel(model, errors);

        if (errors.Any())
        {
            AddErrorsToModelState(errors);
            return View(model);
        }

        // =================== 2. 文件上传验证 ===================
        var files = Request.Form.Files;
        ValidateGameFiles(files, errors);

        if (errors.Any())
        {
            AddErrorsToModelState(errors);
            return View(model);
        }

        // =================== 3. 创建游戏记录 ===================
        var game = new Game
        {
            Title = model.Title ?? "Untitled",
            ShortDescription = model.ShortDescription,
            DetailDescription = model.DetailDescription,
            Price = model.IsFree ? 0 : model.Price ?? 0,
            ReleaseDate = DateTime.UtcNow,
            Status = GameStatus.Pending,
            DeveloperId = developerId,
            CreatedAt = DateTime.UtcNow,
            StorageRequireMB = 0, // will update later if needed
        };

        db.Games.Add(game);
        await db.SaveChangesAsync(); // 保存以获取游戏ID

        // =================== 4. 添加标签 ===================
        if (model.Tags != null && model.Tags.Count > 0)
        {
            foreach (var tagName in model.Tags.Distinct().Take(10))
            {
                var tag = db.Tags.FirstOrDefault(t => t.Name == tagName);

                var gameTag = new GameTag
                {
                    GameId = game.Id,
                    TagId = tag != null ? tag.Id : 0
                };
                db.GameTags.Add(gameTag);
            }
            await db.SaveChangesAsync();
        }

        // =================== 5. 保存文件 ===================
        if (files != null && files.Count > 0)
        {
            await SaveGameFilesAsync(game.Id, files);
        }

        TempData["FlashMessage"] = "Game Uploaded Successfully";
        TempData["FlashMessageType"] = "success";
        return RedirectToAction("Developer");
    }

    // -------------------- 辅助方法 --------------------

    // 模型验证
    private void ValidateGameModel(GamePublishViewModel model, List<string> errors)
    {
        if (!model.IsFree && (!model.Price.HasValue || model.Price <= 0))
            errors.Add("Sold Price must be greater than 0, or 'Be Free' checkbox must be selected.");

        if (model.Tags == null || model.Tags.Count < 1 || model.Tags.Count > 10)
            errors.Add("Please provide between 1 and 10 tags.");
    }

    // 文件验证
    private void ValidateGameFiles(IFormFileCollection files, List<string> errors)
    {
        if (files == null || files.Count == 0)
        {
            errors.Add("Please upload required files (Thumbnail and Game ZIP).");
            return;
        }

        int previewCount = files.Count(f => f.Name == "PreviewImages");
        int trailerCount = files.Count(f => f.Name == "Trailers");
        int thumbnailCount = files.Count(f => f.Name == "Thumbnail");
        int zipCount = files.Count(f => f.Name == "GameZip");

        if (previewCount > MaxPreviewImages) errors.Add($"Preview images: maximum {MaxPreviewImages} files allowed.");
        if (trailerCount > MaxTrailers) errors.Add($"Trailer videos: maximum {MaxTrailers} files allowed.");
        if (thumbnailCount != 1) errors.Add("Exactly one thumbnail is required.");
        if (zipCount != 1) errors.Add("Exactly one game ZIP is required.");

        foreach (var f in files)
        {
            if (f == null || f.Length == 0) { errors.Add($"File \"{f?.FileName}\" is empty."); continue; }

            var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();
            switch (f.Name)
            {
                case "Thumbnail":
                    ValidateFile(f, "image", MaxThumbnailBytes, errors);
                    break;
                case "PreviewImages":
                    ValidateFile(f, "image", MaxPreviewImageBytes, errors);
                    break;
                case "Trailers":
                    ValidateFile(f, "video", MaxTrailerBytes, errors);
                    break;
                case "GameZip":
                    if (ext != ".zip") errors.Add($"Game package \"{f.FileName}\" must be a .zip file.");
                    if (f.Length > MaxZipBytes) errors.Add($"Game package \"{f.FileName}\" exceeds {MaxZipBytes / (1024 * 1024)} MB.");
                    break;
                default:
                    errors.Add($"Unknown file input \"{f.Name}\".");
                    break;
            }
        }
    }

    // 单个文件验证
    private void ValidateFile(IFormFile file, string type, long maxBytes, List<string> errors)
    {
        if (type == "image" && !(file.ContentType?.StartsWith("image/") ?? false))
            errors.Add($"File \"{file.FileName}\" must be an image.");

        if (type == "video" && !(file.ContentType?.StartsWith("video/") ?? false))
            errors.Add($"File \"{file.FileName}\" must be a video.");

        if (file.Length > maxBytes)
            errors.Add($"File \"{file.FileName}\" exceeds {maxBytes / (1024 * 1024)} MB.");
    }

    // 将错误加入 ModelState
    private void AddErrorsToModelState(List<string> errors)
    {
        foreach (var err in errors)
        {
            ModelState.AddModelError("", err);
        }
    }

    // 保存游戏文件
    private async Task SaveGameFilesAsync(int gameId, IFormFileCollection files)
    {
        var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "games", gameId.ToString());
        Directory.CreateDirectory(uploadRoot);

        int sortOrder = 0;

        foreach (var f in files)
        {
            if (f == null || f.Length == 0) continue;

            string mediaType, subDir;
            var ext = Path.GetExtension(f.FileName)?.ToLowerInvariant();

            switch (f.Name)
            {
                case "Thumbnail":
                    mediaType = "thumb"; subDir = "thumb"; break;
                case "GameZip":
                    mediaType = "zip"; subDir = "zip"; break;
                case "PreviewImages":
                    mediaType = "image"; subDir = "previews"; break;
                case "Trailers":
                    mediaType = "video"; subDir = "trailers"; break;
                default:
                    continue; // 忽略未知文件
            }

            var dirPath = Path.Combine(uploadRoot, subDir);
            Directory.CreateDirectory(dirPath);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(dirPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await f.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/games/{gameId}/{subDir}/{fileName}";

            var gm = new GameMedia
            {
                GameId = gameId,
                MediaUrl = relativeUrl,
                MediaType = mediaType,
                SortOrder = (mediaType == "image" || mediaType == "video") ? sortOrder++ : 0
            };

            db.GameMedia.Add(gm);
        }

        await db.SaveChangesAsync();
    }

    #endregion

    #region Game Details (Deprecated)

    // AJAX: get game details and media urls
    //[HttpGet]
    //public async Task<IActionResult> GetGame(int id)
    //{
    //    var game = await db.Games
    //        .Include(g => g.Media)
    //        .FirstOrDefaultAsync(g => g.Id == id);

    //    if (game == null) return NotFound();

    //    var dto = new
    //    {
    //        Id = game.Id,
    //        Title = game.Title,
    //        Description = game.Description,
    //        Price = game.Price,
    //        Status = game.Status.ToString(),
    //        Media = game.Media.Select(m => new { m.MediaUrl, m.MediaType, m.SortOrder }).OrderBy(m => m.SortOrder).ToList()
    //    };

    //    return Json(dto);
    //}

    //[HttpGet]
    //// [Authorize(Roles = "Developer")] // enable if you have authentication/roles
    //public async Task<IActionResult> DeveloperGameDetail(int id)
    //{
    //    var developerId = GetCurrentUserId();
    //    if (developerId == null)
    //        return RedirectToAction("Login", "Account");

    //    var game = await db.Games
    //        .Include(g => g.Media)
    //        .Include(g => g.Purchases)
    //        .Include(g => g.Reviews)
    //        .FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId);

    //    if (game == null) return NotFound();

    //    var vm = new DeveloperGameDetailViewModel
    //    {
    //        Id = game.Id,
    //        Title = game.Title,
    //        Description = game.Description,
    //        Price = game.Price,
    //        Status = game.Status.ToString(),
    //        ReleaseDate = game.ReleaseDate,
    //        LikeCount = game.LikeCount,
    //        SalesCount = game.Purchases.Count(p => p.Status == PurchaseStatus.Completed),
    //        ReviewsCount = game.Reviews?.Count ?? 0,
    //        Media = game.Media.OrderBy(m => m.SortOrder)
    //                         .Select(m => new GameMediaItem { MediaUrl = m.MediaUrl, MediaType = m.MediaType, SortOrder = m.SortOrder })
    //                         .ToList()
    //    };

    //    return View(vm);
    //}

    #endregion

    #region Edit & Delete Game

    //    GET: Developer/DeveloperEditGame/{id}
    //显示编辑游戏页面
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
            return NotFound("This game is not found or not belong to you");
        }

        // 4. 将实体映射到 ViewModel
        var vm = new DeveloperEditGameViewModel
        {
            Id = game.Id,
            Title = game.Title,
            ShortDescription = game.ShortDescription,
            DetailDescription = game.DetailDescription,
            Price = game.Price,
            ThumbnailUrl = game.Media.FirstOrDefault(m => m.MediaType == "thumb")?.MediaUrl,
            // 获取其他媒体资源并排序
            PreviewImageUrls = game.Media
                .Where(m => m.MediaType == "image")
                .OrderBy(m => m.SortOrder)
                .Select(m => m.MediaUrl)
                .ToList(),
            TrailersUrls = game.Media
                .Where(m => m.MediaType == "video")
                .OrderBy(m => m.SortOrder)
                .Select(m => m.MediaUrl)
                .ToList(),
            Tags = db.GameTags
                .Where(gt => gt.GameId == game.Id)
                .Select(gt => gt.Tag.Name)
                .ToList(),
        };

        // 5. 返回视图
        return View(vm);
    }

    //POST: Developer/DeveloperEditGame
    //处理编辑游戏的表单提交
    [HttpPost]
    [ValidateAntiForgeryToken] // 防止 CSRF 攻击
    public async Task<IActionResult> DeveloperEditGame(DeveloperEditGameViewModel model)
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
        game.ShortDescription = model.ShortDescription ?? "";
        game.DetailDescription = model.DetailDescription ?? "";
        game.Price = model.IsFree ? 0 : model.Price ?? 0; // 如果勾选免费，价格设为0
                                                          // 注意：通常不在这里修改 Status，除非有特定的业务逻辑

        // 4. 更新标签
        if (model.Tags != null && model.Tags.Count > 0)
        {
            // 先删除现有标签关联
            var existingTags = db.GameTags.Where(gt => gt.GameId == game.Id);
            db.GameTags.RemoveRange(existingTags);

            // 然后添加新的标签关联
            foreach (var tagName in model.Tags.Distinct().Take(10))
            {
                var tag = db.Tags.FirstOrDefault(t => t.Name == tagName);

                var gameTag = new GameTag
                {
                    GameId = game.Id,
                    TagId = tag != null ? tag.Id : 0
                };
                db.GameTags.Add(gameTag);
            }
            await db.SaveChangesAsync();
        }

        // 5. 处理媒体文件更新

        // 处理要删除的媒体文件
        if (!string.IsNullOrEmpty(model.DeletedMediaJson))
        {
            try
            {
                // 反序列化前端传来的 URL 列表
                var mediaUrlsToDelete = JsonSerializer.Deserialize<List<string>>(model.DeletedMediaJson);
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

        // 处理新上传的缩略图 (替换旧的)
        if (model.NewThumbnail is { Length: > 0 })
        {
            var oldThumb = game.Media.FirstOrDefault(m => m.MediaType == "thumb");
            if (oldThumb != null)
            {
                DeleteFileFromServer(oldThumb.MediaUrl);
                db.GameMedia.Remove(oldThumb);
            }

            var newThumb = await SaveSingleMediaAsync(
                game.Id,
                model.NewThumbnail,
                mediaType: "thumb",
                subDir: "thumb",
                sortOrder: 0
            );

            db.GameMedia.Add(newThumb);
        }

        // 处理新上传的zip文件 (替换旧的)
        if (model.NewGameZip is { Length: > 0 })
        {
            var oldZip = game.Media.FirstOrDefault(m => m.MediaType == "zip");
            if (oldZip != null)
            {
                DeleteFileFromServer(oldZip.MediaUrl);
                db.GameMedia.Remove(oldZip);
            }

            var newZip = await SaveSingleMediaAsync(
                game.Id,
                model.NewGameZip,
                mediaType: "zip",
                subDir: "zip",
                sortOrder: 0
            );

            db.GameMedia.Add(newZip);
        }

        // 计算现有媒体的最大排序值，以便追加新媒体
        int nextSortOrder = (game.Media.Any(m => m.MediaType != "thumb") ? game.Media.Where(m => m.MediaType != "thumb").Max(m => m.SortOrder) : 0) + 1;

        // 处理新上传的预览图
        foreach (var file in model.NewPreviewImages.Where(f => f.Length > 0))
        {
            var media = await SaveSingleMediaAsync(
                game.Id,
                file,
                mediaType: "image",
                subDir: "previews",
                sortOrder: nextSortOrder++
            );

            db.GameMedia.Add(media);
        }

        // 处理新上传的预告片
        foreach (var file in model.NewTrailers.Where(f => f.Length > 0))
        {
            var media = await SaveSingleMediaAsync(
                game.Id,
                file,
                mediaType: "video",
                subDir: "trailers",
                sortOrder: nextSortOrder++
            );

            db.GameMedia.Add(media);
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
        game.Status = GameStatus.Pending; // 每次编辑后都设为待审核
        await db.SaveChangesAsync();

        // 7. 添加成功提示消息并重定向
        TempData["FlashMessage"] = "Game Updated Successfully! Please wait for approval.";
        TempData["FlashMessageType"] = "success";
        // 重定向回开发者仪表盘或游戏详情页
        return RedirectToAction("Developer");
    }

    private async Task<GameMedia> SaveSingleMediaAsync(
    int gameId,
    IFormFile file,
    string mediaType,
    string subDir,
    int sortOrder)
    {
        var uploadRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "games",
            gameId.ToString(),
            subDir
        );

        Directory.CreateDirectory(uploadRoot);

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadRoot, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/games/{gameId}/{subDir}/{fileName}";

        return new GameMedia
        {
            GameId = gameId,
            MediaType = mediaType,
            MediaUrl = relativeUrl,
            SortOrder = sortOrder
        };
    }

    private void DeleteFileFromServer(string mediaUrl)
    {
        var physicalPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            mediaUrl.TrimStart('/')
        );

        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }
    }


    // POST: Delete game
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var developerId = GetCurrentUserId();
        if (developerId == null)
            return RedirectToAction("Login", "Account");

        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id && g.DeveloperId == developerId.Value);

        if (game == null)
            return NotFound();

        // Remove files on disk (uploads/games/{game.Id})
        //try
        //{
        //    var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "games", game.Id.ToString());
        //    if (Directory.Exists(uploadRoot))
        //    {
        //        Directory.Delete(uploadRoot, true);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    // don't throw to user; log if you have a logger. Continue with DB cleanup.
        //    Console.WriteLine("[dev] Error deleting upload folder: " + ex);
        //}

        // Remove related DB rows (be explicit)
        //if (game.Media != null && game.Media.Any()) db.GameMedia.RemoveRange(game.Media);
        //if (game.Purchases != null && game.Purchases.Any()) db.Purchases.RemoveRange(game.Purchases);
        //if (game.Reviews != null && game.Reviews.Any()) db.Reviews.RemoveRange(game.Reviews);

        //db.Games.Remove(game);
        game.Status = GameStatus.Removed; // soft delete
        await db.SaveChangesAsync();

        TempData["FlashMessage"] = "Game removed successfully, you can contact admin to restore your game.";
        TempData["FlashMessageType"] = "info";
        return RedirectToAction("Developer");
    }

    #endregion

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

        // 确保开发者拥有该游戏
        var gameExists = await db.Games.AnyAsync(g => g.Id == id && g.DeveloperId == developerId.Value);
        if (!gameExists) return Forbid();

        var end = DateTime.UtcNow;
        var start = end.AddHours(-hours);

        // 获取指定时间段内的购买记录
        var purchases = await db.Purchases
            .Include(p => p.Payment)
            .Include(p => p.Game) // 确保能访问 Game.ExposureCount
            .Where(p => p.GameId == id
                        && p.Status == PurchaseStatus.Completed
                        && p.Payment.CreatedAt >= start
                        && p.Payment.CreatedAt <= end)
            .ToListAsync();

        var labels = new List<string>();
        var exposureSeries = new List<int>();
        var salesSeries = new List<int>();
        var revenueSeries = new List<decimal>();

        if (hours >= 8760)
        {
            // 年度数据：按月聚合
            var now = DateTime.UtcNow;
            var monthlyData = new Dictionary<string, List<Purchase>>();

            for (int i = 11; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var key = $"{monthStart.Year}-{monthStart.Month:D2}";
                monthlyData[key] = new List<Purchase>();
            }

            // 分配购买记录到对应月份
            foreach (var p in purchases)
            {
                var key = $"{p.Payment.CreatedAt.Year}-{p.Payment.CreatedAt.Month:D2}";
                if (monthlyData.ContainsKey(key))
                    monthlyData[key].Add(p);
            }

            foreach (var kv in monthlyData.OrderBy(k => k.Key))
            {
                labels.Add(kv.Key); // 前端可再格式化为 "Dec"
                salesSeries.Add(kv.Value.Count);
                revenueSeries.Add(kv.Value.Sum(p => p.PriceAtPurchase));

                // 曝光按时间段计算，不重复累加
                var exposureSum = kv.Value.Any() ? kv.Value.First().Game.ExposureCount : 0;
                exposureSeries.Add(exposureSum);
            }
        }
        else
        {
            // 小于一年：按 intervalHours 聚合
            int intervalHours = hours <= 48 ? 6 : 24;
            for (var t = start; t <= end; t = t.AddHours(intervalHours))
            {
                var next = t.AddHours(intervalHours);
                labels.Add(t.ToLocalTime().ToString(hours <= 48 ? "HH:mm" : "dd MMM"));

                var bucket = purchases.Where(p => p.Payment.CreatedAt >= t && p.Payment.CreatedAt < next).ToList();
                salesSeries.Add(bucket.Count);
                revenueSeries.Add(bucket.Sum(p => p.PriceAtPurchase));

                // 曝光按时间段取该时间段游戏的最新 ExposureCount
                var exposure = bucket.Any() ? bucket.First().Game.ExposureCount : 0;
                exposureSeries.Add(exposure);
            }
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
        var totalExposure = game.ExposureCount;

        var sb = new StringBuilder();
        sb.AppendLine("GameId,Title,TotalSales,TotalRevenue");
        var titleEscaped = game.Title?.Replace("\"", "\"\"") ?? string.Empty;
        sb.AppendLine($"{game.Id},\"{titleEscaped}\",{totalSales},{totalRevenue}, {totalExposure}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"game_report_{game.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }


}
