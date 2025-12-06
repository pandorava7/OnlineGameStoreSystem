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

            case "refund-request":
                return RedirectToAction("RefundRequest");


            case "feedback-submit":
                return RedirectToAction("FeedbackSubmit");

            default: return RedirectToAction("Index");
        }
    }

    [Authorize]
    public IActionResult TrackPurchaseHistory(string? search, int? page)
    {
        int pageSize = 1;
        int pageNumber = page ?? 1;

        var query = _db.Payments
            .Include(p => p.Purchases)
            .ThenInclude(purchase => purchase.Game)
            .AsQueryable();

        var userId = User.GetUserId();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Purpose.ToString().Contains(search));
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

    public IActionResult RefundRequest()
    {


        return View();
    }
}
