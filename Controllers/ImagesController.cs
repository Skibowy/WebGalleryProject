using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoWebGallery.Models;
using System.Reflection;

namespace MongoWebGallery.Controllers
{
    public class ImagesController : Controller
    {
        private readonly IMongoCollection<Image> _imageCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Models.Tag> _tagCollection;
        private readonly IMongoCollection<Answer> _answerCollection;
        private readonly IMongoCollection<Technology> _technologyCollection;
        private readonly UserManager<ApplicationUser> _userManager;

        public ImagesController(
            IMongoCollection<Image> imageCollection,
            IMongoCollection<Category> categoryCollection,
            IMongoCollection<Models.Tag> tagCollection,
            IMongoCollection<Answer> answerCollection,
            IMongoCollection<Technology> technologyCollection,
            UserManager<ApplicationUser> userManager)
        {
            _imageCollection = imageCollection;
            _categoryCollection = categoryCollection;
            _tagCollection = tagCollection;
            _answerCollection = answerCollection;
            _technologyCollection = technologyCollection;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string categoryId, string filter, string sort, string searchQuery, int page = 1, int pageSize = 10)
        {
            var categories = await _categoryCollection.Find(_ => true).ToListAsync();
            ViewBag.Categories = categories;

            Category selectedCategory = null;
            if (!string.IsNullOrEmpty(categoryId))
            {
                selectedCategory = await _categoryCollection.Find(c => c.Id == ObjectId.Parse(categoryId)).FirstOrDefaultAsync();
            }
            ViewBag.SelectedCategory = selectedCategory;

            var filterDefinition = Builders<Image>.Filter.Eq(image => image.IsPublic, true);

            if (selectedCategory != null)
            {
                filterDefinition = Builders<Image>.Filter.And(filterDefinition, Builders<Image>.Filter.Eq(image => image.CategoryId, selectedCategory.Id));
            }

            var now = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "today":
                        filterDefinition = Builders<Image>.Filter.And(filterDefinition, Builders<Image>.Filter.Gte(image => image.CreatedDate, now.AddDays(-1)));
                        break;
                    case "week":
                        filterDefinition = Builders<Image>.Filter.And(filterDefinition, Builders<Image>.Filter.Gte(image => image.CreatedDate, now.AddDays(-7)));
                        break;
                    case "month":
                        filterDefinition = Builders<Image>.Filter.And(filterDefinition, Builders<Image>.Filter.Gte(image => image.CreatedDate, now.AddMonths(-1)));
                        break;
                    case "all":
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                filterDefinition = Builders<Image>.Filter.And(filterDefinition, Builders<Image>.Filter.Regex(image => image.Name, new BsonRegularExpression(searchQuery, "i")));
            }

            var sortDefinition = Builders<Image>.Sort.Descending(image => image.CreatedDate);
            switch (sort)
            {
                case "date":
                    sortDefinition = Builders<Image>.Sort.Descending(image => image.CreatedDate);
                    break;
                case "rating":
                    sortDefinition = Builders<Image>.Sort.Descending(image => image.AverageRating);
                    break;
                case "views":
                    sortDefinition = Builders<Image>.Sort.Descending(image => image.ViewCount);
                    break;
            }

            var images = await _imageCollection
                .Find(filterDefinition)
                .Sort(sortDefinition)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return View(images);
        }

        public async Task<IActionResult> TagIndex(string tagId)
        {
            if (!ObjectId.TryParse(tagId, out ObjectId objectId))
            {
                return NotFound();
            }

            var tag = await _tagCollection.Find(t => t.Id == objectId).FirstOrDefaultAsync();
            if (tag == null)
            {
                return NotFound();
            }

            var images = await _imageCollection
                .Find(i => i.IsPublic && i.Tags.Contains(objectId))
                .ToListAsync();

            var viewModel = new TagImagesViewModel
            {
                Tag = tag,
                Images = images
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TechnologyIndex(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest();
            }

            var technology = await _technologyCollection
                .Find(t => t.Id == objectId)
                .FirstOrDefaultAsync();

            if (technology == null)
            {
                return NotFound();
            }

            var images = await _imageCollection
                .Find(i => i.IsPublic && i.TechnologyId == objectId)
                .ToListAsync();

            var viewModel = new TechnologyImagesViewModel
            {
                Technology = technology,
                Images = images
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> UserImages(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var images = await _imageCollection
                .Find(i => i.IsPublic && i.UserId == userId)
                .ToListAsync();

            var viewModel = new UserImagesViewModel
            {
                UserName = user.UserName,
                Images = images
            };

            return View(viewModel);
        }
    }
}
