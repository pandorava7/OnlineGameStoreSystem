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
                        .FirstOrDefault() ?? "/images/default-cover.jpg",
                    Content = r.Content ?? "",
                    CreatedAt = r.CreatedAt
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
            return Ok(new { pref.PublicProfile });
        }

        public class PrivacyDto
        {
            public bool IsPublic { get; set; }
        }
        #endregion

        #region Edit Profile Page
        //public IActionResult EditProfile()
        //{
        //    var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;
            
        //    if (string.IsNullOrEmpty(userIdString))
        //    {
                
        //        return RedirectToAction("Login", "Account");
        //    }

          
        //    int userId;
        //    if (!int.TryParse(userIdString, out userId))
        //    {
        //        return StatusCode(500, "User ID claim format error.");
        //    }



           
        //    var user = db.Users
        //        .Include(u => u.FavouriteTags)
        //        .ThenInclude(ft => ft.Tag)
        //        .FirstOrDefault(u => u.Id == userId); 
        //    if (user == null)
        //    {
               
        //        return NotFound();
        //    }         
        //    var allTags = db.Tags.ToList(); 

            
        //    var viewModel = new EditProfileViewModel
        //    {
        //        Id = user.Id,
        //        Username = user.Username,
        //        AvatarUrl = user.AvatarUrl,
        //        Summary = user.Summary,
        //        AvailableTags = allTags,
        //        SelectedTagIds = user.FavouriteTags.Select(ft => ft.TagId).ToList()
        //        // 检查数据库中是否有头像数据，并获取其大小
        //    CurrentAvatarDataSize = user.AvatarData != null ? user.AvatarData.Length : 0
        //    };

        //    return View(viewModel);
        //}

        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult EditProfile(EditProfileViewModel model)
        //{
         
            
        //    var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;
        //    if (string.IsNullOrEmpty(userIdString))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

           
        //    int currentUserId;
        //    if (!int.TryParse(userIdString, out currentUserId))
        //    {
        //        return StatusCode(500, "User ID claim format error.");
        //    }
            


          
        //    if (model.Id != currentUserId)
        //    {
        //        return Forbid(); // 返回 403 Forbidden
        //    }

            
        //    var userToUpdate = db.Users
        //        .Include(u => u.FavouriteTags)
        //        .FirstOrDefault(u => u.Id == model.Id); // 使用 FirstOrDefault() 同步方法

        //    if (userToUpdate == null)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
                
        //        userToUpdate.Username = model.Username;
        //        userToUpdate.AvatarUrl = model.AvatarUrl;
        //        userToUpdate.Summary = model.Summary;
        //        userToUpdate.UpdatedAt = DateTime.UtcNow;

        //        // 7. Handle FavoriteTags (The many-to-many relationship management)
        //        // 处理收藏标签的多对多关系

        //        // a. Remove existing tags that are no longer selected
        //        var tagsToRemove = userToUpdate.FavouriteTags
        //            .Where(ft => !model.SelectedTagIds.Contains(ft.TagId))
        //            .ToList();
        //        db.FavouriteTags.RemoveRange(tagsToRemove);

        //        // b. Add new tags that were selected
        //        var existingTagIds = userToUpdate.FavouriteTags.Select(ft => ft.TagId).ToList();
        //        var tagsToAdd = model.SelectedTagIds
        //            .Where(id => !existingTagIds.Contains(id))
        //            .Select(id => new FavouriteTags { UserId = userToUpdate.Id, TagId = id })
        //            .ToList();
        //        db.FavouriteTags.AddRange(tagsToAdd);

        //        // 8. Save changes to the database synchronously
        //        db.SaveChanges(); // 使用 SaveChanges() 同步方法

        //        // 9. Redirect on success (Message in English)
        //        TempData["SuccessMessage"] = "Profile updated successfully!"; //===========================================
        //        return RedirectToAction("EditProfile");
        //    }

        //    // If ModelState is invalid, re-fetch all tags to display them again
        //    model.AvailableTags = db.Tags.ToList();
        //    return View(model);
        //}
        #endregion
    }

}

