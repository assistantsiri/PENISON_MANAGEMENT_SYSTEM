using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{


    //public class LoginModel
    //{
    //    [Required(ErrorMessage = "Username is required")]
    //    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    //    public string Username { get; set; }

    //    [Required(ErrorMessage = "Password is required")]
    //    [DataType(DataType.Password)]
    //    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    //    public string Password { get; set; }

    //    [Required(ErrorMessage = "CAPTCHA code is required")]
    //    [StringLength(6, ErrorMessage = "CAPTCHA code must be 6 characters")]
    //    public string CaptchaCode { get; set; }

    //    public int RoleID { get; set; }
    //    public string StaffNo { get; set; }
    //    public string StaffName { get; set; }
    //    public string SALT { get; set; } 
    //}



    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid role")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Staff number is required")]
        [StringLength(20, ErrorMessage = "Staff number cannot exceed 20 characters")]
        public string StaffNo { get; set; }

        [Required(ErrorMessage = "Staff name is required")]
        [StringLength(100, ErrorMessage = "Staff name cannot exceed 100 characters")]
        public string StaffName { get; set; }
    }

    
    public class ChangePasswordModel1
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }

    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
    }
   
}