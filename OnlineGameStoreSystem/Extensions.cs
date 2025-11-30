using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;




namespace OnlineGameStoreSystem.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirst("UserId")?.Value ?? "-1");
    }
}