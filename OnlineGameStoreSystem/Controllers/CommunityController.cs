using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class CommunityController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DB db;

    public CommunityController(ILogger<HomeController> logger, DB context)
    {
        _logger = logger;
        db = context;
    }

    public IActionResult Index()
    {
        var communityViewModel = new CommunityViewModel
        {
            Posts = db.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Select(p => new CommunityPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    ThumbnailUrl = p.Thumbnail ?? "",
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    AuthorName = p.User.Username,
                    AuthorAvatarUrl = p.User.AvatarUrl ?? "",
                    CreatedAt = p.CreatedAt,
                    ViewCount = p.ViewCount,
                    LikeCount = p.Likes.Count,
                    CommentCount = db.Comments.Count(c => c.PostId == p.Id)
                })
                .ToList()
        };


        return View(communityViewModel);
    }
}
