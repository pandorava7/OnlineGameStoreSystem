using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> GetItems(int userId)
    {
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
                               image = g.Media
                                   .Where(m => m.MediaType == "thumb")
                                   .Select(m => m.MediaUrl)
                                   .FirstOrDefault()
                           }).ToListAsync();

        return Json(new { items });
    }

    //// 添加商品
    //[HttpPost]
    //public async Task<IActionResult> AddItem(int userId, int gameId)
    //{
    //    var cart = await db.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId);
    //    if (cart == null)
    //    {
    //        cart = new ShoppingCart
    //        {
    //            UserId = userId,
    //            CreatedAt = DateTime.Now
    //        };
    //        db.ShoppingCarts.Add(cart);
    //        await db.SaveChangesAsync();
    //    }

    //    // 已存在就不再插入
    //    if (!await db.CartItems.AnyAsync(ci => ci.CartId == cart.Id && ci.GameId == gameId))
    //    {
    //        db.CartItems.Add(new CartItem
    //        {
    //            CartId = cart.Id,
    //            GameId = gameId,
    //            AddedAt = DateTime.Now
    //        });
    //        await db.SaveChangesAsync();
    //    }

    //    return Json(new { success = true });
    //}

    // 删除商品
    //[HttpPost]
    //public async Task<IActionResult> RemoveItem(int userId, int gameId)
    //{
    //    var cart = await db.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId);
    //    if (cart == null)
    //        return Json(new { success = false });

    //    var item = await db.CartItems
    //        .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.GameId == gameId);

    //    if (item != null)
    //    {
    //        db.CartItems.Remove(item);
    //        await db.SaveChangesAsync();
    //    }

    //    return Json(new { success = true });
    //}












    [Route("cart")]
    public IActionResult Index(string id)
    {
        // 1. 获取当前用户的 ID (假设是 1)
        int userId = 1;

        // 2. 从数据库获取该用户的购物车
        var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);

        // 如果还没有购物车，给个空的
        if (cart == null)
        {
            return View(new ShoppingCartViewModel
            {
                Items = new List<CartItemViewModel>(),
                TotalPrice = 0
            });
        }

        // 3. ✅ 关键修改：从数据库查询真实的 CartItems (不要用 new List<CartItem>...)
        // 必须加上 .Include(c => c.Game) 否则游戏信息是空的
        var dbItems = db.CartItems
            .Where(c => c.CartId == cart.Id)
            .Include(c => c.Game)
            .ToList();

        // 4. 转换数据格式给页面用
        var viewModelItems = dbItems.Select(item => new CartItemViewModel
        {
            Item = item, // 这里把真实的 ID (比如 5, 6) 传给页面
            ThumbnailUrl = $"/images/example/silksong.png"
        }).ToList();

        var viewModel = new ShoppingCartViewModel
        {
            Items = viewModelItems,
            TotalPrice = viewModelItems.Sum(x => x.Item.Game.Price) // 简单计算总价
        };

        return View("~/Views/Home/ShoppingCart.cshtml", viewModel);
        //var exampleGame = db.Games.FirstOrDefault();

        //  var shoppingCartViewModel = new ShoppingCartViewModel
        //{
        //  Items = new List<CartItemViewModel>
        //{
        //  new  CartItemViewModel
        //{
        //  Item = new CartItem { GameId = 1, CartId = 1, AddedAt = DateTime.Now, Game = exampleGame },
        //ThumbnailUrl = "/images/example/silksong.png"
        //},
        //           new  CartItemViewModel
        //           {
        //               Item = new CartItem { GameId = 2, CartId = 1, AddedAt = DateTime.Now, Game = exampleGame },
        //               ThumbnailUrl = "/images/example/cyberpunk2077.png"
        //           }
        //       },
        //       TotalPrice = 199.99m
        //   };
        //return View(shoppingCartViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddItem(int gameId)
    {
        // ... 找到 cartId 的逻辑 ...
        int cartId = 1; // 示例值

        // 实际添加操作
        var newCartItem = new CartItem
        {
            CartId = cartId,
            GameId = gameId,
            AddedAt = DateTime.Now
        };

        db.CartItems.Add(newCartItem); // 使用 CartItems DbSet
        db.SaveChanges();

        return Json(new { success = true });

    }
    // CartController.cs

    [HttpPost] // 只能通过 POST 请求调用（安全做法）
    public async Task<IActionResult> RemoveItem(int cartId)
    {
        // 1. 在数据库中查找该 CartItem
        var cartItem = db.CartItems.Find(cartId);

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
