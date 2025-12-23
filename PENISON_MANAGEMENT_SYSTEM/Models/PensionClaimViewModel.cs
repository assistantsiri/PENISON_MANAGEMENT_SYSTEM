using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class PensionClaimViewModel
    {
        [Display(Name = "Select Main Folder")]
        [Required(ErrorMessage = "Please select a main folder")]
        public string SelectedFolder { get; set; }

        [Display(Name = "Select Sub Folder")]
        public string SelectedSubFolder { get; set; }

        [Display(Name = "Select Category")]
        public string SelectedCategory { get; set; } 

        [Display(Name = "Year")]
        [Required(ErrorMessage = "Please select year")]
        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
        public int SelectedYear { get; set; }

        [Display(Name = "Month")]
        [Required(ErrorMessage = "Please select month")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int SelectedMonth { get; set; }

        [Display(Name = "Upload File")]
        [Required(ErrorMessage = "Please select a file to upload")]
        public HttpPostedFileBase UploadFile { get; set; }

        public List<string> MainFolders { get; set; }
        public List<string> SubFolders { get; set; }
        public List<YearOption> Years { get; set; }
        public List<MonthOption> Months { get; set; }
        public List<UploadedFile> Files { get; set; }
        public string CurrentPath { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public PensionClaimViewModel()
        {
          
            MainFolders = new List<string>
            {
                "PENSION RECOVERY REMITTANCE DATA",
                "GL AND IBA",
                "PENSION PAYMENT DATA"
            };

          
            Years = new List<YearOption>();
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 10; i--)
            {
                Years.Add(new YearOption { Value = i, Text = i.ToString() });
            }

           
            Months = new List<MonthOption>
            {
                new MonthOption { Value = 1, Text = "January" },
                new MonthOption { Value = 2, Text = "February" },
                new MonthOption { Value = 3, Text = "March" },
                new MonthOption { Value = 4, Text = "April" },
                new MonthOption { Value = 5, Text = "May" },
                new MonthOption { Value = 6, Text = "June" },
                new MonthOption { Value = 7, Text = "July" },
                new MonthOption { Value = 8, Text = "August" },
                new MonthOption { Value = 9, Text = "September" },
                new MonthOption { Value = 10, Text = "October" },
                new MonthOption { Value = 11, Text = "November" },
                new MonthOption { Value = 12, Text = "December" }
            };

           
            SelectedYear = currentYear;
            SelectedMonth = DateTime.Now.Month;
            Files = new List<UploadedFile>();
        }
    }

    public class YearOption
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class MonthOption
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
    public class FolderOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class UploadedFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string FormattedSize { get; set; }
        public string FormattedDate => UploadDate.ToString("dd-MMM-yyyy HH:mm");
        public string IconClass => GetFileIcon(FileType);

        private string GetFileIcon(string fileType)
        {
            switch (fileType.ToLower())
            {
                case "pdf":
                    return "fas fa-file-pdf";
                case "doc":
                case "docx":
                    return "fas fa-file-word";
                case "xls":
                case "xlsx":
                    return "fas fa-file-excel";
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                    return "fas fa-file-image";
                case "txt":
                    return "fas fa-file-alt";
                default:
                    return "fas fa-file";
            }
        }
    }

    public class FileOperationModel
    {
        public string FilePath { get; set; }
        public string FolderType { get; set; }
        public string SubFolderType { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}