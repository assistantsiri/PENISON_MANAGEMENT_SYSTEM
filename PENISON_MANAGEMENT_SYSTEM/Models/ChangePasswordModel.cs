using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    //public class ChangePasswordModel
    //{
    //    [Required(ErrorMessage = "Current password is required")]
    //    [DataType(DataType.Password)]
    //    public string CurrentPassword { get; set; }

    //    [Required(ErrorMessage = "Current password is required")]
    //    [DataType(DataType.Password)]
    //    public string OldPassword { get; set; }


    //    [Required(ErrorMessage = "New password is required")]
    //    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    //    [DataType(DataType.Password)]
    //    public string NewPassword { get; set; }

    //    [Required(ErrorMessage = "Confirm new password is required")]
    //    [DataType(DataType.Password)]
    //    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    //    public string ConfirmNewPassword { get; set; }
    //}
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 number and 1 special character")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}