using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
namespace WebGalleryProject.Models;
[CollectionName("Images")]public class Image
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; }
    public string Base64Image { get; set; }
    public string Prompt { get; set; }
    public bool IsPublic { get; set; }
    public int ViewCount { get; set; }
    public double AverageRating { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid UserId { get; set; }
    public ObjectId CategoryId { get; set; }
    public ObjectId TechnologyId { get; set; }
    public ICollection<ObjectId> Tags { get; set; } = new List<ObjectId>();
    public ICollection<ObjectId> Comments { get; set; } = new List<ObjectId>();
    public ICollection<ObjectId> Answers { get; set; } = new List<ObjectId>();
}
