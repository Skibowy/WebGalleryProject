using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System;

namespace MongoWebGallery.Models
{
    [CollectionName("Answers")]
    public class Answer
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public Guid UserId { get; set; }
        public ObjectId ImageId { get; set; }
        public bool BoolAnswer1 { get; set; }
        public bool BoolAnswer2 { get; set; }
        public bool BoolAnswer3 { get; set; }
        public bool BoolAnswer4 { get; set; }
        public int RatingAnswer { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
