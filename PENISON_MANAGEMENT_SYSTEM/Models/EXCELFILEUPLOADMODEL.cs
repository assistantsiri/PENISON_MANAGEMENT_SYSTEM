using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class EXCELFILEUPLOADMODEL
    {
        public string PPA_ACNO { get; set; }
        public string PPA_PPONO { get; set; }
        public string ppA_name { get; set; }
        public string PPA_DPCD { get; set; }
        public string PPA_Type { get; set; }
        public string PPA_CREATED_BY { get; set; }
        public DateTime PPA_CREATED_DATE { get; set; }
    }
}