using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels
{
    public class DeveloperRegisterVM
    {
        [Required]
        [Display(Name = "Legal English Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "ID / Passport")]
        public string? IdNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? Dob { get; set; }

        [Display(Name = "Street Address 1")]
        public string? Address1 { get; set; }

        [Display(Name = "Street Address 2")]
        public string? Address2 { get; set; }

        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Display(Name = "Country/Region")]
        public string? Country { get; set; }

        public string? City { get; set; }

        [Display(Name = "Phone Country Code")]
        public string? PhoneCountry { get; set; }

        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Notification Email")]
        public string? NotifyEmail { get; set; }

        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [Display(Name = "Contract")]
        public string? Contract { get; set; }

        [Required(ErrorMessage = "You must agree to the terms")]
        public bool Agree { get; set; }
    }

    public class DeveloperRegistrationViewModel
    {
        public string DeveloperName { get; set; }
        public string Email { get; set; }
        public string SelectedPaymentMethod { get; set; }
        public bool ConfirmPayment { get; set; }
    }
}