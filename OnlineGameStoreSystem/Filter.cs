using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

public class RequireAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // 未登录
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // 从 Claims 取 UserId（你肯定有）
        var userIdClaim = user.FindFirst("UserId");
        if (userIdClaim == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var db = context.HttpContext.RequestServices
            .GetRequiredService<DB>();

        var userId = int.Parse(userIdClaim.Value);

        var dbUser = db.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsAdmin })
            .FirstOrDefault();

        if (dbUser == null || !dbUser.IsAdmin)
        {
            context.Result = new ForbidResult(); // 或 RedirectToAction
        }
    }
}

public class RequireDeveloperAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // 未登录
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // 从 Claims 取 UserId（你肯定有）
        var userIdClaim = user.FindFirst("UserId");
        if (userIdClaim == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var db = context.HttpContext.RequestServices
            .GetRequiredService<DB>();

        var userId = int.Parse(userIdClaim.Value);

        var dbUser = db.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsDeveloper })
            .FirstOrDefault();

        if (dbUser == null || !dbUser.IsDeveloper)
        {
            context.Result = new ForbidResult(); // 或 RedirectToAction
        }
    }
}
