using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.Collections.Generic;

namespace MongoWebGallery.Models
{
    [CollectionName("Categories")]
    public class Category
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}
