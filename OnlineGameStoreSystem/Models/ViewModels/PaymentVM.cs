using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels;

#region PaymentVM
    public class PaymentVM
    {
        [Required]
        public decimal Amount { get; set; } 

    [Required(ErrorMessage = "Please select a payment method")]
    public PaymentMethod SelectedPaymentMethod { get; set; }
    
}

#endregion
