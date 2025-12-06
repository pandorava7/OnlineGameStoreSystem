using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Helpers;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Net;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace OnlineGameStoreSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB db;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IEmailSender _emailSender;
        private readonly SecurityHelper hp;
        private readonly IHttpContextAccessor http;

        #region bridge
        public AccountController(DB _db, IEmailSender EmailSender, SecurityHelper helper,
            IHttpContextAccessor accessor)
        {
            db = _db;
            _passwordHasher = new PasswordHasher<User>();
            _emailSender =  EmailSender;
            hp = helper;
            http = accessor;
        }
        #endregion 


        #region log in
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? returnURL)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || !hp.VerifyPassword(user, model.Password))
            {
                ModelState.AddModelError("Password", "Incorrect email or password.");
                return View(model);
            }

            // 🔒 新增账号状态判断
            if (user.Status == "deleted")
            {
                ModelState.AddModelError("Password", "This account has been deleted and cannot log in.");
                return View(model);
            }

            if (user.Status == "banned")
            {
                ModelState.AddModelError("Password", "This account has been banned. Please contact support.");
                return View(model);
            }

            // 登录
            await hp.SignIn(user, model.RememberMe);

            TempData["FlashMessage"] = "Login successfully.";
            TempData["FlashMessageType"] = "success";

            if (!string.IsNullOrEmpty(returnURL))
                return Redirect(returnURL);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await hp.SignOut();

            TempData["FlashMessage"] = "Logout successfully.";
            TempData["FlashMessageType"] = "success";

            return RedirectToAction("Index", "Home");
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
                return View(model);
            }        

            TempData["UserId"] = user.Id;
            // generate 6 digits OTP
            var otp = new Random().Next(100000, 999999).ToString();

            var otpEntry = db.OtpEntries.FirstOrDefault(o => o.UserId == user.Id);
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

            // 4️⃣ 调试：查看所有实体的 EF Core 状态
            foreach (var entry in db.ChangeTracker.Entries())
            {
                Console.WriteLine($"{entry.Entity.GetType().Name} - {entry.State}");
            }

            db.SaveChanges();


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
            TempData.Keep("UserId");
            // 找到用户
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("ResetPassword");
            }
            // 比较旧密码与新密码（使用 PasswordHasher）
            var passwordHasher = new PasswordHasher<User>();

            // 将新密码与旧密码 Hash 比较
            var compare = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.NewPassword);

            // 如果新密码与旧密码一样
            if (compare == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("NewPassword", "New password cannot be the same as the old password.");
                return View(model);
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


        #region account settings
        // 1. 显示当前邮箱和链接的 Action (EmailSettings.cshtml)
        public IActionResult AccountSetting()
        {
            // 从 Claims 取 UserId
            var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account"); // 未登入 → 返回登入页

            int userId = int.Parse(userIdString);

            // 找到当前登入的 user
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 创建 ViewModel
            var viewModel = new ChangeEmailViewModel
            {
                CurrentEmail = user.Email // 获取当前邮箱
            };

            return View(viewModel);
        }
        #endregion

        #region Change Email
        public IActionResult ChangeEmail()
        {
            // 从 Claims 取 UserId
            var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account"); // 未登入 → 返回登入页
            int userId = int.Parse(userIdString);
            // 找到当前登入的 user
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var viewModel = new ChangeEmailViewModel
            {
                CurrentEmail = user!.Email,
                NewEmail = user.Email // <-- 将 Current Email 设置为 NewEmail 字段的初始值
            };

            return View(viewModel);
        }

        //--------------------------------------------------------------
        // ChangeEmail Action (POST - Processes the form submission)
        //--------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            // 从 Claims 取 UserId
            var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account"); // 未登入 → 返回登入页
            int userId = int.Parse(userIdString);
            // 找到当前登入的 user
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Set CurrentEmail back into the model if validation fails
            model.CurrentEmail = user!.Email;

            if (!ModelState.IsValid)
            {
                // If validation fails, return to the view with errors
                return View(model);
            }

            // 1. Check if the new email is already taken or is the same as the current email
            if (model.NewEmail != user.Email)
            {
                // 检查新邮箱是否已被占用 (同步查找)
                var existingUser = db.Users.FirstOrDefault(u => u.Email == model.NewEmail && u.Id != user.Id);
                if (existingUser != null)
                {
                    ModelState.AddModelError("NewEmail", "This email address is already in use by another account.");
                    return View(model);
                }
            }
            else
            {              
                // 如果用户提交了相同的邮箱
                ModelState.AddModelError("NewEmail", "Your email address is already set to this value.");
                return View(model);
            }

            // 2. Update the database
            user.Email = model.NewEmail;
            db.SaveChanges(); // **Save the changes to the database**

            // Success: Redirect to a settings page
            TempData["FlashMessage"] = "Your email address has been successfully updated!";
            TempData["FlashMessageType"] = "success";
            return RedirectToAction("AccountSetting", "Account");
        }
        #endregion

        #region Update Password
        public IActionResult ChangePassword()
        {
            return View();
        }

        //--------------------------------------------------------------
        // ChangePassword Action (POST - Processes the form submission)
        //--------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(UpdatePasswordVM model)
        {
            // 从 Claims 取 UserId
            var userIdString = http.HttpContext!.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account"); // 未登入 → 返回登入页

            int userId = int.Parse(userIdString);

            // 找到当前登入的 user
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Verify Current Password (验证旧密码是否正确)
            var result = _passwordHasher.VerifyHashedPassword(user!, user!.PasswordHash, model.OldPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "! The current password entered is incorrect.");
                return View(model);
            }

            // 2. Check if New Password is the same as Old Password (新密码不能与旧密码相同)
            if (_passwordHasher.VerifyHashedPassword(user!, user.PasswordHash, model.NewPassword) == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("NewPassword", "! New password cannot be the same as the old password.");
                return View(model);
            }

            // 3. Hash and update New Password
            user.PasswordHash = _passwordHasher.HashPassword(user!, model.NewPassword);
            db.SaveChanges();

            // Success: Redirect with a success message
            TempData["FlashMessage"] = "Your password has been successfully changed!";
            TempData["FlashMessageType"] = "success";
            return RedirectToAction("AccountSetting", "Account");
        }


        #endregion

        #region Delete Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userIdString = HttpContext.User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString);

            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // Soft delete
            user.Status = "deleted";
            user.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            // logout using your helper
            await hp.SignOut();

            TempData["FlashMessage"] = "Account deleted successfully.";
            TempData["FlashMessageType"] = "success";

            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
