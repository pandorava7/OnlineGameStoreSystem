using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels;

public class DeveloperRegisterVM : IValidatableObject
{
    public DeveloperRegisterVM()
    {
        Dob = DateTime.Today; // 默认值为今天
    }

    // ---------------- 身份信息 ----------------
    [Required(ErrorMessage = "Legal English Name is required.")]
    [StringLength(100, ErrorMessage = "Legal English Name cannot exceed 100 characters.")]
    [Display(Name = "Legal English Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ID card number / Passport number is required.")]
    [StringLength(12, MinimumLength = 6, ErrorMessage = "ID / Passport number must be between 6 and 12 characters.")]
    [RegularExpression(@"^(?!0+$)[A-Za-z0-9]+$", ErrorMessage = "ID / Passport number must be alphanumeric and cannot be all zeros.")]
    [Display(Name = "ID / Passport")]
    public string IdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateTime Dob { get; set; }

    // ---------------- 地址信息 ----------------
    [Required(ErrorMessage = "Street Address 1 is required.")]
    [StringLength(100, ErrorMessage = "Street Address 1 cannot exceed 100 characters.")]
    [Display(Name = "Street Address 1")]
    public string Address1 { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Street Address 2 cannot exceed 100 characters.")]
    [Display(Name = "Street Address 2")]
    public string? Address2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State/Province is required.")]
    [StringLength(50, ErrorMessage = "State/Province cannot exceed 50 characters.")]
    [Display(Name = "State/Province")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country/Region is required.")]
    [StringLength(50, ErrorMessage = "Country/Region cannot exceed 50 characters.")]
    [Display(Name = "Country/Region")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal Code is required.")]
    [RegularExpression(@"^[0-9A-Za-z\s-]{3,15}$", ErrorMessage = "Invalid Postal Code format.")]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;

    // ---------------- 联系信息 ----------------
    [StringLength(10, ErrorMessage = "Country Code cannot exceed 10 characters.")]
    [Display(Name = "Phone Country Code")]
    public string? PhoneCountry { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(10, ErrorMessage = "Phone number cannot exceed 10 digits.")]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [Display(Name = "Notification Email")]
    public string NotifyEmail { get; set; } = string.Empty;

    // ---------------- 支付信息 ----------------
    [StringLength(50, ErrorMessage = "Account Number cannot exceed 50 characters.")]
    [Display(Name = "Account Number")]
    public string? AccountNumber { get; set; }

    [Required(ErrorMessage = "You must agree to the terms.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms.")]
    public bool Agree { get; set; }

    // ---------------- 合约 ----------------
    [StringLength(4000, ErrorMessage = "Contract content cannot exceed 4000 characters.")]
    [Display(Name = "Contract")]
    public string? Contract { get; set; }


    // ---------------- 选择支付方式（可选） ----------------
    [Required(ErrorMessage = "Please select a payment method.")]
    public string? SelectedPaymentMethod { get; set; }

    [StringLength(50)]
    public string? BankAcctNumber { get; set; }

    [StringLength(50)]
    public string? BankHolderName { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? PayPalEmail { get; set; }

    [StringLength(20)]
    public string? TNGNumber { get; set; }

    // ---------------- 条件验证 ----------------
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SelectedPaymentMethod == "bank")
        {
            if (string.IsNullOrWhiteSpace(BankAcctNumber))
                yield return new ValidationResult("Bank account number is required.", new[] { nameof(BankAcctNumber) });
            if (string.IsNullOrWhiteSpace(BankHolderName))
                yield return new ValidationResult("Bank holder name is required.", new[] { nameof(BankHolderName) });
        }
        else if (SelectedPaymentMethod == "paypal")
        {
            if (string.IsNullOrWhiteSpace(PayPalEmail))
                yield return new ValidationResult("PayPal email is required.", new[] { nameof(PayPalEmail) });
        }
        else if (SelectedPaymentMethod == "tng")
        {
            if (string.IsNullOrWhiteSpace(TNGNumber))
                yield return new ValidationResult("TNG number is required.", new[] { nameof(TNGNumber) });
        }
    }

    //// ---------------- 选择支付方式（可选） ----------------
    //public string? SelectedPaymentMethod { get; set; }
    //public string? BankAcctNumber { get; set; }
    //public string? BankHolderName { get; set; }
    //public string? PayPalEmail { get; set; }
    //public string? TNGNumber { get; set; }
}


public class DeveloperRegistrationViewModel
{
    public string DeveloperName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PaymentMethod SelectedPaymentMethod { get; set; }

    [Required(ErrorMessage = "You must confirm the payment before proceeding.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the payment before proceeding.")]
    public bool ConfirmPayment { get; set; }
}