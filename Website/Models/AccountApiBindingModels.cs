using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Runnymede.Website.Models
{
    public class ChangePasswordBindingModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword1 { get; set; }

        [Required]
        [Compare("NewPassword1", ErrorMessage = "Passwords do not match.")]
        public string NewPassword2 { get; set; }
    }

    ////public class SetPasswordBindingModel
    ////{
    ////    [Required]
    ////    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    ////    [DataType(DataType.Password)]
    ////    [Display(Name = "New password")]
    ////    public string NewPassword { get; set; }

    ////    [DataType(DataType.Password)]
    ////    [Display(Name = "Confirm new password")]
    ////    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    ////    public string ConfirmPassword { get; set; }
    ////}

    public class CreateBindingModel
    {
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)] // Anyway UserManager.CreateAsync() enforces minimum length 6 characters internally.
        public string Password { get; set; }

        [StringLength(100)]
        public string DisplayName { get; set; }

        [Required]
        public bool Consent { get; set; }

        [StringLength(100)]
        public string LocalTime { get; set; }

        public int? LocalTimezoneOffset { get; set; }
    }

    public class SignedInBindingModel
    {
        public bool Persistent { get; set; }

        [StringLength(100)]
        public string LocalTime { get; set; }

        public int? LocalTimezoneOffset { get; set; }
    }
}
