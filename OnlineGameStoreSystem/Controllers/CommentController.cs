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
        var comments = await db.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                c.UserId,
                AuthorName = c.User.Username,
                AuthorAvatarUrl = c.User.AvatarUrl
            })
            .ToListAsync();

        var currentUserId = User.GetUserId();

        return Json(new { comments, currentUserId });

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

}
