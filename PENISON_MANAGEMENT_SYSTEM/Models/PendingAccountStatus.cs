using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class PendingAccountStatus
    {
        public string AccountType { get; set; }
        public int PendingCount { get; set; }

        public int CurrentDataUploadedCount { get; set; }
    }
}