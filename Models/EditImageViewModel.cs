using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MongoWebGallery.Models
{
    public class EditImageViewModel
    {
        public ObjectId Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Prompt { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        [Required(ErrorMessage = "Proszę wybrać przynajmniej jeden tag.")]
        public ObjectId? SelectedTag1 { get; set; }

        public ObjectId? SelectedTag2 { get; set; }

        public ObjectId? SelectedTag3 { get; set; }

        [Required]
        public ObjectId SelectedCategoryId { get; set; }

        [Required(ErrorMessage = "Proszę wybrać technologię.")]
        public ObjectId SelectedTechnologyId { get; set; }
    }
}
