using PENISON_MANAGEMENT_SYSTEM.DA;
using PENISON_MANAGEMENT_SYSTEM.Models;
using PENISON_MANAGEMENT_SYSTEM.WrapperHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class UserController : Controller
    {
       
        Helper helper = new Helper();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return View();
        }

        
        [HttpPost]
        public ActionResult Login(LoginModel loginModel)
        {
            
            try
            {
                if (ModelState.IsValid)
                {

                    var sessionCaptcha = Session["CaptchaCode"]?.ToString() ?? "";
                    var userCaptcha = loginModel.CaptchaCode?.ToUpper() ?? "";

                    if (userCaptcha != sessionCaptcha)
                    {
                        ModelState.AddModelError("CaptchaCode", "CAPTCHA code is incorrect. Please try again.");
                        TempData["LoginSuccess"] = "CAPTCHA verification failed";
                        return View(loginModel);
                    }


                    var user = helper.ValidateLogin(loginModel);

                    

                    if (user != null && user.Status == "LOCKED")
                    {
                        ModelState.AddModelError("", $"Your account is locked until {user.LockedUntil}.");
                        TempData["LoginSuccess"] = "Your account is temporarily locked.";
                        return View(loginModel);
                    }


                    if (user == null || (user.Status == "INVALID"))
                    {
                        ModelState.AddModelError("", "Invalid username or password.");
                        TempData["LoginSuccess"] = "Invalid login credentials.";
                        return View(loginModel);
                    }

                    #region
                    //if (user.Status == "SUCCESS")
                    //{
                    //    Session.Clear();
                    //    var sessionToken = Guid.NewGuid().ToString();

                    //    Session["SessionToken"] = sessionToken;

                    //    HttpCookie cookie = new HttpCookie("SessionToken", sessionToken)
                    //    {
                    //        HttpOnly = true,
                    //        Secure = Request.IsSecureConnection,
                    //        SameSite = SameSiteMode.Strict
                    //    };
                    //    Response.Cookies.Add(cookie);

                    //    //if (!string.IsNullOrEmpty(user.SessionToken))
                    //    //{
                    //    //    ModelState.AddModelError("", "This user is already logged in from another device.");
                    //    //    return View(loginModel);
                    //    //}

                    //    // Date Commented Testing Purpose
                    //    if (!string.IsNullOrEmpty(user.SessionToken))
                    //    {
                    //        if (user.LastActivityTime.HasValue)
                    //        {
                    //            double minutesInactive = (DateTime.Now - user.LastActivityTime.Value).TotalMinutes;

                    //            if (minutesInactive < 30)
                    //            {

                    //                ModelState.AddModelError("", "This user is already logged in from another device. Try after 30 minutes or contact admin.");
                    //                TempData["AlreadyLogin"] = "This user is already logged in from another device. Try after 30 minutes or contact admin..";
                    //                return View(loginModel);
                    //            }
                    //            else
                    //            {

                    //                helper.ClearSessionToken(user.Username);
                    //            }
                    //        }
                    //        else
                    //        {

                    //            helper.ClearSessionToken(user.Username);
                    //        }
                    //    }

                    //    Session["USERNAME"] = user.Username;
                    //    Session["STAFFNO"] = user.StaffNo;
                    //    Session["STAFFNAME"] = user.StaffName;

                    //    TempData["LoginSuccess"] = "Login successful! Welcome back.";
                    //    return RedirectToAction("Dashboard", "Dashboard");



                    //}
                    #endregion
                    if (user.Status == "SUCCESS")
                    {
                       
                        //Session.Clear();
                        //Session.Abandon();

                       
                        System.Web.HttpContext.Current.Session.Clear();

                       
                        var sessionToken = Guid.NewGuid().ToString();
                        Session["SessionToken"] = sessionToken;

                        HttpCookie cookie = new HttpCookie("SessionToken", sessionToken)
                        {
                            HttpOnly = true,
                            Secure = Request.IsSecureConnection,
                            SameSite = SameSiteMode.Strict
                        };
                        Response.Cookies.Add(cookie);
                        var cookieToken = Request.Cookies["SessionToken"]?.Value;
                        System.Diagnostics.Debug.WriteLine("SessionToken: " + cookieToken);
                        //if (!string.IsNullOrEmpty(user.SessionToken))
                        //{
                        //    if (user.LastActivityTime.HasValue)
                        //    {
                        //        double minutesInactive = (DateTime.Now - user.LastActivityTime.Value).TotalMinutes;

                        //        if (minutesInactive < 30)
                        //        {

                        //            ModelState.AddModelError("", "This user is already logged in from another device. Try after 30 minutes or contact admin.");
                        //            TempData["AlreadyLogin"] = "This user is already logged in from another device. Try after 30 minutes or contact admin..";
                        //            return View(loginModel);
                        //        }
                        //        else
                        //        {

                        //            helper.ClearSessionToken(user.Username);
                        //        }
                        //    }
                        //    else
                        //    {

                        //        helper.ClearSessionToken(user.Username);
                        //    }
                        //}
                        Session["USERNAME"] = user.Username;
                        Session["STAFFNO"] = user.StaffNo;
                        Session["STAFFNAME"] = user.StaffName;
                        Session["ROLE"] = RoleHelper.GetRoleName(user.RoleID);
                        TempData["LoginSuccess"] = "Login successful! Welcome back.";
                        return RedirectToAction("Dashboard", "Dashboard");
                    }

                }
                else
                {
                    TempData["LoginSuccess"] = "Please correct the errors and try again.";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during login. Please try again later.");
                TempData["LoginSuccess"] = "System error occurred";
            }

            return View(loginModel);
        }

        //[HttpGet]
        //public ActionResult Logout()
        //{
        //    try
        //    {
        //        var username = Session["USERNAME"].ToString();

        //        helper.ClearSessionToken(username);

        //        Session.Clear();
        //        if (Request.Cookies["SessionToken"] != null)
        //        {
        //            var cookie = new HttpCookie("SessionToken")
        //            {
        //                Expires = DateTime.Now.AddDays(-1),
        //                HttpOnly = true
        //            };
        //            Response.Cookies.Add(cookie);
        //        }
        //        HttpContext.Session.Clear();

        //        TempData["Logoutsuccessfully"] = "You have been successfully logged out.";


        //        return RedirectToAction("Login");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Logoutsuccessfully"] = "An error occurred during logout.";
        //        TempData["Messageresult"] = "error";
        //        return RedirectToAction("Login");
        //    }
        //}

        [HttpGet]
        public ActionResult LogoutMainCode()
        {
            try
            {
               
                var username = Session["USERNAME"] as string;
                if (!string.IsNullOrEmpty(username))
                {
                    helper.ClearSessionToken(username);
                }         
                Session.Clear();
                HttpContext.Session.Clear();
                Session.Abandon();           
                if (Request.Cookies["SessionToken"] != null)
                {
                    var cookie = new HttpCookie("SessionToken")
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        HttpOnly = true,
                        Secure = true, 
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Add(cookie);
                }
                FormsAuthentication.SignOut();

                TempData["Logoutsuccessfully"] = "You have been successfully logged out.";
                TempData["Messageresult"] = "success";
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                TempData["Logoutsuccessfully"] = "An error occurred during logout.";
                TempData["Messageresult"] = "error";
                return RedirectToAction("Login", "User");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                var username = Session["USERNAME"] as string;
                if (!string.IsNullOrEmpty(username))
                {

                }
                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();

                if (Request.Cookies["SessionToken"] != null)
                {
                    var cookie = new HttpCookie("SessionToken")
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Add(cookie);
                }

                TempData["Logoutsuccessfully"] = "You have been successfully logged out.";
                TempData["Messageresult"] = "success";

                return View();
            }
            catch (Exception ex)
            {
                TempData["Logoutsuccessfully"] = "An error occurred during logout." + ex.Message;
                TempData["Messageresult"] = "error";

                return View();
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Role == "Admin")
                {
                    model.Role = "2";
                }
                else
                {
                    model.Role = "1";
                }
                try
                {
                    if (helper.IsUsernameExists(model.USERNAME))
                    {
                        ViewBag.Message = "Username already exists. Please choose a different username.";
                        ViewBag.MessageType = "error";
                        return View(model);
                    }
                    if (helper.IsStaffNoExists(model.StaffNo))
                    {
                        ViewBag.Message = "Staff number already registered.";
                        ViewBag.MessageType = "error";
                        return View(model);
                    }
                    string encryptedPassword = CryptoHelper.Encrypt(model.PSW);
                    string hashedPassword = PasswordHasher.Hash(model.PSW);
                    if (helper.RegisterUser(model, encryptedPassword, hashedPassword))
                    {
                        ViewBag.Message = "Registration successful! You can now login.";
                        ViewBag.MessageType = "success";
                        ModelState.Clear();
                        return View();
                    }
                    else
                    {
                        ViewBag.Message = "Registration failed. Please try again.";
                        ViewBag.MessageType = "error";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "An error occurred during registration: " + ex.Message;
                    ViewBag.MessageType = "error";
                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
           
            var model = new ChangePasswordModel();
            if (User.Identity.IsAuthenticated)
            {
                model.Username = User.Identity.Name;
            }
            return View(model);
        }

        [HttpPost]
       
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
               
                int result = helper.ChangeUserPassword(model.Username, model.OldPassword, model.NewPassword);

                if (result == 1)
                {
                    TempData["SuccessMessage"] = "Password changed successfully! Please login with your new password.";
                    return View(model);
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("OldPassword", "Current password is incorrect");
                }
                else if (result == -1)
                {
                    ModelState.AddModelError("Username", "Username not found");
                }
                else
                {
                    ModelState.AddModelError("", "Error changing password");
                }
            }

          
            return View(model);
        }

        public ActionResult Error()
        {
            var statusCode = Response.StatusCode;
            var errorMessage = "An unexpected error occurred";

            switch (statusCode)
            {
                case 404:
                    errorMessage = "The page you're looking for doesn't exist.";
                    break;
                case 500:
                    errorMessage = "Internal server error occurred.";
                    break;
                case 403:
                    errorMessage = "You don't have permission to access this resource.";
                    break;
                case 401:
                    errorMessage = "Please log in to access this page.";
                    break;
            }

            ViewBag.StatusCode = statusCode;
            ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        public ActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }

        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }


       
    }
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public static string Hash(string password)
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    var key = pbkdf2.GetBytes(KeySize);
                    return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
                }
            }
        }

        public static bool Verify(string password, string hash)
        {
            var parts = hash.Split('.');
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, iterations))
            {
                var keyToCheck = pbkdf2.GetBytes(key.Length);
                return CryptographicEquals(key, keyToCheck);
            }
        }

        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}