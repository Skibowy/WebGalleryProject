using MongoDB.Bson;
using System.Collections.Generic;

namespace WebGalleryProject.Models
{
    public class ImageDetailsViewModel
    {
        public Image Image { get; set; }
        public List<Tag> Tags { get; set; }
        public Category Category { get; set; }
        public Technology Technology { get; set; }
        public List<Comment> Comments { get; set; }
        public bool HasUserAnswered { get; set; }
        public Answer UserAnswer { get; set; }
        public int AnswersCount { get; set; } // Liczba odpowiedzi
        public DateTime CreatedDate { get; set; }
        public string OwnerName { get; set; }
    }
}