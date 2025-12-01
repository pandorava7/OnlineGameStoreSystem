using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Transactions;
using OnlineGameStoreSystem.Helpers;

namespace OnlineGameStoreSystem.Controllers;

public class PaymentController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DB db;

    public PaymentController(ILogger<HomeController> logger, DB context)
    {
        _logger = logger;
        db = context;
    }

    [Route("payment/method")]
    public IActionResult Index(PaymentMethod? selectedPaymentMethod)
    {
        var userId = User.GetUserId();

        // 检查，确保用户登录，有购物车，购物车有物品
        if (userId == -1)
        {
            TempData["FlashMessage"] = "Please log in";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("ShoppingCart", "Home");
        }

        var cart = db.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            TempData["FlashMessage"] = "You dont have a cart";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("ShoppingCart", "Home");
        }

        if (!cart.Items.Any())
        {
            TempData["FlashMessage"] = "You cart is empty";
            TempData["FlashMessageType"] = "error";
            return RedirectToAction("ShoppingCart", "Home");
        }

        var vm = new PaymentMethodViewModel
        {
            SelectedPaymentMethod = selectedPaymentMethod
        };
        return View(vm);
    }

    [Route("payment/summary")]
    public IActionResult Summary(PaymentMethodViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // 回显页面，显示验证信息
            return View("Index", model);
        }

        var userId = User.GetUserId();

        // 1️⃣ 找到当前用户的购物车
        var cart = db.ShoppingCarts
            .Include(c => c.Items)               // 包含 CartItems
                .ThenInclude(ci => ci.Game)         // 包含每个 CartItem 的 Game
                .ThenInclude(g => g.Media)          // 包含 Game 的 Media
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            TempData["FlashMessage"] = "You don't have a cart";
            TempData["FlashMessageType"] = "error";
            return View(new PaymentSummaryViewModel());
        }

        // 2️⃣ 构建 PaymentSummaryGame 列表
        var gameList = cart.Items.Select(ci =>
        {
            var game = ci.Game;
            return new PaymentSummaryGame
            {
                Title = game.Title,
                Price = game.Price,
                ThumbnailUrl = game.Media
                    .Where(m => m.MediaType == "thumb")
                    .Select(m => m.MediaUrl)
                    .FirstOrDefault() ?? string.Empty
            };
        }).ToList();

        // 3️⃣ 构建 ViewModel
        var vm = new PaymentSummaryViewModel
        {
            SelectedPaymentMethod = model.SelectedPaymentMethod,
            GameList = gameList
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestModel request)
    {
        var selectedPaymentMethod = request.SelectedPaymentMethod;

        if (selectedPaymentMethod == null)
        {
            return Json(new
            {
                success = false,
                message = "Please select a payment method."
            });
        }

        var userId = User.GetUserId();

        // 1️⃣ 取购物车
        var cart = await db.ShoppingCarts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Game)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Json(new
            {
                success = false,
                message = "Your cart is empty."
            });
        }

        decimal totalAmount = cart.Items.Sum(ci => ci.Game.Price);

        // 2️⃣ 创建 Payment
        var payment = new Payment
        {
            UserId = userId,
            Amount = totalAmount,
            PaymentMethod = selectedPaymentMethod.Value,
            TransactionId = Guid.NewGuid().ToString(),
            Status = PaymentStatus.Pending
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        // 3️⃣ 创建 Purchase
        var purchases = cart.Items.Select(ci => new Purchase
        {
            UserId = userId,
            GameId = ci.GameId,
            PaymentId = payment.Id,
            PriceAtPurchase = ci.Game.Price,
            Status = PurchaseStatus.Pending
        }).ToList();

        db.Purchases.AddRange(purchases);
        await db.SaveChangesAsync();

        // 4️⃣ 模拟支付成功
        payment.Status = PaymentStatus.Completed;
        purchases.ForEach(p => p.Status = PurchaseStatus.Completed);
        await db.SaveChangesAsync();

        // 5️⃣ 清空购物车
        db.CartItems.RemoveRange(cart.Items);
        await db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = "Your payment is successfully!",
            paymentId = payment.Id
        });
    }

    public class PaymentRequestModel
    {
        public PaymentMethod? SelectedPaymentMethod { get; set; }
    }


    [HttpGet]
    [Route("payment/success")]
    public IActionResult Success()
    {
        return View();
    }
}