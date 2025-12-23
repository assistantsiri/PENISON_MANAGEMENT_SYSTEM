using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class FolderViewModel
    {
        public int FolderId { get; set; }

        [Required(ErrorMessage = "Folder name is required")]
        [StringLength(100, ErrorMessage = "Folder name cannot exceed 100 characters")]
        public string FolderName { get; set; }

        public int? ParentFolderId { get; set; }

        public string ParentFolderName { get; set; }

        public string FullPath { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public int FileCount { get; set; }

        public int SubFolderCount { get; set; }

        public List<FolderViewModel> SubFolders { get; set; }

        public List<FileViewModel> Files { get; set; }

        public bool IsRoot { get { return ParentFolderId == null; } }
    }

    public class FileViewModel_FORSCROLL
    {
        public int FileId { get; set; }

        [Required(ErrorMessage = "Please select a file")]
        public string OriginalFileName { get; set; }

        public string StoredFileName { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        public string FileSizeFormatted
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = FileSize;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return String.Format("{0:0.##} {1}", len, sizes[order]);
            }
        }

        public int FolderId { get; set; }

        public string FolderName { get; set; }

        public int? Year { get; set; }

        public int? Month { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadDate { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        public bool CanDelete { get; set; }

        public string IconClass
        {
            get
            {
                if (FileType.Contains("pdf")) return "fa fa-file-pdf-o text-danger";
                if (FileType.Contains("word") || FileType.Contains("doc")) return "fa fa-file-word-o text-primary";
                if (FileType.Contains("excel") || FileType.Contains("xls")) return "fa fa-file-excel-o text-success";
                if (FileType.Contains("image")) return "fa fa-file-image-o text-info";
                return "fa fa-file-o";
            }
        }
    }

    public class CreateFolderModel
    {
        [Required(ErrorMessage = "Folder name is required")]
        [StringLength(100, ErrorMessage = "Folder name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Folder name can only contain letters, numbers, spaces, hyphens and underscores")]
        public string FolderName { get; set; }

        public int? ParentFolderId { get; set; }

        public string CurrentPath { get; set; }
    }

    public class UploadFileModel
    {
        [Required(ErrorMessage = "Please select a file")]
        public HttpPostedFileBase File { get; set; }

        public int FolderId { get; set; }

        [Display(Name = "Year (Optional)")]
        [Range(2000, 2100, ErrorMessage = "Please enter a valid year")]
        public int? Year { get; set; }

        [Display(Name = "Month (Optional)")]
        [Range(1, 12, ErrorMessage = "Please enter a valid month (1-12)")]
        public int? Month { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
    }

    public class BreadcrumbItem
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public bool IsActive { get; set; }
    }


    public class GalleryViewModel
    {
      
        public string SelectedCategory { get; set; }

       
        public List<string> DpCodes { get; set; }

     
        public HashSet<string> ConvertedFolders { get; set; }

        public GalleryViewModel()
        {
            DpCodes = new List<string>();
            ConvertedFolders = new HashSet<string>();
        }
    }

}