using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class UploadModelForFile
    {
       
            [Required(ErrorMessage = "Please select an account type")]
            [Display(Name = "Account Type")]
            public string AccountType { get; set; }

            [Required(ErrorMessage = "Please enter a file name")]
            [Display(Name = "File Name")]
            public string FileName { get; set; }

            [Required(ErrorMessage = "Please select a file")]
            [Display(Name = "Document")]
            public HttpPostedFileBase Document { get; set; }

            public List<string> AccountTypes => new List<string> { "RAILWAYS", "STATE", "POSTAL" };
        
    }

    public class PensionCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
    }

   
    //public class PensionFile
    //{
    //    public int FileId { get; set; }
    //    public string FileName { get; set; }
    //    public string FilePath { get; set; }
    //    public long FileSize { get; set; }
    //    public DateTime UploadDate { get; set; }
    //    public int Year { get; set; }
    //    public string Month { get; set; }
    //    public int CategoryId { get; set; }
    //    public string CategoryName { get; set; }
    //    public string Section { get; set; }
    //    public string UploadedBy { get; set; }
    //}

    
    public class FileUploadModel
    {
        public HttpPostedFileBase File { get; set; }
        public int Year { get; set; }
        public string Month { get; set; }
        public int CategoryId { get; set; }
        public string Section { get; set; }
    }

   
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class PensionFile
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public int Year { get; set; }
        public string Month { get; set; }
        public string Section { get; set; }
        public string UploadedBy { get; set; }
    }
}