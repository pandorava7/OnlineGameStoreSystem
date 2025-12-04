using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class AdminController : Controller
{
    private readonly DB db;

    public AdminController(DB context)
    {
        db = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult StatusManage()
    {
        return View();
    }

    public IActionResult RegistrationApproval()
    {
        return View();
    }

    public IActionResult Permissions()
    {
        return View();
    }

    public IActionResult GameReleaseReview()
    {
        return View();
    }

    public IActionResult GameManagement()
    {
        return View();
    }

    public IActionResult RefundHandling()
    {
        return View();
    }

    public IActionResult WebsiteStatistics()
    {
        return View();
    }

    public IActionResult TrackPurchase()
    {
        return View();
    }

    public IActionResult PostManagement()
    {
        return View();
    }

    public IActionResult CommentManagement()
    {
        return View();
    }

    public IActionResult GameReviewManagement()
    {
        return View();
    }

    public IActionResult CommunityStatistics()
    {
        return View();
    }

}
