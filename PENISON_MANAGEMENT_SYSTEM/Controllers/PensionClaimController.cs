using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PENISON_MANAGEMENT_SYSTEM.Models;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class PensionClaimController : Controller
    {
        private readonly string baseFolder = "PensionClaimData";
        private readonly string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long maxFileSize = 50 * 1024 * 1024; // 50MB

        public ActionResult Index()
        {
            var model = new PensionClaimViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult GetSubFolders(string mainFolder)
        {
            List<string> subFolders = new List<string>();
            bool showCategory = false;

            switch (mainFolder)
            {
                case "PENSION RECOVERY REMITTANCE DATA":
                    subFolders = new List<string>
                    {
                        "CENTRAL RECOVERY SCROL",
                        "KHAJANEE II RECOVERY REMITTANCE DATA",
                        "OTHER STATE RECOVERY REMITTANCE DATA",
                        "GL BASED RECOVERY DATA",
                        "IBA BASED RECOVERY DATA"
                    };
                    showCategory = false;
                    break;

                case "PENSION PAYMENT DATA":
                    subFolders = new List<string>
                    {
                        "MANUAL PENSION PAYMENT DETAILS",
                        "PENSION PAYMENT DETAILS"
                    };
                    showCategory = false;
                    break;

                case "GL AND IBA":
                    subFolders = new List<string>();
                    showCategory = false;
                    break;
            }

            return Json(new
            {
                subFolders = subFolders,
                showSubFolder = subFolders.Any(),
                showCategory = showCategory
            });
        }

        [HttpPost]
        public ActionResult GetCategories(string mainFolder, string subFolder)
        {
            List<string> categories = new List<string>();
            bool showCategory = false;

            if (mainFolder == "PENSION RECOVERY REMITTANCE DATA" && subFolder == "CENTRAL RECOVERY SCROL")
            {
                categories = new List<string>
                {
                    "CIVIL",
                    "DEFENCE",
                    "RAILWAYS",
                    "POSTAL",
                    "TELECOM"
                };
                showCategory = categories.Any();
            }

            return Json(new
            {
                categories = categories,
                showCategory = showCategory
            });
        }

        [HttpPost]
        public ActionResult BrowseFolder(string SelectedFolder, string SelectedSubFolder, string SelectedCategory, int SelectedYear, int SelectedMonth)
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedFolder))
                {
                    return Json(new { success = false, message = "Please select a main folder" });
                }

                // Get folder path with correct structure
                string folderPath = GetFolderPath(SelectedFolder, SelectedSubFolder, SelectedCategory, SelectedYear, SelectedMonth);
                string physicalPath = Server.MapPath(folderPath);

                // Create directory if it doesn't exist
                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                // Get all files in the directory
                var files = Directory.GetFiles(physicalPath);
                var fileList = new List<UploadedFile>();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var uploadedFile = new UploadedFile
                    {
                        FileName = fileInfo.Name,
                        FilePath = file,
                        FileType = fileInfo.Extension.Replace(".", "").ToUpper(),
                        FileSize = fileInfo.Length,
                        FormattedSize = FormatFileSize(fileInfo.Length),
                        UploadDate = fileInfo.CreationTime
                    };
                    fileList.Add(uploadedFile);
                }

                // Generate file list HTML
                string fileListHtml = GenerateFileListHtml(fileList);

                // Set current path
                string currentPath = GetDisplayPath(SelectedFolder, SelectedSubFolder, SelectedCategory, SelectedYear, SelectedMonth);

                return Json(new
                {
                    success = true,
                    message = $"Found {fileList.Count} file(s) in {currentPath}",
                    fileListHtml = fileListHtml,
                    currentPath = currentPath,
                    fileCount = fileList.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error browsing folder: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public ActionResult UploadFile()
        {
            try
            {
                // Get form values
                string selectedFolder = Request.Form["SelectedFolder"];
                string selectedSubFolder = Request.Form["SelectedSubFolder"];
                string selectedCategory = Request.Form["SelectedCategory"];
                int selectedYear = Convert.ToInt32(Request.Form["SelectedYear"]);
                int selectedMonth = Convert.ToInt32(Request.Form["SelectedMonth"]);

                HttpPostedFileBase uploadFile = Request.Files["UploadFile"];

                // Validate inputs
                if (string.IsNullOrEmpty(selectedFolder))
                {
                    return Json(new { success = false, message = "Please select a main folder" });
                }

                if (uploadFile == null || uploadFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Please select a file to upload" });
                }

                // Validate file size
                if (uploadFile.ContentLength > maxFileSize)
                {
                    return Json(new { success = false, message = "File size exceeds maximum limit of 50MB" });
                }

                // Validate file extension
                string fileExtension = Path.GetExtension(uploadFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    string allowed = string.Join(", ", allowedExtensions);
                    return Json(new { success = false, message = $"File type not allowed. Allowed types: {allowed}" });
                }

                // Get folder path
                string folderPath = GetFolderPath(selectedFolder, selectedSubFolder, selectedCategory, selectedYear, selectedMonth);
                string physicalPath = Server.MapPath(folderPath);

                // Create directory if it doesn't exist
                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                // Generate safe filename (keep original name but make it unique)
                string originalFileName = Path.GetFileNameWithoutExtension(uploadFile.FileName);
                string extension = Path.GetExtension(uploadFile.FileName);
                string safeFileName = $"{originalFileName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                string fullPath = Path.Combine(physicalPath, safeFileName);

                // Save file
                uploadFile.SaveAs(fullPath);

                // Get updated file list AFTER upload
                var files = Directory.GetFiles(physicalPath);
                var fileList = new List<UploadedFile>();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var uploadedFile = new UploadedFile
                    {
                        FileName = fileInfo.Name,
                        FilePath = file,
                        FileType = fileInfo.Extension.Replace(".", "").ToUpper(),
                        FileSize = fileInfo.Length,
                        FormattedSize = FormatFileSize(fileInfo.Length),
                        UploadDate = fileInfo.CreationTime
                    };
                    fileList.Add(uploadedFile);
                }

                // Generate updated file list HTML
                string fileListHtml = GenerateFileListHtml(fileList);

                // Set current path
                string currentPath = GetDisplayPath(selectedFolder, selectedSubFolder, selectedCategory, selectedYear, selectedMonth);

                return Json(new
                {
                    success = true,
                    message = $"File '{uploadFile.FileName}' uploaded successfully!",
                    fileListHtml = fileListHtml,
                    currentPath = currentPath,
                    fileCount = fileList.Count,
                    refreshNeeded = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error uploading file: {ex.Message}"
                });
            }
        }

        public ActionResult DownloadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = "File not found" }, JsonRequestBehavior.AllowGet);
                }

                string fileName = Path.GetFileName(filePath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error downloading file: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = "File not found" });
                }

                // Get directory path to refresh file list after deletion
                string directoryPath = Path.GetDirectoryName(filePath);
                System.IO.File.Delete(filePath);

                // Get updated file list
                var files = Directory.GetFiles(directoryPath);
                var fileList = new List<UploadedFile>();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var uploadedFile = new UploadedFile
                    {
                        FileName = fileInfo.Name,
                        FilePath = file,
                        FileType = fileInfo.Extension.Replace(".", "").ToUpper(),
                        FileSize = fileInfo.Length,
                        FormattedSize = FormatFileSize(fileInfo.Length),
                        UploadDate = fileInfo.CreationTime
                    };
                    fileList.Add(uploadedFile);
                }

                // Generate updated file list HTML
                string fileListHtml = GenerateFileListHtml(fileList);

                return Json(new
                {
                    success = true,
                    message = "File deleted successfully",
                    fileListHtml = fileListHtml,
                    fileCount = fileList.Count,
                    refreshNeeded = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting file: {ex.Message}" });
            }
        }

        [HttpPost]
        public ActionResult ViewFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = "File not found" });
                }

                string extension = Path.GetExtension(filePath).ToLower();
                string contentType = GetContentType(extension);
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                if (extension == ".pdf" || extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    string base64String = Convert.ToBase64String(fileBytes);
                    return Json(new
                    {
                        success = true,
                        fileData = base64String,
                        contentType = contentType,
                        fileName = Path.GetFileName(filePath),
                        isViewable = true
                    });
                }
                else
                {
                    // For non-viewable files
                    return Json(new
                    {
                        success = true,
                        isViewable = false,
                        fileName = Path.GetFileName(filePath)
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error viewing file: {ex.Message}" });
            }
        }

        #region Helper Methods

        private string GenerateFileListHtml(List<UploadedFile> files)
        {
            if (files == null || !files.Any())
            {
                return @"<div class='empty-state'>
                    <div class='empty-state-icon'>
                        <span class='icon-folder-open'></span>
                    </div>
                    <div class='empty-state-title'>No Files Found</div>
                    <div class='empty-state-text'>
                        The selected folder is empty. Upload files using the form above.
                    </div>
                </div>";
            }

            var html = new System.Text.StringBuilder();
            html.AppendLine("<table class='files-table'>");
            html.AppendLine("<thead><tr><th width='40%'>File Name</th><th width='15%'>Type</th><th width='15%'>Size</th><th width='15%'>Upload Date</th><th width='15%'>Actions</th></tr></thead>");
            html.AppendLine("<tbody>");

            foreach (var file in files)
            {
                string escapedPath = file.FilePath.Replace("\\", "\\\\").Replace("\"", "\\\"");

                html.AppendLine("<tr>");
                html.AppendLine($"<td><span class='{GetFileIcon(file.FileType)} file-icon'></span><span class='file-name'>{file.FileName}</span></td>");
                html.AppendLine($"<td><span class='file-type'>{file.FileType}</span></td>");
                html.AppendLine($"<td>{file.FormattedSize}</td>");
                html.AppendLine($"<td>{file.FormattedDate}</td>");
                html.AppendLine($@"<td>
                    <div class='file-actions'>
                        <button class='action-icon view-btn' onclick='viewFile(""{escapedPath}"")' title='View File'>
                            <span class='icon-view'></span>
                        </button>
                        <button class='action-icon download-btn' onclick='downloadFile(""{escapedPath}"")' title='Download File'>
                            <span class='icon-download'></span>
                        </button>
                        <button class='action-icon delete-btn' onclick='deleteFile(""{escapedPath}"")' title='Delete File'>
                            <span class='icon-delete'></span>
                        </button>
                    </div>
                </td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            return html.ToString();
        }

        private string GetFolderPath(string mainFolder, string subFolder, string category, int year, int month)
        {
            string monthName = new DateTime(year, month, 1).ToString("MMMM");
            string yearMonthFolder = $"{year}/{month:00}-{monthName}";

            // Start with base folder: PensionClaimData
            string path = $"{baseFolder}/{mainFolder}";

            if (!string.IsNullOrEmpty(subFolder))
            {
                path += $"/{subFolder}";
            }

            if (!string.IsNullOrEmpty(category))
            {
                path += $"/{category}";
            }

            path += $"/{yearMonthFolder}";

            return "~/" + path;
        }

        private string GetDisplayPath(string mainFolder, string subFolder, string category, int year, int month)
        {
            string monthName = new DateTime(year, month, 1).ToString("MMMM");
            string path = $"PENSION CLAIM DATA › {mainFolder}";

            if (!string.IsNullOrEmpty(subFolder))
            {
                path += $" › {subFolder}";
            }

            if (!string.IsNullOrEmpty(category))
            {
                path += $" › {category}";
            }

            path += $" › {year} › {monthName}";

            return path;
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB" };
            int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
            return $"{bytes / Math.Pow(1024, i):0.##} {sizes[i]}";
        }

        private string GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            };
        }

        private string GetFileIcon(string fileType)
        {
            var type = fileType.ToLower();
            if (type.Contains("pdf")) return "icon-pdf";
            if (type.Contains("doc")) return "icon-doc";
            if (type.Contains("xls")) return "icon-xls";
            if (type.Contains("jpg") || type.Contains("jpeg") || type.Contains("png") || type.Contains("gif")) return "icon-img";
            if (type.Contains("txt")) return "icon-txt";
            return "icon-file-default";
        }

        #endregion
    }
}