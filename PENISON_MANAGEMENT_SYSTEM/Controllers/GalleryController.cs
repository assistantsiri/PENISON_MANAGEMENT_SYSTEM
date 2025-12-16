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
    }
}