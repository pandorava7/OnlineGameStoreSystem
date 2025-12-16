using Microsoft.EntityFrameworkCore;

public static class TestDataCleaner
{
    public static void ClearAllTestData(DB db)
    {
        db.DeveloperRevenues.RemoveRange(db.DeveloperRevenues);
        db.GameLikes.RemoveRange(db.GameLikes);
        db.Purchases.RemoveRange(db.Purchases);
        db.Payments.RemoveRange(db.Payments);

        foreach (var game in db.Games)
        {
            game.LikeCount = 0;
            game.TotalDownload = 0;
        }

        db.SaveChanges();
        Console.WriteLine("🧹 测试数据已全部清空");
    }
}
