namespace OnlineGameStoreSystem.Models
{
    public enum PaymentMethod
    {
        Paypal,
        TnG
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    public enum PurchaseStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum GameStatus
    {
        Published,
        Pending,
        Removed
    }
}


