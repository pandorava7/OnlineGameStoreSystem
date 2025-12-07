using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace OnlineGameStoreSystem.Controllers;

public class AdminController : Controller
{
    private readonly DB _db;

    public AdminController(DB context)
    {
        _db = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult StatusManage(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search));
        }

        var vm = query
            .Where(u => u.Status != "deleted")
            .Select(u => new StatusManageVM
            {
                UserId = u.Id,
                AvatarUrl = string.IsNullOrEmpty(u.AvatarUrl) == false ? u.AvatarUrl : "/images/avatar_default.png",
                UserName = u.Username,
                UserEmail = u.Email,
                Status = u.Status,
            })
            .OrderBy(u => u.UserId)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "User >> Status Manage";
        ViewData["Description"] = "This page is for managing user statuses.";

        return View(vm);
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        var user = _db.Users
            .Where(u => u.Id == id)
            .Select(u => new StatusManageVM
            {
                UserId = u.Id,
                UserName = u.Username,
                UserEmail = u.Email,
                Status = u.Status,
            })
            .FirstOrDefault();

        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    public IActionResult EditUser(StatusManageVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _db.Users.Find(model.UserId);
        if (user == null) return NotFound();

        user.Username = model.UserName;
        user.Email = model.UserEmail;
        user.Status = model.Status;

        _db.SaveChanges();

        return RedirectToAction("StatusManage");
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var user = _db.Users.FirstOrDefault(x => x.Id == id);
        if (user != null)
        {
            user.Status = "deleted";
            _db.SaveChanges();
        }

        // 删除后回到管理页面
        return RedirectToAction("StatusManage");
    }


    //public IActionResult RegistrationApproval()
    //{
    //    return View();
    //}

    //public IActionResult Permissions()
    //{
    //    return View();
    //}

    public IActionResult GameReleaseReview(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Games
            .Include(g => g.Developer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.Title.Contains(search) || g.Developer.Username.Contains(search));
        }

        var vm = query
            .Where(g => g.Status == GameStatus.Pending)
            .Select(g => new GameReleaseReviewVM
            {
                GameId = g.Id,
                DeveloperId = g.DeveloperId,
                DeveloperName = g.Developer.Username,
                Title = g.Title,
                CreatedAt = g.CreatedAt,
            })
            .OrderBy(g => g.CreatedAt)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Game >> Game Release Review";
        ViewData["Description"] = "This page is for review and approve the game release.";

        return View(vm);
    }

    public IActionResult GameReview(int gameId)
    {
        var game = _db.Games
            .Include(g => g.Media)
            .Include(g => g.Developer)
            .Include(g => g.Tags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.Id == gameId)
            .FirstOrDefault();
        if (game == null)
        {
            return NotFound("Game not found");
        }

        // 获取该游戏的视图信息
        var vm = new GameReviewVM
        {
            GameId = game.Id,
            DeveloperName = game.Developer.Username,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Tags = game.Tags.Select(gt => gt.Tag.Name).ToArray(),
            ThumbnailUrl = game.Media.FirstOrDefault(gm => gm.MediaType == "thumb")?.MediaUrl,
            PreviewUrls = game.Media.Where(gm => gm.MediaType == "image").Select(gm => gm.MediaUrl).ToArray(),
            VideoUrls = game.Media.Where(gm => gm.MediaType == "video").Select(gm => gm.MediaUrl).ToArray(),
            CreatedAt = game.CreatedAt,
        };

        return View(vm);
    }

    public IActionResult ApproveGame(int gameId, bool approve)
    {
        // 更新游戏状态
        var game = _db.Games.Find(gameId);
        if (game == null)
        {
            return NotFound("Game is not found");
        }

        if (game.Status == GameStatus.Removed)
            return BadRequest("This game is already being removed");
        if (approve == true && game.Status == GameStatus.Published)
            return BadRequest("This game is already being published");

        ConsoleHelper.WriteRed("Approve: " + approve);
        game.Status = approve ? GameStatus.Published : GameStatus.Removed;
        game.ReleaseDate = DateTime.Now;
        _db.SaveChanges();

        TempData["FlashMessage"] = approve ? "Successfully to release this game." : "Successfully to remove this game.";
        TempData["FlashMessageType"] = "success";

        return RedirectToAction("GameReleaseReview");
    }

    public IActionResult GameManagement(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Games
            .Include(g => g.Developer)
            .Include(g => g.Media)
            .Include(g => g.Tags)
                .ThenInclude(gt => gt.Tag)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.Title.Contains(search) || g.Developer.Username.Contains(search));
        }

        var vm = query
            .Select(g => new GameManagementVM
            {
                GameId = g.Id,
                ThumbnailUrl = g.Media.Where(m => m.MediaType == "thumb").Select(m => m.MediaUrl).FirstOrDefault(),
                Title = g.Title,
                DeveloperId = g.Developer.Id,
                DeveloperName = g.Developer.Username,
                Price = g.Price,
                Tags = g.Tags.Select(gt => gt.Tag.Name).ToArray(),
                Status = g.Status.ToString(),
                CreatedAt = g.CreatedAt,
            })
            .OrderBy(g => g.CreatedAt)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Game >> Game Management";
        ViewData["Description"] = "This page is for game management, view the information for each game and provide function to manage them.";

        return View(vm);
    }

    public IActionResult RefundHandling(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Purchases
            .Include(r => r.User)
            .Include(r => r.Payment)
            .Where(r => r.Status == PurchaseStatus.Refunding)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.User.Username.Contains(search));
        }

        var vm = query
            .Select(p => new RefundHandlingVM
            {
                PurchaseId = p.Id,
                PurchaseDate = p.Payment.CreatedAt,
                UserId = p.UserId,
                AvatarUrl = string.IsNullOrEmpty(p.User.AvatarUrl) == false ? p.User.AvatarUrl : "/images/avatar_default.png",
                UserName = p.User.Username,
                RefundGameId = p.GameId,
                RefundGameName = p.Game.Title,
                RefundReason = p.RefundReason ?? "no reason",
                RefundRequestDate = p.RefundRequestedAt,
            })
            .OrderBy(p => p.RefundRequestDate)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Purchase >> Refund Handling";
        ViewData["Description"] = "This page is for refund handling, review the reason from user and approve/reject the refund request.";

        return View(vm);
    }

    public IActionResult HandleRefund(int purchaseId, bool approve)
    {
        // 更新游戏状态
        var purchase = _db.Purchases.Find(purchaseId);
        if (purchase == null)
            return NotFound("Purchase is not found");

        if (purchase.Status != PurchaseStatus.Refunding)
            return BadRequest("This purchase is not valid for refund, the status is: " + purchase.Status.ToString());

        ConsoleHelper.WriteRed("Approve: " + approve);
        purchase.Status = approve ? PurchaseStatus.Refunded : PurchaseStatus.Completed;
        _db.SaveChanges();

        TempData["FlashMessage"] = approve ? "Successfully to refund this purchase." : "Reject the refund request of this purchase.";
        TempData["FlashMessageType"] = approve ? "success" : "info";

        return RedirectToAction("RefundHandling");
    }

    public IActionResult WebsiteStatistics()
    {
        return View();
    }

    public IActionResult TrackPurchase(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Payments
            .Include(r => r.User)
            .Include(p => p.Purchases)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.User.Username.Contains(search));
        }

        var vm = query
            .Select(p => new TrackPurchaseVM
            {
                PaymentId = p.Id,
                PurchaseDate = p.CreatedAt,
                Status = p.Status.ToString(),
                PaymentPurpose = p.Purpose.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                Price = p.Amount,
                UserId = p.UserId,
                AvatarUrl = string.IsNullOrEmpty(p.User.AvatarUrl) == false ? p.User.AvatarUrl : "/images/avatar_default.png",
                UserName = p.User.Username,
            })
            .OrderBy(p => p.PurchaseDate)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Purchase >> Track Purchase";
        ViewData["Description"] = "This page is for tracking all the purchase record, see the detail of each payment and purchase.";

        return View(vm);
    }

    public IActionResult PurchaseDetailList(int paymentId)
    {
        var payment = _db.Payments
            .Include(p => p.Purchases)
                .ThenInclude(p => p.Game)
            .FirstOrDefault(p => p.Id == paymentId);

        if (payment == null)
            return NotFound("payment not found");

        decimal subtotal = payment.Purchases.Sum(p => p.PriceAtPurchase);
        decimal total = payment.Amount;
        decimal discount = total - subtotal;

        var vm = new PurchaseDetailVM
        {
            PaymentId = payment.Id,
            PaymentMethod = payment.PaymentMethod.ToString(),
            PurchaseDate = payment.CreatedAt,
            TransactionId = payment.TransactionId,
            PurchaseItems = payment.Purpose == PaymentPurposeType.DeveloperRegistration
                    ? new List<PurchaseItem> { new PurchaseItem { Name = "Registration Fee", Price = SystemConstants.RegistrationFee } }
                     : payment.Purchases
                        .Select(p => new PurchaseItem
                        {
                            PurchaseId = p.Id,
                            Name = p.Game.Title,
                            Price = p.PriceAtPurchase
                        })
                        .ToList(),
            Subtotal = subtotal,
            Discount = discount,
            Total = total,
        };

        return View(vm);
    }

    public IActionResult PostManagement(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Posts
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.User.Username.Contains(search));
        }

        var vm = query
            .Select(p => new PostManagementVM
            {
                PostId = p.Id,
                PostTitle = p.Title,
                PostDate = p.CreatedAt,
                UserId = p.UserId,
                AvatarUrl = string.IsNullOrEmpty(p.User.AvatarUrl) == false ? p.User.AvatarUrl : "/images/avatar_default.png",
                UserName = p.User.Username,
            })
            .OrderBy(p => p.PostDate)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Community >> Post Management";
        ViewData["Description"] = "This page is for view and manage the posts of community.";

        return View(vm);
    }

    public IActionResult RemovePost(int postId)
    {
        var post = _db.Posts.Find(postId);
        if (post == null)
            return NotFound("post not found");

        post.Status = ActiveStatus.Banned;
        _db.SaveChanges();

        return RedirectToAction("PostManagement");
    }

    public IActionResult CommentManagement(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Comments
            .Include(c => c.Post)
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.User.Username.Contains(search));
        }

        var vm = query
            .Select(p => new CommentManagementVM
            {
                PostId = p.Post.Id,
                PostTitle = p.Post.Title,
                CommentId = p.Id,
                CommentContent = p.Content,
                CommentDate = p.CreatedAt,
                UserId = p.UserId,
                AvatarUrl = string.IsNullOrEmpty(p.User.AvatarUrl) == false ? p.User.AvatarUrl : "/images/avatar_default.png",
                UserName = p.User.Username,
            })
            .OrderBy(p => p.CommentDate)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Community >> Comment Management";
        ViewData["Description"] = "This page is for view and manage the comments of each post.";

        return View(vm);
    }

    public IActionResult RemoveComment(int commentId)
    {
        var comment = _db.Comments.Find(commentId);
        if (comment == null)
            return NotFound("comment not found");

        comment.Status = ActiveStatus.Banned;
        _db.SaveChanges();

        return RedirectToAction("CommentManagement");
    }

    public IActionResult GameReviewManagement(string? search, int? page)
    {
        int pageSize = 5;
        int pageNumber = page ?? 1;

        var query = _db.Reviews
            .Include(r => r.Game)
            .ThenInclude(g=>g.Media)
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.User.Username.Contains(search));
        }

        var vm = query
            .Select(r => new GameReviewManagementVM
            {
                ReviewId = r.Id,
                ReviewRating = r.Rating,
                ReviewContent = r.Content ?? "",
                ReviewDate = r.CreatedAt,
                GameId = r.GameId,
                GameThumbnailUrl = r.Game.Media.Where(m => m.MediaType == "thumb").Select(m => m.MediaUrl).FirstOrDefault(),
                GameTitle = r.Game.Title,
                UserId = r.UserId,
                AvatarUrl = string.IsNullOrEmpty(r.User.AvatarUrl) == false ? r.User.AvatarUrl : "/images/avatar_default.png",
                UserName = r.User.Username,
            })
            .OrderBy(p => p.ReviewDate)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "Community >> Game Review Management";
        ViewData["Description"] = "This page is for view and manage the game review of each game.";

        return View(vm);
    }

    public IActionResult RemoveGameReview(int reviewId)
    {
        var review = _db.Reviews.Find(reviewId);
        if (review == null)
            return NotFound("review not found");

        review.Status = ActiveStatus.Banned;
        _db.SaveChanges();

        return RedirectToAction("GameReviewManagement");
    }

    public IActionResult CommunityStatistics()
    {
        return View();
    }

}
