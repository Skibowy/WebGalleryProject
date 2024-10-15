using System.ComponentModel.DataAnnotations;

namespace WebGalleryProject.Models;

public class User
{
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

}