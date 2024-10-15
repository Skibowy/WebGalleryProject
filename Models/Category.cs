using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
namespace WebGalleryProject.Models;

[CollectionName("Categories")]
public class Category
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
}
