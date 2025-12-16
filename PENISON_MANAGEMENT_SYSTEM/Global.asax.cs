using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PENISON_MANAGEMENT_SYSTEM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Web.Mvc.MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
        }
       

        //protected void Application_AcquireRequestState(object sender, EventArgs e)
        //{
        //    var routeData = HttpContext.Current.Request.RequestContext.RouteData;
        //    var controller = (routeData.Values["controller"] ?? "").ToString().ToLower();
        //    var action = (routeData.Values["action"] ?? "").ToString().ToLower();


        //    if (controller == "User" && (action == "Login" || action == "Logout"))
        //    {
        //        return;
        //    }

        //    var sessionToken = Session["SessionToken"]?.ToString();
        //    var cookieToken = Request.Cookies["SessionToken"]?.Value;

        //    if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(cookieToken) || sessionToken != cookieToken)
        //    {

        //        Response.Redirect("~/User/Login");
        //    }
        //}



        //protected void Application_EndRequest(object sender, EventArgs e)
        //{

        //    for (int i = 0; i < Response.Cookies.Count; i++)
        //    {
        //        HttpCookie cookie = Response.Cookies[i];
        //        if (cookie != null)
        //        {

        //            string cookieHeader = Response.Headers["Set-Cookie"];
        //            if (!string.IsNullOrEmpty(cookieHeader) && !cookieHeader.Contains("SameSite"))
        //            {
        //                Response.Headers["Set-Cookie"] =
        //                  cookieHeader.Replace("HttpOnly", "HttpOnly; SameSite=Strict; Secure");
        //            }
        //        }
        //    }
        //}
    }

    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var userRole = httpContext.Session["Role"]?.ToString();
            return !string.IsNullOrEmpty(userRole) && userRole == "Admin";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/User/Login");
        }
    }
}
