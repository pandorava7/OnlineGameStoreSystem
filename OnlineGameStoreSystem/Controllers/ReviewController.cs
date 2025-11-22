using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using System.Diagnostics;

namespace OnlineGameStoreSystem.Controllers;

public class ReviewController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DB db;

    public ReviewController(ILogger<HomeController> logger, DB context)
    {
        _logger = logger;
        db = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Add([FromBody] ReviewInputModel model)
    {
        return Json(new { success = true, message = "评论成功", rating = model.SelectedRating, text = model.Text});
    }
}
