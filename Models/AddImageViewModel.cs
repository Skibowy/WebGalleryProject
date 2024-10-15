using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebGalleryProject.Models
{
    public class AddImageViewModel
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Prompt { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        [Required(ErrorMessage = "Please upload a valid image file.")]
        public IFormFile ImageFile { get; set; } = null!;

        [Required(ErrorMessage = "Please select at least one tag.")]
        public ObjectId? SelectedTag1 { get; set; }

        public ObjectId? SelectedTag2 { get; set; }

        public ObjectId? SelectedTag3 { get; set; }

        [Required]
        public ObjectId SelectedCategoryId { get; set; }

        [Required(ErrorMessage = "Please select a technology.")]
        public ObjectId SelectedTechnologyId { get; set; }
    }
}
