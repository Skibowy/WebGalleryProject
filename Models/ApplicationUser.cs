using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
namespace WebGalleryProject.Models;

[CollectionName("Users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{

}
