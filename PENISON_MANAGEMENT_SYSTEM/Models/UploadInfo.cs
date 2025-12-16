using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class UploadInfo
    {
        public int ID { get; set; }
        public string DPCODE { get; set; }
        public string Accountnumber { get; set; }
        public string PPANO { get; set; }
        public string FILENAME { get; set; }
        public DateTime UPLOADEDDATE { get; set; }
        public string UPLOADEDBY { get; set; }
        public string APPROVEDBY { get; set; }
        public string ACCOUNTTYPE { get; set; }
        public string STATUS { get; set; }
    }
}