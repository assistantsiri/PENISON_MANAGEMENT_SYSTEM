
using PENISON_MANAGEMENT_SYSTEM.DA;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class DashBoardController : Controller
    {
        ENQUERYDA enqueryda = new ENQUERYDA();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DashboardExiting()
        {
            ViewBag.RailwaysUploadCount = enqueryda.GetUploadedFileCount("R");
            ViewBag.CivilUploadCount = enqueryda.GetUploadedFileCount("C");
            ViewBag.TelecomUploadCount = enqueryda.GetUploadedFileCount("T");
            ViewBag.PostalUploadCount = enqueryda.GetUploadedFileCount("P");
            ViewBag.Karnataka_StateUploadCount = enqueryda.GetUploadedFileCount("K");
            ViewBag.Other_StatesUploadCount = enqueryda.GetUploadedFileCount("S");
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            List<PendingAccountStatus> chartData = enqueryda.GetPendingAccountStats();

            if (chartData == null || !chartData.Any())
            {
                ViewBag.ChartData = null;
            }
            else
            {
                ViewBag.ChartData = chartData;
            }
            var uploadedData = enqueryda.GetUploadedStatsByDate(DateTime.Today);
            ViewBag.UploadedChartData = uploadedData;
            TempData["Logoutsuccessfully"] = "Welcome To Pension Management System Dashboard....";
            TempData["Messageresult"] = "success";
            return View();
        }

        //[Authorize(Roles = "Admin,Staff")]
        public ActionResult Dashboard()
        {
            if (Session["USERNAME"] == null)
            {
                TempData["ErrorMessage"] = "You must log in first.";
                TempData["Messageresult"] = "error";
                return RedirectToAction("Login", "User");
            }

            var role = Session["ROLE"]?.ToString();

            if (role != "Admin" && role != "Staff")
            {
                Session.Clear();
                Session.Abandon();
                HttpContext.Session.Clear();
                TempData["ErrorMessage"] = "You are not authorized to access the dashboard.";
                TempData["Messageresult"] = "error";
                return RedirectToAction("Login", "User");
            }

            ViewBag.RailwaysUploadCount = enqueryda.GetUploadedFileCount("R");
            ViewBag.CivilUploadCount = enqueryda.GetUploadedFileCount("C");
            ViewBag.TelecomUploadCount = enqueryda.GetUploadedFileCount("T");
            ViewBag.PostalUploadCount = enqueryda.GetUploadedFileCount("P");
            ViewBag.Karnataka_StateUploadCount = enqueryda.GetUploadedFileCount("K");
            ViewBag.Other_StatesUploadCount = enqueryda.GetUploadedFileCount("S");
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");

            List<PendingAccountStatus> chartData = enqueryda.GetPendingAccountStats();
            ViewBag.ChartData = chartData?.Any() == true ? chartData : null;

            var uploadedData = enqueryda.GetUploadedStatsByDate(DateTime.Today);
            ViewBag.UploadedChartData = uploadedData;

            TempData["Logoutsuccessfully"] = "Welcome To Pension Management System Dashboard....";
            TempData["Messageresult"] = "success";

            return View();
        }


        public ActionResult SuperAdminDashboard()
        {
            if (Session["USERNAME"] == null)
            {
                TempData["ErrorMessage"] = "You must log in first.";
                TempData["Messageresult"] = "error";
                return RedirectToAction("Login", "User");
            }

            var role = Session["ROLE"]?.ToString();
            Session.Abandon();
            if (role != "Admin" && role != "Staff")
            {
                Session.Clear();
                Session.Abandon();
                HttpContext.Session.Clear();
                TempData["ErrorMessage"] = "You are not authorized to access the dashboard.";
                TempData["Messageresult"] = "error";
                return RedirectToAction("Login", "User");
            }
            Session.Clear();
            Session.Abandon();
            HttpContext.Session.Clear();
            ViewBag.RailwaysUploadCount = enqueryda.GetUploadedFileCount("R");
            ViewBag.CivilUploadCount = enqueryda.GetUploadedFileCount("C");
            ViewBag.TelecomUploadCount = enqueryda.GetUploadedFileCount("T");
            ViewBag.PostalUploadCount = enqueryda.GetUploadedFileCount("P");
            ViewBag.Karnataka_StateUploadCount = enqueryda.GetUploadedFileCount("K");
            ViewBag.Other_StatesUploadCount = enqueryda.GetUploadedFileCount("S");
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");

            List<PendingAccountStatus> chartData = enqueryda.GetPendingAccountStats();
            ViewBag.ChartData = chartData?.Any() == true ? chartData : null;

            var uploadedData = enqueryda.GetUploadedStatsByDate(DateTime.Today);
            ViewBag.UploadedChartData = uploadedData;

            TempData["Logoutsuccessfully"] = "Welcome To Pension Management System Dashboard....";
            TempData["Messageresult"] = "success";

            return View();
        }



        public ActionResult PendingAccountChart()
        {
            var chartData = enqueryda.GetPendingAccountStats();
            return View(chartData);
        }
    }
}