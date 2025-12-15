using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;




namespace OnlineGameStoreSystem.Extensions;

public static class ControllerExtensions
{
    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
    {
        controller.ViewData.Model = model;

        using var sw = new StringWriter();
        var engine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
        var viewResult = engine!.FindView(controller.ControllerContext, viewName, !partial);

        if (viewResult.View == null) throw new ArgumentNullException($"{viewName} does not match any available view");

        var viewContext = new ViewContext(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Return the UserId claim value as an integer.
    /// Return -1 if the claim is not found or cannot be parsed.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirst("UserId")?.Value ?? "-1");
    }
}