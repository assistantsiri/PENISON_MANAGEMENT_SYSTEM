//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;


//namespace PENISON_MANAGEMENT_SYSTEM.Controllers
//{
//    public class SFTPController : Controller
//    {
//        // GET: SFTP
//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult SFTPUPLOADDATA()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult SFTPUPLOADDATA(HttpPostedFileBase uploadedFile)
//        {
//            if (uploadedFile == null || uploadedFile.ContentLength == 0)
//            {
//                TempData["Message"] = "No file selected.";
//                return RedirectToAction("SFTPUPLOADDATA");
//            }

//            try
//            {
//                string fileName = Path.GetFileName(uploadedFile.FileName);
//                string tempFilePath = Server.MapPath("~/TempUploads/");
//                if (!Directory.Exists(tempFilePath))
//                    Directory.CreateDirectory(tempFilePath);

//                string fullPath = Path.Combine(tempFilePath, fileName);
//                uploadedFile.SaveAs(fullPath);

//                string result = FileTransferRJSINSFTP(fullPath, "/remote/folder/");
//                LogHelper.WriteLog("Upload Result: " + result);

//                TempData["Message"] = result;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.WriteLog("Upload Exception: " + ex.ToString());
//                TempData["Message"] = "Upload failed: " + ex.Message;
//            }

//            return RedirectToAction("SFTPUPLOADDATA");
//        }

//        public static string FileTransferRJSINSFTP(string sourcePath, string remoteFolder)
//        {
//            try
//            {
//                string fileName = System.IO.Path.GetFileName(sourcePath);
//                string remoteBase = ConfigurationManager.AppSettings["PMSUPLOADEDDATA"].ToString();
//                string remoteFullPath = $"{remoteBase}{fileName}";

//                SessionOptions sessionOptions = new SessionOptions
//                {
//                    Protocol = Protocol.Sftp,
//                    HostName = ConfigurationManager.AppSettings["DataSFTPIP"],
//                    UserName = ConfigurationManager.AppSettings["DataSFTPUser"],
//                    Password = ConfigurationManager.AppSettings["DataSFTPPasswd"],
//                    PortNumber = 22,
//                    SshHostKeyFingerprint = ConfigurationManager.AppSettings["DataSFTPSSH"]
//                };

//                using (Session session = new Session())
//                {
//                    session.Open(sessionOptions);

//                    TransferOptions transferOptions = new TransferOptions
//                    {
//                        TransferMode = TransferMode.Binary
//                    };

//                    TransferOperationResult transferResult = session.PutFiles(sourcePath, remoteFullPath, false, transferOptions);

//                    transferResult.Check();
//                    session.Close();
//                }

//                return "File uploaded successfully.";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.WriteLog("SFTP Error: " + ex.ToString());
//                return $"SFTP Upload failed: {ex.Message}";
//            }
//        }
//    }

//    public static class LogHelper
//    {
//        public static void WriteLog(string message)
//        {
//            try
//            {
//                string logDir = HttpContext.Current.Server.MapPath("~/Logs/");
//                if (!Directory.Exists(logDir))
//                    Directory.CreateDirectory(logDir);

//                string logFile = Path.Combine(logDir, "log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
//                using (StreamWriter sw = new StreamWriter(logFile, true))
//                {
//                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
//                }
//            }
//            catch (Exception ex)
//            {
                
//            }
//        }
//    }
//}