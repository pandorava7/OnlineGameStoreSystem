using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Extensions;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace OnlineGameStoreSystem.Controllers;

public class AdminController : Controller
{
    private readonly DB _db;

    public AdminController(DB context)
    {
        _db = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult StatusManage(string? search, int? page)
    {
        int pageSize = 1;
        int pageNumber = page ?? 1;

        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search));
        }

        var vm = query
            .Where(u => u.Status != "deleted")
            .Select(u => new StatusManageVM
            {
                UserId = u.Id,
                UserName = u.Username,
                UserEmail = u.Email,
                Status = u.Status,
            })
            .OrderBy(u => u.UserId)
            .ToPagedList(pageNumber, pageSize);

        ViewData["Title"] = "User >> Status Manage";
        ViewData["Description"] = "This page is for managing user statuses.";

        return View(vm);
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        var user = _db.Users
            .Where(u => u.Id == id)
            .Select(u => new StatusManageVM
            {
                UserId = u.Id,
                UserName = u.Username,
                UserEmail = u.Email,
                Status = u.Status,
            })
            .FirstOrDefault();

        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    public IActionResult EditUser(StatusManageVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _db.Users.Find(model.UserId);
        if (user == null) return NotFound();

        user.Username = model.UserName;
        user.Email = model.UserEmail;
        user.Status = model.Status;

        _db.SaveChanges();

        return RedirectToAction("StatusManage");
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var user = _db.Users.FirstOrDefault(x => x.Id == id);
        if (user != null)
        {
            user.Status = "deleted";
            _db.SaveChanges();
        }

        // 删除后回到管理页面
        return RedirectToAction("StatusManage");
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
