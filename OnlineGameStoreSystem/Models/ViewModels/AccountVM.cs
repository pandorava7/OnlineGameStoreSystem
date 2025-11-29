using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels
{


    #region Register
    public class RegisterVM{

        [Required(ErrorMessage = "! Username is required")]
        [MaxLength(50)]
        [RegularExpression(@"^\S+$", ErrorMessage = "! Username cannot contain spaces")]
        public string Username { get; set; } = null!;


        [Required(ErrorMessage = "! Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "! Password is required")]
        [DataType(DataType.Password)]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "! Password must be between 8 and 12 characters")]
        [RegularExpression(@"^[A-Z][A-Za-z0-9!@#$%^&*()_+=-]{7,11}$",
        ErrorMessage = "! Password must start with a capital letter and be 8–12 characters long")]
        public string Password { get; set; } = null!;


        [Required(ErrorMessage = "! Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "! Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
    #endregion

    #region Login
    public class LoginVM
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
    #endregion

    #region ResetPassword
    public class ResetPasswordVM
    {
        // Email field, required and must be a valid email address
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        // OTP (One-Time Password), required with a custom error message
        [Required(ErrorMessage = "! OTP is required")]
        [MaxLength(6, ErrorMessage = "! OTP must be 6 digits")]
        public string OTP { get; set; } = null!;

        [Required(ErrorMessage = "! Password is required")]
        [DataType(DataType.Password)]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "! Password must be between 8 and 12 characters")]
        [RegularExpression(@"^[A-Z][A-Za-z0-9!@#$%^&*()_+=-]{7,11}$",
         ErrorMessage = "! Password must start with a capital letter and be 8–12 characters long")]
        public string NewPassword { get; set; } = null!;

        // Confirm password field, required, must match the NewPassword field
        [Required(ErrorMessage = "! Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "! Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;

    }
    #endregion
}