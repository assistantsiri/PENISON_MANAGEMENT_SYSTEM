using iTextSharp.text;
using iTextSharp.text.pdf;
using PENISON_MANAGEMENT_SYSTEM.DA;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    //public class GalleryController : Controller
    //{
    //    public ActionResult Index(string category, string searchText, int page = 1)
    //    {
    //        int pageSize = 20;
    //        var model = new GalleryViewModel
    //        {
    //            SelectedCategory = category,
    //            SearchText = searchText,
    //            Page = page,
    //            Images = new List<ImageData>()
    //        };

    //        if (!string.IsNullOrEmpty(category))
    //        {
    //            string key = "imagefilepath" + category.ToLower();
    //            string rootPath = ConfigurationManager.AppSettings[key];

    //            if (Directory.Exists(rootPath))
    //            {
    //                var folders = Directory.GetDirectories(rootPath);

    //                foreach (var folder in folders)
    //                {
    //                    string dpCode = Path.GetFileName(folder);

    //                    var files = Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly);

    //                    foreach (var f in files)
    //                    {
    //                        model.Images.Add(new ImageData
    //                        {
    //                            FileName = Path.GetFileName(f),
    //                            FullPath = f,
    //                            DpCode = dpCode
    //                        });
    //                    }
    //                }

    //                // Apply search
    //                if (!string.IsNullOrEmpty(searchText))
    //                {
    //                    model.Images = model.Images
    //                        .Where(x => x.FileName.Contains(searchText))
    //                        .ToList();
    //                }

    //                // Pagination
    //                int totalImages = model.Images.Count;
    //                model.TotalPages = (int)Math.Ceiling((double)totalImages / pageSize);

    //                model.Images = model.Images
    //                    .OrderBy(x => x.FileName)
    //                    .Skip((page - 1) * pageSize)
    //                    .Take(pageSize)
    //                    .ToList();
    //            }
    //        }

    //        return View(model);
    //    }

    //    // View Image
    //    public FileResult ViewImage(string path)
    //    {
    //        byte[] img = System.IO.File.ReadAllBytes(path);
    //        return File(img, "image/jpeg");
    //    }


    //    public FileResult Download(string path)
    //    {
    //        byte[] fileBytes = System.IO.File.ReadAllBytes(path);
    //        string fileName = Path.GetFileName(path);
    //        return File(fileBytes, "application/octet-stream", fileName);
    //    }
    //}

    public class GalleryController : Controller
    {
        public ActionResult Index(string category, string searchText, int page = 1)
        {
            int pageSize = 20;
            var model = new GalleryViewModel
            {
                SelectedCategory = category,
                SearchText = searchText,
                Page = page,
                Images = new List<ImageData>()
            };

            if (!string.IsNullOrEmpty(category))
            {
                string key = "imagefilepaths" + category.ToLower();
                string rootPath = ConfigurationManager.AppSettings[key];

                if (Directory.Exists(rootPath))
                {
                    var dpFolders = Directory.GetDirectories(rootPath);

                    foreach (var folder in dpFolders)
                    {
                        string dpCode = Path.GetFileName(folder);

                        var files = Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly);

                        foreach (var f in files)
                        {
                            model.Images.Add(new ImageData
                            {
                                FileName = Path.GetFileName(f),
                                FullPath = f.Replace("\\", "/"), 
                                DpCode = dpCode
                            });
                        }
                    }

                 
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        model.Images = model.Images
                            .Where(x =>
                            {
                                string file = Path.GetFileNameWithoutExtension(x.FileName);
                                return file.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
                            })
                            .ToList();
                    }

                    int totalImages = model.Images.Count;
                    model.TotalPages = (int)Math.Ceiling((double)totalImages / pageSize);

                    model.Images = model.Images
                        .OrderBy(x => x.FileName)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
            }

            return View(model);
        }

        
        public FileResult ViewImage(string path)
        {
            if (!System.IO.File.Exists(path))
                return null;

            byte[] img = System.IO.File.ReadAllBytes(path);
            return File(img, "image/jpeg");
        }

       
        public FileResult Download(string path)
        {
            if (!System.IO.File.Exists(path))
                return null;

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = Path.GetFileName(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        public ActionResult CCPCVIEW()
        {
            return View();
        }

        public ActionResult CPPCVIEW()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetDpCodes(string category)
        {
            var dpCodes = GetDpCodesByCategory(category);
            var repo = new Repository();

            var result = new List<object>();

            foreach (var dp in dpCodes)
            {
                string dpFolder = $@"D:\{category}\{dp}";
                bool isConverted = false;

                if (Directory.Exists(dpFolder))
                {
                    var folderAccounts = Directory.GetFiles(dpFolder)
                        .Select(f =>
                        {
                            string name = Path.GetFileNameWithoutExtension(f);
                            char[] split = { '_', '-', ',', '\'', '.' };
                            return name.Split(split, StringSplitOptions.RemoveEmptyEntries)[0]
                                       .Trim()
                                       .ToUpper();
                        })
                        .Distinct()
                        .ToList();

                    var dbAccounts = repo.GetConvertedAccounts(category, dp);

                   
                    if (folderAccounts.Count > 0 &&
                        folderAccounts.All(a => dbAccounts.Contains(a)))
                    {
                        isConverted = true;
                    }
                }

                result.Add(new
                {
                    DpCode = dp,
                    IsConverted = isConverted
                });
            }

            return Json(result);
        }

        private List<string> GetDpCodesByCategory(string category)
        {
            var dpCodes = new List<string>();

            try
            {
                string basePath = @"D:\";

                string categoryPath = Path.Combine(basePath, category);

                if (!Directory.Exists(categoryPath))
                {
                    return dpCodes;
                }

                dpCodes = Directory.GetDirectories(categoryPath)
                                   .Select(dir => Path.GetFileName(dir))
                                   .OrderBy(name => name)
                                   .ToList();
            }
            catch (Exception ex)
            {

            }

            return dpCodes;
        }

        [HttpPost]
        public ActionResult ConvertMultipleDpToPdf(string category, List<string> dpCodes)
        {
            try
            {
                foreach (var dpCode in dpCodes)
                {
                    
                    ConvertDpInternal(dpCode, category);
                }

                return Json(new
                {
                    success = true,
                    message = "Selected DP folders converted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private void ConvertDpInternal(string dpCode, string category)
        {
            string sourceFolder = $@"D:\{category}\{dpCode}";
            if (!Directory.Exists(sourceFolder)) return;

            var images = Directory.GetFiles(sourceFolder)
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jfif", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!images.Any()) return;

            string basePath = Server.MapPath("~/Uploadspdf");
            Repository repo = new Repository();

            var groupedImages = images.GroupBy(img =>
            {
                string name = Path.GetFileNameWithoutExtension(img);
                char[] split = { '_', '-', ',', '\'', '.' };
                return name.Split(split, StringSplitOptions.RemoveEmptyEntries)[0]
                           .Trim()
                           .ToUpper();
            });

            foreach (var group in groupedImages)
            {
                string accountNumber = group.Key;

                string accountFolder = Path.Combine(basePath, accountNumber);
                if (!Directory.Exists(accountFolder))
                    Directory.CreateDirectory(accountFolder);

                string existingPdfPath = Path.Combine(accountFolder, accountNumber + ".pdf");
                string tempPdfPath = Path.Combine(accountFolder, Guid.NewGuid() + ".pdf");

                
                using (FileStream fs = new FileStream(tempPdfPath, FileMode.Create))
                {
                    Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    foreach (var img in group)
                    {
                        var image = Image.GetInstance(img);
                        image.ScaleToFit(PageSize.A4.Width - 20, PageSize.A4.Height - 20);
                        image.Alignment = Element.ALIGN_CENTER;
                        doc.Add(image);
                        doc.NewPage();
                    }
                    doc.Close();
                }

               
                if (System.IO.File.Exists(existingPdfPath))
                {
                    string mergedPdfPath = Path.Combine(accountFolder, "merged_temp.pdf");

                    MergePdfs(existingPdfPath, tempPdfPath, mergedPdfPath);

                    System.IO.File.Delete(existingPdfPath);
                    System.IO.File.Delete(tempPdfPath);
                    System.IO.File.Move(mergedPdfPath, existingPdfPath);
                }
                else
                {
                    System.IO.File.Move(tempPdfPath, existingPdfPath);
                }

                
                FileInfo fi = new FileInfo(existingPdfPath);
                decimal sizeKB = Math.Round((decimal)fi.Length / 1024, 2);

                repo.SavePdfDetails(
                    dpCode,
                    accountNumber,
                    category,
                    $"/{category}/{accountNumber}/{accountNumber}.pdf",
                    sizeKB,
                    User.Identity.Name
                );
            }
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
    }
}