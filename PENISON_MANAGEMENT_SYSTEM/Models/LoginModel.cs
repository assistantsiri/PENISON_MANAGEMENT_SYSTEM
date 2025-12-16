using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class LoginModel
    {
        //public LoginModel()
        //{
        //    Username = string.Empty;
        //    SALT = string.Empty;
        //    Password = string.Empty;
        //    RoleID = 0;
        //}
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; }
        public string SALT { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }
        public int RoleID { get; set; }
        [Required(ErrorMessage = "CAPTCHA code is required")]
        [StringLength(6, ErrorMessage = "CAPTCHA code must be 6 characters")]
        public string CaptchaCode { get; set; }

        public string StaffNo {  get; set; }
        public string StaffName {  get; set; }
        public string TempPassword { get; set; }


        public int LoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool IsActive { get; set; }

        public string Status { get; set; }

        public string LoginMessage { get; set; }

        public string SessionToken { get; set; }

        public string ActiveSessionToken { get; set; }

        public DateTime? LastActivityTime { get; set; }

        public bool CanLogin { get; set; }


    }
    public class UserModel
    {
        [Required(ErrorMessage = "Staff Number is required")]
        [Display(Name = "Staff Number")]
        public string StaffNo { get; set; }

        [Required(ErrorMessage = "Staff Name is required")]
        [Display(Name = "Staff Name")]
        public string StaffName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string USERNAME { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PSW { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; }
    }
}