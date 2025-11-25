using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;

namespace OnlineGameStoreSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB db;
        private readonly PasswordHasher<User> _passwordHasher;

        #region bridge
        public AccountController(DB _db)
        {
            db = _db;
            _passwordHasher = new PasswordHasher<User>(); 
        }
        #endregion 

        #region log in
        public IActionResult Login()
        {
            return View();
        }
        #endregion 

        #region Register
        public IActionResult Register() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 检查是否已有相同邮箱
            if (db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            db.Users.Add(user);
            db.SaveChanges();

            // 注册成功后跳转登录页
            // 将消息存入 TempData
            TempData["FlashMessage"] = "Account registered successfully!";
            TempData["FlashMessageType"] = "success"; // success, error, warning, info
            return RedirectToAction("Login", "Account");
        }
        #endregion


        #region Reset Password
        public IActionResult ResetPassword()
        {
            return View();
        }

        public IActionResult OTP()
        {
            return View();
        }

        public IActionResult ResetPassword2()
        {
            return View();
        }
        #endregion

    }
}
