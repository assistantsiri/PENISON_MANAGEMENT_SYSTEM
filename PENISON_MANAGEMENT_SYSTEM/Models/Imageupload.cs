using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class AccountUploadGroup
    {
        public string Accountnumber { get; set; }
        public DateTime LatestUploadDate { get; set; }
        public string UploadedBy { get; set; }
        public string AccountType { get; set; }
        public List<Imageupload> Uploads { get; set; }
    }

    public class Imageupload
    {


        public DateTime LatestUploadDate { get; set; }
        public string UploadedBy { get; set; }
        public string AccountType { get; set; }

        public List<HttpPostedFileBase> UploadedFiles { get; set; }
        public int DPCODE { get; set; }
        public string Accountnumber { get; set; }
        public int ID { get; set; }

        public string PPANO { get; set; }
        public string FILENAME { get; set; }
        public DateTime UPLOADEDDATE { get; set; }
        public string UPLOADEDBY { get; set; }
        public string APPROVEDBY { get; set; }
        public string FolderName { get; set; }
        public string ImageNames { get; set; }
        public string Status { get; set; }
        public string FILESIZE { get; set; }
        public string MergedPdfPath { get; set; }


        //================FOR EXIXTING IMAGE ================//
        public string FileName { get; set; }
        public byte[] PdfData { get; set; }
        public DateTime UploadedDate { get; set; }
        public string FileSize { get; set; }
        //================FOR EXIXTING IMAGE ================//
    }
}