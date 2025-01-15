using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;
using System.Runtime.CompilerServices;

namespace MongoWebGallery.Models
{
    [CollectionName("Roles")]
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
    }
}
