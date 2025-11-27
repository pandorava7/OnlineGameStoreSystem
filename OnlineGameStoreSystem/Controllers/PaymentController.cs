using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using OnlineGameStoreSystem.Models.ViewModels;
using System.Transactions;

namespace OnlineGameStoreSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DB db;

        public PaymentController(ILogger<HomeController> logger, DB context)
        {
            _logger = logger;
            db = context;
        }
        [Route("Payment")]
        public IActionResult Index()
        {
            // 创建一个空的 model 传给页面
            var model = new Payment();
            return View(model);
        }

     
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentVM input)
        {
            if (!ModelState.IsValid)
            {
                return View("PaymentPage", input);
            }

            // 1. 创建 Payment 对象 (之前叫 Transaction)
            var newPayment = new Payment();

            // 2. 填资料
            newPayment.UserId = 1; // 这里的 1 记得换成获取当前用户的代码
            newPayment.Amount = input.Amount;
            newPayment.PaymentMethod = input.SelectedPaymentMethod;
            newPayment.CreatedAt = DateTime.Now;

            // 如果你的表里还需要 TransactionId 这种单号
            newPayment.TransactionId = "TX" + DateTime.Now.Ticks.ToString();

            // 3. 保存到 Payment 表
            db.Payments.Add(newPayment); // 假设你的 DbSet 叫 Payments
            await db.SaveChangesAsync();

            TempData["Messages"] = "Payment processed successfully!";
            return RedirectToAction("PaymentSuccess");
        }
        public IActionResult PaymentSuccess()
        {
            return View();
        }
    }
}