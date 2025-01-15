using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.Collections.Generic;

namespace MongoWebGallery.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public List<ObjectId> Images { get; set; } = new List<ObjectId>();
    }
}
