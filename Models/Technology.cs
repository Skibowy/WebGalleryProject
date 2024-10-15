using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace WebGalleryProject.Models;
[CollectionName("Technologies")]
public class Technology
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}
