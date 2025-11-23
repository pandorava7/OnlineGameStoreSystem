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

    [Route("community/post/{id}")]
    public IActionResult PostDetails(int id)
    {
        var post = db.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .FirstOrDefault(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }
        post.ViewCount++;
        db.SaveChanges();
        var postDetailsViewModel = new PostDetailsViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            AuthorName = post.User.Username,
            AuthorAvatarUrl = post.User.AvatarUrl ?? "",
            CreatedAt = post.CreatedAt,
            ViewCount = post.ViewCount,
            LikeCount = post.Likes.Count,
            Comments = db.Comments
                .Where(c => c.PostId == post.Id)
                .Include(c => c.User)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorName = c.User.Username,
                    AuthorAvatarUrl = c.User.AvatarUrl ?? "",
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        };
        return View(postDetailsViewModel);
    }

    public IActionResult Publish()
    {
        return View();
    }
}
