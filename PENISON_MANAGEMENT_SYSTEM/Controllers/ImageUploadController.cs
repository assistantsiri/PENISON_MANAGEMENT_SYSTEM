using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PENISON_MANAGEMENT_SYSTEM.DA;
using PENISON_MANAGEMENT_SYSTEM.ExistingImageProcessor;
using PENISON_MANAGEMENT_SYSTEM.Models;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class ImageUploadController : Controller
    {
        Helper helper = new Helper();
        PPAAccountDA ppaaccountDA = new PPAAccountDA();
        private const int ITEMS_PER_PAGE = 10;
        private string UploadFolder => Server.MapPath("~/UploadedPdfs");
        private readonly string ViewRailwaysImage = ConfigurationManager.AppSettings["folderpath1"];
        private readonly string ViewCivilImage = ConfigurationManager.AppSettings["folderpath2"];
        private readonly string ViewKarnatakaStateImage = ConfigurationManager.AppSettings["folderpath3"];
        private readonly string ViewOtherStateImage = ConfigurationManager.AppSettings["folderpath4"];
        private readonly string ViewPostalImage = ConfigurationManager.AppSettings["folderpath5"];
        private readonly string ViewTelecomeImage = ConfigurationManager.AppSettings["folderpath6"];
        public ActionResult Index()
        {
            ViewBag.UploadedFiles = helper.GetUploadedFiles();
            return View();

        }
        [HttpGet]
        public ActionResult UploadImage()
        {
            if (Session["USERNAME"] == null)
            {
                TempData["ErrorMessage"] = "Please login to access this page";
                return RedirectToAction("Login", "User");
            }
            return View();
        }
        [HttpPost]
        public ActionResult UploadImage(IEnumerable<HttpPostedFileBase> files, string action)
        {
            if (!string.IsNullOrEmpty(action) && action == "UploadImg")
            {
                if (files == null || !files.Any())
                {
                    TempData["Message"] = "No files selected for upload!";
                    return RedirectToAction("UploadImage");
                }

                string uploadFolderPath = Server.MapPath("~/UPLOADS");
                List<UploadedFileViewModel> uploadedFiles = new List<UploadedFileViewModel>();
                DateTime createdDate = DateTime.Now;

                List<(string dpcode, string accountNumber, string fileName)> dpcodeAccountInfos = new List<(string, string, string)>();

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string folderName = Path.GetDirectoryName(file.FileName);
                        string filePath = Path.Combine(uploadFolderPath, folderName ?? string.Empty, fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? uploadFolderPath);

                        file.SaveAs(filePath);

                        var dpcodeAccountInfo = ExtractDpcodeAndAccountNumber(fileName);
                        dpcodeAccountInfos.Add((dpcodeAccountInfo.dpcode, dpcodeAccountInfo.accountNumber, fileName));

                        uploadedFiles.Add(new UploadedFileViewModel
                        {
                            DPCode = dpcodeAccountInfo.dpcode,
                            AccountNumber = dpcodeAccountInfo.accountNumber,
                            FolderName = folderName,
                            FileName = fileName
                        });

                        helper.SaveFileRecord(dpcodeAccountInfo.dpcode, dpcodeAccountInfo.accountNumber);
                    }
                }

                foreach (var info in dpcodeAccountInfos)
                {
                    helper.InsertIntoUploadedFilePath(Convert.ToInt32(info.dpcode), info.fileName, createdDate);
                }

                ViewBag.UploadedFiles = uploadedFiles;
                TempData["Message"] = "Files uploaded successfully!";
                return View("UserUploadImage");
            }
            else if (action == "UploadImg2")
            {
                return View("UploadImg2");
            }

            return View();
        }

        [HttpGet]
        public ActionResult UserUploadImage()
        {

            ViewBag.AccountTypeList = GetAccountTypeList();
            return View(new Imageupload());
        }
        #region
        //[HttpPost]
        //public ActionResult UserUploadImage(Imageupload model)
        //{
        //    ViewBag.AccountTypeList = GetAccountTypeList(model.AccountType);

        //    if (model.UploadedFiles == null)
        //    {
        //        TempData["Message"] = "No files selected for upload!";
        //        return RedirectToAction("UserUploadImage");
        //    }

        //    string baseUploadPath = Server.MapPath("~/App_Data/MyUploads");

        //    if (!Directory.Exists(baseUploadPath))
        //    {
        //        Directory.CreateDirectory(baseUploadPath);
        //    }

        //    List<Imageupload> uploadedModels = new List<Imageupload>();
        //    DateTime createdDate = DateTime.Now;

        //    foreach (var file in model.UploadedFiles)
        //    {
        //        if (file != null && file.ContentLength > 0)
        //        {
        //            try
        //            {
        //                string fileName = Path.GetFileName(file.FileName);
        //                string userFolder = Path.GetDirectoryName(file.FileName) ?? string.Empty;

        //                string subFolderPath = Path.Combine(baseUploadPath, userFolder);
        //                Directory.CreateDirectory(subFolderPath);

        //                string fullFilePath = Path.Combine(subFolderPath, fileName);
        //                file.SaveAs(fullFilePath);

        //                var info = ExtractDpcodeAndAccountNumber(fileName);
        //                var SaffNO = Session["STAFFNO"];

        //                var uploadModel = new Imageupload
        //                {
        //                    DPCODE = Convert.ToInt32(info.dpcode),
        //                    Accountnumber = info.accountNumber,
        //                    PPANO = "123",
        //                    FILENAME = fileName,
        //                    UPLOADEDBY = SaffNO?.ToString(),
        //                    UPLOADEDDATE = createdDate,
        //                    Status = "Pending",
        //                    AccountType = model.AccountType,
        //                    FolderName = Path.Combine(baseUploadPath, userFolder)
        //                };

        //                helper.InsertOrUpdatePMSImageUploadinjpg(uploadModel, "INSERT");
        //                uploadedModels.Add(uploadModel);
        //            }
        //            catch (Exception ex)
        //            {
        //                TempData["Message"] = $"Error uploading file: {ex.Message}";
        //            }
        //        }
        //    }

        //    ViewBag.UploadedFiles = uploadedModels;
        //    TempData["Message"] = "Files uploaded successfully!";
        //    ViewBag.SelectedType = model.AccountType;
        //    return View("UserUploadImage", model);
        //}
        #endregion
        [HttpPost]
        public ActionResult UserUploadImage(Imageupload model)
        {
            ViewBag.AccountTypeList = GetAccountTypeList(model.AccountType);

            if (model.UploadedFiles == null || !model.UploadedFiles.Any())
            {
                TempData["Message"] = "No files selected for upload!";
                return RedirectToAction("UserUploadImage");
            }

            string baseUploadPath = Server.MapPath("~/UploadedPdfs");

            if (!Directory.Exists(baseUploadPath))
                Directory.CreateDirectory(baseUploadPath);

            List<Imageupload> uploadedModels = new List<Imageupload>();
            DateTime createdDate = DateTime.Now;
            string SaffNO = Session["STAFFNO"]?.ToString();
            int successCount = 0;

            foreach (var file in model.UploadedFiles)
            {
                if (file != null && file.ContentLength > 0)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string fileExtension = Path.GetExtension(fileName).ToLower();


                        var info = ExtractDpcodeAndAccountNumber(fileName);
                        // string accountNumber = info.accountNumber;
                        string beforeDot = fileName.Split('.')[0];
                        string numberPart = new string(beforeDot.Where(char.IsDigit).ToArray());
                        string accountNumber = numberPart;
                        int dpcode = Convert.ToInt32(info.dpcode);


                        string accountFolder = Path.Combine(baseUploadPath, accountNumber);
                        if (!Directory.Exists(accountFolder))
                            Directory.CreateDirectory(accountFolder);

                        string existingPdfPath = Path.Combine(accountFolder, "merged.pdf");
                        string newPdfPath = Path.Combine(accountFolder, Guid.NewGuid() + ".pdf");


                        if (fileExtension == ".pdf")
                        {
                            file.SaveAs(newPdfPath);
                        }
                        else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                        {
                            using (var fs = new FileStream(newPdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                using (Document doc = new Document(PageSize.A4, 25, 25, 30, 30))
                                {
                                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                                    doc.Open();


                                    using (var imageStream = new MemoryStream())
                                    {
                                        file.InputStream.CopyTo(imageStream);
                                        imageStream.Position = 0;

                                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageStream);
                                        image.ScaleToFit(doc.PageSize.Width - 40, doc.PageSize.Height - 40);
                                        image.Alignment = Element.ALIGN_CENTER;
                                        doc.Add(image);
                                    }

                                    doc.Close();
                                }
                            }
                        }

                        else
                        {
                            continue;
                        }


                        if (System.IO.File.Exists(existingPdfPath))
                        {
                            string mergedTemp = Path.Combine(accountFolder, "merged_temp.pdf");
                            MergePdfs(existingPdfPath, newPdfPath, mergedTemp);

                            System.IO.File.Delete(newPdfPath);
                            System.IO.File.Delete(existingPdfPath);
                            System.IO.File.Move(mergedTemp, existingPdfPath);
                        }
                        else
                        {
                            System.IO.File.Move(newPdfPath, existingPdfPath);
                        }


                        var uploadModel = new Imageupload
                        {
                            DPCODE = dpcode,
                            Accountnumber = accountNumber,
                            PPANO = "123",
                            FILENAME = fileName,
                            UPLOADEDBY = SaffNO,
                            UPLOADEDDATE = createdDate,
                            Status = "Pending",
                            AccountType = model.AccountType,
                            FolderName = accountFolder,
                            FILESIZE = file.ContentLength.ToString()
                        };

                        helper.InsertOrUpdatePMSImageUploadinjpg(uploadModel, "INSERT");
                        uploadedModels.Add(uploadModel);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = $"Error uploading file: {ex.Message}";
                    }
                }
            }

            ViewBag.UploadedFiles = uploadedModels;
            TempData["Message"] = $"{successCount} file(s) uploaded successfully!";
            ViewBag.SelectedType = model.AccountType;
            return View("UserUploadImage", model);
        }
        public ActionResult UserViewAllFolders()
        {
            ViewBag.RailwaysUploadCount = helper.GetUploadedFileCount("R");
            ViewBag.CivilUploadCount = helper.GetUploadedFileCount("C");
            ViewBag.TelecomUploadCount = helper.GetUploadedFileCount("T");
            ViewBag.PostalUploadCount = helper.GetUploadedFileCount("P");
            ViewBag.StateUploadCount = helper.GetUploadedFileCount("S");
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");

            List<Imageupload> folders = new List<Imageupload>();

            string uploadRoot = Server.MapPath("~/App_Data/MyUploads");

            if (Directory.Exists(uploadRoot))
            {
                foreach (var dir in Directory.GetDirectories(uploadRoot))
                {
                    string folderName = Path.GetFileName(dir);
                    var (dp, acc) = ExtractDpcodeAndAccountNumber(folderName);
                    var imageFiles = Directory.GetFiles(dir)
                                       .Where(file => new[] { ".jpg", ".jpeg", ".png", ".gif" }
                                       .Contains(Path.GetExtension(file).ToLower()))
                                       .Select(Path.GetFileName)
                                       .ToList();
                    string imageListString = string.Join(", ", imageFiles);
                    folders.Add(new Imageupload
                    {
                        FolderName = folderName,
                        DPCODE = Convert.ToInt32(dp),
                        Accountnumber = acc,
                        ImageNames = imageListString
                    });
                }
            }

            ViewBag.AllFolderList = folders;




            return View(folders);
        }
        [HttpPost]
        public ActionResult USERUPLOADIMAGES(string accountType)
        {
            var data = helper.GetUploadedImagesByAccountType(accountType);
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;

            return View("UserViewAllFolders", data);
        }
        [HttpGet]
        [Obsolete]
        public ActionResult GenerateFolderPdf(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Folder name is required");

            string uploadsRoot = Server.MapPath("~/App_Data/MyUploads");
            string folderPath = Path.Combine(uploadsRoot, folder);

            if (!Directory.Exists(folderPath))
                return HttpNotFound($"Folder '{folder}' not found.");

            var sb = new StringBuilder();
            sb.Append(@"<html><head><style>
                             body { font-family: Arial; margin: 20px; }
                            .img-grid { display: flex; flex-wrap: wrap; gap: 15px; justify-content:center; }
                             img { max-width: 220px; border: 1px solid #ccc; border-radius: 10px; padding: 4px; }
                             </style></head><body>");
            sb.Append($"<h2 style='text-align:center;'>📂 Images from Folder: {folder}</h2>");
            sb.Append("<div class='img-grid'>");

            var imageFiles = Directory.GetFiles(folderPath)
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

            foreach (var file in imageFiles)
            {
                var base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(file));
                string mimeType = Path.GetExtension(file).ToLower() == ".png" ? "image/png" : "image/jpeg";
                string imageName = Path.GetFileName(file);

                sb.Append("<div style='text-align:center;'>");
                sb.Append($"<img src='data:{mimeType};base64,{base64}' alt='{imageName}' style='width:6cm; height:4cm; object-fit:cover; border: 1px solid #ccc; border-radius: 10px; padding: 4px;' />");
                sb.Append($"<div style='margin-top:5px; font-size:14px; font-weight:bold; color:#333;'>{imageName}</div>");
                sb.Append("</div>");

            }

            sb.Append("</div></body></html>");


            var converter = new SelectPdf.HtmlToPdf();
            var pdfDoc = converter.ConvertHtmlString(sb.ToString());

            byte[] pdfBytes = pdfDoc.Save();
            pdfDoc.Close();

            Response.Headers["Content-Disposition"] = $"inline; filename=Images_{folder}.pdf";
            return File(pdfBytes, "application/pdf");
        }
        private (string dpcode, string accountNumber) ExtractDpcodeAndAccountNumber(string filename)
        {
            var withoutExtension = Path.GetFileNameWithoutExtension(filename);
            var parts = withoutExtension.Split('-');
            //if (parts.Length < 2)
            //{
            //    throw new InvalidOperationException("Filename format is incorrect.");
            //}

            var code = parts[0];
            var dpcode = string.Empty;
            var accountNumber = string.Empty;


            if (code.Length == 13)
            {
                dpcode = code.Substring(0, 4);
                accountNumber = code.Substring(4);
            }
            else if (code.Length > 13)
            {
                dpcode = code.Substring(0, 5);
                accountNumber = code.Substring(5);
            }
            else if (code.Length < 13)
            {
                dpcode = code.Substring(0, 4);
                accountNumber = code.Substring(4);
            }

            return (dpcode, accountNumber);
        }
        private (string dpcode, string accountNumber) EXTRACTDPCODEANDACCOUNTNUMBERVIEW(string folderName)
        {
            var code = folderName;
            var dpcode = string.Empty;
            var accountNumber = string.Empty;

            if (code.Length == 13)
            {
                dpcode = code.Substring(0, 4);
                accountNumber = code.Substring(4);
            }
            else if (code.Length > 13)
            {
                dpcode = code.Substring(0, 5);
                accountNumber = code.Substring(5);
            }
            else if (code.Length < 13)
            {
                dpcode = code.Substring(0, 4);
                accountNumber = code.Substring(4);
            }

            return (dpcode, accountNumber);
        }
        public List<PMSImageUpload> GetUploadedFolders()
        {
            var uploadsPath = Server.MapPath("~/UPLOADS");
            var list = new List<PMSImageUpload>();

            if (!Directory.Exists(uploadsPath))
                return list;

            var folders = Directory.GetDirectories(uploadsPath);

            foreach (var folder in folders)
            {
                var folderName = Path.GetFileName(folder);
                var (dp, acc) = ExtractDpcodeAndAccountNumber(folderName);

                list.Add(new PMSImageUpload
                {
                    FolderName = folderName,
                    DpCode = dp,
                    AccountNumber = acc,
                    FolderUrl = folderName
                });
            }

            return list;
        }
        [HttpGet]
        public JsonResult UPLOADIMAGESOFRAILWAYS(string accountType)
        {

            var data = helper.GetUploadedImagesByAccountType("R");
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;
            return Json(data);
        }
        [HttpGet]
        public JsonResult UPLOADIMAGESOFKARNATAKASTATE(string accountType)
        {

            var data = helper.GetUploadedImagesByAccountType("K");
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;
            return Json(data);
        }
        public JsonResult UPLOADIMAGESOFOTHERSTATES(string accountType)
        {

            var data = helper.GetUploadedImagesByAccountType("S");
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;
            return Json(data);
        }

        [HttpGet]
        public JsonResult UPLOADIMAGESOFCIVIL(string accountType)
        {

            var data = helper.GetUploadedImagesByAccountType("C");
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;
            return Json(data);
        }
        [HttpGet]
        public JsonResult UPLOADIMAGESOFTELECOM(string accountType)
        {

            var data = helper.GetUploadedImagesByAccountType("T");
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;
            return Json(data);
        }
        [HttpGet]
        public JsonResult UPLOADIMAGESOFPOSTAL(string accountType)
        {

            var data = helper.GetUploadedImagesByAccountType("P");
            ViewBag.RailwaysUploadCount = data.Count;
            ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM");
            ViewBag.SelectedType = accountType;
            return Json(data);
        }


        [HttpGet]
        public ActionResult UPLOADPDF()
        {
            //var username = Session["USERNAME"]?.ToString();
            //if (string.IsNullOrEmpty(username))
            //{
            //    ViewBag.Message = "You are not logged in, but you can still upload PDFs.";
            //    return RedirectToAction("Login", "User");
            //}
            var sessionValue = Convert.ToString(Session["STAFFNO"]);
            if (string.IsNullOrEmpty(sessionValue))
            {
                TempData["AlertType"] = "error";
                TempData["AlertMessage"] = "Please login first to access this page.";
                TempData["AlertDetails"] = "?";
                return RedirectToAction("Login", "User");
            }

            ViewBag.AccountTypeList = GetAccountTypeList();

            return View(new Imageupload());
        }

        [HttpPost]
        public ActionResult UPLOADPDFDBSAVED(IEnumerable<HttpPostedFileBase> pdfFiles, string AccountType)
        {
            var staffNo = Session["STAFFNO"]?.ToString();
            int successCount = 0;

            foreach (var pdfFile in pdfFiles)
            {
                if (pdfFile == null || pdfFile.ContentLength == 0) continue;

                string fileName = pdfFile.FileName;
                string accountNumber = Path.GetFileNameWithoutExtension(fileName).Split('-')[0];


                byte[] newPdfBytes;
                using (var ms = new MemoryStream())
                {
                    pdfFile.InputStream.CopyTo(ms);
                    newPdfBytes = ms.ToArray();
                }


                helper.InsertPdfVersion(accountNumber, fileName, newPdfBytes, staffNo, AccountType, "Pending", false);


                byte[] existingMergedPdf = helper.GetLatestMergedPdf(accountNumber);


                byte[] finalMergedPdf = existingMergedPdf != null
                    ? MergePdfBytes(existingMergedPdf, newPdfBytes)
                    : newPdfBytes;


                helper.InsertPdfVersion(accountNumber, "Merged_" + fileName, finalMergedPdf, staffNo, AccountType, "Pending", true);

                successCount++;
            }

            TempData["AlertType"] = "success";
            TempData["AlertMessage"] = $"{successCount} PDF(s) uploaded and merged successfully!";
            return RedirectToAction("UploadPdf");
        }

        public static byte[] MergePdfBytes(byte[] pdf1, byte[] pdf2)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document();
                PdfCopy copy = new PdfCopy(doc, ms);
                doc.Open();

                foreach (var pdfBytes in new[] { pdf1, pdf2 })
                {
                    using (var reader = new PdfReader(pdfBytes))
                    {
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            copy.AddPage(copy.GetImportedPage(reader, i));
                        }
                    }
                }

                doc.Close();
                return ms.ToArray();
            }
        }

        public ActionResult ViewPdf(int versionId)
        {
            versionId = 1;
            byte[] pdfBytes = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT PdfData FROM CPPCIMAGEUPLOAD_VERSIONS WHERE VersionID=@Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", versionId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        pdfBytes = (byte[])result;
                }
            }

            return File(pdfBytes, "application/pdf");
        }



        [HttpPost]
        public ActionResult UPLOADPDF(IEnumerable<HttpPostedFileBase> pdfFiles, string AccountType)
        {
            try
            {

                if (pdfFiles == null || !pdfFiles.Any())
                {
                    TempData["AlertType"] = "error";
                    TempData["AlertMessage"] = "No PDF files found in the selected folder!";
                    return RedirectToAction("UPLOADPDF");
                }

                if (string.IsNullOrEmpty(AccountType))
                {
                    TempData["AlertType"] = "error";
                    TempData["AlertMessage"] = "Please select an Account Type.";
                    return RedirectToAction("UPLOADPDF");
                }

                var SaffNO = Session["STAFFNO"]?.ToString();
                var date = DateTime.Now;
                var createdDate = date.ToString("dd-MM-yyyy");


                List<Imageupload> uploadedModels = new List<Imageupload>();
                int successCount = 0;

                foreach (var pdfFile in pdfFiles)
                {
                    if (pdfFile == null || pdfFile.ContentLength == 0)
                        continue;

                    //string accountNumber = Path.GetFileNameWithoutExtension(pdfFile.FileName);
                    string filenamewithoutextension = Path.GetFileNameWithoutExtension(pdfFile.FileName);
                    string accountNumber = filenamewithoutextension.Split('-')[0];
                    string fileName = pdfFile.FileName;
                    long fileSize = pdfFile.ContentLength;
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    if (string.IsNullOrWhiteSpace(accountNumber))
                    {
                        continue;
                    }


                    string accountFolder = Path.Combine(Server.MapPath("~/UploadedPdfs"), accountNumber);
                    if (!Directory.Exists(accountFolder))
                    {
                        Directory.CreateDirectory(accountFolder);
                    }


                    string existingPdfPath = Path.Combine(accountFolder, $"{accountNumber}.pdf");
                    string newPdfPath = Path.Combine(accountFolder, Guid.NewGuid() + ".pdf");

                    /*=============UPLOADED FILE TAKE AS THIER EXTENSION WISE=================*/

                    if (fileExtension == ".pdf")
                    {
                        pdfFile.SaveAs(newPdfPath);
                    }
                    else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                    {
                        using (var fs = new FileStream(newPdfPath, FileMode.Create))
                        {
                            Document doc = new Document(PageSize.A4);
                            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                            doc.Open();

                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(pdfFile.InputStream);
                            image.ScaleToFit(doc.PageSize.Width - 40, doc.PageSize.Height - 40);
                            image.Alignment = Element.ALIGN_CENTER;
                            doc.Add(image);
                            doc.Close();
                        }
                    }
                    else
                    {
                        continue;
                    }


                    pdfFile.SaveAs(newPdfPath);

                    if (System.IO.File.Exists(existingPdfPath))
                    {
                        string mergedPdfPath = Path.Combine(accountFolder, "merged_temp.pdf");
                        MergePdfs(existingPdfPath, newPdfPath, mergedPdfPath);

                        System.IO.File.Delete(newPdfPath);
                        System.IO.File.Delete(existingPdfPath);
                        System.IO.File.Move(mergedPdfPath, existingPdfPath);
                    }
                    else
                    {
                        System.IO.File.Move(newPdfPath, existingPdfPath);
                    }


                    var uploadModel = new Imageupload
                    {
                        Accountnumber = accountNumber,
                        FILENAME = fileName,
                        FILESIZE = Convert.ToInt64(fileSize).ToString(),
                        UPLOADEDBY = SaffNO,
                        UPLOADEDDATE = Convert.ToDateTime(date),
                        Status = "Pending",
                        AccountType = AccountType,
                        FolderName = accountFolder
                    };

                    helper.InsertOrUpdatePMSImageUpload(uploadModel, "INSERT");
                    uploadedModels.Add(uploadModel);
                    successCount++;
                }

                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = $"{successCount} PDF(s) uploaded successfully!";
                TempData["AlertDetails"] = $"{pdfFiles.Count()} file(s) processed";

                return RedirectToAction("UPLOADPDF");
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertMessage"] = "PDF upload failed!";
                TempData["AlertDetails"] = ex.Message;
                return RedirectToAction("UPLOADPDF");
            }
        }
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }
        private SelectList GetAccountTypeList(string selectedValue = null)
        {
            return new SelectList(new[]
            {
        new { Text = "-- SELECT ACCOUNT TYPE --", Value = "" },
        new { Text = "Railways", Value = "R" },
        new { Text = "Karnataka State", Value = "K" },
        new { Text = "Telecom", Value = "T" },
        new { Text = "Civil", Value = "C" },
        new { Text = "Postal", Value = "P" },
         new { Text = "Defence", Value = "D" },
        new { Text = "Other State", Value = "S" }
            }, "Value", "Text", selectedValue);
        }
        private void MergePdfs(string existingPdf, string newPdf, string outputPdf)
        {
            using (var stream = new FileStream(outputPdf, FileMode.Create))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                document.Open();

                if (System.IO.File.Exists(existingPdf))
                    AddPagesFromPdf(existingPdf, pdf);

                AddPagesFromPdf(newPdf, pdf);

                document.Close();
            }
        }
        private void AddPagesFromPdf(string pdfPath, PdfCopy pdf)
        {
            using (PdfReader reader = new PdfReader(pdfPath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfImportedPage page = pdf.GetImportedPage(reader, i);
                    pdf.AddPage(page);
                }
            }
        }
        public ActionResult UploadedListDBVIEW()
        {
            var dbImages = helper.GetUploadedImages();

            var model = dbImages
                .GroupBy(i => i.Accountnumber)
                .Select(g => new Imageupload
                {
                    Accountnumber = g.Key,
                    LatestUploadDate = g.Max(x => x.UPLOADEDDATE),
                    UploadedBy = g.OrderByDescending(x => x.UPLOADEDDATE).First().UPLOADEDBY,
                    AccountType = g.OrderByDescending(x => x.UPLOADEDDATE).First().AccountType
                    //Uploads = g.OrderByDescending(x => x.UPLOADEDDATE).ToList()
                })
                .OrderByDescending(m => m.LatestUploadDate)
                .ToList();

            return View(model);
        }
        public ActionResult UploadedListTest()
        {
            var dbImages = helper.GetUploadedImages();
            var model = new List<Imageupload>();

            if (Directory.Exists(UploadFolder))
            {
                foreach (var dir in Directory.GetDirectories(UploadFolder))
                {
                    var accountNumber = Path.GetFileName(dir);
                    var mergedPdfPath = Path.Combine(dir, $"{accountNumber}.pdf");

                    if (System.IO.File.Exists(mergedPdfPath))
                    {
                        var dbImage = dbImages.FirstOrDefault(i => i.Accountnumber == accountNumber);
                        var fileInfo = new FileInfo(mergedPdfPath);

                        if (dbImage != null)
                        {

                            model.Add(new Imageupload
                            {
                                Accountnumber = dbImage.Accountnumber,
                                UPLOADEDDATE = dbImage.UPLOADEDDATE,
                                UPLOADEDBY = dbImage.UPLOADEDBY,
                                AccountType = dbImage.AccountType,
                                FILESIZE = dbImage.FILESIZE
                            });
                        }
                        else
                        {

                            model.Add(new Imageupload
                            {
                                Accountnumber = accountNumber,
                                UPLOADEDDATE = fileInfo.LastWriteTime,
                                FILESIZE = $"{fileInfo.Length / 1024} KB"
                            });
                        }
                    }
                }
            }


            var remainingDbImages = dbImages.Where(db => !model.Any(m => m.Accountnumber == db.Accountnumber));
            foreach (var dbImage in remainingDbImages)
            {
                model.Add(new Imageupload
                {
                    Accountnumber = dbImage.Accountnumber,
                    UPLOADEDDATE = dbImage.UPLOADEDDATE,
                    UPLOADEDBY = dbImage.UPLOADEDBY,
                    AccountType = dbImage.AccountType,
                    FILESIZE = dbImage.FILESIZE
                });
            }


            model = model.OrderByDescending(m => m.UPLOADEDDATE).ToList();

            return View(model);
        }

        public ActionResult UploadedList()
        {
            var model = new List<Imageupload>();

            if (Directory.Exists(UploadFolder))
            {
                foreach (var dir in Directory.GetDirectories(UploadFolder))
                {
                    var accountNumber = Path.GetFileName(dir);
                    var mergedPdfPath = Path.Combine(dir, $"{accountNumber}.pdf");

                    if (System.IO.File.Exists(mergedPdfPath))
                    {
                        var fileInfo = new FileInfo(mergedPdfPath);

                        model.Add(new Imageupload
                        {
                            Accountnumber = accountNumber,
                            UPLOADEDDATE = fileInfo.LastWriteTime,
                            FILESIZE = $"{fileInfo.Length / 1024} KB"
                        });
                    }
                }
            }

            model = model.OrderByDescending(m => m.UPLOADEDDATE).ToList();

            return View(model);
        }

        public ActionResult ViewMergedPdfDB(string accountNumber, bool download = false)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return new HttpStatusCodeResult(400, "Invalid account number.");
            List<byte[]> pdfList = new List<byte[]>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT PdfData 
                       FROM CPPCIMAGEUPLOAD
                       WHERE AccountNumber=@Acc 
                       ORDER BY UploadedDate ASC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 3600;
                    cmd.Parameters.AddWithValue("@Acc", accountNumber);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pdfList.Add((byte[])reader["PdfData"]);
                        }
                    }
                }
            }

            if (!pdfList.Any())
                return HttpNotFound("No PDFs found for this account.");
            byte[] mergedPdf = MergeMultiplePdfs(pdfList);

            if (download)
            {
                return File(mergedPdf, "application/pdf", $"{accountNumber}_merged.pdf");
            }
            else
            {
                Response.AppendHeader("Content-Disposition", $"inline; filename={accountNumber}_merged.pdf");
                return File(mergedPdf, "application/pdf");
            }
        }
        public static byte[] MergeMultiplePdfs(List<byte[]> pdfList)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document();
                PdfCopy copy = new PdfCopy(doc, ms);
                doc.Open();

                foreach (var pdfBytes in pdfList)
                {
                    using (var reader = new PdfReader(pdfBytes))
                    {
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            copy.AddPage(copy.GetImportedPage(reader, i));
                        }
                    }
                }

                doc.Close();
                return ms.ToArray();
            }
        }

        public ActionResult ViewMergedPdf(string accountNumber, bool download = false)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return new HttpStatusCodeResult(400, "Invalid account number.");

            string accountFolder = Path.Combine(UploadFolder, accountNumber);

            if (!Directory.Exists(accountFolder))
                return HttpNotFound("Account folder not found.");

            string mergedPath = Path.Combine(accountFolder, $"{accountNumber}.pdf");

            if (!System.IO.File.Exists(mergedPath))
                return HttpNotFound("Merged PDF not found.");

            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(mergedPath);

                if (download)
                {
                    return File(fileBytes, "application/pdf", $"{accountNumber}_merged.pdf");
                }
                else
                {
                    Response.AppendHeader("Content-Disposition", $"inline; filename={accountNumber}_merged.pdf");
                    return File(fileBytes, "application/pdf");
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"Error loading PDF: {ex.Message}");
            }
        }
        #region
        //public ActionResult DownloadFolder(string accountNumber)
        //{
        //    string UploadFolder = Server.MapPath("~/UploadedPdfs");
        //    var folderPath = Path.Combine(UploadFolder, accountNumber);

        //    if (!Directory.Exists(folderPath))
        //    {
        //        return HttpNotFound("Folder not found");
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        //        {
        //            foreach (var file in Directory.GetFiles(folderPath))
        //            {
        //                var entry = archive.CreateEntry(Path.GetFileName(file));
        //                using (var entryStream = entry.Open())
        //                using (var fileStream = System.IO.File.OpenRead(file))
        //                {
        //                    fileStream.CopyTo(entryStream);
        //                }
        //            }
        //        }

        //        memoryStream.Seek(0, SeekOrigin.Begin);
        //        return File(memoryStream.ToArray(), "application/zip", $"{accountNumber}.zip");
        //    }
        //}
        #endregion
        public ActionResult DownloadFolder()
        {
            string folderToZip = Server.MapPath("~/UploadedPdfs");

            if (!Directory.Exists(folderToZip))
            {
                return HttpNotFound("Uploaded folder not found");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {

                    foreach (string filePath in Directory.GetFiles(folderToZip, "*.*", SearchOption.AllDirectories))
                    {
                        string relativePath = filePath.Substring(folderToZip.Length + 1);
                        archive.CreateEntryFromFile(filePath, relativePath);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/zip", "AllUploadedPDFs.zip");
            }
        }
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                return Json(new { success = false, message = "Please select an Excel file." });
            }
            if (!excelFile.FileName.EndsWith(".xlsx") && !excelFile.FileName.EndsWith(".xls"))
            {
                return Json(new { success = false, message = "Please upload a valid Excel file (.xlsx or .xls)." });
            }

            try
            {
                List<string> accountNumbers = new List<string>();
                int emptyRows = 0;

                using (var package = new ExcelPackage(excelFile.InputStream))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        return Json(new { success = false, message = "The Excel file contains no worksheets." });
                    }

                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null || worksheet.Dimension == null)
                    {
                        return Json(new { success = false, message = "The worksheet is empty." });
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int startRow = worksheet.Cells[1, 1].Text.Trim().Equals("AccountNumber", StringComparison.OrdinalIgnoreCase) ? 2 : 1;

                    for (int row = startRow; row <= rowCount; row++)
                    {
                        string accountNo = worksheet.Cells[row, 1]?.Text?.Trim();
                        if (!string.IsNullOrEmpty(accountNo))
                        {
                            accountNumbers.Add(accountNo);
                        }
                        else
                        {
                            emptyRows++;
                        }
                    }
                }

                if (accountNumbers.Count == 0)
                {
                    return Json(new { success = false, message = "No valid account numbers found in the Excel file." });
                }
                Session["ExcelAccountNumbers"] = accountNumbers;
                Session["ExcelProcessStart"] = DateTime.Now;
                Session["MatchedCount"] = 0;
                Session["ProcessedCount"] = 0;

                return Json(new
                {
                    success = true,
                    count = accountNumbers.Count,
                    emptyRows = emptyRows,
                    message = $"Successfully processed {accountNumbers.Count} account numbers. {(emptyRows > 0 ? $"{emptyRows} empty rows were skipped." : "")}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing the Excel file. Please ensure it's a valid Excel file with account numbers in the first column."
                });
            }
        }
        public ActionResult DownloadMatchedPdfs()
        {
            var accountNumbers = Session["ExcelAccountNumbers"] as List<string>;
            if (accountNumbers == null || !accountNumbers.Any())
            {
                return Json(new { success = false, message = "No account numbers found to process. Please upload an Excel file first." },
                           JsonRequestBehavior.AllowGet);
            }

            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "PdfExport_" + Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                int matchedCount = 0;
                int processedCount = 0;
                var matchedAccounts = new List<string>();


                foreach (var account in accountNumbers.Distinct())
                {
                    processedCount++;
                    Session["ProcessedCount"] = processedCount;

                    var pdfPath = Path.Combine(UploadFolder, account, "merged.pdf");

                    if (System.IO.File.Exists(pdfPath))
                    {
                        string destPath = Path.Combine(tempFolder, $"{account}.pdf");
                        try
                        {
                            System.IO.File.Copy(pdfPath, destPath);
                            matchedCount++;
                            matchedAccounts.Add(account);
                            Session["MatchedCount"] = matchedCount;
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    if (processedCount % 10 == 0)
                    {
                        Thread.Sleep(100);
                    }
                }

                if (matchedCount == 0)
                {
                    Directory.Delete(tempFolder, true);
                    return Json(new { success = false, message = "No matching PDFs found for the provided account numbers." },
                               JsonRequestBehavior.AllowGet);
                }

                string zipFileName = $"MatchedPdfs_{DateTime.Now:yyyyMMddHHmmss}.zip";
                string zipPath = Path.Combine(Path.GetTempPath(), zipFileName);


                ZipFile.CreateFromDirectory(tempFolder, zipPath, CompressionLevel.Optimal, false);
                Directory.Delete(tempFolder, true);
                Session["MatchedCount"] = matchedCount;
                Session["TotalCount"] = accountNumbers.Count;
                Session["DownloadComplete"] = true;
                Response.BufferOutput = false;
                return new FilePathResult(zipPath, "application/zip")
                {
                    FileDownloadName = zipFileName
                };
            }
            catch (Exception ex)
            {
                // Logger.Error("Error creating PDF package", ex);
                return Json(new { success = false, message = "Error creating ZIP file: " + ex.Message },
                           JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CheckDownloadStatus()
        {
            try
            {
                int matched = Session["MatchedCount"] as int? ?? 0;
                //int total = Session["ExcelAccountNumbers"]?C ?? 0;
                int processed = Session["ProcessedCount"] as int? ?? 0;
                bool completed = Session["DownloadComplete"] as bool? ?? false;


                int percentage = 0;
                //if (total > 0)
                //{
                //    percentage = completed ? 100 : (int)((processed * 100) / total);
                //}

                return Json(new
                {
                    completed = completed,
                    matched = matched,
                    //total = total,
                    processed = processed,
                    percentage = percentage,
                    elapsedSeconds = completed ?
                        (DateTime.Now - (Session["ExcelProcessStart"] as DateTime? ?? DateTime.Now)).TotalSeconds : 0
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { error = "Error checking status" }, JsonRequestBehavior.AllowGet);
            }
        }

        #region
        //public ActionResult PPADETAILSWITHIMAGE(string searchAccountNumber = null, string bankType = null)
        //{
        //    var allAccounts = ppaaccountDA.SP_PPADETAILSWITHIMAGE(searchAccountNumber);
        //    var filteredAccounts = new List<PPADETAILSWITHIMAGE>();

        //    foreach (var account in allAccounts)
        //    {
        //        var ppaDetail = new PPADETAILSWITHIMAGE
        //        {
        //            PPA_ACNO = account.PPA_ACNO,
        //            PPA_PPONO = account.PPA_PPONO,
        //            PPA_name = account.PPA_name,
        //            PPA_DPCD = account.PPA_DPCD,
        //            PPA_Type = account.PPA_Type,
        //            BankType = GetBankType(account.PPA_DPCD),
        //            CalculatedDpcode = GetCalculatedDpcode(account.PPA_DPCD)
        //        };

        //        string imagePath = GetImagePath(account.PPA_ACNO, ppaDetail.CalculatedDpcode);
        //        ppaDetail.ImagePath = imagePath;
        //        ppaDetail.ImageExists = System.IO.File.Exists(imagePath);

        //        if (string.IsNullOrEmpty(bankType) || bankType == ppaDetail.BankType)
        //        {
        //            filteredAccounts.Add(ppaDetail);
        //        }
        //    }

        //    var viewModel = new RailwayAccountViewModel
        //    {
        //        Accounts = filteredAccounts,
        //        SearchTerm = searchAccountNumber,
        //        SelectedBankType = bankType,
        //        BankTypes = new List<SelectListItem>
        //        {
        //           new SelectListItem { Value = "", Text = "All Banks" },
        //           new SelectListItem { Value = "canarabank", Text = "Canara Bank" },
        //           new SelectListItem { Value = "syndicate", Text = "Syndicate Bank" }
        //        }
        //    };

        //    return View(viewModel);
        //}

        //[HttpGet]
        //public ActionResult PPADETAILSWITHIMAGE(string searchAccountNumber = null, string bankType = null)
        //{

        //    List<PPADETAILSWITHIMAGE> filteredAccounts = new List<PPADETAILSWITHIMAGE>();

        //    if (!string.IsNullOrEmpty(searchAccountNumber))
        //    {
        //        var allAccounts = ppaaccountDA.SP_PPADETAILSWITHIMAGE(searchAccountNumber);

        //        foreach (var account in allAccounts)
        //        {
        //            var ppaDetail = new PPADETAILSWITHIMAGE
        //            {
        //                PPA_ACNO = account.PPA_ACNO,
        //                PPA_PPONO = account.PPA_PPONO,
        //                PPA_name = account.PPA_name,
        //                PPA_DPCD = account.PPA_DPCD,
        //                PPA_Type = account.PPA_Type,
        //                BankType = GetBankType(account.PPA_DPCD),
        //                CalculatedDpcode = GetCalculatedDpcode(account.PPA_DPCD)
        //            };

        //            // Only check image existence when search is performed
        //            string imagePath = GetImagePath(account.PPA_ACNO, ppaDetail.PPA_DPCD);
        //            ppaDetail.ImagePath = imagePath;
        //            ppaDetail.ImageExists = System.IO.File.Exists(imagePath);

        //            if (string.IsNullOrEmpty(bankType) || bankType == ppaDetail.BankType)
        //            {
        //                filteredAccounts.Add(ppaDetail);
        //            }
        //        }
        //    }

        //    var viewModel = new RailwayAccountViewModel
        //    {
        //        Accounts = filteredAccounts,
        //        SearchTerm = searchAccountNumber,
        //        SelectedBankType = bankType,
        //        BankTypes = new List<SelectListItem>
        //{
        //    new SelectListItem { Value = "", Text = "All Banks" },
        //    new SelectListItem { Value = "canarabank", Text = "Canara Bank" },
        //    new SelectListItem { Value = "syndicate", Text = "Syndicate Bank" }
        //}
        //    };

        //    return View(viewModel);
        //}




        //[HttpGet]
        //public ActionResult PPADETAILSWITHIMAGE(string searchAccountNumber = null, string bankType = null, int page = 1)
        //{
        //    List<PPADETAILSWITHIMAGE> filteredAccounts = new List<PPADETAILSWITHIMAGE>();

        //    if (!string.IsNullOrEmpty(searchAccountNumber))
        //    {
        //        var allAccounts = ppaaccountDA.SP_PPADETAILSWITHIMAGE(searchAccountNumber);

        //        foreach (var account in allAccounts)
        //        {
        //            var ppaDetail = new PPADETAILSWITHIMAGE
        //            {
        //                PPA_ACNO = account.PPA_ACNO,
        //                PPA_PPONO = account.PPA_PPONO,
        //                PPA_name = account.PPA_name,
        //                PPA_DPCD = account.PPA_DPCD,
        //                PPA_Type = account.PPA_Type,
        //                BankType = GetBankType(account.PPA_DPCD),
        //                CalculatedDpcode = GetCalculatedDpcode(account.PPA_DPCD)
        //            };

        //            // Get all image paths for this account
        //            var imagePaths = GetImagePaths(account.PPA_ACNO, ppaDetail.PPA_DPCD);
        //            ppaDetail.ImagePaths = imagePaths;
        //            ppaDetail.HasImages = imagePaths.Any();
        //            ppaDetail.ImageCount = imagePaths.Count;

        //            // Apply bank type filter
        //            if (string.IsNullOrEmpty(bankType) || bankType == ppaDetail.BankType)
        //            {
        //                filteredAccounts.Add(ppaDetail);
        //            }
        //        }
        //    }

        //    // Pagination logic
        //    var totalItems = filteredAccounts.Count;
        //    var totalPages = (int)Math.Ceiling((double)totalItems / ITEMS_PER_PAGE);
        //    var paginatedAccounts = filteredAccounts.Skip((page - 1) * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE).ToList();

        //    var viewModel = new RailwayAccountViewModel
        //    {
        //        Accounts = paginatedAccounts,
        //        SearchTerm = searchAccountNumber,
        //        SelectedBankType = bankType,
        //        CurrentPage = page,
        //        TotalPages = totalPages,
        //        TotalItems = totalItems,
        //        BankTypes = new List<SelectListItem>
        //    {
        //        new SelectListItem { Value = "", Text = "All Banks" },
        //        new SelectListItem { Value = "canarabank", Text = "Canara Bank" },
        //        new SelectListItem { Value = "syndicate", Text = "Syndicate Bank" }
        //    }
        //    };

        //    return View(viewModel);
        //}

        //// Helper methods (GetBankType, GetCalculatedDpcode, GetImagePaths, ViewImage, ViewAllImages)
        //// ... (keep these methods as they are in your original code)

        //// Add a new method to get image by filename
        //public ActionResult GetImage(string fileName, string dpCode)
        //{
        //    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(dpCode))
        //    {
        //        return Content("File name or DP code is missing");
        //    }

        //    string imagePath = Path.Combine(ViewRailwaysImage, dpCode, fileName);

        //    if (System.IO.File.Exists(imagePath))
        //    {
        //        return File(imagePath, "image/jpeg");
        //    }

        //    return Content("Image not found: " + fileName);
        //}

        //// ADD THESE HELPER METHODS TO YOUR CONTROLLER:
        //private string GetBankType(string dpCode)
        //{
        //    if (string.IsNullOrEmpty(dpCode))
        //        return "other";

        //    if (dpCode.Length == 4)
        //        return "canarabank";

        //    if (dpCode.Length == 5 && dpCode.StartsWith("1"))
        //        return "syndicate";

        //    return "other";
        //}

        //private string GetCalculatedDpcode(string dpCode)
        //{
        //    if (string.IsNullOrEmpty(dpCode))
        //        return dpCode;

        //    if (dpCode.Length == 4)
        //        return "canarabank";

        //    if (dpCode.Length == 5 && dpCode.StartsWith("1"))
        //        return "syndicate" + dpCode.Substring(1);

        //    return dpCode;
        //}

        //private List<string> GetImagePaths(string accountNumber, string DPCODE)
        //{
        //    string cleanAccountNumber = accountNumber;
        //    if (accountNumber.Contains("_"))
        //    {
        //        cleanAccountNumber = accountNumber.Split('_')[0];
        //    }

        //    string folderPath = Path.Combine(ViewRailwaysImage, DPCODE);

        //    if (!Directory.Exists(folderPath))
        //        return new List<string>();

        //    string searchPattern = accountNumber + "*.jpg";
        //    var files = Directory.GetFiles(folderPath, searchPattern)
        //                        .OrderBy(f => f)
        //                        .ToList();

        //    return files;
        //}

        //public ActionResult ViewImage(string accountNumber, string dpCode, string fileName = null)
        //{
        //    if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(dpCode))
        //    {
        //        return Content("Account number or DP code is missing");
        //    }
        //    if (!string.IsNullOrEmpty(fileName))
        //    {
        //        string cleanAccountNumber = accountNumber.Contains("_") ? accountNumber.Split('_')[0] : accountNumber;
        //        string imagePath = Path.Combine(ViewRailwaysImage, dpCode, cleanAccountNumber, fileName);

        //        if (System.IO.File.Exists(imagePath))
        //        {
        //            return File(imagePath, "image/jpeg");
        //        }
        //    }
        //    var imagePaths = GetImagePaths(accountNumber, dpCode);
        //    if (imagePaths.Any())
        //    {
        //        return File(imagePaths.First(), "image/jpeg");
        //    }

        //    return Content("Image not found for account: " + accountNumber);
        //}

        //public ActionResult ViewAllImages(string accountNumber, string dpCode)
        //{
        //    var imagePaths = GetImagePaths(accountNumber, dpCode);
        //    return Json(imagePaths.Select(Path.GetFileName), JsonRequestBehavior.AllowGet);
        //}
        #endregion

        /*===================FETCH EXISTING IMAGE AND DISPLAY=============*/

        [HttpGet]
        public ActionResult PPADETAILSWITHIMAGE1(string searchAccountNumber = null, string bankType = null, int page = 1)
        {
            List<PPADETAILSWITHIMAGE> filteredAccounts = new List<PPADETAILSWITHIMAGE>();

            if (!string.IsNullOrEmpty(searchAccountNumber))
            {
                var allAccounts = ppaaccountDA.SP_PPADETAILSWITHIMAGE(searchAccountNumber);

                foreach (var account in allAccounts)
                {
                    var ppaDetail = new PPADETAILSWITHIMAGE
                    {
                        PPA_ACNO = account.PPA_ACNO,
                        PPA_PPONO = account.PPA_PPONO,
                        PPA_name = account.PPA_name,
                        PPA_DPCD = account.PPA_DPCD,
                        PPA_Type = account.PPA_Type,
                        BankType = GetBankType(account.PPA_DPCD),
                        CalculatedDpcode = GetCalculatedDpcode(account.PPA_DPCD)
                    };


                    var imagePaths = GetImagePaths(account.PPA_ACNO, ppaDetail.PPA_DPCD);
                    ppaDetail.ImagePaths = imagePaths;
                    ppaDetail.HasImages = imagePaths.Any();
                    ppaDetail.ImageCount = imagePaths.Count;


                    if (string.IsNullOrEmpty(bankType) || bankType == ppaDetail.BankType)
                    {
                        filteredAccounts.Add(ppaDetail);
                    }
                }
            }


            var totalItems = filteredAccounts.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / ITEMS_PER_PAGE);
            var paginatedAccounts = filteredAccounts.Skip((page - 1) * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE).ToList();

            var viewModel = new RailwayAccountViewModel
            {
                Accounts = paginatedAccounts,
                SearchTerm = searchAccountNumber,
                SelectedBankType = bankType,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                BankTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Banks" },
                new SelectListItem { Value = "canarabank", Text = "Canara Bank" },
                new SelectListItem { Value = "syndicate", Text = "Syndicate Bank" }
            }
            };

            return View(viewModel);
        }


        public ActionResult ConvertExitingJpgToPDF1()
        {
            var processor = new ImageProcessor();
            processor.ProcessAllConfiguredFolders();


            var summary = new List<ConversionSummaryModel>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT AccountType, COUNT(*) AS TotalConverted
                       FROM CPPCIMAGEUPLOAD
                       WHERE CAST(UploadedDate AS DATE) = CAST(GETDATE() AS DATE)
                       GROUP BY AccountType";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summary.Add(new ConversionSummaryModel
                        {
                            AccountType = reader["AccountType"].ToString(),
                            TotalConverted = Convert.ToInt32(reader["TotalConverted"])
                        });
                    }
                }
            }

            ViewBag.TotalConvertedAll = summary.Sum(s => s.TotalConverted);
            return View(summary);
        }

        public ActionResult ConvertExitingJpgToPD2F()
        {
            //try
            //{
            //    var processor = new ImageProcessor();
            //    var result = processor.ProcessAllConfiguredFolders();


            //    var reportGenerator = new ReportGenerator();
            //    string summaryReport = reportGenerator.GenerateDailySummaryReport();
            //    string progressSnapshot = reportGenerator.GenerateProgressSnapshot();


            //    var summary = GetConversionSummary();

            //    ViewBag.TotalConvertedAll = summary.Sum(s => s.TotalConverted);
            //    ViewBag.TotalFailedAll = summary.Sum(s => s.TotalFailed);
            //    ViewBag.ConversionResult = result;
            //    ViewBag.SessionId = result.SessionId;
            //    ViewBag.SummaryReportPath = summaryReport;
            //    ViewBag.ProgressSnapshotPath = progressSnapshot;
            //    ViewBag.LogFilePath = result.LogFilePath;

            //    return View(summary);
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.Error = $"Error during conversion: {ex.Message}";
            //    return View(new List<ConversionSummaryModel>());
            //}

            return View();
        }


        [HttpPost]
        public JsonResult _ConvertExitingJpgToPDF1()
        {
            try
            {
                var processor = new ImageProcessor();
                var result = processor.ProcessAllConfiguredFolders();


                var reportGenerator = new ReportGenerator();
                string summaryReport = reportGenerator.GenerateDailySummaryReport();
                string progressSnapshot = reportGenerator.GenerateProgressSnapshot();

                return Json(new
                {
                    success = true,
                    sessionId = result.SessionId,
                    logFilePath = result.LogFilePath,
                    summaryReportPath = summaryReport,
                    progressSnapshotPath = progressSnapshot
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetConversionProgress11()
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
                {
                    conn.Open();
                    string sql = @"SELECT FolderPath, DpCode, AccountType, TotalFiles, ProcessedFiles, 
                                  CurrentFile, Status, LastUpdated,
                                  (SELECT COUNT(*) FROM DummyFileConversionLog fcl 
                                   WHERE fcl.LogId = cl.LogId AND fcl.Status = 'Success') as ConvertedFiles,
                                  (SELECT COUNT(*) FROM DummyFileConversionLog fcl 
                                   WHERE fcl.LogId = cl.LogId AND fcl.Status = 'Failed') as FailedFiles
                           FROM DummyConversionLog cl
                           WHERE CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)
                           ORDER BY LastUpdated DESC";

                    var progress = new List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            progress.Add(new
                            {
                                FolderPath = reader["FolderPath"].ToString(),
                                DpCode = reader["DpCode"].ToString(),
                                AccountType = reader["AccountType"].ToString(),
                                TotalFiles = Convert.ToInt32(reader["TotalFiles"]),
                                ProcessedFiles = Convert.ToInt32(reader["ProcessedFiles"]),
                                ConvertedFiles = Convert.ToInt32(reader["ConvertedFiles"]),
                                FailedFiles = Convert.ToInt32(reader["FailedFiles"]),
                                CurrentFile = reader["CurrentFile"].ToString(),
                                Status = reader["Status"].ToString(),
                                LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
                            });
                        }
                    }

                    return Json(progress, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetConversionProgress1()
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
                {
                    conn.Open();
                    string sql = @"SELECT FolderPath, DpCode, AccountType, TotalFiles, ProcessedFiles, 
                                  CurrentFile, Status, LastUpdated,
                                  CASE WHEN TotalFiles > 0 THEN 
                                      CAST(ProcessedFiles AS FLOAT) / TotalFiles * 100 
                                  ELSE 0 END as ProgressPercentage
                           FROM DummyFolderProgress 
                           WHERE CAST(LastUpdated AS DATE) = CAST(GETDATE() AS DATE)
                           ORDER BY LastUpdated DESC";

                    var progress = new List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            progress.Add(new
                            {
                                FolderPath = reader["FolderPath"].ToString(),
                                DpCode = reader["DpCode"].ToString(),
                                AccountType = reader["AccountType"].ToString(),
                                TotalFiles = Convert.ToInt32(reader["TotalFiles"]),
                                ProcessedFiles = Convert.ToInt32(reader["ProcessedFiles"]),
                                CurrentFile = reader["CurrentFile"].ToString(),
                                Status = reader["Status"].ToString(),
                                LastUpdated = Convert.ToDateTime(reader["LastUpdated"]),
                                ProgressPercentage = Convert.ToDouble(reader["ProgressPercentage"])
                            });
                        }
                    }

                    return Json(progress, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadLogFile1(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("Log file not found");
            }

            string fileName = Path.GetFileName(filePath);
            string mimeType = "text/plain";
            return File(filePath, mimeType, fileName);
        }

        private List<ConversionSummaryModel_Log> GetConversionSummary1()
        {
            var summary = new List<ConversionSummaryModel_Log>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
            {
                conn.Open();

                string sql = @"SELECT AccountType, FolderPath, DpCode, 
                              SUM(ConvertedFiles) as TotalConverted,
                              SUM(FailedFiles) as TotalFailed
                       FROM DummyConversionLog 
                       WHERE CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)
                       GROUP BY AccountType, FolderPath, DpCode";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summary.Add(new ConversionSummaryModel_Log
                        {
                            AccountType = reader["AccountType"].ToString(),
                            TotalConverted = Convert.ToInt32(reader["TotalConverted"]),
                            TotalFailed = Convert.ToInt32(reader["TotalFailed"]),
                            FolderPath = reader["FolderPath"].ToString(),
                            DpCode = reader["DpCode"].ToString()
                        });
                    }
                }
            }
            return summary;
        }

        //public ActionResult GetImage(string fileName, string dpCode)
        //{
        //    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(dpCode))
        //    {
        //        return Content("File name or DP code is missing");
        //    }

        //    string imagePath = Path.Combine(ViewRailwaysImage, dpCode, fileName);

        //    if (System.IO.File.Exists(imagePath))
        //    {
        //        return File(imagePath, "image/jpeg");
        //    }

        //    return Content("Image not found: " + fileName);
        //}
        public ActionResult GetImage(string fileName, string dpCode)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(dpCode))
            {
                return Content("File name or DP code is missing");
            }


            var FetchImagePaths = new List<string>
            {
              Path.Combine(ViewRailwaysImage, dpCode, fileName),
              Path.Combine(ViewCivilImage, dpCode, fileName),
              Path.Combine(ViewKarnatakaStateImage, dpCode, fileName),
              Path.Combine(ViewOtherStateImage, dpCode, fileName),
              Path.Combine(ViewPostalImage, dpCode, fileName),
              Path.Combine(ViewTelecomeImage, dpCode, fileName)
            };

            foreach (var path in FetchImagePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return File(path, "image/jpeg");
                }
            }

            return Content("Image not found in both folders: " + fileName);
        }

        private string GetBankType(string dpCode)
        {
            if (string.IsNullOrEmpty(dpCode))
                return "other";

            if (dpCode.Length <= 4)
                return "canarabank";

            if (dpCode.Length == 5 && dpCode.StartsWith("1"))
                return "syndicate";

            return "other";
        }
        private string GetCalculatedDpcode(string dpCode)
        {
            if (string.IsNullOrEmpty(dpCode))
                return dpCode;

            if (dpCode.Length <= 4)
                return "canarabank";

            if (dpCode.Length == 5 && dpCode.StartsWith("1"))
                return "syndicate" + dpCode.Substring(1);

            return dpCode;
        }
        private List<string> GetImagePaths(string accountNumber, string DPCODE)
        {
            string cleanAccountNumber = accountNumber;
            if (accountNumber.Contains("_"))
            {
                cleanAccountNumber = accountNumber.Split('_')[0];
            }


            var FetchImagePaths = new List<string>
                {
                    Path.Combine(ViewRailwaysImage, DPCODE),
                    Path.Combine(ViewCivilImage, DPCODE),
                    Path.Combine(ViewKarnatakaStateImage, DPCODE),
                    Path.Combine(ViewOtherStateImage, DPCODE),
                    Path.Combine(ViewPostalImage, DPCODE),
                    Path.Combine(ViewTelecomeImage, DPCODE)
                };

            var resultFiles = new List<string>();
            string searchPattern = cleanAccountNumber + "*.jpg";

            foreach (var folderPath in FetchImagePaths)
            {
                if (Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath, searchPattern)
                                         .OrderBy(f => f)
                                         .ToList();

                    if (files.Any())
                    {
                        resultFiles.AddRange(files);
                    }
                }
            }

            return resultFiles;
        }

        //private List<string> GetImagePaths(string accountNumber, string DPCODE)
        //{
        //    string cleanAccountNumber = accountNumber;
        //    if (accountNumber.Contains("_"))
        //    {
        //        cleanAccountNumber = accountNumber.Split('_')[0];
        //    }

        //    //string folderPath = Path.Combine(ViewRailwaysImage, DPCODE);
        //    var FetchImagePaths = new List<string>
        //    {
        //      Path.Combine(ViewRailwaysImage, DPCODE),
        //      Path.Combine(ViewCivilImage, DPCODE),
        //      Path.Combine(ViewKarnatakaStateImage, DPCODE),
        //      Path.Combine(ViewOtherStateImage, DPCODE),
        //      Path.Combine(ViewPostalImage, DPCODE),
        //      Path.Combine(ViewTelecomeImage, DPCODE)
        //    };

        //    foreach (var path in FetchImagePaths)
        //    {
        //        if (System.IO.File.Exists(path))
        //        {
        //            return File(path, "image/jpeg");
        //        }
        //    }

        //    if (!Directory.Exists(FetchImagePaths))
        //        return new List<string>();

        //    string searchPattern = accountNumber + "*.jpg";
        //    var files = Directory.GetFiles(folderPath, searchPattern)
        //                        .OrderBy(f => f)
        //                        .ToList();

        //    return files;
        //}
        public ActionResult ViewImage(string accountNumber, string dpCode, string fileName = null)
        {
            if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(dpCode))
            {
                return Content("Account number or DP code is missing");
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                string cleanAccountNumber = accountNumber.Contains("_") ? accountNumber.Split('_')[0] : accountNumber;
                string imagePath = Path.Combine(ViewRailwaysImage, dpCode, cleanAccountNumber, fileName);

                if (System.IO.File.Exists(imagePath))
                {
                    return File(imagePath, "image/jpeg");
                }
            }
            var imagePaths = GetImagePaths(accountNumber, dpCode);
            if (imagePaths.Any())
            {
                return File(imagePaths.First(), "image/jpeg");
            }

            return Content("Image not found for account: " + accountNumber);
        }
        public ActionResult ViewAllImages(string accountNumber, string dpCode)
        {
            var imagePaths = GetImagePaths(accountNumber, dpCode);
            return Json(imagePaths.Select(Path.GetFileName), JsonRequestBehavior.AllowGet);
        }

        /*===================FETCH EXISTING IMAGE AND DISPLAY=============*/


        [HttpGet]
        public ActionResult ConvertExitingJpgToPDF22()
        {
            try
            {

                var summary = GetConversionSummaryFromMainTable();
                ViewBag.TotalConvertedAll = summary.Sum(s => s.TotalConverted);
                ViewBag.TotalFailedAll = summary.Sum(s => s.TotalFailed);

                return View(summary);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading page: {ex.Message}";
                return View(new List<ConversionSummaryModel>());
            }
        }

        [HttpGet]
        public ActionResult ConvertExitingJpgToPDF()
        {
            try
            {
                //var summary = GetConversionSummaryFromMainTable();
                //if (summary == null || summary.Count == 0)
                //{
                //    ViewBag.TotalConvertedAll = 0;
                //    ViewBag.TotalFailedAll = 0;
                //    return View();  
                //}
                //ViewBag.TotalConvertedAll = summary.Sum(s => s.TotalConverted);
                //ViewBag.TotalFailedAll = summary.Sum(s => s.TotalFailed);

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading page: {ex.Message}";
                return View();
            }
        }




        [HttpPost]
        public JsonResult _ConvertExitingJpgToPDF()
        {
            try
            {
                var processor = new ImageProcessor();
                var result = processor.ProcessAllConfiguredFolders();

                return Json(new
                {
                    success = true,
                    sessionId = result.SessionId,
                    totalConverted = result.TotalConverted,
                    totalFailed = result.TotalFailed,
                    message = $"Conversion completed! Converted: {result.TotalConverted}, Failed: {result.TotalFailed}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to convert images: {ex.Message}"
                });
            }
        }


        [HttpGet]
        public ActionResult DownloadConversionProgress()
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
                {
                    conn.Open();

                    string sql = @"
                    SELECT 
                        cl.DpCode,
                        cl.FolderPath,
                        cl.AccountType,
                        cl.TotalFiles,
                        cl.ConvertedFiles,
                        cl.FailedFiles,
                        cl.Status,
                        cl.StartTime,
                        cl.EndTime,
                        cp.CurrentFile,
                        cp.ProcessedFiles,
                        cp.LastUpdated
                    FROM EXISTINGIMAGECONVERSION_LOG cl
                    LEFT JOIN EXISTINGIMAGECONVERSION_PROGRESS cp ON cl.DpCode = cp.DpCode AND cl.FolderPath = cp.FolderPath
                    WHERE CAST(cl.StartTime AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY cl.StartTime DESC";

                    DataTable dataTable = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }


                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Conversion Progress");


                        worksheet.Cells[1, 1].Value = "JPG TO PDF CONVERSION PROGRESS REPORT";
                        worksheet.Cells[1, 1, 1, 8].Merge = true;
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1].Style.Font.Size = 16;
                        worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                        worksheet.Cells[2, 1].Value = $"Report Generated: {DateTime.Now:dddd, MMMM dd, yyyy hh:mm:ss tt}";
                        worksheet.Cells[2, 1, 2, 8].Merge = true;
                        worksheet.Cells[2, 1].Style.Font.Bold = true;
                        worksheet.Cells[2, 1].Style.Font.Size = 11;
                        worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                        int headerRow = 4;
                        string[] headers = {
                        "DP Code", "Folder Path", "Account Type", "Total Files",
                        "Converted Files", "Failed Files", "Success Rate %", "Status",
                        "Current File", "Processed Files", "Start Time", "End Time", "Last Updated"
                    };


                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[headerRow, i + 1].Value = headers[i];
                            worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                            worksheet.Cells[headerRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }


                        int row = headerRow + 1;
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            int totalFiles = Convert.ToInt32(dataRow["TotalFiles"]);
                            int convertedFiles = Convert.ToInt32(dataRow["ConvertedFiles"]);
                            double successRate = totalFiles > 0 ? Math.Round((convertedFiles / (double)totalFiles) * 100, 2) : 0;

                            worksheet.Cells[row, 1].Value = dataRow["DpCode"]?.ToString() ?? "";
                            worksheet.Cells[row, 2].Value = dataRow["FolderPath"]?.ToString() ?? "";
                            worksheet.Cells[row, 3].Value = dataRow["AccountType"]?.ToString() ?? "";
                            worksheet.Cells[row, 4].Value = totalFiles;
                            worksheet.Cells[row, 5].Value = convertedFiles;
                            worksheet.Cells[row, 6].Value = Convert.ToInt32(dataRow["FailedFiles"]);
                            worksheet.Cells[row, 7].Value = successRate;
                            worksheet.Cells[row, 8].Value = dataRow["Status"]?.ToString() ?? "";
                            worksheet.Cells[row, 9].Value = dataRow["CurrentFile"]?.ToString() ?? "Processing...";
                            worksheet.Cells[row, 10].Value = dataRow["ProcessedFiles"] != DBNull.Value ? Convert.ToInt32(dataRow["ProcessedFiles"]) : 0;
                            worksheet.Cells[row, 11].Value = dataRow["StartTime"] != DBNull.Value ? Convert.ToDateTime(dataRow["StartTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                            worksheet.Cells[row, 12].Value = dataRow["EndTime"] != DBNull.Value ? Convert.ToDateTime(dataRow["EndTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "In Progress";
                            worksheet.Cells[row, 13].Value = dataRow["LastUpdated"] != DBNull.Value ? Convert.ToDateTime(dataRow["LastUpdated"]).ToString("yyyy-MM-dd HH:mm:ss") : "N/A";

                            row++;
                        }


                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();


                        byte[] fileContents = package.GetAsByteArray();


                        string fileName = $"Conversion_Progress_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        return File(fileContents,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = $"Error generating report: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private int CountByStatus(DataTable dataTable, string status)
        {
            int count = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                if (row["Status"]?.ToString()?.ToLower() == status.ToLower())
                    count++;
            }
            return count;
        }
        [HttpGet]
        public JsonResult GetConversionProgress()
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
                {
                    conn.Open();

                    string sql = @"
                SELECT 
                    cl.DpCode,
                    cl.FolderPath,
                    cl.AccountType,
                    cl.TotalFiles,
                    cl.ConvertedFiles,
                    cl.FailedFiles,
                    cl.Status,
                    cl.StartTime,
                    cl.EndTime,
                    cp.CurrentFile,
                    cp.ProcessedFiles,
                    cp.LastUpdated
                FROM EXISTINGIMAGECONVERSION_LOG cl
                LEFT JOIN EXISTINGIMAGECONVERSION_PROGRESS cp ON cl.DpCode = cp.DpCode AND cl.FolderPath = cp.FolderPath
                WHERE CAST(cl.StartTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY cl.StartTime DESC";

                    var progress = new List<object>();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            progress.Add(new
                            {
                                DpCode = reader["DpCode"].ToString(),
                                FolderPath = reader["FolderPath"].ToString(),
                                AccountType = reader["AccountType"].ToString(),
                                TotalFiles = Convert.ToInt32(reader["TotalFiles"]),
                                ConvertedFiles = Convert.ToInt32(reader["ConvertedFiles"]),
                                FailedFiles = Convert.ToInt32(reader["FailedFiles"]),
                                Status = reader["Status"].ToString(),
                                CurrentFile = reader["CurrentFile"]?.ToString() ?? "Processing...",
                                ProcessedFiles = reader["ProcessedFiles"] != DBNull.Value ? Convert.ToInt32(reader["ProcessedFiles"]) : 0,
                                StartTime = Convert.ToDateTime(reader["StartTime"]),
                                EndTime = reader["EndTime"] != DBNull.Value ? Convert.ToDateTime(reader["EndTime"]) : (DateTime?)null,
                                LastUpdated = reader["LastUpdated"] != DBNull.Value ? Convert.ToDateTime(reader["LastUpdated"]) : (DateTime?)null
                            });
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        progress = progress,
                        lastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = $"Error fetching progress: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ConversionSummaryModel_Log> GetConversionSummaryFromMainTable22()
        {
            var summary = new List<ConversionSummaryModel_Log>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
            {
                conn.Open();

                string sql = @"
            SELECT 
                AccountType,
                Accountnumber,
                DpCode,
                FolderPath,
                COUNT(*) AS TotalConverted,
                0 AS TotalFailed
            FROM CPPCIMAGEUPLOAD
            WHERE CAST(UploadedDate AS DATE) = CAST(GETDATE() AS DATE)
            GROUP BY AccountType, Accountnumber, DpCode, FolderPath
            ORDER BY AccountType, DpCode, Accountnumber";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summary.Add(new ConversionSummaryModel_Log
                        {
                            AccountType = reader["AccountType"].ToString(),
                            //= reader["Accountnumber"].ToString(),
                            DpCode = reader["DpCode"].ToString(),
                            FolderPath = reader["FolderPath"].ToString(),
                            TotalConverted = Convert.ToInt32(reader["TotalConverted"]),
                            TotalFailed = Convert.ToInt32(reader["TotalFailed"])
                        });
                    }
                }
            }

            return summary;
        }

        private List<ConversionSummaryModel_Log> GetConversionSummaryFromMainTable()
        {
            var summary = new List<ConversionSummaryModel_Log>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conString"].ConnectionString))
            {
                conn.Open();

                string sql = @"
        SELECT 
            AccountType,
            Accountnumber,
            DpCode,
            FolderPath,
            COUNT(*) AS TotalConverted,
            0 AS TotalFailed
        FROM CPPCIMAGEUPLOAD
        WHERE CAST(UploadedDate AS DATE) = CAST(GETDATE() AS DATE)
        GROUP BY AccountType, Accountnumber, DpCode, FolderPath
        ORDER BY AccountType, DpCode, Accountnumber";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summary.Add(new ConversionSummaryModel_Log
                        {
                            AccountType = reader["AccountType"].ToString(),
                            //= reader["Accountnumber"].ToString(),
                            DpCode = reader["DpCode"].ToString(),
                            FolderPath = reader["FolderPath"].ToString(),
                            TotalConverted = Convert.ToInt32(reader["TotalConverted"]),
                            TotalFailed = Convert.ToInt32(reader["TotalFailed"])
                        });
                    }
                }
            }

            return summary;
        }

        [HttpGet]
        public ActionResult UploadDpCodeFiles()
        {
            return View();
        }


        [HttpPost]
        public ActionResult UploadDpCodeFilesWorking(string department)
        {
            if (Request.Files.Count == 0)
            {
                ViewBag.Message = "Please select at least one file or folder.";
                return View();
            }

            try
            {
                string departmentRoot = $@"D:\{department}";

                // Ensure department folder exists
                if (!Directory.Exists(departmentRoot))
                    Directory.CreateDirectory(departmentRoot);

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (file == null || file.ContentLength == 0)
                        continue;

                    // fullRelativePath = "45123/20357023355.jpg"
                    string fullRelativePath = file.FileName;

                    // DPCode = top-level folder name from selected folder
                    string dpCodeFolderName = fullRelativePath.Split('/', '\\').First(); // 45123

                    // DPCode folder path inside department
                    string dpCodeFolderPath = Path.Combine(departmentRoot, dpCodeFolderName);

                    if (!Directory.Exists(dpCodeFolderPath))
                        Directory.CreateDirectory(dpCodeFolderPath);

                    // Now create folder for the file name without extension
                    string fileName = Path.GetFileName(file.FileName); // 20357023355.jpg
                    string fileFolderName = Path.GetFileNameWithoutExtension(fileName); // 20357023355
                    string fileFolderPath = Path.Combine(dpCodeFolderPath, fileFolderName);

                    if (!Directory.Exists(fileFolderPath))
                        Directory.CreateDirectory(fileFolderPath);

                    // Save file inside this folder
                    string savePath = Path.Combine(fileFolderPath, fileName);
                    file.SaveAs(savePath);
                }

                ViewBag.Message = "All files uploaded successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
            }

            return View();
        }

        [HttpPost]
        public ActionResult UploadDpCodeFiles(string department, IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any())
            {
                ViewBag.Message = "Please select at least one file or folder.";
                return View();
            }

            try
            {
                string departmentRoot = $@"D:\{department}";

                // Department folder must exist
                if (!Directory.Exists(departmentRoot))
                {
                    ViewBag.Message = $"Department folder '{department}' does not exist.";
                    return View();
                }

                bool anyFileSaved = false;

                foreach (var file in files)
                {
                    if (file == null || file.ContentLength == 0)
                        continue;

                    string fileName = Path.GetFileName(file.FileName); // e.g., 20357023355.jpg
                    string accountNumberFolder = Path.GetFileNameWithoutExtension(fileName); // 20357023355

                    // Search all DPCode folders inside department
                    var dpCodeFolders = Directory.GetDirectories(departmentRoot);

                    foreach (var dpCodeFolderPath in dpCodeFolders)
                    {
                        // Check if AccountNumber folder exists inside DPCode
                        string accountFolderPath = Path.Combine(dpCodeFolderPath, accountNumberFolder);

                        if (Directory.Exists(accountFolderPath))
                        {
                            string savePath = Path.Combine(accountFolderPath, fileName);

                            // Save only if file doesn't exist
                            if (!System.IO.File.Exists(savePath))
                            {
                                file.SaveAs(savePath);
                                anyFileSaved = true;
                            }

                            break; // stop searching after first match
                        }
                    }
                }

                ViewBag.Message = anyFileSaved
                    ? "Files uploaded successfully!"
                    : "No matching folder found or files already exist. Nothing uploaded.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
            }

            return View();
        }



















    }
}