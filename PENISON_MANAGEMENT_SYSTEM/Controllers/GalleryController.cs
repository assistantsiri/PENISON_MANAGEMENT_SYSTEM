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
        public ActionResult GetCategoryWiseImage(string category, string dpCode = null)
        {
            var model = new GalleryViewModel
            {
                SelectedCategory = category,
                SelectedDpCode = dpCode,
                DpCodes = new List<string>(),
                Images = new List<ImageData>()
            };

            if (!string.IsNullOrEmpty(category))
            {
                string key = "imagefilepaths" + category.ToLower();
                string rootPath = ConfigurationManager.AppSettings[key];

                if (Directory.Exists(rootPath))
                {
                    // STEP 1: Load DP folders
                    model.DpCodes = Directory.GetDirectories(rootPath)
                                             .Select(Path.GetFileName)
                                             .OrderBy(x => x)
                                             .ToList();

                    // STEP 2: Load images ONLY after DP selected
                    if (!string.IsNullOrEmpty(dpCode))
                    {
                        string dpPath = Path.Combine(rootPath, dpCode);

                        if (Directory.Exists(dpPath))
                        {
                            var files = Directory.GetFiles(dpPath, "*.jpg");

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
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ConvertDpToPdf(string category, string dpCode)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(dpCode))
                return RedirectToAction("Index");

            // SOURCE IMAGE PATH
            string key = "imagefilepaths" + category.ToLower();
            string rootPath = ConfigurationManager.AppSettings[key];
            string dpImagePath = Path.Combine(rootPath, dpCode);

            if (!Directory.Exists(dpImagePath))
                return RedirectToAction("Index", new { category });

            var imageFiles = Directory.GetFiles(dpImagePath, "*.jpg");

            if (imageFiles.Length == 0)
                return RedirectToAction("Index", new { category, dpCode });

            /* ========= TARGET PDF FOLDER ========= */

            string dpPdfFolder = Path.Combine(Server.MapPath("~/UploadedPdfs"), dpCode);
            if (!Directory.Exists(dpPdfFolder))
            {
                Directory.CreateDirectory(dpPdfFolder);
            }

            string existingPdfPath = Path.Combine(dpPdfFolder, dpCode + ".pdf");
            string newPdfPath = Path.Combine(dpPdfFolder, Guid.NewGuid().ToString() + ".pdf");

            /* ========= CREATE PDF FROM JPG ========= */

            using (var fs = new FileStream(newPdfPath, FileMode.Create, FileAccess.Write))
            {
                using (var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4))
                {
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    foreach (var imgPath in imageFiles)
                    {
                        var img = iTextSharp.text.Image.GetInstance(imgPath);
                        img.ScaleToFit(doc.PageSize.Width - 40, doc.PageSize.Height - 40);
                        img.Alignment = Element.ALIGN_CENTER;
                        doc.Add(img);
                        doc.NewPage();
                    }

                    doc.Close();
                }
            }

            /* ========= MERGE IF PDF EXISTS ========= */

            if (System.IO.File.Exists(existingPdfPath))
            {
                string mergedPdfPath = Path.Combine(dpPdfFolder, "merged_temp.pdf");

                MergePdfs(existingPdfPath, newPdfPath, mergedPdfPath);

                System.IO.File.Delete(existingPdfPath);
                System.IO.File.Delete(newPdfPath);

                System.IO.File.Move(mergedPdfPath, existingPdfPath);
            }
            else
            {
                System.IO.File.Move(newPdfPath, existingPdfPath);
            }

            return File(existingPdfPath, "application/pdf", dpCode + ".pdf");
        }
        private void MergePdfs(string pdf1, string pdf2, string outputPath)
        {
            using (var stream = new FileStream(outputPath, FileMode.Create))
            {
                var document = new iTextSharp.text.Document();
                var pdf = new PdfCopy(document, stream);
                document.Open();

                foreach (string file in new[] { pdf1, pdf2 })
                {
                    var reader = new PdfReader(file);
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        pdf.AddPage(pdf.GetImportedPage(reader, i));
                    }
                    reader.Close();
                }

                document.Close();
            }
        }

    }
}