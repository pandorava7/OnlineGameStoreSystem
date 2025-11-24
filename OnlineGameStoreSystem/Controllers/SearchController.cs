using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class SearchController : Controller
{
    private readonly DB db;

    public SearchController(DB context)
    {
        db = context;
    }

    public IActionResult Index(string term)
    {
        if (string.IsNullOrEmpty(term))
        {
            return View("Index", new List<Post>());
        }

        var results = db.Posts
            .Where(p => p.Title.Contains(term) || p.Content.Contains(term))
            .ToList();

        return View("/Views/Home/Search.cshtml",results);
    }
}
