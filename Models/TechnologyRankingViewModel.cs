using MongoDB.Bson;

namespace MongoWebGallery.Models
{
    public class TechnologyRankingViewModel
    {
        public ObjectId TechnologyId { get; set; }
        public string TechnologyName { get; set; }
        public double AverageRating { get; set; }
        public int NumberOfImages { get; set; }
        public int NumberOfAnswers { get; set; }
    }
}
