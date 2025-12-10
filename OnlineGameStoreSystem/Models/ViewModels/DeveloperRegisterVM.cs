using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels
{
    public class DeveloperRegisterVM
    {
        // 身份信息
        [Required(ErrorMessage = "Legal English Name is required.")]
        [StringLength(100, ErrorMessage = "Legal English Name cannot exceed 100 characters.")]
        [Display(Name = "Legal English Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(12, ErrorMessage = "ID / Passport number cannot exceed 12 characters.")]
        [RegularExpression(@"^(?!0+$).*$", ErrorMessage = "ID / Passport cannot be all zeros.")]
        [Display(Name = "ID / Passport")]
        public string? IdNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? Dob { get; set; }

        // 地址信息
        [StringLength(100, ErrorMessage = "Street Address 1 cannot exceed 100 characters.")]
        [Display(Name = "Street Address 1")]
        public string? Address1 { get; set; }

        [StringLength(100, ErrorMessage = "Street Address 2 cannot exceed 100 characters.")]
        [Display(Name = "Street Address 2")]
        public string? Address2 { get; set; }

        [StringLength(50, ErrorMessage = "State/Province cannot exceed 50 characters.")]
        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [RegularExpression(@"^[0-9A-Za-z\s-]{3,15}$", ErrorMessage = "Invalid Postal Code format.")]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "Country/Region cannot exceed 50 characters.")]
        [Display(Name = "Country/Region")]
        public string? Country { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string? City { get; set; }

        // 联系信息
        [StringLength(10, ErrorMessage = "Country Code cannot exceed 10 characters.")]
        [Display(Name = "Phone Country Code")]
        public string? PhoneCountry { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(10, ErrorMessage = "Phone number cannot exceed 10 digits.")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Required(ErrorMessage = "Notification Email is required.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Notification Email")]
        public string? NotifyEmail { get; set; }

        // 支付与合同
        [StringLength(50, ErrorMessage = "Account Number cannot exceed 50 characters.")]
        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [StringLength(4000, ErrorMessage = "Contract content cannot exceed 4000 characters.")]
        [Display(Name = "Contract")]
        public string? Contract { get; set; }

        [Required(ErrorMessage = "You must agree to the terms.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms.")]
        public bool Agree { get; set; }

        // [新增] 用于存储前端选择的支付方式 (通过隐藏字段提交)
        public string? SelectedPaymentMethod { get; set; }
        public string? BankAcctNumber { get; set; }
        public string? BankHolderName { get; set; }
        public string? PayPalEmail { get; set; }
        public string? TNGNumber { get; set; }
    }

    public class DeveloperRegistrationViewModel
    {
        public string DeveloperName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SelectedPaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must confirm the payment before proceeding.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the payment before proceeding.")]
        public bool ConfirmPayment { get; set; }
    }
}