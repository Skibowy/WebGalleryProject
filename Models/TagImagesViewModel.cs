using System.Collections.Generic;

namespace WebGalleryProject.Models
{
    public class TagImagesViewModel
    {
        public Tag Tag { get; set; }
        public List<Image> Images { get; set; }
    }
}
