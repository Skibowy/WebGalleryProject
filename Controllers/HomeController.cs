using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoWebGallery.Models;
using System.Threading.Tasks;

namespace MongoWebGallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoCollection<Image> _imageCollection;
        private readonly IMongoCollection<Answer> _answerCollection;

        public HomeController(IMongoCollection<Image> imageCollection, IMongoCollection<Answer> answerCollection)
        {
            _imageCollection = imageCollection;
            _answerCollection = answerCollection;
        }

        public async Task<IActionResult> Index()
        {
            var publicImages = await _imageCollection
                .Find(image => image.IsPublic)
                .ToListAsync();

            foreach (var image in publicImages)
            {
                var answers = await _answerCollection.Find(a => a.ImageId == image.Id).ToListAsync();
                if (answers.Count > 0)
                {
                    image.AverageRating = answers.Average(a => a.RatingAnswer);
                }
                else
                {
                    image.AverageRating = 0;
                }
            }

            var topRatedImages = publicImages.OrderByDescending(i => i.AverageRating).Take(5).ToList();

            var mostViewedImages = publicImages.OrderByDescending(i => i.ViewCount).Take(5).ToList();

            ViewBag.TopRatedImages = topRatedImages;
            ViewBag.MostViewedImages = mostViewedImages;

            return View(publicImages);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
