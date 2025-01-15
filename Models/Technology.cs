using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System;

namespace MongoWebGallery.Models
{
    [CollectionName("Technologies")]
    public class Technology
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRequired]
        public string Name { get; set; }

        [BsonRequired]
        public string Url { get; set; }

        public bool IsApproved { get; set; } = false;

        public Guid? SubmittedBy { get; set; }
    }
}
