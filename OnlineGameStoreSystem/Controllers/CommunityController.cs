using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Extensions;

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

    // 获取社区首页帖子列表
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
                    LikeCount = p.LikeCount,
                    CommentCount = db.Comments.Count(c => c.PostId == p.Id)
                })
                .ToList()
        };

        return View(communityViewModel);
    }

    // 获取帖子详情
    [Route("community/post/{id}")]
    public IActionResult PostDetail(int id)
    {
        var post = db.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .FirstOrDefault(p => p.Id == id);
        if (post == null)
        {
            Console.WriteLine("Post not found with ID: " + id);
            return NotFound();
        }
        post.ViewCount++;
        db.SaveChanges();
        var postDetailsViewModel = new PostDetailsViewModel
        {
            Id = post.Id,
            Title = post.Title,
            ThumbnailUrl = post.Thumbnail ?? "",
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

    // 管理自己的帖子页面
    [Route("community/posts")]
    public IActionResult PostManage()
    {
        // 获取自己的帖子列表
        var userId = User.GetUserId();

        var userPosts = db.Posts
            .Where(p => p.UserId == userId)
            .Select(p => new UserPostViewModel
            {
                Id = p.Id,
                Content = p.Content,
                Title = p.Title,
                ThumbnailUrl = p.Thumbnail ?? "",
                UpdatedAt = p.UpdatedAt,
                ViewCount = p.ViewCount,
                LikeCount = p.Likes.Count,
                CommentCount = db.Comments.Count(c => c.PostId == p.Id)
            })
            .OrderByDescending(p => p.UpdatedAt)
            .ToList();

        var vm = new UserPostsViewModel
        {
            Posts = userPosts
        };

        return View(vm);
    }

    // 发布或编辑帖子页面
    [Route("community/publish")]
    [HttpGet]
    public IActionResult PublishPage(int? id)
    {
        if (id.HasValue)
        {
            // 编辑逻辑
            var post = db.Posts.Find(id.Value);
            if (post == null) return NotFound();

            var model = new CreatePostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ThumbnailUrl = post.Thumbnail ?? ""
            };

            return View(model);
        }

        // 新帖逻辑
        return View(new CreatePostViewModel());
    }


    // 发布新帖子上传逻辑
    [HttpPost]
    public async Task<IActionResult> CreatePost(CreatePostViewModel model)
    {
        // 简单验证
        if (!ModelState.IsValid)
        {
            ModelStateHelper.LogErrors(ModelState);
            return RedirectToAction("PublishPage", model);
        }

        // 处理缩略图上传
        string thumbnailUrl = model.Thumbnail != null
            ? await TrySaveThumbnailAsync(model.Thumbnail)
            : "";

        // 如果已有原本的图片，且没有上传新图片，则用原本图片
        if (model.ThumbnailUrl != null && thumbnailUrl == "")
        {
            thumbnailUrl = model.ThumbnailUrl;
        }

        // 保存帖子
        if (model.Id == null)
        {
            // 新帖
            var post = new Post
            {
                UserId = User.GetUserId(),
                Title = model.Title,
                Content = model.Content,
                Thumbnail = thumbnailUrl
            };
            db.Posts.Add(post);
        }
        else
        {
            // 编辑
            var post = db.Posts.Find(model.Id.Value);
            if (post == null)
                return NotFound();

            post.Title = model.Title;
            post.Content = model.Content;
            if (thumbnailUrl != null)
                post.Thumbnail = thumbnailUrl;
        }

        db.SaveChanges();
        return RedirectToAction("PostManage");
    }

    // 封装安全上传，返回 null 或 URL
    private async Task<string> TrySaveThumbnailAsync(IFormFile file)
    {
        try
        {
            return await FileHelper.SaveImageAsync(file);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return "";
        }
    }

    [HttpPost]
    public IActionResult DeletePost(int id)
    {
        var post = db.Posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        db.Posts.Remove(post);
        db.SaveChanges();

        return RedirectToAction("PostManage"); // 或者你要跳的页面
    }
}
