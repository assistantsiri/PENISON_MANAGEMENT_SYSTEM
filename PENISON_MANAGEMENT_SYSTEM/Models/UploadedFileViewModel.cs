using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class UploadedFileViewModel
    {
        public string DPCode { get; set; }
        public string AccountNumber { get; set; }
        public string PPA_NO { get; set; }

        public string FileName { get; set; }
        public string FolderName { get; set; }
    }
}