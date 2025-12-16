using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class FileViewModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileSize { get; set; }
        public string LastModified { get; set; }
        public string OnlineViewUrl { get; set; }   
        public string FileType { get; set; }
    }
}