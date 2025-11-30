using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class CommentController : Controller
{
    private readonly DB db;

    public CommentController(DB context)
    {
        db = context;
    }

    // 添加评论
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int postId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("内容不能为空");

        var post = await db.Posts.FindAsync(postId);
        if (post == null)
            return NotFound("帖子不存在");

        var userId = int.Parse(User.FindFirst("UserId")!.Value);

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return Ok();
    }

    // 获取指定帖子的所有评论
    [HttpGet]
    public async Task<IActionResult> GetByPost(int postId)
    {
        var userId = User.GetUserId(); // -1 表示未登录

        var comments = await db.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.User)
            .Include(c => c.Likes) // ⭐ 必须 include Likes
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                c.LikeCount,
                AuthorName = c.User.Username,
                AuthorAvatarUrl = c.User.AvatarUrl,

                // ⭐ 告诉前端当前用户是否点过赞
                IsLiked = userId != -1 && c.Likes.Any(l => l.UserId == userId)
            })
            .ToListAsync();

        return Json(new
        {
            comments,
            currentUserId = userId
        });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int commentId)
    {
        var comment = await db.Comments.FindAsync(commentId);
        if (comment == null) return NotFound();

        var userId = User.GetUserId();

        if (comment.UserId != userId)
            return Forbid(); // 只有作者可以删除

        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike([FromBody] LikeRequest request)
    {
        if (request.CommentId <= 0)
            return BadRequest(new { success = false });

        var comment = await db.Comments.FindAsync(request.CommentId);
        if (comment == null)
            return NotFound(new { success = false });

        var userId = User.GetUserId();
        if (userId == -1)
            return Unauthorized(new { success = false });

        var user = await db.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized(new { success = false });

        var existing = await db.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == request.CommentId && l.UserId == userId);

        bool liked;

        if (existing != null)
        {
            // 已点赞 → 取消点赞
            db.CommentLikes.Remove(existing);
            comment.LikeCount--;
            liked = false;
        }
        else
        {
            // 未点赞 → 新增点赞
            db.CommentLikes.Add(new CommentLike
            {
                UserId = userId,
                CommentId = request.CommentId,
                CreatedAt = DateTime.UtcNow
            });

            comment.LikeCount++;
            liked = true;
        }

        await db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            liked,
            likeCount = comment.LikeCount
        });
    }


}