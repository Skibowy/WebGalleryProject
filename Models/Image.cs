using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MongoWebGallery.Models
{
    [CollectionName("Images")]
    public class Image
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [Required]
        public string Name { get; set; }
        public string Base64Image { get; set; }
        [Required]
        public string Prompt { get; set; }
        public bool IsPublic { get; set; }
        public int ViewCount { get; set; }
        public double AverageRating { get; set; } = 0;
        public DateTime CreatedDate { get; set; }
        public Guid UserId { get; set; }
        public ObjectId CategoryId { get; set; }
        public ObjectId TechnologyId { get; set; }
        public ICollection<ObjectId> Tags { get; set; } = new List<ObjectId>();
        public ICollection<ObjectId> Comments { get; set; } = new List<ObjectId>();
        public ICollection<ObjectId> Answers { get; set; } = new List<ObjectId>();
    }
}
