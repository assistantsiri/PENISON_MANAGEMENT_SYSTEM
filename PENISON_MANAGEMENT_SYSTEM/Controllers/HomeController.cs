
using PENISON_MANAGEMENT_SYSTEM.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class HomeController : Controller
    {
        private string UploadFolder => Server.MapPath("~/PROJECTDEATILS/");
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
      
        public ActionResult PROJECTDEATILS()
        {
            // Get the PDF file path
            string pdfPath = Server.MapPath("~/PROJECTDEATILS/CENTRALIZED PENSION MODULE.pdf");

            // Check if file exists
            if (System.IO.File.Exists(pdfPath))
            {
                ViewBag.PdfPath = Url.Content("~/PROJECTDEATILS/CENTRALIZED PENSION MODULE.pdf");
            }
            else
            {
                ViewBag.Error = "PDF file not found";
            }

            return View();
        }

        public ActionResult ViewPDF()
        {
            string pdfPath = Server.MapPath("~/PROJECTDEATILS/CENTRALIZED PENSION MODULE.pdf");

            if (System.IO.File.Exists(pdfPath))
            {
                return File(pdfPath, "application/pdf");
            }

            return HttpNotFound();
        }



    }
}