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
            .Include(g=> g.Media)
            .Include(g => g.Developer)
            .Include(g => g.Tags)
                .ThenInclude(gt=>gt.Tag)
            .Where(g=> g.Id == gameId)
            .FirstOrDefault();
        if (game == null) {
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
            ThumbnailUrl = game.Media.FirstOrDefault(gm=>gm.MediaType=="thumb")?.MediaUrl,
            PreviewUrls = game.Media.Where(gm=>gm.MediaType=="image").Select(gm=>gm.MediaUrl).ToArray(),
            VideoUrls = game.Media.Where(gm => gm.MediaType == "video").Select(gm => gm.MediaUrl).ToArray(),
            CreatedAt = game.CreatedAt,
        };

        return View(vm);
    }

    public IActionResult ApproveGame(int gameId, bool approve)
    {
        // 更新游戏状态
        var game = _db.Games.Find(gameId);
        if(game == null)
        {
            return NotFound("Game is not found");
        }

        if(game.Status != GameStatus.Pending)
        {
            return BadRequest("This game is already being removed or released");
        }

        ConsoleHelper.WriteRed("Approve: " + approve);
        game.Status = approve ? GameStatus.Published : GameStatus.Removed;
        game.ReleaseDate = DateTime.Now;
        _db.SaveChanges();

        TempData["FlashMessage"] = approve ? "Successfully to release this game." : "Successfully to remove this game.";
        TempData["FlashMessageType"] = "success";

        return RedirectToAction("GameReleaseReview");
    }

    public IActionResult GameManagement()
    {
        return View();
    }

    public IActionResult RefundHandling()
    {
        return View();
    }

    public IActionResult WebsiteStatistics()
    {
        return View();
    }

    public IActionResult TrackPurchase()
    {
        return View();
    }

    public IActionResult PostManagement()
    {
        return View();
    }

    public IActionResult CommentManagement()
    {
        return View();
    }

    public IActionResult GameReviewManagement()
    {
        return View();
    }

    public IActionResult CommunityStatistics()
    {
        return View();
    }

}
