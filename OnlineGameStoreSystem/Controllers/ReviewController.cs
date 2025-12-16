using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class ReviewController : Controller
{
    private readonly DB db;

    public ReviewController(DB context)
    {
        db = context;
    }

    // 添加评论
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int gameId, string content, int rating)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("Content cannot be empty");

        var game = await db.Games.FindAsync(gameId);
        if (game == null)
            return NotFound("Game is not exist");

        if (rating <= 0 || rating > 5)
            return BadRequest("please make sure rating between 1 ~ 5");

        var userId = User.GetUserId();
        var user = await db.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized("User is not logged in");

        var review = new Review
        {
            GameId = gameId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            Rating = rating
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        return Ok();
    }

    // 获取指定游戏的所有评论
    [HttpGet]
    public async Task<IActionResult> GetByGame(int gameId)
    {
        var userId = User.GetUserId(); // -1 表示未登录

        var reviews = await db.Reviews
            .Where(c => c.GameId == gameId)
            .Include(c => c.User)
            .Include(c => c.Likes) // ⭐ 必须 include Likes
            .OrderBy(c => c.CreatedAt)
            .Where(c => c.Status == ActiveStatus.Active)
            .Select(c => new
            {
                c.Id,
                c.UserId,
                c.Content,
                c.CreatedAt,
                c.LikeCount,
                c.Rating,
                AuthorId = c.UserId,
                AuthorName = c.User.Username,
                AuthorAvatarUrl = c.User.AvatarUrl,

                // ⭐ 告诉前端当前用户是否点过赞
                IsLiked = userId != -1 && c.Likes.Any(l => l.UserId == userId)
            })
            .ToListAsync();

        return Json(new
        {
            reviews,
            currentUserId = userId
        });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int reviewId)
    {
        var review = await db.Reviews.FindAsync(reviewId);
        if (review == null) return NotFound();

        var userId = User.GetUserId();

        if (review.UserId != userId)
            return Forbid(); // 只有作者可以删除

        //db.Reviews.Remove(review);
        review.Status = ActiveStatus.Banned; // 软删除
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Report(int reviewId)
    {
        var review = await db.Reviews.FindAsync(reviewId);
        if (review == null) return NotFound();

        //db.Comments.Remove(comment);
        review.ReportedCount += 1;
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike([FromBody] LikeRequest request)
    {
        if (request.ReviewId <= 0)
            return BadRequest(new { success = false });

        var review = await db.Reviews.FindAsync(request.ReviewId);
        if (review == null)
            return NotFound(new { success = false });

        var userId = User.GetUserId();
        if (userId == -1)
            return Unauthorized(new { success = false });

        var user = await db.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized(new { success = false });

        var existing = await db.ReviewLikes
            .FirstOrDefaultAsync(l => l.ReviewId == request.ReviewId && l.UserId == userId);

        bool liked;

        if (existing != null)
        {
            // 已点赞 → 取消点赞
            db.ReviewLikes.Remove(existing);
            review.LikeCount--;
            liked = false;
        }
        else
        {
            // 未点赞 → 新增点赞
            db.ReviewLikes.Add(new ReviewLike
            {
                UserId = userId,
                ReviewId = request.ReviewId,
                CreatedAt = DateTime.UtcNow
            });

            review.LikeCount++;
            liked = true;
        }

        await db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            liked,
            likeCount = review.LikeCount
        });
    }


}
