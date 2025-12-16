using OfficeOpenXml;

using PENISON_MANAGEMENT_SYSTEM.DA;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class EXCELFILEUPLOADController : Controller
    {
     
        PPAAccountDA da = new PPAAccountDA();
        public ActionResult Index()
        {
            return View();
        }
        /*@@@@@@@@@@@@@@@@@@@@@@@   PPA ACCOUNTS UPLOAD @@@@@@@@@@@@@ START @@@@@@@@@@*/
     
        [HttpGet]
        public ActionResult Upload()
        {
            var model = new List<EXCELFILEUPLOADMODEL>();
            //var model = da.GetAllPPADetails();

            return View(model);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase excelFile)
        {
            List<EXCELFILEUPLOADMODEL> ppaList = new List<EXCELFILEUPLOADMODEL>();

            if (excelFile == null || excelFile.ContentLength == 0)
            {
                TempData["Message"] = "Please select a valid Excel file.";
                return View(ppaList);
            }

            string staffNo = Session["staffno"]?.ToString();
            if (string.IsNullOrEmpty(staffNo))
            {
                TempData["Message"] = "User session expired.";
                return View(ppaList);
            }

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage(excelFile.InputStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text)) continue;

                        EXCELFILEUPLOADMODEL detail = new EXCELFILEUPLOADMODEL
                        {
                            PPA_ACNO = worksheet.Cells[row, 1].Text.Trim(),
                            PPA_PPONO = worksheet.Cells[row, 2].Text.Trim(),
                            ppA_name = worksheet.Cells[row, 3].Text.Trim(),
                            PPA_DPCD = worksheet.Cells[row, 4].Text.Trim(),
                            PPA_Type = worksheet.Cells[row, 5].Text.Trim(),
                            PPA_CREATED_BY = staffNo,
                            PPA_CREATED_DATE = DateTime.Now
                        };

                        ppaList.Add(detail);
                    }
                }

                
                da.InsertExcelData(ppaList); 

                TempData["Message"] = "✔ Data uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Error: " + ex.Message;
            }

            return View(ppaList); 
        }




        /*@@@@@@@@@@@@@@@@@@@@@@@   PPA ACCOUNTS UPLOAD @@@@@@@@@@@@@ END @@@@@@@@@@@@@@*/
    }
}