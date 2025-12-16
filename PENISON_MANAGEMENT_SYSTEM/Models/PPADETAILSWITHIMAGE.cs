using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    
    public class PPADETAILSWITHIMAGE
    {
        public string PPA_ACNO { get; set; }
        public string PPA_PPONO { get; set; }
        public string PPA_name { get; set; }
        public string PPA_DPCD { get; set; }
        public string PPA_Type { get; set; }
        public string BankType { get; set; }
        public string CalculatedDpcode { get; set; }
        public List<string> ImagePaths { get; set; } 
        public bool HasImages { get; set; }
        public int ImageCount { get; set; }

        public List<PPADETAILSWITHIMAGE> Accounts { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedBankType { get; set; }
        public List<SelectListItem> BankTypes { get; set; }

        // Pagination properties
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }

    public class RailwayAccountViewModel
    {
        public List<PPADETAILSWITHIMAGE> Accounts { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedBankType { get; set; }
        public List<SelectListItem> BankTypes { get; set; }

        // Pagination properties
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }

    public class ConversionSummaryModel
    {
        public string AccountType { get; set; }
        public int TotalConverted { get; set; }
    }
}