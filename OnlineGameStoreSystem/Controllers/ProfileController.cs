using Microsoft.AspNetCore.Hosting; // 需要注入 IWebHostEnvironment
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.IO; // Required for file operations
using System.Security.Claims;
using static OnlineGameStoreSystem.Models.ViewModels.PublicProfileViewModel;

namespace OnlineGameStoreSystem.Controllers;

public class ProfileController : Controller
{
    private readonly DB db;
    private readonly IHttpContextAccessor http;
    private readonly IWebHostEnvironment _webHostEnvironment; // 注入环境服务

    public ProfileController(DB context, IHttpContextAccessor accessor,
        IWebHostEnvironment webHostEnvironment)
    {
        db = context;
        http = accessor;
        _webHostEnvironment = webHostEnvironment; // 接收注入
    }

    #region Profile Index Page
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
        var reviews = db.Reviews
            .Where(r => r.UserId == userId && r.Status == ActiveStatus.Active)
            .Include(r => r.Game)
            .ThenInclude(g => g.Media)
            .OrderByDescending(c => c.CreatedAt)
            .Select(r => new ReviewItem
            {
                GameId = r.Game.Id,
                GameTitle = r.Game.Title,
                CoverUrl = r.Game.Media
                    .Where(m => m.MediaType == "thumb")
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.MediaUrl)
                    .FirstOrDefault() ?? "/images/default-cover.jpg",
                Content = r.Content ?? "",
                CreatedAt = r.CreatedAt
            })
            .ToList();

        var purchasedGames = db.Purchases
        .Where(p => p.UserId == userId && p.Status == PurchaseStatus.Completed
        && p.Payment.Status == PaymentStatus.Completed)
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

        var prefs = db.UserPreferences.FirstOrDefault(p => p.UserId == userId);
        if (prefs == null)
        {
            prefs = new UserPreferences
            {
                UserId = userId,
                PublicProfile = true // 默认公开
            };
            db.UserPreferences.Add(prefs);
            db.SaveChanges();
        }

        // 建立 ViewModel（跟你要求的完全一致）
        var vm = new ProfileViewModel
        {
            AvatarUrl = user.AvatarUrl ?? "/images/avatar_default.png",
            Username = user.Username,
            Summary = string.IsNullOrEmpty(user.Summary) ? "Empty" : user.Summary,
            IsDeveloper = user.IsDeveloper,
            CreatedAt = user.CreatedAt,
            FavoriteTags = favTags,
            UserReviews = reviews,
            PurchasedGames = purchasedGames,
            UserPreferences = prefs
        };



        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdatePrivacy([FromBody] PrivacyDto dto)
    {
        var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
        var pref = db.UserPreferences.FirstOrDefault(p => p.UserId == userId);

        if (pref == null)
        {
            pref = new UserPreferences
            {
                UserId = userId,
                PublicProfile = dto.IsPublic
            };
            db.UserPreferences.Add(pref);
        }
        else
        {
            pref.PublicProfile = dto.IsPublic;
        }

        db.SaveChanges();
        // 设置 TempData
        TempData["FlashMessage"] = dto.IsPublic ? "Profile is now public!" : "Profile is now private!";
        TempData["FlashMessageType"] = "success"; // success / error / info
        return Ok(new { pref.PublicProfile });
    }

    public class PrivacyDto
    {
        public bool IsPublic { get; set; }
    }
    #endregion

    #region Edit Profile Page
    public IActionResult EditProfile()
    {                                           // 1. 从 Claims 中获取 UserId 字符串
        var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;

        // 2. 检查是否登入或 Claim 是否存在
        if (string.IsNullOrEmpty(userIdString))
        {
            // 未登入 → 返回登入页
            return RedirectToAction("Login", "Account");
        }

        // 3. 尝试解析 UserId 字符串
        int userId;
        if (!int.TryParse(userIdString, out userId))
        {
            // 如果解析失败，说明 Claim 格式不正确
            return StatusCode(500, "User ID claim format error.");
        }
        // --- GetUserId Logic Combined End ---


        // 4. Fetch User data and their current favorite tags synchronously
        // 同步获取用户数据和他们当前的收藏标签
        var user = db.Users // 假设 db 替换为 _dbContext
            .Include(u => u.FavouriteTags)
            .ThenInclude(ft => ft.Tag)
            .FirstOrDefault(u => u.Id == userId); // 使用 FirstOrDefault() 同步方法

        if (user == null)
        {
            // 用户不存在，返回 Not Found
            return NotFound();
        }

        // 5. Fetch ALL available tags synchronously
        var allTags = db.Tags.ToList(); // 使用 ToList() 同步方法

        // 6. Map entities to ViewModel
        var viewModel = new EditProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
            Summary = user.Summary,
            AvailableTags = allTags,
            SelectedTagIds = user.FavouriteTags.Select(ft => ft.TagId).ToList(),
        };

        return View(viewModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model) // 1. 更改为异步方法
    {
        // 1. 获取当前登录用户的 ID 字符串
        // 假设 http 变量引用了注入的 _httpContextAccessor
        var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;

        // 2. 检查是否登入
        if (string.IsNullOrEmpty(userIdString))
        {
            // 未登入 → 返回登入页
            return RedirectToAction("Login", "Account");
        }

        // 3. 解析当前登录用户的 ID
        int currentUserId;
        if (!int.TryParse(userIdString, out currentUserId))
        {
            return StatusCode(500, "User ID claim format error.");
        }

        // 4. ***关键安全检查***
        // 确保提交的 ID 与当前登录用户的 ID 一致，防止越权操作。
        if (model.Id != currentUserId)
        {
            return Forbid(); // 返回 403 Forbidden
        }


        // 5. 使用经过安全验证的 ID (currentUserId) 查询要更新的用户
        var userToUpdate = db.Users
            .Include(u => u.FavouriteTags)
            .FirstOrDefault(u => u.Id == currentUserId); // 修正点：使用 currentUserId

        if (userToUpdate == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // --- 文件上传处理 (调用 SaveImageAsync) ---
            if (model.NewAvatarFile != null)
            {
                try
                {
                    // 1. **获取旧头像 URL**
                    var oldAvatarUrl = userToUpdate.AvatarUrl;

                    // 2. 调用异步方法保存新头像
                    string newAvatarUrl = await FileHelper.SaveImageAsync(
                        model.NewAvatarFile,
                        folder: "images/Profile",
                        maxFileSizeMB: 2
                    );

                    if (!string.IsNullOrEmpty(newAvatarUrl))
                    {
                        // 3. 更新数据库中的 AvatarUrl
                        userToUpdate.AvatarUrl = newAvatarUrl;

                        // 4. **删除旧头像文件逻辑**
                        if (!string.IsNullOrEmpty(oldAvatarUrl) &&
                            !oldAvatarUrl.Equals("/images/avatar_default.png", StringComparison.OrdinalIgnoreCase))
                        {
                            // 组合 wwwroot 路径和旧头像的相对 URL
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldAvatarUrl.TrimStart('/'));

                            // 检查文件是否存在，并且确保它不在默认头像路径
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 文件验证错误处理
                    ModelState.AddModelError("NewAvatarFile", ex.Message);
                    model.AvailableTags = db.Tags.ToList();
                    return View(model);
                }
            }
            // --- 文件上传处理结束 ---


            // 5. Update basic user properties
            userToUpdate.Username = model.Username;
            userToUpdate.Summary = model.Summary;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            // 6. Handle FavoriteTags (与之前逻辑相同)
            // ... (标签移除和添加逻辑) ...
            var selectedTagIds = model.SelectedTagIds ?? new List<int>();

            // a. 移除不再选中的标签
            var tagsToRemove = userToUpdate.FavouriteTags
                .Where(ft => !selectedTagIds.Contains(ft.TagId))
                .ToList();
            db.FavouriteTags.RemoveRange(tagsToRemove);

            // b. 添加新选中的标签
            var existingTagIds = userToUpdate.FavouriteTags.Select(ft => ft.TagId).ToList();
            var tagsToAdd = selectedTagIds
                .Where(id => !existingTagIds.Contains(id))
                .Select(id => new FavouriteTags { UserId = userToUpdate.Id, TagId = id })
                .ToList();
            db.FavouriteTags.AddRange(tagsToAdd);

            // 7. Save changes to the database synchronously
            db.SaveChanges();
            await db.SaveChangesAsync();

            TempData["FlashMessage"] = "Profile Updated Successfully";
            TempData["FlashMessageType"] = "success";
            return RedirectToAction("Index");
        }

        // If ModelState is invalid, re-fetch all tags
        // 必须重新加载标签，否则 View 中的多选框会丢失标签列表
        model.AvailableTags = db.Tags.ToList();
        return View(model);
    }
    #endregion

    #region Public ViewProfile
    public IActionResult ViewPage(int userId)
    {
        // 获取当前登录用户 ID，如果未登录则 null
        int? currentUserId = null;
        var claim = HttpContext.User.FindFirst("UserId");
        if (claim != null)
            currentUserId = int.Parse(claim.Value);

        if (currentUserId == null)
        {
            TempData["FlashMessage"] = "Please login first to view profiles.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Login", "Account");
        }

        // 查询用户和相关数据
        var user = db.Users
            .Include(u => u.Preferences)
            .Include(u => u.FavouriteTags)
            .Include(u => u.Reviews)
            .ThenInclude(r => r.Game)
            .ThenInclude(g => g.Media)
            .Include(u => u.Purchases)
            .ThenInclude(p => p.Game)
            .ThenInclude(g => g.Media)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return NotFound();

        var prefs = user.Preferences ?? new UserPreferences
        {
            UserId = user.Id,
            PublicProfile = true
        };

        // 非本人且资料私密 → 提示并返回上一页或首页
        if (currentUserId != user.Id && !prefs.PublicProfile)
        {
            TempData["FlashMessage"] = "This user has set their profile to private.";
            TempData["FlashMessageType"] = "error";

            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "Home");
        }

        // 如果是本人，则直接显示自己的 Profile 页面
        if (currentUserId == user.Id)
        {
            // 可以选择返回 owner 的 Profile View，也可以直接重用这个页面
            return RedirectToAction("Index", "Profile"); // 假设你的 owner 页面是 ProfileController.Index
        }

        // 安全构建 ViewModel
        var vm = new PublicProfileViewModel
        {
            AvatarUrl = user.AvatarUrl ?? "/images/avatar_default.png",
            Username = user.Username,
            Summary = user.Summary ?? "",
            IsDeveloper = user.IsDeveloper,
            CreatedAt = user.CreatedAt,
            // 找 Favorite Tags
            FavoriteTags = db.FavouriteTags
                         .Where(f => f.UserId == userId)
                         .Select(f => f.Tag)
                         .ToList(),

            UserReviews = db.Reviews
            .Where(r => r.UserId == userId)
            .Include(r => r.Game)
            .ThenInclude(g => g.Media)
            .OrderByDescending(c => c.CreatedAt)
            .Select(r => new ReviewItem
            {
                GameId = r.Game.Id,
                GameTitle = r.Game.Title,
                CoverUrl = r.Game.Media
                    .Where(m => m.MediaType == "thumb")
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.MediaUrl)
                    .FirstOrDefault() ?? "/images/placeholder.png",
                Content = r.Content ?? "",
                CreatedAt = r.CreatedAt
            })
            .ToList(),

            PurchasedGames = db.Purchases
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
                        .FirstOrDefault() ?? "/images/placeholder.png",
                    PriceAtPurchase = p.PriceAtPurchase,
                    PurchasedAt = p.Payment.CreatedAt
                }
            )
            .ToList()
        };

        if (user.IsDeveloper)
        {
            vm.PublishedGames = db.Games
                .Where(g => g.DeveloperId == user.Id && g.Status == GameStatus.Published)
                .Include(g => g.Media)
                .OrderByDescending(g => g.ReleaseDate)
                .Select(g => new PublishedGameVM
                {
                    GameId = g.Id,
                    Title = g.Title,
                    Price = g.DiscountPrice ?? g.Price,
                    ReleaseDate = g.ReleaseDate,
                    CoverUrl = g.Media
                        .Where(m => m.MediaType == "thumb")
                        .OrderBy(m => m.SortOrder)
                        .Select(m => m.MediaUrl)
                        .FirstOrDefault() ?? "/images/placeholder.png"
                })
                .ToList();
        }

        return View(vm);
    }
    #endregion 

    #region Developer Registration
    // GET: show registration form
    [HttpGet]
    public IActionResult DeveloperRegister()
    {
        // validate if user is logged in and not already a developer
        var userId = User.GetUserId();
        if (userId == -1)
        {
            TempData["FlashMessage"] = "Please log in to register as a developer.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Login", "Account");
        }
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            TempData["FlashMessage"] = "User not found.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Index", "Home");
        }
        if (user.IsDeveloper)
        {
            TempData["FlashMessage"] = "You are already registered as a developer.";
            TempData["FlashMessageType"] = "info";
            return RedirectToAction("Developer", "Developer");
        }
        // Render Views/Developer/DeveloperRegister.cshtml
        var vm = new DeveloperRegisterVM();
        return View("DeveloperRegister", vm);
    }

    // POST: Handle submission of the initial registration form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeveloperResgiterApply(DeveloperRegisterVM model)
    {
        if (!ModelState.IsValid)
        {
            return View("DeveloperRegister", model);
        }

        // Get current logged in user id
        var userId = User.GetUserId();
        if (userId == -1)
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
        TempData["Reg_UserId"] = userId; // Keep track of which user is registering

        // Redirect to the payment action
        return RedirectToAction("DeveloperProcessPayment");
    }

    // GET: Show the payment confirmation page
    [HttpGet]
    public IActionResult DeveloperProcessPayment(DeveloperRegistrationViewModel vm)
    {
        // validate if user is logged in and not already a developer
        var userId = User.GetUserId();
        if (userId == -1)
        {
            TempData["FlashMessage"] = "Please log in to register as a developer.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Login", "Account");
        }
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            TempData["FlashMessage"] = "User not found.";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("Index", "Home");
        }
        if (user.IsDeveloper)
        {
            TempData["FlashMessage"] = "You are already registered as a developer.";
            TempData["FlashMessageType"] = "info";
            return RedirectToAction("Developer", "Developer");
        }

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
            SelectedPaymentMethod = PaymentMethod.Paypal,
            ConfirmPayment = false
        };

        // ⭐ 关键：清掉 GET 的 ModelState
        //ModelState.Clear();

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

        if (model.ConfirmPayment == false)
        {
            // The user must check the "Confirm" box
            ModelState.AddModelError("ConfirmPayment", "Please confirm your payment to proceed.");
            // Preserve userId again for re-displaying the form
            TempData.Keep("Reg_UserId");
            return RedirectToAction("DeveloperPayment", model);
        }

        //if(model.SelectedPaymentMethod == null)
        //{
        //    ModelState.AddModelError("SelectedPaymentMethod", "Please select a payment method.");
        //    TempData.Keep("Reg_UserId");
        //    return View("DeveloperPayment", model);
        //}

        // Create a payment object
        var payment = new Payment
        {
            UserId = userId,
            Amount = 50.00m, // Fixed developer registration fee
            PaymentMethod = model.SelectedPaymentMethod,
            TransactionId = Guid.NewGuid().ToString(),
            Purpose = PaymentPurposeType.DeveloperRegistration,
            Status = PaymentStatus.Completed, // Assume success for simulation
            CreatedAt = DateTime.UtcNow
        };

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

                TempData["FlashMessage"] = "Payment successful! You are now a registered developer.";
                TempData["FlashMessageType"] = "success";
                // Redirect to the developer console
                return RedirectToAction("Developer", "Developer");
            }
            else
            {
                // User not found in DB (should be rare)
                TempData["FlashMessage"] = "User not found.";
                TempData["FlashMessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }
        }
        else
        {
            // Payment failed
            TempData["FlashMessage"] = "Payment failed.";
            TempData["FlashMessageType"] = "error";
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

    #endregion
}

