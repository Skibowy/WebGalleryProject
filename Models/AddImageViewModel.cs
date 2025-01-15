using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MongoWebGallery.Models
{
    public class AddImageViewModel
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Prompt { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        [Required(ErrorMessage = "Proszę przesłać poprawny plik obrazu.")]
        public IFormFile ImageFile { get; set; } = null!;

        [Required(ErrorMessage = "Proszę wybrać co najmniej jeden tag.")]
        public ObjectId? SelectedTag1 { get; set; }

        public ObjectId? SelectedTag2 { get; set; }

        public ObjectId? SelectedTag3 { get; set; }

        [Required]
        public ObjectId SelectedCategoryId { get; set; }

        public ObjectId? SelectedTechnologyId { get; set; }

        public string? NewTechnology { get; set; }
        public string? NewTechnologyUrl { get; set; }
    }
}