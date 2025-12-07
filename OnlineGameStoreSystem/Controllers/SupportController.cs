using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using X.PagedList.Extensions;

namespace OnlineGameStoreSystem.Controllers;

public class SupportController : Controller
{
    private readonly DB _db;

    public SupportController(DB context)
    {
        _db = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Depend on question string and redirect to diff action
    public IActionResult RedirectByQuestion(string question)
    {
        switch (question)
        {
            case "track-purchase-history":
                return RedirectToAction("TrackPurchaseHistory");

            case "feedback-submit":
                return RedirectToAction("FeedbackSubmit");

            default: return RedirectToAction("Index");
        }
    }

    [Authorize]
    public IActionResult TrackPurchaseHistory(string? search, int? page)
    {
        int pageSize = 10;
        int pageNumber = page ?? 1;

        var query = _db.Payments
            .Include(p => p.Purchases)
            .ThenInclude(purchase => purchase.Game)
            .AsQueryable();

        var userId = User.GetUserId();

        if (!string.IsNullOrEmpty(search))
        {
            query = query
                .AsEnumerable() // 切换到内存 LINQ
                .Where(p => p.Purpose.ToString().ToLower().Contains(search.ToLower()))
                .AsQueryable();
        }

        var vm = query
            .Where(p => p.UserId == userId)
            .Select(p => new PurchaseItemVM
            {
                // if this payment is registration, just create new list for that
                // if this is not registration, take purchase gamaes name to list
                PaymentId = p.Id,
                Items = p.Purpose == PaymentPurposeType.DeveloperRegistration
                    ? new List<string> { "Registration fee" }
                     : p.Purchases.
                     Select(purchase => purchase.Game.Title.ToString())
                     .ToList(),
                PaymentMethod = p.PaymentMethod.ToString(),
                PurchaseDate = p.CreatedAt,
                PurchasePurpose = p.Purpose.ToString(),
                Total = p.Amount,
            })
            .OrderBy(p => p.PurchaseDate)
            .ToPagedList(pageNumber, pageSize);

        var user = _db.Users.Find(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        ViewData["UserName"] = user.Username;

        return View(vm);
    }

    public IActionResult ViewPurchaseDetail(int paymentId)
    {
        var payment = _db.Payments.Find(paymentId);
        if (payment == null)
            return NotFound("payment not found");

        decimal subtotal = payment.Purchases.Sum(p => p.PriceAtPurchase);
        decimal total = payment.Amount;
        decimal discount = total - subtotal;

        var vm = new PurchaseDetailVM
        {
            PaymentId = payment.Id,
            PaymentMethod = payment.PaymentMethod.ToString(),
            PurchaseDate = payment.CreatedAt,
            TransactionId = payment.TransactionId,
            PurchaseItems = payment.Purpose == PaymentPurposeType.DeveloperRegistration
                    ? new List<PurchaseItem> { new PurchaseItem { Name = "Registration Fee", Price = SystemConstants.RegistrationFee } }
                     : payment.Purchases
                        .Select(p => new PurchaseItem
                        {
                            Name = p.Game.Title,
                            Price = p.PriceAtPurchase
                        })
                        .ToList(),
            Subtotal = subtotal,
            Discount = discount,
            Total = total,
        };

        return View(vm);
    }

    public IActionResult RefundRequest(int paymentId)
    {
        var payment = _db.Payments
            .Include(p => p.Purchases)
                .ThenInclude(pu => pu.Game)
                    .ThenInclude(g => g.Media)
            .FirstOrDefault(p => p.Id == paymentId);

        if (payment == null)
            return NotFound("payment not found");

        var vm = new RefundPaymentVM
        {
            PaymentId = payment.Id,
            TotalAmount = payment.Amount,
            Purchases = payment.Purchases.Select(p => new RefundPurchaseItemVM
            {
                PurchaseId = p.Id,
                GameThumbnailUrl = p.Game.Media.FirstOrDefault(gm => gm.MediaType == "thumb")?.MediaUrl,
                GameTitle = p.Game.Title,
                Price = p.PriceAtPurchase
            }).ToList(),
            Reason = ""
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult RefundRequest(string reason, int[] selectedPurchaseIds, int paymentId)
    {
        // 重新加载 Payment 和 Purchases
        var payment = _db.Payments
            .Include(p => p.Purchases)
                .ThenInclude(pu => pu.Game)
                    .ThenInclude(g => g.Media)
            .FirstOrDefault(p => p.Id == paymentId);

        if (payment == null)
            return NotFound("payment not found");

        var vm = new RefundPaymentVM
        {
            PaymentId = payment.Id,
            TotalAmount = payment.Amount,
            Purchases = payment.Purchases.Select(p => new RefundPurchaseItemVM
            {
                PurchaseId = p.Id,
                GameThumbnailUrl = p.Game.Media.FirstOrDefault(gm => gm.MediaType == "thumb")?.MediaUrl,
                GameTitle = p.Game.Title,
                Price = p.PriceAtPurchase
            }).ToList(),
            Reason = reason
        };

        if (selectedPurchaseIds == null || selectedPurchaseIds.Length == 0)
        {
            ModelState.AddModelError("select", "Please select at least one item to refund.");
            return View(vm);
        }

        if(string.IsNullOrEmpty(reason))
        {
            ModelState.AddModelError("reason", "Please write your reason.");
            return View(vm);
        }

        // 处理退款逻辑
        foreach (var purchaseId in selectedPurchaseIds)
        {
            var purchase = _db.Purchases.Find(purchaseId);
            if (purchase != null)
            {
                // 标记Purchase为正在退款，此Purchase将会移交给管理员审核
                purchase.Status = PurchaseStatus.Refunding;
                purchase.RefundReason = reason;
                purchase.RefundRequestedAt = DateTime.Now;
            }
        }

        _db.SaveChanges();

        TempData["FlashMessage"] = "Refund request submitted successfully.";
        TempData["FlashMessageType"] = "success";
        return RedirectToAction("TrackPurchaseHistory");
    }


}
