using System.ComponentModel.DataAnnotations;

namespace MongoWebGallery.Models
{
    public class UserSettingsViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }

}
