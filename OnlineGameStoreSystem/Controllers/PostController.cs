using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;

public class PostController : Controller
{
    private readonly DB db;

    public PostController(DB context)
    {
        db = context;
    }

    [HttpPost]
    public async Task<IActionResult> Like([FromBody] LikeRequest request)
    {
        if (request == null || request.PostId <= 0)
        {
            return Json(new
            {
                success = false,
                message = "Invalid PostId"
            });
        }

        var post = await db.Posts.FindAsync(request.PostId);
        if (post == null)
        {
            return Json(new
            {
                success = false,
                message = "Post not found"
            });
        }

        var userId = User.GetUserId();
        if (userId == -1)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Json(new
            {
                success = false,
                message = "Not logged in"
            });
        }

        var userExists = await db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return Json(new
            {
                success = false,
                message = "User not found"
            });
        }

        var existingLike = await db.PostLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == request.PostId);

        if (existingLike != null)
        {
            return Json(new
            {
                success = false,
                message = "Already liked"
            });
        }

        var like = new PostLike
        {
            UserId = userId,
            PostId = request.PostId,
            CreatedAt = DateTime.UtcNow
        };

        db.PostLikes.Add(like);

        post.LikeCount++;
        await db.SaveChangesAsync();

        return Json(new
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
    public int ReviewId { get; set; }
}
