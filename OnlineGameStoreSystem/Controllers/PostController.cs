using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;

[Authorize]
public class PostController : Controller
{
    private readonly DB db;

    public PostController(DB context)
    {
        db = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like([FromBody] LikeRequest request)
    {
        if (request.PostId <= 0)
            return BadRequest(new { success = false, message = "Invalid PostId" });

        var post = await db.Posts.FindAsync(request.PostId);
        if (post == null)
            return NotFound(new { success = false, message = "Post not found" });

        var userId = User.GetUserId();
        if (userId == -1)
            return Unauthorized(new { success = false, message = "Not logged in" });

        var user  = await db.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "User not found" });

        // 检查是否已点赞
        var existingLike = await db.PostLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == request.PostId);

        if (existingLike != null)
        {
            return Ok(new { success = false, message = "Already liked" });
        }

        // 添加新的 Like
        var like = new PostLike
        {
            UserId = userId,
            User = user,
            PostId = request.PostId,
            Post = post,
            CreatedAt = DateTime.UtcNow,
        };
        db.PostLikes.Add(like);

        // 更新 Post 点赞数
        post.LikeCount++;
        await db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            post = new
            {
                post.Id,
                post.Title,
                post.LikeCount
            }
        });
    }
}

public class LikeRequest
{
    public int PostId { get; set; }

    public int CommentId { get; set; }
}
