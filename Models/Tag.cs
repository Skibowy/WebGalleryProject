using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.Collections.Generic;

namespace MongoWebGallery.Models
{
    [CollectionName("Tags")]
    public class Tag
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}
