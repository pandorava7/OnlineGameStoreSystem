using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;

        #region bridge
        public AccountController(DB _db, IEmailSender EmailSender)
        {
            db = _db;
            _passwordHasher = new PasswordHasher<User>();
            _emailSender =  EmailSender;
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

            user.PasswordHash = _passwordHasher.HashPassword(user,model.Password);

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

        [HttpPost]
        public async Task<IActionResult>ResetPassword(ResetPasswordVM model)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email does not exist");
                return View();
            }
            TempData["UserId"] = user.Id;
            // generate 6 digits OTP
            var otp = new Random().Next(100000, 999999).ToString();

            var otpEntry = db.OtpEntries.FirstOrDefault(o => o.UserId == user.Id);
            if (otpEntry != null)
            {
                otpEntry.OtpCode = otp;
                otpEntry.Expiry = DateTime.Now.AddMinutes(5);
                db.SaveChanges();
            }
            else
            {
                db.OtpEntries.Add(new OtpEntry
                {
                    UserId = user.Id,
                    OtpCode = otp,
                    Expiry = DateTime.Now.AddMinutes(5)
                });
                db.SaveChanges();
            }

     
            // send OTP to email
            await _emailSender.SendEmailAsync(model.Email, "Your OTP Code", $"Your OTP is：{otp}");
            return RedirectToAction("OTP");
        }

        #region Send ONE TIME PASSWORD (otp) 
        public IActionResult OTP()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OTP(ResetPasswordVM model)
        {
         
            int userId = (int)TempData["UserId"];

            // 找到用户
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                ModelState.AddModelError("OTP", "User not found.");
                return RedirectToAction("ResetPassword");
            }

            // 从数据库找到最新的 OTP
            var otpEntry = db.OtpEntries
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Expiry)
                .FirstOrDefault();

            // 判断 OTP 是否过期
            if (otpEntry.Expiry < DateTime.Now)
            {
                ModelState.AddModelError("OTP", "Your OTP has expired, please request a new one.");
                return RedirectToAction("ResetPassword");
            }

            // 判断 OTP 是否正确
            if (model.OTP != otpEntry.OtpCode)
            {
                ModelState.AddModelError("OTP", "Your OTP is incorrect.");
                TempData["UserId"] = userId; // 保留
                return View(model);
            }

            // OTP 正确 → 去 ResetPassword2
            TempData["UserId"] = userId; // 带过去
            TempData["FlashMessage"] = "OTP verified successfully!";
            TempData["FlashMessageType"] = "success";
            return RedirectToAction("ResetPassword2");
        }

        // Resend OTP method 
        public async Task<IActionResult> ResendOtp()
        {
            if (TempData["UserId"] == null)
            {
                return RedirectToAction("ResetPassword");
            }

            int userId = (int)TempData["UserId"];
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("ResetPassword");
            }

            // 生成新的 OTP
            var otp = new Random().Next(100000, 999999).ToString();

            var otpEntry = db.OtpEntries.FirstOrDefault(o => o.UserId == userId);
            if (otpEntry != null)
            {
                otpEntry.OtpCode = otp;
                otpEntry.Expiry = DateTime.Now.AddMinutes(5);
            }
            else
            {
                db.OtpEntries.Add(new OtpEntry
                {
                    UserId = user.Id,
                    OtpCode = otp,
                    Expiry = DateTime.Now.AddMinutes(5)
                });
            }

            db.SaveChanges();

            // 发送邮件
            await _emailSender.SendEmailAsync(user.Email, "Your OTP Code", $"Your new OTP is: {otp}");

            TempData["UserId"] = userId; // 保留 UserId
            return RedirectToAction("OTP"); // 返回 OTP 页面
        }
        #endregion

        public IActionResult ResetPassword2()
        {
            return View();
        }


        [HttpPost]
        public IActionResult ResetPassword2(ResetPasswordVM model)
        {
            // 从 TempData 获取 UserId（OTP 验证后存入的）
            if (TempData["UserId"] == null)
            {
                // 没有 UserId，说明流程不正确，返回输入邮箱页面
                return RedirectToAction("ResetPassword");
            }

            int userId = (int)TempData["UserId"];
            // 找到用户
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("ResetPassword");
            }
            // 验证新密码和确认密码是否一致（如果有 ConfirmPassword 字段）
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                TempData["UserId"] = userId; // 保留 UserId
                return View(model);
            }
            // 更新用户密码（安全哈希）
            user.PasswordHash = _passwordHasher.HashPassword(user,model.NewPassword);
            db.SaveChanges();
            // 可选：删除已使用的 OTP
            var otpEntries = db.OtpEntries.Where(o => o.UserId == userId).ToList();
            db.OtpEntries.RemoveRange(otpEntries);
            db.SaveChanges();

            // 显示成功消息
            TempData["FlashMessage"] = "Your password has been successfully changed!";
            TempData["FlashMessageType"] = "success";
            // 重设密码完成，跳转登录页面
            return RedirectToAction("Login");
        }
        #endregion

    }
}
