using Microsoft.EntityFrameworkCore;
using OnlineGameStoreSystem.Models;
using System;
using System.Linq;

public static class TestDataGenerator
{
    private static readonly Random _rand = new Random();

    public static void GenerateTestData(DB db)
    {
        var userIds = new[] { 9, 10, 11 };
        var gameIds = new[] { 1, 2, 3, 4, 5 };

        var users = db.Users.Where(u => userIds.Contains(u.Id)).ToList();
        var games = db.Games.Where(g => gameIds.Contains(g.Id)).ToList();

        var startTime = DateTime.UtcNow.AddYears(-1);
        var endTime = DateTime.UtcNow;

        foreach (var user in users)
        {
            foreach (var game in games)
            {
                // ===== 1. 购买 =====
                int purchaseCount = _rand.Next(10, 101);

                for (int i = 0; i < purchaseCount; i++)
                {
                    var time = RandomDate(startTime, endTime);
                    var price = game.GetDiscountedPrice();

                    var payment = new Payment
                    {
                        UserId = user.Id,
                        Amount = price,
                        PaymentMethod = _rand.Next(0, 2) == 0
                            ? PaymentMethod.Paypal
                            : PaymentMethod.TnG,
                        TransactionId = Guid.NewGuid().ToString(),
                        Status = PaymentStatus.Completed,
                        Purpose = PaymentPurposeType.Purchase,
                        CreatedAt = time
                    };
                    db.Payments.Add(payment);
                    db.SaveChanges();

                    var purchase = new Purchase
                    {
                        UserId = user.Id,
                        GameId = game.Id,
                        PaymentId = payment.Id,
                        PriceAtPurchase = price,
                        Status = PurchaseStatus.Completed,
                        RefundRequestedAt = DateTime.MinValue
                    };
                    db.Purchases.Add(purchase);
                    db.SaveChanges();

                    // ===== 2. 下载统计（70%~80% 概率）=====
                    if (_rand.NextDouble() >= 0.2) // ≈ 80%
                    {
                        game.TotalDownload++;
                    }

                    // ===== 3. 开发者收入 =====
                    var revenue = new DeveloperRevenue
                    {
                        DeveloperId = 2,
                        GameId = game.Id,
                        PurchaseId = purchase.Id,
                        Amount = price,
                        PlatformFee = price * 0.3m,
                        NetAmount = price * 0.7m,
                        GeneratedAt = time
                    };
                    db.DeveloperRevenues.Add(revenue);
                }

                // ===== 4. 点赞 =====
                int likeCount = _rand.Next(50, 501);
                for (int i = 0; i < likeCount; i++)
                {
                    db.GameLikes.Add(new GameLike
                    {
                        UserId = user.Id,
                        GameId = game.Id,
                        CreatedAt = RandomDate(startTime, endTime)
                    });

                    game.LikeCount++;
                }

                db.Games.Update(game);
                db.SaveChanges();
            }
        }

        Console.WriteLine("✅ 测试数据生成完成");
    }

    private static DateTime RandomDate(DateTime start, DateTime end)
    {
        var range = (end - start).TotalSeconds;
        return start.AddSeconds(_rand.NextDouble() * range);
    }
}
