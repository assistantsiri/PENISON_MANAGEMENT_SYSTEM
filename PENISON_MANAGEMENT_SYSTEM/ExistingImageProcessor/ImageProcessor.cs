using iTextSharp.text;
using iTextSharp.text.pdf;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.ExistingImageProcessor
{
    //public class ImageProcessor
    //{
    //    private readonly string _connectionString;
    //    private Guid _currentSessionId;
    //    public ImageProcessor()
    //    {
    //        _connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
    //        _currentSessionId = Guid.NewGuid();
    //    }

    //    public void ProcessAllConfiguredFolders()
    //    {
    //        var folders = new[]
    //        {
    //        new { Path = ConfigurationManager.AppSettings["imagefilepathpostal"], Type = "Postal" },
    //        new { Path = ConfigurationManager.AppSettings["imagefilepathTelecom"], Type = "Telecom" },
    //        new { Path = ConfigurationManager.AppSettings["imagefilepathRailways"], Type = "Railways" }
    //    };

    //        foreach (var folder in folders)
    //        {
    //            if (!string.IsNullOrEmpty(folder.Path) && Directory.Exists(folder.Path))
    //            {
    //                string typeShort = folder.Type.Substring(0, 1);
    //                ProcessAndSaveImages(folder.Path, typeShort);
    //            }
    //        }
    //    }

    //    private void ProcessAndSaveImages(string basePath, string accountType)
    //    {
    //        foreach (var dir in Directory.GetDirectories(basePath))
    //        {
    //            var jpgFiles = Directory.GetFiles(dir, "*.jpg");

    //            var grouped = jpgFiles.GroupBy(f =>
    //            {
    //                var name = Path.GetFileNameWithoutExtension(f);
    //                var parts = name.Split('-', '_');
    //                return parts[0];
    //            });

    //            foreach (var group in grouped)
    //            {
    //                string accountNumber = group.Key;
    //                byte[] pdfBytes = ConvertImagesToPdf(group.ToList());
    //                SavePdfToDb(accountNumber, pdfBytes, accountType);
    //            }
    //        }
    //    }

    //    private byte[] ConvertImagesToPdf(List<string> imagePaths)
    //    {
    //        using (var ms = new MemoryStream())
    //        {
    //            using (var doc = new Document(PageSize.A4))
    //            {
    //                PdfWriter.GetInstance(doc, ms);
    //                doc.Open();

    //                foreach (var imgPath in imagePaths)
    //                {
    //                    var img = iTextSharp.text.Image.GetInstance(imgPath);
    //                    img.ScaleToFit(doc.PageSize.Width - 20, doc.PageSize.Height - 20);
    //                    img.Alignment = Element.ALIGN_CENTER;
    //                    doc.Add(img);
    //                    doc.NewPage();
    //                }

    //                doc.Close();
    //            }
    //            return ms.ToArray();
    //        }
    //    }

    //    private void SavePdfToDb(string accountNumber, byte[] pdfBytes, string accountType)
    //    {
    //        using (var conn = new SqlConnection(_connectionString))
    //        {
    //            conn.Open();
    //            string sql = @"INSERT INTO CPPCIMAGEUPLOAD
    //                       (AccountNumber, FileName, PdfData, UploadedDate, UploadedBy, AccountType, FileSize)
    //                       VALUES (@Acc, @FileName, @PdfData, GETDATE(), @UploadedBy, @AccountType, @FileSize)";

    //            using (var cmd = new SqlCommand(sql, conn))
    //            {
    //                cmd.Parameters.AddWithValue("@Acc", accountNumber);
    //                cmd.Parameters.AddWithValue("@FileName", accountNumber + ".pdf");
    //                cmd.Parameters.AddWithValue("@PdfData", pdfBytes);
    //                cmd.Parameters.AddWithValue("@UploadedBy", "SystemJob");
    //                cmd.Parameters.AddWithValue("@AccountType", accountType);
    //                cmd.Parameters.AddWithValue("@FileSize", pdfBytes.Length);

    //                cmd.ExecuteNonQuery();
    //            }
    //        }
    //    }
    //}

    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using iTextSharp.text;
    using iTextSharp.text.pdf;

    public class ImageProcessor
    {
        private readonly string _connectionString;
        private Guid _currentSessionId;

        public ImageProcessor()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            _currentSessionId = Guid.NewGuid();
        }

        public ConversionResult ProcessAllConfiguredFolders()
        {
            var result = new ConversionResult
            {
                SessionId = _currentSessionId
            };

            var folders = new[]
            {
            new { Path = ConfigurationManager.AppSettings["imagefilepathpostal"], Type = "Postal" },
            new { Path = ConfigurationManager.AppSettings["imagefilepathTelecom"], Type = "Telecom" },
            new { Path = ConfigurationManager.AppSettings["imagefilepathRailways"], Type = "Railways" }
        };

            foreach (var folder in folders)
            {
                if (!string.IsNullOrEmpty(folder.Path) && Directory.Exists(folder.Path))
                {
                    string typeShort = folder.Type.Substring(0, 1);
                    var folderResult = ProcessAndSaveImages(folder.Path, typeShort);
                    result.FolderResults.Add(folderResult);
                }
            }

            result.TotalConverted = result.FolderResults.Sum(f => f.ConvertedFiles);
            result.TotalFailed = result.FolderResults.Sum(f => f.FailedFiles);

            return result;
        }

        private FolderConversionResult ProcessAndSaveImages(string basePath, string accountType)
        {
            var folderResult = new FolderConversionResult
            {
                FolderPath = basePath,
                AccountType = accountType
            };

           
            //long mainLogId = CreateMainConversionLog(basePath, accountType);

            foreach (var dpFolder in Directory.GetDirectories(basePath))
            {
                string dpCode = Path.GetFileName(dpFolder);

                
                //long dpLogId = CreateDpConversionLog(dpCode, dpFolder, accountType, mainLogId);

                try
                {
                    var jpgFiles = Directory.GetFiles(dpFolder, "*.jpg");
                    folderResult.TotalFiles += jpgFiles.Length;

                    if (jpgFiles.Length == 0)
                    {
                        //UpdateDpConversionLog(dpLogId, 0, 0, 0, "Completed - No Files");
                        continue;
                    }

                    var grouped = jpgFiles.GroupBy(f =>
                    {
                        var name = Path.GetFileNameWithoutExtension(f);
                        var parts = name.Split('-', '_');
                        return parts.Length > 0 ? parts[0] : "Unknown";
                    });

                    int convertedInDp = 0;
                    int failedInDp = 0;

                    foreach (var group in grouped)
                    {
                        string accountNumber = group.Key;
                        string currentFile = group.First();

                        try
                        {
                            
                            byte[] pdfBytes = ConvertImagesToPdf(group.ToList());

                         
                            SavePdfToDb(accountNumber, pdfBytes, accountType, dpCode, dpFolder);

                          
                            //LogFileConversion(dpLogId, accountNumber, currentFile, "Success", null, pdfBytes.Length);
                            convertedInDp++;
                            folderResult.ConvertedFiles++;

                        }
                        catch (Exception ex)
                        {
                          
                            //LogFileConversion(dpLogId, accountNumber, currentFile, "Failed", ex.Message, 0);
                            failedInDp++;
                            folderResult.FailedFiles++;
                        }

                       
                        UpdateProgress(currentFile, convertedInDp + failedInDp, jpgFiles.Length, dpCode, basePath, accountType);
                    }

                    //UpdateDpConversionLog(dpLogId, jpgFiles.Length, convertedInDp, failedInDp, "Completed");
                }
                catch (Exception ex)
                {
                    //UpdateDpConversionLog(dpLogId, 0, 0, 0, "Failed", ex.Message);
                }
            }

            //UpdateMainConversionLog(mainLogId, folderResult.TotalFiles, folderResult.ConvertedFiles, folderResult.FailedFiles, "Completed");

            return folderResult;
        }

        private byte[] ConvertImagesToPdf(List<string> imagePaths)
        {
            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(PageSize.A4))
                {
                    PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    foreach (var imgPath in imagePaths)
                    {
                        var img = iTextSharp.text.Image.GetInstance(imgPath);
                        img.ScaleToFit(doc.PageSize.Width - 20, doc.PageSize.Height - 20);
                        img.Alignment = Element.ALIGN_CENTER;
                        doc.Add(img);
                        doc.NewPage();
                    }

                    doc.Close();
                }
                return ms.ToArray();
            }
        }

       
        private void SavePdfToDb(string accountNumber, byte[] pdfBytes, string accountType, string dpCode, string folderPath)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO CPPCIMAGEUPLOAD
                        (AccountNumber, FileName, PdfData, UploadedDate, UploadedBy, AccountType, FileSize, DpCode, FolderPath, Status)
                        VALUES (@AccountNumber, @FileName, @PdfData, GETDATE(), @UploadedBy, @AccountType, @FileSize, @DpCode, @FolderPath, 'Success')";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    cmd.Parameters.AddWithValue("@FileName", accountNumber + ".pdf");
                    cmd.Parameters.AddWithValue("@PdfData", pdfBytes);
                    cmd.Parameters.AddWithValue("@UploadedBy", "SystemJob");
                    cmd.Parameters.AddWithValue("@AccountType", accountType);
                    cmd.Parameters.AddWithValue("@FileSize", pdfBytes.Length);
                    cmd.Parameters.AddWithValue("@DpCode", dpCode);
                    cmd.Parameters.AddWithValue("@FolderPath", folderPath);

                    cmd.ExecuteNonQuery();
                }
            }
        }

      
        private long CreateMainConversionLog(string folderPath, string accountType)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO EXISTINGIMAGECONVERSION_LOG 
                          (FolderPath, AccountType, StartTime, Status, SessionId)
                          VALUES (@FolderPath, @AccountType, GETDATE(), 'In Progress', @SessionId);
                          SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FolderPath", folderPath);
                    cmd.Parameters.AddWithValue("@AccountType", accountType);
                    cmd.Parameters.AddWithValue("@SessionId", _currentSessionId);
                    return Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
        }

        private long CreateDpConversionLog(string dpCode, string folderPath, string accountType, long mainLogId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO EXISTINGIMAGECONVERSION_LOG 
                          (DpCode, FolderPath, AccountType, StartTime, Status, SessionId)
                          VALUES (@DpCode, @FolderPath, @AccountType, GETDATE(), 'In Progress', @SessionId);
                          SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DpCode", dpCode);
                    cmd.Parameters.AddWithValue("@FolderPath", folderPath);
                    cmd.Parameters.AddWithValue("@AccountType", accountType);
                    cmd.Parameters.AddWithValue("@SessionId", _currentSessionId);
                    
                    return Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
        }

        private void UpdateMainConversionLog(long logId, int totalFiles, int converted, int failed, string status)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE EXISTINGIMAGECONVERSION_LOG 
                          SET TotalFiles = @TotalFiles, 
                              ConvertedFiles = @Converted, 
                              FailedFiles = @Failed,
                              EndTime = GETDATE(),
                              Status = @Status
                          WHERE LogId = @LogId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LogId", logId);
                    cmd.Parameters.AddWithValue("@TotalFiles", totalFiles);
                    cmd.Parameters.AddWithValue("@Converted", converted);
                    cmd.Parameters.AddWithValue("@Failed", failed);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateDpConversionLog(long logId, int totalFiles, int converted, int failed, string status, string errorMessage = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"UPDATE EXISTINGIMAGECONVERSION_LOG 
                          SET TotalFiles = @TotalFiles, 
                              ConvertedFiles = @Converted, 
                              FailedFiles = @Failed,
                              EndTime = GETDATE(),
                              Status = @Status,
                              ErrorMessage = @ErrorMessage
                          WHERE LogId = @LogId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LogId", logId);
                    cmd.Parameters.AddWithValue("@TotalFiles", totalFiles);
                    cmd.Parameters.AddWithValue("@Converted", converted);
                    cmd.Parameters.AddWithValue("@Failed", failed);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LogFileConversion(long logId, string accountNumber, string filePath, string status, string errorMessage, long fileSize)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO EXISTINGFILECONVERSION_LOG 
                          (LogId, AccountNumber, FileName, FilePath, Status, ErrorMessage, FileSize, PdfGenerated)
                          VALUES (@LogId, @AccountNumber, @FileName, @FilePath, @Status, @ErrorMessage, @FileSize, @PdfGenerated)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LogId", logId);
                    cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    cmd.Parameters.AddWithValue("@FileName", Path.GetFileName(filePath));
                    cmd.Parameters.AddWithValue("@FilePath", filePath);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileSize", fileSize);
                    cmd.Parameters.AddWithValue("@PdfGenerated", status == "Success");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateProgress(string currentFile, int processed, int total, string dpCode, string folderPath, string accountType)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

             
                string checkSql = "SELECT COUNT(*) FROM EXISTINGIMAGECONVERSION_PROGRESS WHERE FolderPath = @FolderPath AND DpCode = @DpCode";
                using (var checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@FolderPath", folderPath);
                    checkCmd.Parameters.AddWithValue("@DpCode", dpCode);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exists == 0)
                    {
                        
                        string insertSql = @"INSERT INTO EXISTINGIMAGECONVERSION_PROGRESS 
                                      (FolderPath, DpCode, AccountType, CurrentFile, ProcessedFiles, TotalFiles, Status, SessionId)
                                      VALUES (@FolderPath, @DpCode, @AccountType, @CurrentFile, @ProcessedFiles, @TotalFiles, 'In Progress', @SessionId)";

                        using (var insertCmd = new SqlCommand(insertSql, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@FolderPath", folderPath);
                            insertCmd.Parameters.AddWithValue("@DpCode", dpCode);
                            insertCmd.Parameters.AddWithValue("@AccountType", accountType);
                            insertCmd.Parameters.AddWithValue("@CurrentFile", currentFile);
                            insertCmd.Parameters.AddWithValue("@ProcessedFiles", processed);
                            insertCmd.Parameters.AddWithValue("@TotalFiles", total);
                            insertCmd.Parameters.AddWithValue("@SessionId", _currentSessionId);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                       
                        string updateSql = @"UPDATE EXISTINGIMAGECONVERSION_PROGRESS 
                                      SET CurrentFile = @CurrentFile, 
                                          ProcessedFiles = @ProcessedFiles, 
                                          TotalFiles = @TotalFiles,
                                          LastUpdated = GETDATE()
                                      WHERE FolderPath = @FolderPath AND DpCode = @DpCode";

                        using (var updateCmd = new SqlCommand(updateSql, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@FolderPath", folderPath);
                            updateCmd.Parameters.AddWithValue("@DpCode", dpCode);
                            updateCmd.Parameters.AddWithValue("@CurrentFile", currentFile);
                            updateCmd.Parameters.AddWithValue("@ProcessedFiles", processed);
                            updateCmd.Parameters.AddWithValue("@TotalFiles", total);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}