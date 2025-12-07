using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class WishlistController : Controller
{
    private readonly DB db;

    public WishlistController(DB context)
    {
        db = context;
    }

    // 获取购物车商品列表
    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var userId = User.GetUserId();

        var cart = await db.ShoppingCarts
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return Json(new { items = new List<object>() });

        var items = await (from ci in db.CartItems
                           join g in db.Games on ci.GameId equals g.Id
                           where ci.CartId == cart.Id
                           select new
                           {
                               id = ci.Id,
                               name = g.Title,
                               price = g.Price,
                               discount_price = g.DiscountPrice,
                               image = g.Media
                                   .Where(m => m.MediaType == "thumb")
                                   .Select(m => m.MediaUrl)
                                   .FirstOrDefault()
                           }).ToListAsync();

        return Json(new { items });
    }



    [HttpPost]
    public async Task<IActionResult> AddItem(int gameId)
    {
        var userId = User.GetUserId();

        // 查找用户
        var user = await db.Users
                           .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return Json(new { success = false, message = "User not found" });

        // 检查用户是否已经添加该游戏进愿望单
        bool alreadyWish = user.Wishlists.Any(p => p.GameId == gameId);
        if (alreadyWish)
        {
            return Json(new { success = false, message = "You already add this game to wishlist" });
        }

        // 添加商品到愿望单
        var wishItem = new Wishlist
        {
            UserId = userId,
            GameId = gameId,
            AddedAt = DateTime.Now
        };

        db.Wishlists.Add(wishItem);
        await db.SaveChangesAsync();

        return Json(new { success = true });
    }


    [HttpPost] // 只能通过 POST 请求调用（安全做法）
    public async Task<IActionResult> RemoveItem(int wishItemId)
    {
        // 1. 在数据库中查找该 CartItem
        var wishItem = db.Wishlists.Find(wishItemId);

        // 2. 如果找到了，就删除
        if (wishItem != null)
        {
            db.Wishlists.Remove(wishItem);
            db.SaveChanges(); // 保存更改到数据库

            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false });
        }
    }
}
