using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.Models
{
    public class ImageViewModel
    {
        public string SelectedCategory { get; set; }
        public List<string> ImageUrls { get; set; }
    }
    public class ImageData
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string DpCode { get; set; }
    }
    public class GalleryViewModel
    {
        public string SelectedCategory { get; set; }
        public string SearchText { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }

        public List<ImageData> Images { get; set; }
    }


}