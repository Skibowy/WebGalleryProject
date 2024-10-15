using System.ComponentModel.DataAnnotations;

namespace WebGalleryProject.Models
{
    public class AnswerViewModel
    {
        [Required]
        public bool BoolAnswer1 { get; set; }
        [Required]
        public bool BoolAnswer2 { get; set; }
        [Required]
        public bool BoolAnswer3 { get; set; }
        [Required]
        public bool BoolAnswer4 { get; set; }
        [Required]
        public int RatingAnswer { get; set; }
    }

}
