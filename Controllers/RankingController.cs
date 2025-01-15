using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoWebGallery.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoWebGallery.Controllers
{
    public class RankingController : Controller
    {
        private readonly IMongoCollection<Technology> _technologyCollection;
        private readonly IMongoCollection<Image> _imageCollection;
        private readonly IMongoCollection<Answer> _answerCollection;

        public RankingController(IMongoDatabase database)
        {
            _technologyCollection = database.GetCollection<Technology>("Technologies");
            _imageCollection = database.GetCollection<Image>("Images");
            _answerCollection = database.GetCollection<Answer>("Answers"); 
        }

        public async Task<IActionResult> Index()
        {
            var technologies = await _technologyCollection.Find(_ => true).ToListAsync();

            var technologyRankings = new List<TechnologyRankingViewModel>();

            foreach (var technology in technologies)
            {
                var images = await _imageCollection.Find(img => img.TechnologyId == technology.Id).ToListAsync();
                var ratedImages = images.Where(img => img.AverageRating > 0).ToList();

                var answers = await _answerCollection.Find(ans => images.Select(img => img.Id).Contains(ans.ImageId)).ToListAsync();
                int totalAnswers = answers.Count;

                double averageRating = 0;
                if (ratedImages.Any())
                {
                    averageRating = ratedImages.Average(img => img.AverageRating);
                }

                technologyRankings.Add(new TechnologyRankingViewModel
                {
                    TechnologyId = technology.Id,
                    TechnologyName = technology.Name,
                    AverageRating = averageRating,
                    NumberOfImages = ratedImages.Count,
                    NumberOfAnswers = totalAnswers
                });
            }

            var sortedRankings = technologyRankings.OrderByDescending(tr => tr.AverageRating).ToList();

            return View(sortedRankings);
        }
    }
}
