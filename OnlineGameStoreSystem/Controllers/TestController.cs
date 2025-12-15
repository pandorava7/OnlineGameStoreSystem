using Microsoft.AspNetCore.Mvc;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Models.ViewModels;

public class TestController : Controller
{
    //[HttpGet]
    //public IActionResult Index()
    //{
    //    return View(new GamePublishViewModelTest());
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public IActionResult Index(GamePublishViewModelTest model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        ModelStateHelper.LogErrors(ModelState);
    //        return View(model);
    //    }

    //    // 打印所有上传文件信息
    //    var allFiles = new List<IFormFile> { model.Thumbnail, model.GameZip };
    //    allFiles.AddRange(model.PreviewImages);
    //    allFiles.AddRange(model.Trailers);

    //    Console.WriteLine("------ Uploaded Files ------");
    //    foreach (var f in allFiles)
    //    {
    //        if (f != null)
    //            Console.WriteLine($"{f.Name} - {f.FileName} - {f.Length} bytes");
    //    }

    //    TempData["Message"] = "Files uploaded successfully!";
    //    return RedirectToAction("Index");
    //}

}
