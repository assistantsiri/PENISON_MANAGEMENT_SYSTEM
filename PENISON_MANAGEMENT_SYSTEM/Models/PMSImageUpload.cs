using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class PMSImageUpload
    {
        public int ID { get; set; }
        public int DPCODE { get; set; }
        public string Accountnumber { get; set; }
        public string PPANO { get; set; }
        public string FILENAME { get; set; }
        public DateTime UPLOADEDDATE { get; set; }
        public string UPLOADEDBY { get; set; }
        public string APPROVEDBY { get; set; }
        public string ACCOUNTTYPE { get; set; }
        public string FolderName { get; set; }
        public string DpCode { get; set; }
        public string AccountNumber { get; set; }
        public string FolderUrl { get; set; }
        public HttpPostedFileBase UploadedFile { get; set; }
        public List<SelectListItem> AccountTypes { get; set; }
        public string DESCRIPTION { get; set; }
        public string VALUE { get; set; }
        public string FILESIZE {  get; set; }
    }
    public class AccountType
    {
        public string VALUE { get; set; }        
        public string DESCRIPTION { get; set; }  
    }


}