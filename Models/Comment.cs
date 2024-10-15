using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace WebGalleryProject.Models;

[CollectionName("Comments")]
public class Comment
{
    [BsonId]
    public ObjectId Id { get; set; }
    public Guid UserId { get; set; }
    public ObjectId ImageId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
}
