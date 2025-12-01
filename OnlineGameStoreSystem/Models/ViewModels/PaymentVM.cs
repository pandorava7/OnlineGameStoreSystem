using OnlineGameStoreSystem.Models;
using System.ComponentModel.DataAnnotations;

#region PaymentVM
public class PaymentMethodViewModel
{
    [Required(ErrorMessage = "Please select a payment method")]
    public PaymentMethod? SelectedPaymentMethod { get; set; }
}

public class PaymentSummaryViewModel
{
    public List<PaymentSummaryGame> GameList { get; set; } = new List<PaymentSummaryGame>();
    public PaymentMethod? SelectedPaymentMethod { get; set; }
}
public class PaymentSummaryGame
{
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl {  get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProcessPaymentViewModel
{
    [Required]
    public PaymentMethod? SelectedPaymentMethod { get; set; }
}

#endregion
