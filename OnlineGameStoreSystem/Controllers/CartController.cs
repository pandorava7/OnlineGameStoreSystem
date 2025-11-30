using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class CartController : Controller
{
    private readonly DB db;

    public CartController(DB context)
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
        // 假设当前用户 ID 是 1，实际可用 User.Identity 或其它方式获取
        var userId = User.GetUserId();

        // 查找用户
        var user = await db.Users.FindAsync(userId);
        if (user == null)
            return Json(new { success = false, message = "用户不存在" });

        // 查找用户现有的购物车
        var cart = await db.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId);

        // 如果购物车不存在，则创建
        if (cart == null)
        {
            cart = new ShoppingCart
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                User = user
            };
            db.ShoppingCarts.Add(cart);
            await db.SaveChangesAsync(); // 先保存，生成购物车 Id
        }

        // 添加商品到购物车
        var newCartItem = new CartItem
        {
            CartId = cart.Id, // 使用刚创建或已存在的购物车 Id
            GameId = gameId,
            AddedAt = DateTime.Now
        };

        db.CartItems.Add(newCartItem);
        await db.SaveChangesAsync();

        return Json(new { success = true });
    }


    [HttpPost] // 只能通过 POST 请求调用（安全做法）
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        // 1. 在数据库中查找该 CartItem
        var cartItem = db.CartItems.Find(cartItemId);

        // 2. 如果找到了，就删除
        if (cartItem != null)
        {
            db.CartItems.Remove(cartItem);
            db.SaveChanges(); // 保存更改到数据库

            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false });
        }
    }
}
