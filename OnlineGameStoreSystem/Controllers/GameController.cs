using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Helpers;

[Route("[controller]")]
public class GameController : Controller
{
    private readonly DB db;

    public GameController(DB context)
    {
        db = context;
    }

    [HttpPost("Like")]
    public async Task<IActionResult> Like([FromBody] GameLikeRequest request)
    {
        if (request == null || request.GameId <= 0)
        {
            return Json(new
            {
                success = false,
                message = "Invalid GameId"
            });
        }

        var game = await db.Games.FindAsync(request.GameId);
        if (game == null)
        {
            return Json(new
            {
                success = false,
                message = "Game not found"
            });
        }

        var userId = User.GetUserId();
        if (userId == -1)
        {
            //Response.StatusCode = StatusCodes.Status401Unauthorized;
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

        var existingLike = await db.GameLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.GameId == request.GameId);

        if (existingLike != null)
        {
            return Json(new
            {
                success = false,
                message = "Already liked"
            });
        }

        var like = new GameLike
        {
            UserId = userId,
            GameId = request.GameId,
            CreatedAt = DateTime.UtcNow
        };

        db.GameLikes.Add(like);

        game.LikeCount++;
        await db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            game = new
            {
                game.Id,
                game.Title,
                game.LikeCount
            }
        });
    }
}

public class GameLikeRequest
{
    public int GameId { get; set; }
}
