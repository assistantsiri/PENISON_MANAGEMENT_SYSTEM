using OfficeOpenXml;
using OfficeOpenXml.Style;
using PENISON_MANAGEMENT_SYSTEM.DA;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class ReportController : Controller
    {
        ImageReportDA imageRportDA = new ImageReportDA();
        ENQUERYDA ENQUERYDA = new ENQUERYDA();
        //private readonly PensionDataAccess _dataAccess;


        //public ReportController(PensionDataAccess dataAccess)
        //{
        //    _dataAccess = dataAccess;
        //}

     
        public ActionResult ALLREPORTS()
        {
            ViewBag.AccountTypeList = GetAccountTypeList();
            return View();
        }
    
        public ActionResult DailyReport()
        {
            ViewBag.AccountTypeList = GetAccountTypeList();
            return View(new ReportFilterViewModel());
        }
        [HttpPost]
        public ActionResult DailyReport(ReportFilterViewModel filter)
        {
            ViewBag.AccountTypeList = GetAccountTypeList(filter.AccountType);
            ViewBag.FromDate = filter.FromDate;
            ViewBag.ToDate = filter.ToDate;
            ViewBag.AccountType = filter.AccountType;

            var data = imageRportDA.GetDailyReport(filter.FromDate, filter.ToDate, filter.AccountType);
            return View("ReportResult", data);
        }

    
        public ActionResult DOWNLOADDAILYREPORT(DateTime fromDate, DateTime toDate, string accountType)
        {
            var data = imageRportDA.GetDailyReport(fromDate, toDate, accountType);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.AccountType = accountType;

            return new Rotativa.ViewAsPdf("DAILYPDFDOWNLAODREPORT", data)
            {
                FileName = $"DailyUploadReport_{fromDate:ddMMyyyy}_to_{toDate:ddMMyyyy}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }


        public ActionResult DAILYPDFDOWNLAODREPORT()
        {
            return View();
        }
    
        public ActionResult GenerateAllReports()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GenerateAllReports(ReportFilterViewModel filter)
        {
           // ViewBag.AccountTypeList = GetAccountTypeList(filter.AccountType);
            ViewBag.FromDate = filter.FromDate;
            ViewBag.ToDate = filter.ToDate;
           // ViewBag.AccountType = filter.AccountType;

            var data = imageRportDA.GetAllAccounttypeReports(filter.FromDate, filter.ToDate);
            return View("MonthlyReportResult", data);
        }

        public ActionResult DOWNLOADALLREPORT(DateTime fromDate, DateTime toDate)
        {
            var data = imageRportDA.GetAllAccounttypeReports(fromDate, toDate);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            

            return new Rotativa.ViewAsPdf("ALLACCOUNTTYPEPDFDOWNLAODREPORT", data)
            {
                FileName = $"DailyUploadReport_{fromDate:ddMMyyyy}_to_{toDate:ddMMyyyy}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }


        public ActionResult ALLACCOUNTTYPEPDFDOWNLAODREPORT()
        {
            return View();
        }

        #region
        /*
        public ActionResult MonthlyReport()
        {
            ViewBag.AccountTypeList = GetAccountTypeList();
            return View(new ReportFilterViewModel());
        }
        [HttpPost]
        public ActionResult MonthlyReport(ReportFilterViewModel filter)
        {
            var data = imageRportDA.GetMonthlyReport(filter.Month.Value, filter.Year.Value, filter.AccountType);
            return View("MonthlyReportResult", data);
        }
        public ActionResult DOWNLOADMONTHLYREPORT(DateTime date, string accountType)
        {
            var data = imageRportDA.GetDailyReport(date, accountType);
            ViewBag.Date = date;
            ViewBag.AccountType = accountType;

            return new Rotativa.ViewAsPdf("MONTHLYPDFDOWNLAODREPORT", data)
            {
                FileName = "MonthlyUploadReport_" + date.ToString("ddMMyyyy") + ".pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }

        public ActionResult MONTHLYPDFDOWNLAODREPORT()
        {
            return View();
        }


        public ActionResult YearlyReport()
        {
            ViewBag.AccountTypeList = GetAccountTypeList();
            return View(new ReportFilterViewModel());
        }

        [HttpPost]
        public ActionResult YearlyReport(ReportFilterViewModel filter)
        {
            var data = imageRportDA.GetYearlyReport(filter.Year.Value, filter.AccountType);
            return View("YearlyReportResult", data);
        }

        public ActionResult DOWNLOADYEARLYREPORT(DateTime date, string accountType)
        {
            var data = imageRportDA.GetDailyReport(date, accountType);
            ViewBag.Date = date;
            ViewBag.AccountType = accountType;

            return new Rotativa.ViewAsPdf("YEARLYPDFDOWNLAODREPORT", data)
            {
                FileName = "YearlyUploadReport_" + date.ToString("ddMMyyyy") + ".pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }

        public ActionResult YEARLYPDFDOWNLAODREPORT()
        {
            return View();
        }
        */
        #endregion
        private SelectList GetAccountTypeList(string selectedValue = null)
        {
            return new SelectList(new[]
            {
                new { Text = "-- Select Type --", Value = "" },
                new { Text = "Railways", Value = "R" },
                new { Text = "Other State's", Value = "S" },
                new { Text = "Telecom", Value = "T" },
                new { Text = "Civil", Value = "C" },
                new { Text = "Postal", Value = "P" },
                new { Text = "karnataka State", Value = "K" }
            }, "Value", "Text", selectedValue);
        }


        [HttpGet]
        public ActionResult ENQUERY()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ENQUERY(string message)
        {
            try
            {
                
                System.Diagnostics.Debug.WriteLine($"Received message: {message ?? "NULL"}");

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { error = "Please enter a search term" });
                }

                var result = ENQUERYDA.ENQUERY(message.Trim());

               
                System.Diagnostics.Debug.WriteLine($"Returning {result?.Count ?? 0} records");

                return Json(new
                {
                    success = true,
                    data = result,
                    count = result?.Count ?? 0
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex}");
                return Json(new
                {
                    success = false,
                    error = "An error occurred while processing your request",
                    details = ex.Message
                });
            }
        }
        public ActionResult INDIVIDUALREPORTINEXCEL()
        {
            ViewBag.AccountTypeList = GetAccountTypeList();
            return View(new ReportFilterViewModel());
        }

        [HttpPost]
        public ActionResult INDIVIDUALREPORTINEXCEL(ReportFilterViewModel filter, string exportType = "")
        {
            ViewBag.AccountTypeList = GetAccountTypeList(filter.AccountType);
            ViewBag.FromDate = filter.FromDate;
            ViewBag.ToDate = filter.ToDate;
            ViewBag.AccountType = filter.AccountType;

            exportType = "excel";
            var data = imageRportDA.GetDailyReport(filter.FromDate, filter.ToDate, filter.AccountType);

            if (!string.IsNullOrEmpty(exportType))
            {
                if (exportType.ToLower() == "excel")
                {
                    return ExportToExcel(data);
                }
               
            }

            return View("ReportResult", data);
        }
 
        private ActionResult ExportToExcel(object reportData)
        {
            try
            {
                if (reportData is IEnumerable<PMSImageUpload> pmsData)
                {
                    return ExportPMSDataToExcel(pmsData);
                }
                else if (reportData is DataTable dt)
                {
                    return ExportDataTableToExcel(dt);
                }
                else if (reportData is IEnumerable<dynamic> dynamicData)
                {
                    return ExportDynamicDataToExcel(dynamicData);
                }

                return Content("Unsupported data format");
            }
            catch (Exception ex)
            {
                return Content($"Export failed: {ex.Message}");
            }
        }
  
        private ActionResult ExportPMSDataToExcel(IEnumerable<PMSImageUpload> data)
        {
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("PensionReport");

                
                ws.Cells[1, 1].Value = "Account Number";
                ws.Cells[1, 2].Value = "Account Type";
                ws.Cells[1, 3].Value = "Upload Date";
                ws.Cells[1, 4].Value = "Uploaded By";
                ws.Cells[1, 5].Value = "File Size";

                
                int row = 2;
                foreach (var item in data)
                {
                    ws.Cells[row, 1].Value = item.Accountnumber;
                    ws.Cells[row, 2].Value = item.ACCOUNTTYPE;
                    ws.Cells[row, 3].Value = item.UPLOADEDDATE.ToString("yyyy-MM-dd");
                    ws.Cells[row, 4].Value = item.UPLOADEDBY;
                    ws.Cells[row, 5].Value = item.FILESIZE;
                    row++;
                }

                FormatWorksheet(ws, 5);
                return CreateExcelResult(pkg, "PensionReport");
            }
        }

        private ActionResult ExportDataTableToExcel(DataTable dt)
        {
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("DataReport");

               
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = dt.Columns[i].ColumnName;
                }

               
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ws.Cells[i + 2, j + 1].Value = dt.Rows[i][j];
                    }
                }

                FormatWorksheet(ws, dt.Columns.Count);
                return CreateExcelResult(pkg, "DataReport");
            }
        }

        private ActionResult ExportDynamicDataToExcel(IEnumerable<dynamic> data)
        {
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("DynamicReport");
                var firstItem = data.FirstOrDefault() as IDictionary<string, object>;

                if (firstItem == null) return Content("No data to export");

                var props = firstItem.Keys.ToList();

                // Headers
                for (int i = 0; i < props.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = props[i];
                }

                // Data
                int row = 2;
                foreach (var item in data)
                {
                    var dict = item as IDictionary<string, object>;
                    for (int col = 0; col < props.Count; col++)
                    {
                        ws.Cells[row, col + 1].Value = dict[props[col]]?.ToString();
                    }
                    row++;
                }

                FormatWorksheet(ws, props.Count);
                return CreateExcelResult(pkg, "DynamicReport");
            }
        }

        private void FormatWorksheet(ExcelWorksheet ws, int cols)
        {
            // Header style
            using (var range = ws.Cells[1, 1, 1, cols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Auto-fit and freeze pane
            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            ws.View.FreezePanes(2, 1);
        }

        private ActionResult CreateExcelResult(ExcelPackage pkg, string reportName)
        {
            var stream = new MemoryStream(pkg.GetAsByteArray());
            return File(stream,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       $"{reportName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }


        public ActionResult NOTEANDDOCUMENTUPLOADINPMS()
        {
            return View();
        }

        [HttpPost]
        public JsonResult NOTEANDDOCUMENTUPLOAD()
        {
            try
            {
                var files = Request.Files.GetMultiple("files");
                var fileDataJson = Request.Form["fileData"];

                if (files == null || files.Count == 0 || string.IsNullOrEmpty(fileDataJson))
                {
                    return Json(new { success = false, message = "No files or file data received" });
                }

                var fileInfoList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FileInfoModel>>(fileDataJson);

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var info = fileInfoList[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        string uploadPath = Server.MapPath("~/UPLOADEDNOTEANDDOCUMENT");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }
                        string financialYear = GetFinancialYear(DateTime.Now);
                        string financialYearPath = Path.Combine(uploadPath, financialYear);
                        if (!Directory.Exists(financialYearPath))
                        {
                            Directory.CreateDirectory(financialYearPath);
                        }
                        string accountTypePath = Path.Combine(financialYearPath, info.accountType);
                        if (!Directory.Exists(accountTypePath))
                        {
                            Directory.CreateDirectory(accountTypePath);
                        }
                        string customNamePath = Path.Combine(accountTypePath, info.customName);
                        if (!Directory.Exists(customNamePath))
                        {
                            Directory.CreateDirectory(customNamePath);
                        }

                        string currentDate = DateTime.Now.ToString("ddMMyyyy");
                        string fileExtension = Path.GetExtension(file.FileName);
                        string finalFileName = $"{info.customName}_{currentDate}{fileExtension}";

                        string filePath = Path.Combine(customNamePath, finalFileName);
                        file.SaveAs(filePath);
                    }
                }

                return Json(new { success = true, message = $"{files.Count} files uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error uploading files: " + ex.Message });
            }
        }

        private string GetFinancialYear(DateTime date)
        {
            int year = date.Month >= 4 ? date.Year : date.Year - 1;
            return $"{year}-{(year + 1).ToString().Substring(2)}";
        }



        //public ActionResult PensionuploadoldandnewFile()
        //{
        //    return View();
        //}

        //[HttpGet]
        //public JsonResult GetFiles(int year, string section)
        //{
        //    try
        //    {
        //        var files = ENQUERYDA.GetFiles(year, null, null, section);
        //        return Json(new ApiResponse { Success = true, Data = files });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new ApiResponse { Success = false, Message = ex.Message });
        //    }
        //}

        //[HttpPost]
        //public async Task<JsonResult> Upload(FileUploadModel model)
        //{
        //    try
        //    {
        //        if (model.File == null || model.File.ContentLength == 0)
        //        {
        //            return Json(new ApiResponse { Success = false, Message = "No file selected" });
        //        }


        //        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        //        var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

        //        if (!allowedExtensions.Contains(fileExtension))
        //        {
        //            return Json(new ApiResponse { Success = false, Message = "Invalid file type. Only PDF, Word, and Excel files are allowed." });
        //        }


        //        if (model.File.ContentLength > 10 * 1024 * 1024)
        //        {
        //            return Json(new ApiResponse { Success = false, Message = "File size exceeds 10MB limit." });
        //        }


        //        //var uploadsPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", model.Year.ToString(), model.Section);
        //        var uploadsPath = Server.MapPath($"~/uploads/{model.Year}/{model.Section}");

        //        if (!Directory.Exists(uploadsPath))
        //        {
        //            Directory.CreateDirectory(uploadsPath);
        //        }


        //        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
        //        var filePath = Path.Combine(uploadsPath, fileName);


        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await model.File.InputStream.CopyToAsync(stream);
        //        }


        //        var pensionFile = new PensionFile
        //        {
        //            FileName = model.File.FileName,
        //            FilePath = filePath,
        //            FileSize = model.File.ContentLength,
        //            Year = model.Year,
        //            Month = model.Month,
        //            CategoryId = model.CategoryId,
        //            Section = model.Section,
        //            UploadedBy = User.Identity.Name
        //        };

        //        var fileId = ENQUERYDA.InsertFile(pensionFile);

        //        return Json(new ApiResponse
        //        {
        //            Success = true,
        //            Message = "File uploaded successfully",
        //            Data = new
        //            {
        //                FileId = fileId,
        //                Path = $"/uploads/{model.Year}/{model.Section}/{fileName}"
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new ApiResponse { Success = false, Message = ex.Message });
        //    }
        //}

        //[HttpGet]
        //public FileResult Download(int id)
        //{
        //    try
        //    {
        //        var file = ENQUERYDA.GetFiles().FirstOrDefault(f => f.FileId == id);

        //        if (file == null || !System.IO.File.Exists(file.FilePath))
        //        {
        //            throw new FileNotFoundException("File not found");
        //        }

        //        var fileBytes = System.IO.File.ReadAllBytes(file.FilePath);
        //        return File(fileBytes, "application/octet-stream", file.FileName);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        //[HttpPost]
        //public JsonResult Delete(int id)
        //{
        //    try
        //    {
        //        var file = ENQUERYDA.GetFiles().FirstOrDefault(f => f.FileId == id);

        //        if (file == null)
        //        {
        //            return Json(new ApiResponse { Success = false, Message = "File not found" });
        //        }

        //        if (System.IO.File.Exists(file.FilePath))
        //        {
        //            System.IO.File.Delete(file.FilePath);
        //        }
        //        var success = ENQUERYDA.DeleteFile(id);

        //        return Json(new ApiResponse
        //        {
        //            Success = success,
        //            Message = success ? "File deleted successfully" : "Failed to delete file"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new ApiResponse { Success = false, Message = ex.Message });
        //    }
        //}

        //[HttpGet]
        //public JsonResult GetCategories()
        //{
        //    try
        //    {
        //        var categories = ENQUERYDA.GetAllCategories();
        //        return Json(new ApiResponse { Success = true, Data = categories });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new ApiResponse { Success = false, Message = ex.Message });
        //    }
        //}

        public ActionResult PensionuploadoldandnewFile()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetFiles(int year, string section, string month = null)
        {
            try
            {
                var files = ENQUERYDA.GetFiles(year, section, month);
                return Json(new { success = true, data = files });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Upload(FileUploadModel model)
        {
            try
            {
                if (model.File == null || model.File.ContentLength == 0)
                    return Json(new { success = false, message = "No file selected." });

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return Json(new { success = false, message = "Invalid file type." });

                if (model.File.ContentLength > 10 * 1024 * 1024)
                    return Json(new { success = false, message = "File size exceeds 10MB." });

                //var uploadsPath = Path.Combine(HostingEnvironment.WebRootPath, "uploads", model.Year.ToString(), model.Section, model.Category);
                var uploadsPath = "";
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    await model.File.CopyToAsync(stream);
                //}

                var pensionFile = new PensionFile
                {
                    FileName = model.File.FileName,
                    FilePath = filePath,
                    FileSize = model.File.ContentLength,
                    Year = model.Year,
                    Month = model.Month,
                    Section = model.Section,
                    UploadedBy = User.Identity.Name
                };

                var fileId = ENQUERYDA.InsertFile(pensionFile);

                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    data = new
                    {
                        FileId = fileId,
                        OriginalName = model.File.FileName,
                        FileSize = model.File.ContentLength,
                        UploadDate = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var files = ENQUERYDA.GetFiles(0, "");  // Fetch all to find the file by ID
                var file = files.FirstOrDefault(f => f.FileId == id);

                if (file == null)
                    return Json(new { success = false, message = "File not found." });

                if (System.IO.File.Exists(file.FilePath))
                    System.IO.File.Delete(file.FilePath);

                var success = ENQUERYDA.DeleteFile(id);

                return Json(new { success, message = success ? "Deleted successfully." : "Deletion failed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public FileResult Download(int id)
        {
            var files = ENQUERYDA.GetFiles(0, ""); // Fetch all to find the file by ID
            var file = files.FirstOrDefault(f => f.FileId == id);

            if (file == null || !System.IO.File.Exists(file.FilePath))
                throw new FileNotFoundException("File not found.");

            var fileBytes = System.IO.File.ReadAllBytes(file.FilePath);
            return File(fileBytes, "application/octet-stream", file.FileName);
        }


    }

    public static class HttpFileCollectionExtensions
    {
        public static List<HttpPostedFileBase> GetMultiple(this HttpFileCollectionBase files, string name)
        {
            var result = new List<HttpPostedFileBase>();
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file != null && file.ContentLength > 0 && file.FileName == name)
                {
                    result.Add(file);
                }
            }
            return result;
        }

    }
    public class FileInfoModel
    {
        public string originalName { get; set; }
        public string customName { get; set; }
        public string accountType { get; set; }
        public long size { get; set; }
    }

}