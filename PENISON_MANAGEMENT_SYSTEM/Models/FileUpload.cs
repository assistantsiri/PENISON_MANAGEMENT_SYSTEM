using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class FileUpload
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FolderPath { get; set; }
        public DateTime UploadDate { get; set; }
        public string AccountNo { get; set; }
        public string PPANo { get; set; }

        public int DPCD { get; set; }
        public string AccountType { get; set; }
        public string BankName { get; set; }
        public string Updtby { get; set; }

        /*==============ACCOUNTTYPE=====================*/
          public string DESCRIPTION { get; set; }
          public string VALUE { get; set; }
  
        /*===================================*/
    }

    public class ConversionResult
    {
        public int TotalConverted { get; set; }
        public int TotalFailed { get; set; }
        public List<FolderConversionResult> FolderResults { get; set; } = new List<FolderConversionResult>();
        public Guid SessionId { get; set; }
        public string LogFilePath { get; set; }
    }

    public class FolderConversionResult
    {
        public string FolderPath { get; set; }
        public string AccountType { get; set; }
        public int TotalFiles { get; set; }
        public int ConvertedFiles { get; set; }
        public int FailedFiles { get; set; }
        public List<FileConversionDetail> FileDetails { get; set; } = new List<FileConversionDetail>();
    }

    public class FileConversionDetail
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public long FileSize { get; set; }
    }

    public class ConversionSummaryModel_Log
    {
        public string AccountType { get; set; }
        public int TotalConverted { get; set; }
        public int TotalFailed { get; set; }
        public string FolderPath { get; set; }
        public string DpCode { get; set; }
    }
}