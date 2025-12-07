namespace OnlineGameStoreSystem.Models
{
    public enum ActiveStatus
    {
        Active,
        Banned,
    }

    public enum LikeableType
    {
        Review,
        Comment
    }

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
        Refunding,
        Refunded
    }

    public enum GameStatus
    {
        Published,
        Pending,
        Removed
    }

    public enum PaymentPurposeType
    {
        DeveloperRegistration,
        Purchase,
    }
}


