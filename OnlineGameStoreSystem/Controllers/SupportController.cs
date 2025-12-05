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
}
